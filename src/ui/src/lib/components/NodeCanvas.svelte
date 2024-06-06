<script lang="ts">
	import { SvelteFlow, Background, BackgroundVariant } from '@xyflow/svelte';
	import '@xyflow/svelte/dist/base.css';
	import { nodes, connections } from '$lib/core/canvas';
</script>

<SvelteFlow
	{nodes}
	edges={connections}
	snapGrid={[20, 20]}
	fitView
	on:nodeclick={(event) => console.log('on node click', event.detail.node)}
>
	<Background variant={BackgroundVariant.Dots} />
</SvelteFlow>

<style lang="scss">
	:global(.svelte-flow) {
		background-color: transparent !important;
	}

	:global(.svelte-flow__attribution) {
		display: none;
	}
</style>
