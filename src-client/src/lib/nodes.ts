import type { Edge, Node } from '@xyflow/svelte';
import type { NamedValue, NodeConnection, NodeInstance } from './models/workspace';

export function edgeToConnection(edge: Edge): NodeConnection {
	if (!edge.id.includes('->')) {
		console.log('edgeToConnection: invalid edge', edge);
		return null as any;
	}

	let from = edge.id.split('->')[0];
	let to = edge.id.split('->')[1];

	return {
		from: {
			node: from.split('~')[0],
			id: from.split('~')[1]
		},
		to: {
			node: to.split('~')[0],
			id: to.split('~')[1]
		}
	};
}

export function connectionToEdge(connection: NodeConnection): Edge {
	return {
		id: `${connection.from.node}~${connection.from.id}->${connection.to.node}~${connection.to.id}`,
		source: `${connection.from.node}`,
		target: `${connection.to.node}`
	};
}

export function nodeToInstance(node: Node): NodeInstance {
	return {
		id: node.id,
		name: node.data['name'] as string,
		type: node.data['type'] as string,
		position: node.position,
		properties: node.data['properties'] as NamedValue[],
		inputs: node.data['inputs'] as NamedValue[],
		outputs: node.data['outputs'] as NamedValue[]
	};
}

export function instanceToNode(instance: NodeInstance): Node {
	return {
		id: instance.id,
		position: instance.position,
		type: 'vla',
		data: {
			label: instance.type.split('Node')[0],
			name: instance.name,
			type: instance.type,
			properties: instance.properties,
			inputs: instance.inputs,
			outputs: instance.outputs
		}
	};
}
