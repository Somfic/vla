<script lang="ts">
	import { get } from 'svelte/store';
	import Value from './Value.svelte';
	import type { TypeDefinition } from '$lib/models/definition';
	import { result } from '$lib/state.svelte';

	export let instanceId: string;
	export let parameterId: string;
	export let input: boolean = false;
	export let output: boolean = false;
	export let type: TypeDefinition;

	let value: string | undefined = undefined;

	result.subscribe((r) => {
		let nodeResults = r.find((r) => r.instanceId === instanceId);
		let parameterResults = nodeResults?.inputs.concat(nodeResults?.outputs);
		let parameterResult = parameterResults?.find((r) => r.parameterId === parameterId);
		value = parameterResult?.value;
	});
</script>

<div class="computed" class:input class:output class:value>
	<Value {type} bind:value readonly output />
</div>

<style lang="scss">
	.computed {
		opacity: 0.5;
	}
</style>
