<script lang="ts">
	import { get } from 'svelte/store';
	import { type TypeDefinition } from '$lib/nodes';
	import Value from './Value.svelte';
	import { web } from '$lib/state.svelte';

	export let id: string;
	export let input: boolean = false;
	export let output: boolean = false;
	export let type: TypeDefinition;

	let value: string | undefined = undefined;

	web.subscribe((r) => {
		if (r == undefined) return;
		if (r.result.values == undefined) return;

		value = r.result.values.find((v) => v.id == id)?.value;
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
