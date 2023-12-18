import { state } from './state.svelte';
import { sendMessage } from './ws';

export function runWeb(web: Web) {
	setTimeout(() => {
		let message = {
			Web: web,
			Id: 'RunWeb'
		};

		sendMessage(message);
	}, 1);
}

export function instanceFromId(id: string): NodeInstance {
	return (
		state.workspaces
			.map((w) => w.webs)
			.flat()
			.map((w) => w.instances)
			.flat()
			.find((i) => i.id == id) ?? ({} as NodeInstance)
	);
}

export function saveWorkspace(workspace: Workspace) {
	let message = {
		workspace: workspace,
		id: 'WorkspaceChanged'
	};

	sendMessage(message);
}

export interface Workspace {
	name: string;
	path: string;
	created: Date;
	lastModified: Date;
	color: ColorDefinition;
	webs: Web[];
	structures: NodeStructure[];
	types: TypeDefinition[];
}

export interface Web {
	id: string;
	name: string;
	instances: NodeInstance[];
	connections: NodeInstanceConnection[];
	result: WebResult;
}

export interface NodeStructure {
	nodeType: string;
	name: string;
	category: string;
	searchTerms: string[];
	properties: PropertyStructure[];
	inputs: ParameterStructure[];
	outputs: ParameterStructure[];
	executeMethod: string;
}

export interface ParameterStructure {
	id: string;
	name: string;
	type: string;
	defaultValue: any;
}

export interface PropertyStructure {
	name: string;
	type: string;
	htmlType: string;
}

export interface TypeDefinition {
	name: string;
	type: string;
	htmlType: string;
	values: TypeDefinitionValue[];
	defaultValue: any;
	color: ColorDefinition;
	shape: string;
}

export interface ColorDefinition {
	r: number;
	g: number;
	b: number;
	h: number;
	s: number;
	l: number;
	hex: string;
}

export interface TypeDefinitionValue {
	name: string;
	value: string;
}

export interface NodeInstance {
	id: string;
	nodeType: string;
	properties: PropertyInstance[];
	inputs: ParameterInstance[];
	metadata: NodeMetadata;
}

export interface NodeMetadata {
	position: NodePosition;
}

export interface NodePosition {
	x: number;
	y: number;
}
export interface PropertyInstance {
	name: string;
	type: string;
	value: string;
}
export interface NodeInstanceConnection {
	from: ConnectedProperty;
	to: ConnectedProperty;
}

export interface ConnectedProperty {
	instanceId: string;
	propertyId: string;
}

export interface WebResult {
	values: ParameterInstance[];
	instances: InstanceValue[];
}

export interface ParameterInstance {
	id: string;
	value: string;
}

export interface InstanceValue {
	id: string;
	value: {
		name: string;
	};
}
