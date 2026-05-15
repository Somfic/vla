import { invoke as tauriInvoke } from "@tauri-apps/api/core";
import type { Error as RpcErrorPayload } from "./error";

export class RpcError extends Error {
	payload: RpcErrorPayload;

	constructor(payload: RpcErrorPayload) {
		super(payload.kind);
		this.name = "RpcError";
		this.payload = payload;
	}
}

export async function invoke<T>(command: string, args?: Record<string, unknown>): Promise<T> {
	try {
		return await tauriInvoke<T>(command, args);
	} catch (err) {
		if (isErrorPayload(err)) {
			throw new RpcError(err);
		}
		throw err;
	}
}

function isErrorPayload(value: unknown): value is RpcErrorPayload {
	return (
		typeof value === "object" &&
		value !== null &&
		"kind" in value &&
		typeof (value as { kind: unknown }).kind === "string"
	);
}
