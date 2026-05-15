import { invoke as tauriInvoke, Channel } from "@tauri-apps/api/core";
import { listen as tauriListen, type UnlistenFn } from "@tauri-apps/api/event";
import type { Error as RpcErrorPayload } from "./error";

// Re-exported so generated namespace files import event/stream primitives from
// here (single source of truth, same as `invoke`).
export { Channel };
export type { UnlistenFn };

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

/**
 * Subscribe to a backend broadcast event. Unwraps the Tauri event envelope and
 * passes the typed payload straight to `handler`. Returns an unlisten fn.
 */
export async function listen<T>(
	event: string,
	handler: (payload: T) => void,
): Promise<UnlistenFn> {
	return tauriListen<T>(event, (e) => handler(e.payload));
}

function isErrorPayload(value: unknown): value is RpcErrorPayload {
	return (
		typeof value === "object" &&
		value !== null &&
		"kind" in value &&
		typeof (value as { kind: unknown }).kind === "string"
	);
}
