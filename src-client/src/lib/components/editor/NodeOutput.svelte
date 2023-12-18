<script lang="ts">
	import type { NodeInstance, NodeStructure, ParameterStructure } from '$lib/nodes';
	import { createEventDispatcher } from 'svelte';
	import EditorAnchor from './EditorAnchor.svelte';
	import { Anchor } from 'svelvet';
	import { state } from '$lib/state.svelte';
	import ComputedValue from './ComputedValue.svelte';

	let { structure, instance, output } = $props<{
		structure: NodeStructure;
		instance: NodeInstance;
		output: ParameterStructure;
	}>();

	let connections = $derived(
		(state.web?.connections
			.filter((c) => c.from.instanceId == output.id && c.from.propertyId == output.id)
			.map((c) => [`${c.to.instanceId}`, `${c.to.propertyId}`]) ?? []) as [string, string][]
	);

	// FIXME: The type definition *could* be undefined, but it shouldn't be. We should probably handle this better
	let typeDefinition = $derived(
		state.workspace?.types.find((t) => t.name.replace('&', '') == output.type.replace('&', ''))
	)!;
</script>

<div class="output">
	<div class="value">
		<ComputedValue type={typeDefinition} id={`${instance.id}.${output.id}`} output />
	</div>
	<div class="name">{output.name}</div>
	<div class="anchor">
		<Anchor
			let:linked
			let:hovering
			let:connecting
			output
			id={output.id}
			multiple={false}
			{connections}
		>
			<EditorAnchor {structure} parameter={output} {linked} {hovering} {connecting} output />
		</Anchor>
	</div>
</div>
