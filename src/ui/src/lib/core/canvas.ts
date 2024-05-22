import type { Connection } from '$lib/models/connection';
import type { Node } from '$lib/models/node';
import { emit } from '@tauri-apps/api/event';
import { writable, type Writable } from 'svelte/store';

export const nodes: Writable<Node[]> = writable([
	{
		id: '1',
		type: 'input',
		data: { label: 'Input Node' },
		position: { x: 0, y: 0 },
		selected: false
	},
	{
		id: '2',
		type: 'default',
		data: { label: 'Node' },
		position: { x: 0, y: 150 },
		selected: false
	}
]);

nodes.subscribe(async (nodes) => {
	await emit('canvas_nodes_changed', nodes);
});

export const connections: Writable<Connection[]> = writable([
	{
		id: '1-2',
		type: 'default',
		source: '1',
		target: '2',
		selected: false
	}
]);

connections.subscribe(async (connections) => {
	await emit('canvas_connections_changed', connections);
});
