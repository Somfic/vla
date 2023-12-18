<script lang="ts">
	import { get } from 'svelte/store';
	import { type ParameterStructure, type ParameterInstance, type NodeStructure } from '$lib/nodes';
	import Value from './Value.svelte';
	import { createEventDispatcher } from 'svelte';
	import { state } from '$lib/state.svelte';

	let { structure, parameter, linked, connecting } = $props<{
		structure: NodeStructure;
		parameter: ParameterInstance;
		linked: boolean;
		connecting: boolean;
	}>();

	const dispatch = createEventDispatcher();

	function handleClick(e: MouseEvent) {
		//e.stopImmediatePropagation();
		e.stopPropagation();
		//e.preventDefault();
	}

	let parameterType = $derived(
		structure.inputs
			.concat(structure.outputs)
			.find((i) => i.id == parameter.id)
			?.type.replace('&', '')
	);

	// FIXME: The type *could* not exist, but it shouldn't. We should probably handle this better
	let typeDefinition = $derived(
		state.workspace?.types.find((t) => t.type.replace('&', '') == parameterType)
	)!;
</script>

<div class="default-wrapper" class:linked class:connecting>
	<!-- svelte-ignore a11y-click-events-have-key-events -->
	<!-- svelte-ignore a11y-no-static-element-interactions -->
	<div class="default" on:mousedown={handleClick}>
		<Value
			on:change={() => dispatch('change')}
			type={typeDefinition}
			bind:value={parameter.value}
			input
		/>
	</div>
</div>

<style lang="scss">
	@import '../../../styles/theme.scss';

	.default-wrapper {
		position: absolute;
		top: -0.5rem;
		transition: 200ms ease all;

		&.linked,
		&.connecting {
			opacity: 0;
			pointer-events: none;
		}
	}

	.default {
		display: inline-block;
		left: calc(-100% - 0.5rem);
		position: relative;
		opacity: 0.5;
		transition: all ease 200ms;

		&:hover {
			opacity: 1;
		}
	}
</style>
