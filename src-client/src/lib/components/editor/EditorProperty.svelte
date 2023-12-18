<script lang="ts">
	import { get } from 'svelte/store';
	import type { NodeStructure, PropertyStructure, PropertyInstance } from '$lib/nodes';
	import Value from './Value.svelte';
	import { createEventDispatcher } from 'svelte';
	import { state } from '$lib/state.svelte';

	let { property, value } = $props<{
		property: PropertyStructure;
		value: any;
	}>();

	const dispatch = createEventDispatcher();

	// FIXME: The type *could* not exist, but it shouldn't be. We should probably handle this better
	let type = $derived(state.workspace?.types.find((x) => x.type == property.type))!;
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
