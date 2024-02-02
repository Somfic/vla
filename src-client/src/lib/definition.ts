import { get } from 'svelte/store';
import { workspace } from './state.svelte';

export function findDefinitionByType(type: string | undefined) {
	if (!type) return undefined;

	return get(workspace)?.types.find((t) => t.type == type);
}

function compareTypes(a: string, b: string) {
	let aComponents = a.split(',').map((c) => c.trim());
	let bComponents = b.split(',').map((c) => c.trim());

	//     name                                namespace
	return aComponents[0] == bComponents[0] && aComponents[1] == bComponents[1];
}
