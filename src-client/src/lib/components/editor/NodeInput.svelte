<script lang="ts">
	import type { NodeStructure, ParameterInstance, ParameterStructure } from '$lib/nodes';
	import { createEventDispatcher } from 'svelte';
	import EditorAnchorDefaultValue from './EditorAnchorDefaultValue.svelte';
	import EditorAnchor from './EditorAnchor.svelte';
	import { Anchor } from 'svelvet';
	import { state } from '$lib/state.svelte';

	let { structure, input } = $props<{ structure: NodeStructure; input: ParameterInstance }>();

	let connections = $derived(
		(state.web?.connections
			.filter((c) => c.from.instanceId == input.id && c.from.propertyId == input.id)
			.map((c) => [`${c.to.instanceId}`, `${c.to.propertyId}`]) ?? []) as [string, string][]
	);

	const dispatch = createEventDispatcher();
</script>

<div class="input">
	<div class="anchor-wrapper">
		<Anchor let:linked let:hovering let:connecting input id={input.id} nodeConnect {connections}>
			<EditorAnchorDefaultValue
				on:change={() => dispatch('change')}
				{structure}
				bind:parameter={input}
				{linked}
				{connecting}
			/>
			<EditorAnchor {structure} parameter={input} {linked} {hovering} {connecting} input />
		</Anchor>
	</div>
	<div class="name">
		{structure.inputs.concat(structure.outputs).find((i) => i.id == input.id)?.name}
	</div>
</div>
