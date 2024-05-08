<script lang="ts">
	import { SvelteFlow, Controls, Background, BackgroundVariant, MiniMap } from '@xyflow/svelte';

	import '@xyflow/svelte/dist/base.css';
	import { writable } from 'svelte/store';

	const nodes = writable([
		{
			id: '1',
			type: 'input',
			data: { label: 'Input Node' },
			position: { x: 0, y: 0 }
		},
		{
			id: '2',
			type: 'default',
			data: { label: 'Node' },
			position: { x: 0, y: 150 }
		}
	]);

	const connections = writable([
		{
			id: '1-2',
			type: 'default',
			source: '1',
			target: '2',
			label: 'Edge Text'
		}
	]);
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
