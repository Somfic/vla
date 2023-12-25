import type { Dependency } from './addon';
import type { ColorDefinition, TypeDefinition } from './definition';
import type { NodeStructure } from './structure';
import type { Web } from './web';

export interface Workspace {
	name: string;
	path: string;
	created: Date;
	lastModified: Date;
	color: ColorDefinition;
	webs: Web[];
	structures: NodeStructure[];
	types: TypeDefinition[];
	addons: Dependency[];
}
