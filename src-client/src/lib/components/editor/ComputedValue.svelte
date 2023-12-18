<script lang="ts">
	import { type TypeDefinition } from '$lib/nodes';
	import Value from './Value.svelte';
	import { state } from '$lib/state.svelte';

	let {
		id,
		input = false,
		output = false,
		type
	} = $props<{
		id: string;
		input?: boolean;
		output?: boolean;
		type: TypeDefinition;
	}>();

	let value = $derived(state.web?.result.values.find((v) => v.id == id)?.value);
</script>

<div class="computed" class:input class:output class:value>
	<Value {type} {value} readonly output />
</div>

<style lang="scss">
	.computed {
		opacity: 0.5;
	}
</style>
