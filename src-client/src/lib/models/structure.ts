export interface NodeStructure {
	nodeType: string;
	name: string;
	description: string;
	category: string;
	searchTerms: string[];
	properties: PropertyStructure[];
	inputs: InputParameterStructure[];
	outputs: OutputParameterStructure[];
	executeMethod: string;
}

export interface ParameterStructure {
	id: string;
	name: string;
	description: string;
	type: string;
}

export interface InputParameterStructure extends ParameterStructure {
	id: string;
	name: string;
	description: string;
	type: string;
	defaultValue: string;
}

export interface OutputParameterStructure extends ParameterStructure {}

export interface PropertyStructure {
	id: string;
	name: string;
	description: string;
	type: string;
	defaultValue: string;
}
