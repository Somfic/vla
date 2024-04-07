import { get, writable, type Writable } from 'svelte/store';
import { reset, workspaces, executionResults } from './state.svelte';
import { listWorkspaces } from './methods';
import { invoke } from '@tauri-apps/api/tauri';

let ws: WebSocket = null as any;

export let isConnected = writable(false);
export let messages: Writable<string[]> = writable([]);

export function startListening() {
	reset();

	console.debug('Websocket is connecting ...');
	ws = new WebSocket('ws://127.0.0.1:55155');

	ws.onopen = () => {
		console.info('Websocket connected');
		isConnected.set(true);
		listWorkspaces();
	};

	ws.onmessage = (e) => {
		messages.update((m) => [...m, e.data]);

		const message = JSON.parse(e.data) as ISocketMessage;
		//console.trace('<', message);

		switch (message.id.toLowerCase()) {
			case 'workspaces':
				workspaces.set(message.data['workspaces']);
				break;

			case 'execution':
				executionResults.set(message.data['results']);
				console.log(message.data['results']);
				break;
		}
	};

	ws.onerror = (e) => {
		console.error('Websocket error', e);
	};

	ws.onclose = (e) => {
		console.info('Websocket closed', e);
		isConnected.set(false);

		setTimeout(() => {
			startListening();
		}, 5000);
	};
}

export function sendMessage<T>(id: string, data: T = null as any) {
	if (!ws) return;
	if (ws.readyState !== ws.OPEN) return console.log('Not open');

	let message: SocketMessage<T> = new SocketMessage(id, data);

	// console.trace('>', message);

	ws.send(JSON.stringify(message));
}

interface ISocketMessage {
	id: string;
	data: any;
}

class SocketMessage<T> implements ISocketMessage {
	id: string;
	data: T;

	constructor(id: string, data: T = null as any) {
		this.id = id;
		this.data = data;
	}
}
