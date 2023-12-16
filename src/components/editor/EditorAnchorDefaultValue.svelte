<script lang="ts">
	import { get } from 'svelte/store';
	import {
		types,
		type ParameterStructure,
		typeToDefinition,
		type ParameterInstance,
		type NodeStructure
	} from '../../lib/nodes';
	import Value from './Value.svelte';
	import { createEventDispatcher } from 'svelte';

	export let structure: NodeStructure;
	export let parameter: ParameterInstance;
	export let linked: boolean;
	export let connecting: boolean;

	const dispatch = createEventDispatcher();

	function handleClick(e: MouseEvent) {
		//e.stopImmediatePropagation();
		e.stopPropagation();
		//e.preventDefault();
	}

	$: parameterType = structure.inputs.find((i) => i.id == parameter.id)?.type.replace('&', '')!;
</script>

<div class="default-wrapper" class:linked class:connecting>
	<!-- svelte-ignore a11y-click-events-have-key-events -->
	<!-- svelte-ignore a11y-no-static-element-interactions -->
	<div class="default" on:mousedown={handleClick}>
		<Value
			on:change={() => dispatch('change')}
			type={typeToDefinition(parameterType)}
			bind:value={parameter.value}
			input
		/>
	</div>
</div>

<style lang="scss">
	@import '../../styles/theme.scss';

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
