<script lang="ts">
	import { get } from 'svelte/store';
	import Value from './Value.svelte';
	import { createEventDispatcher } from 'svelte';
	import { workspace } from '$lib/state.svelte';
	import type { PropertyStructure } from '$lib/models/structure';
	import { findDefinitionByType } from '$lib/definition';

	export let property: PropertyStructure;
	export let value: any;

	const dispatch = createEventDispatcher();

	$: type = findDefinitionByType(property.type)!;
</script>

<div class="property">
	<div class="name">{property.name} ({property.id})</div>
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
