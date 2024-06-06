import type { Connection } from '$lib/models/connection';
import type { Node } from '$lib/models/node';
import { emit } from '@tauri-apps/api/event';
import { writable, type Writable } from 'svelte/store';

export const nodes: Writable<Node[]> = writable([
	{
		id: '1',
		type: 'input',
		data: { label: 'Node 1' },
		position: { x: 250, y: 50 },
		selected: false,
	},
]);

nodes.subscribe(async (nodes) => {
	await emit('canvas_nodes_changed', nodes);
});

export const connections: Writable<Connection[]> = writable([]);

connections.subscribe(async (connections) => {
	await emit('canvas_connections_changed', connections);
});
