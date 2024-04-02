export interface Workspace {
	name: string;
	path: string;
	created: Date;
	lastModified: Date;
	webs: Web[];
}

export interface Web {
	name: string;
	instances: NodeInstance[];
	connections: NodeConnection[];
}

export interface NodeInstance {
	id: string;
	name: string;
	type: string;
	position: { x: number; y: number };
	properties: NamedValue[];
	inputs: NamedValue[];
	outputs: NamedValue[];
}

export interface NodeConnection {
	from: ConnectedProperty;
	to: ConnectedProperty;
}

export interface NamedValue {
	id: string;
	label: string;
	value: unknown;
}

export interface ConnectedProperty {
	node: string;
	id: string;
}
