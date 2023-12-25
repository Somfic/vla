import type { NodeConnection } from './connection';
import type { NodeInstance } from './instance';

export interface Web {
	id: string;
	name: string;
	instances: NodeInstance[];
	connections: NodeConnection[];
}
