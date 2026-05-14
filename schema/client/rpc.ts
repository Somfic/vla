type JsonRpcRequest = {
	jsonrpc: "2.0";
	id: number;
	method: string;
	params?: unknown;
};

type JsonRpcResponse =
	| { jsonrpc: "2.0"; id: number; result: unknown }
	| { jsonrpc: "2.0"; id: number; error: { code: number; message: string; data?: unknown } };

export class RpcError extends Error {
	constructor(public code: number, message: string, public data?: unknown) {
		super(message);
		this.name = "RpcError";
	}
}

type Pending = {
	resolve: (value: unknown) => void;
	reject: (reason: unknown) => void;
};

export class RpcClient {
	#socket: WebSocket;
	#nextId = 1;
	#pending = new Map<number, Pending>();
	#ready: Promise<void>;

	constructor(url: string) {
		this.#socket = new WebSocket(url);
		this.#socket.addEventListener("message", (event) => this.#handleMessage(event));
		this.#ready = new Promise((resolve, reject) => {
			this.#socket.addEventListener("open", () => resolve(), { once: true });
			this.#socket.addEventListener("error", (event) => reject(event), { once: true });
		});
	}

	async ready(): Promise<void> {
		await this.#ready;
	}

	async call<T>(method: string, params?: unknown): Promise<T> {
		await this.#ready;
		const id = this.#nextId++;
		const request: JsonRpcRequest = { jsonrpc: "2.0", id, method, params };
		return new Promise<T>((resolve, reject) => {
			this.#pending.set(id, {
				resolve: (value) => resolve(value as T),
				reject,
			});
			this.#socket.send(JSON.stringify(request));
		});
	}

	close(): void {
		this.#socket.close();
		for (const { reject } of this.#pending.values()) {
			reject(new RpcError(-1, "connection closed"));
		}
		this.#pending.clear();
	}

	#handleMessage(event: MessageEvent): void {
		const raw = typeof event.data === "string" ? event.data : "";
		if (!raw) return;
		const message = JSON.parse(raw) as JsonRpcResponse;
		const pending = this.#pending.get(message.id);
		if (!pending) return;
		this.#pending.delete(message.id);
		if ("error" in message) {
			pending.reject(new RpcError(message.error.code, message.error.message, message.error.data));
		} else {
			pending.resolve(message.result);
		}
	}
}
