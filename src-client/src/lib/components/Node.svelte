<script lang="ts">
	import type { NamedValue } from '$lib/models/workspace';
	import { Handle, Position, type NodeProps } from '@xyflow/svelte';
	import type { Writable } from 'svelte/store';

	type $$Props = NodeProps;

	export let data: {
		name: string;
		inputs: NamedValue[];
		outputs: NamedValue[];
		properties: NamedValue[];
	};

	const { inputs, outputs, properties } = data;
</script>

<div class="node">
	<div class="title">{data.name}</div>
	<div class="properties">
		{#each properties as property}
			<div class="property">
				{property.label}: {property.value}
			</div>
		{/each}
		{#each inputs as input}
			<div class="input">
				<p>Input: {input.label} ({input.value})</p>
				<Handle type="source" position={Position.Left} id={input.id} />
			</div>
		{/each}
		{#each outputs as output}
			<div class="output">
				<p>Output: {output.label} ({output.value})</p>
				<Handle type="target" position={Position.Right} id={output.id} />
			</div>
		{/each}
	</div>
</div>

<style lang="scss">
	@import '../../styles/theme.scss';

	.node {
		border: $border;
		border-radius: $border-radius;
		padding: $gap;
	}
</style>
