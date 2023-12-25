<script lang="ts">
	import { get } from 'svelte/store';
	import { workspace } from '$lib/state.svelte';
	import type {
		InputParameterStructure,
		NodeStructure,
		OutputParameterStructure,
		ParameterStructure
	} from '$lib/models/structure';
	import type { ParameterInstance } from '$lib/models/instance';

	export let input: boolean = false;
	export let output: boolean = false;
	export let structure: NodeStructure;
	export let parameter: ParameterInstance | InputParameterStructure | OutputParameterStructure;
	export let linked: boolean;
	export let hovering: boolean;
	export let connecting: boolean;

	$: parameterType = (structure.inputs as ParameterStructure[])
		.concat(structure.outputs)
		.find((i) => i.id == parameter.id)
		?.type.replace('&', '');
	$: typeDefinition = get(workspace)?.types.find((t) => t.type.replace('&', '') == parameterType)!;
</script>

<div
	class="anchor"
	class:input
	class:output
	class:linked
	class:hovering
	class:connecting
	style={`--type-color: ${typeDefinition.color.hex}`}
>
	<svg viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg">
		{#if typeDefinition.shape == 'circle'}
			<circle cx="50" cy="50" r="40" />
		{:else if typeDefinition.shape == 'diamond'}
			<polygon points="49,1 99,49 49,99 1,49" />
		{:else if typeDefinition.shape == 'square'}
			<rect x="10" y="10" width="80" height="80" />
		{/if}
	</svg>
</div>

<style lang="scss">
	@import '../../../styles/theme.scss';

	.anchor {
		width: 12px;
		height: 12px;
		filter: drop-shadow(0px 0px 5px transparent);
		transition: all ease 200ms;

		&.input {
			margin-left: -5px;
		}

		&.output {
			margin-right: -5px;
		}

		&.connecting,
		&.linked,
		&:hover {
			svg {
				fill: var(--type-color);
			}
		}

		&.linked {
			filter: drop-shadow(0px 0px 5px var(--type-color));
		}

		svg {
			position: absolute;
			top: 0;
			left: 0;
			width: 100%;
			height: 100%;
			fill: $background;
			stroke: var(--type-color);
			stroke-width: 12px;
			stroke-linecap: round;
			transition: all ease 200ms;
		}
	}
</style>
