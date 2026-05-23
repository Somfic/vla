<script lang="ts">
	import { Background, SvelteFlow } from "@xyflow/svelte";
	import "@xyflow/svelte/dist/base.css";
	import Node from "$components/canvas/Node.svelte";
	import Edge from "$components/canvas/Edge.svelte";
	import { graph } from "$lib/graph.svelte";

	let nodeTypes = { v1: Node };
	let edgeTypes = { default: Edge };
</script>

<div class="canvas">
	<SvelteFlow
		bind:nodes={graph.nodes}
		bind:edges={graph.edges}
		{edgeTypes}
		{nodeTypes}
		fitView
	>
		<Background />
	</SvelteFlow>
</div>

<style lang="scss">
	.canvas {
		flex-grow: 1;
		position: relative;
		width: 100%;
		height: 100%;
	}

	:global(.svelte-flow__attribution) {
		display: none;
	}

	:global(.svelte-flow__background) {
		background: var(--glow-bg-base);
	}
</style>
