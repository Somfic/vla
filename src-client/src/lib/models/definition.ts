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
