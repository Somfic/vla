<script lang="ts">
	import type { NamedValue } from '$lib/models/workspace';
	import { Handle, Position, type NodeProps } from '@xyflow/svelte';
	import type { Writable } from 'svelte/store';
	import Value from './Value.svelte';

	type $$Props = NodeProps;

	export let data: {
		name: string;
		inputs: NamedValue[];
		outputs: NamedValue[];
		properties: NamedValue[];
	};
</script>

<div class="node">
	<div class="title">{data.name}</div>
	<div class="properties">
		{#each data.properties as property}
			<div class="property">
				<p class="label">{property.label}</p>
				<Value bind:value={property.value} type={property.type} />
			</div>
		{/each}
	</div>
	<div class="inputoutput">
		<div class="inputs">
			{#each data.inputs as input}
				<div class="input">
					<p class="label">{input.label}</p>
					<Value bind:value={input.value} type={input.type} />
					<Handle type="source" position={Position.Left} id={input.id} />
				</div>
			{/each}
		</div>
		<div class="outputs">
			{#each data.outputs as output}
				<div class="output">
					<p class="label">{output.label}</p>
					<Value bind:value={output.value} type={output.type} />
					<Handle type="target" position={Position.Right} id={output.id} />
				</div>
			{/each}
		</div>
	</div>
</div>

<style lang="scss">
	@import '../../styles/theme.scss';

	.node {
		border: $border;
		border-radius: $border-radius;
		background-color: $background;
	}

	.title {
		font-weight: 800;
		border-bottom: $border;
		padding: $gap / 2 $gap;
	}

	.properties {
		padding: $gap;
	}

	.inputoutput {
		display: flex;
		padding: $gap;
		gap: $gap;

		.inputs {
			display: flex;
			flex-direction: column;
		}

		.outputs {
			display: flex;
			flex-direction: column;
		}
	}

	.label {
		margin-right: $gap;
	}

	.property,
	.input,
	.output {
		display: flex;
		align-items: center;
		justify-content: space-between;
		margin: $gap / 5;
	}
</style>
