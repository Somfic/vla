<script lang="ts">
	import { Edge, type WritableEdge } from 'svelvet';
	import { type ParameterStructure, instanceFromId } from '$lib/nodes';
	import { blendColors } from '$lib/color';
	import { state as s } from '$lib/state.svelte';

	let ref: SVGPathElement | undefined = $state();
	let edge: WritableEdge | undefined = $state();

	let source = $derived(findParameter(edge?.source.id, false));
	let target = $derived(findParameter(edge?.target.id, true));

	let sourceType = $derived(
		s.workspace?.types.find((t) => t.type.replace('&', '') == source?.type.replace('&', ''))
	);

	let targetType = $derived(
		s.workspace?.types.find((t) => t.type.replace('&', '') == target?.type.replace('&', ''))
	);

	let startColor = $derived(sourceType?.color?.hex ?? '#ffffff');
	let stopColor = $derived(targetType?.color?.hex ?? '#ffffff');
	let midwayColor = $derived(blendColors(startColor, stopColor, 0.5));

	let gradientName = $derived(`gradient-${edge?.target.id ?? 'default'}`);

	function findParameter(
		id: `A-${string}` | undefined | null,
		isInput: boolean
	): ParameterStructure | undefined {
		if (id == null) return undefined;

		let parameterId = id.split('/')[0].substring(2); // remove "A-"
		let instanceId = id.split('/')[1].substring(2); // remove "N-"

		let instance = instanceFromId(instanceId);
		let structure = s.workspace?.structures.find((s) => s.nodeType == instance?.nodeType);

		if (isInput) {
			return structure?.inputs.find((p) => p.id == parameterId);
		} else {
			return structure?.outputs.find((p) => p.id == parameterId);
		}
	}
</script>

<Edge bind:edge let:path let:destroy edgeClick={() => alert('Edge clicked')}>
	<!-- Define a gradient that goes from the source color to the target color -->
	<defs>
		<linearGradient id={gradientName} x1="0%" y1="0%" x2="100%" y2="0%">
			<stop offset="0%" stop-color={startColor} />
			<stop offset="50%" stop-color={midwayColor} />
			<stop offset="100%" stop-color={stopColor} />
		</linearGradient>
	</defs>

	<path bind:this={ref} d={path} style:--gradient-name={`url(#${gradientName})`} />
</Edge>

<style lang="scss">
	path {
		// Gradient fill based on the source and target colors
		stroke: var(--gradient-name);
		stroke-width: 4px;
		z-index: 1;
	}
</style>
