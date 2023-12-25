import type { ColorDefinition } from './definition';

export interface NodeInstance {
	id: string;
	nodeType: string;
	properties: PropertyInstance[];
	inputs: ParameterInstance[];
	metadata: NodeMetadata;
}

export interface PropertyInstance {
	id: string;
	type: string;
	value: string;
}

export interface ParameterInstance {
	id: string;
	defaultValue: string;
}

export interface NodeMetadata {
	position: NodePosition;
	color: ColorDefinition;
}

export interface NodePosition {
	x: number;
	y: number;
}
