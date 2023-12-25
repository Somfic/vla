import type { ParameterInstance } from './instance';

export interface NodeExecutionResult {
	instanceId: string;
	inputs: ParameterResult[];
	outputs: ParameterResult[];
	exception: string;
}

export interface ParameterResult {
	parameterId: string;
	value: any;
}
