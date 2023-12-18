<script lang="ts">
	import { get } from 'svelte/store';
	import type { NodeStructure, PropertyStructure, PropertyInstance } from '$lib/nodes';
	import Value from './Value.svelte';
	import { createEventDispatcher } from 'svelte';
	import { workspace } from '$lib/state.svelte';

	export let property: PropertyStructure;
	export let value: any;

	const dispatch = createEventDispatcher();

	$: type = get(workspace)?.types.find((x) => x.type == property.type)!;
</script>

<div class="property">
	<div class="name">{property.name}</div>
	<Value {type} bind:value on:change={() => dispatch('change')} />
</div>

<style lang="scss">
	@import '../../../styles/theme.scss';

	.property {
		display: flex;
		align-items: center;
		justify-content: space-between;

		margin: 1rem 15px;
		gap: 6px;
	}
</style>
