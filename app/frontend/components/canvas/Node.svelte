<script lang="ts">
	import ArgumentInput from "./arguments/ArgumentInput.svelte";
	import Handle from "./Handle.svelte";
	import type { CanvasNodeProps } from "$lib/graph.svelte";

	let node: CanvasNodeProps = $props();
</script>

{#if node.data.brick}
	<div class="node">
		<div class="header">
			{#each node.data.brick.execution_inputs as input}
				<div class="input">
					<Handle
						input={{
							...input,
							type: "flow",
							defaultValue: null,
						}}
						{node}
					/>
				</div>
			{/each}
			{node.data.brick.label}
		</div>

		{#if node.data.brick.arguments.length > 0}
			<div class="arguments">
				{#each node.data.brick.arguments as argument}
					<ArgumentInput {argument} data={node.data} />
				{/each}
			</div>
		{/if}

		<div class="handles">
			<div class="inputs">
				{#each node.data.brick.inputs as input, i}
					<div class="input">
						<Handle bind:input={node.data.brick.inputs[i]} {node} />
						<div class="label">
							{input.label}
						</div>
					</div>
				{/each}
			</div>

			<div class="outputs">
				{#each node.data.brick.execution_outputs as output}
					<div class="output">
						<div class="label">
							{output.label}
						</div>
						<Handle output={{ ...output, type: "flow" }} {node} />
					</div>
				{/each}
				{#each node.data.brick.outputs as output}
					<div class="output">
						<div class="label">
							{output.label}
						</div>
						<Handle {node} {output} />
					</div>
				{/each}
			</div>
		</div>
	</div>
{/if}

<style lang="scss">
	.node {
		display: flex;
		flex-direction: column;
		border: 1px solid var(--glow-border-color);
		border-radius: 12px;
		background-color: var(--glow-bg-surface);
		transition: border-color var(--glow-dur-fast) var(--glow-ease-out);
		position: relative;

		.header {
			padding: 0.5rem;
			border-bottom: 1px solid var(--glow-border-color);
			border-radius: 12px 12px 0 0;
			display: flex;
			align-items: center;
			background-color: var(--glow-bg-surface-element);
			font-size: 1rem;
			min-height: calc(1rem + 2 * 0.5rem);
		}
	}

	:global(.selected > .node) {
		border-color: var(--glow-primary);
	}

	.handles {
		display: flex;
		justify-content: space-between;
		gap: 0.25rem;
	}

	.inputs,
	.outputs {
		padding: 0.5rem;
		display: flex;
		flex-direction: column;
	}

	.inputs .input,
	.outputs .output {
		position: relative;
		display: flex;
		flex-grow: 1;
		gap: 0.25rem;
		align-items: center;
	}

	.outputs .output {
		justify-content: flex-end;
	}

	.label {
		color: var(--glow-text-secondary);
		font-size: 0.75rem;
	}

	.arguments {
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
		padding: 0.5rem;
	}
</style>
