<script lang="ts">
	import { get, writable } from 'svelte/store';
	import {
		SvelteFlow,
		Controls,
		Background,
		BackgroundVariant,
		MiniMap,
		type Node,
		type Edge
	} from '@xyflow/svelte';

	import '@xyflow/svelte/dist/style.css';
	import type { Web } from '$lib/models/workspace';
	import { createEventDispatcher } from 'svelte';
	import { connectionToEdge, edgeToConnection, instanceToNode, nodeToInstance } from '$lib/nodes';
	import NodeComponent from './Node.svelte';

	export let web: Web;

	let timeout: NodeJS.Timeout;
	let lastHash = '';

	const dispatch = createEventDispatcher();

	const nodes = writable<Node[]>();
	const edges = writable<Edge[]>();

	nodes.subscribe((_) => queueUpdateWeb());
	edges.subscribe((_) => queueUpdateWeb());

	$: setNodesAndEdgesFromWeb(web);

	function setNodesAndEdgesFromWeb(web: Web) {
		let newNodes = web.instances.map<Node>(instanceToNode);
		let newEdges = web.connections.map<Edge>(connectionToEdge);

		if (get(nodes) != newNodes) {
			nodes.set(newNodes);
		}

		if (get(edges) != newEdges) {
			edges.set(newEdges);
		}
	}

	function queueUpdateWeb() {
		clearTimeout(timeout);
		timeout = setTimeout(updateWeb, 1000);
	}

	function updateWeb() {
		let extractedNodes = get(nodes);
		let extractedEdges = get(edges);

		let updatedWeb = web;

		if (extractedNodes) {
			updatedWeb.instances = get(nodes).map(nodeToInstance);
		}

		if (extractedEdges) {
			updatedWeb.connections = get(edges).map(edgeToConnection);
		}

		// Hash the web to prevent infinite loops
		let hash = JSON.stringify(updatedWeb);
		if (hash === lastHash) {
			return;
		}
		lastHash = hash;

		dispatch('changed', updatedWeb);
	}

	const nodeTypes = {
		vla: NodeComponent
	};
</script>

<SvelteFlow
	{nodeTypes}
	{nodes}
	{edges}
	colorMode={'dark'}
	fitView
	on:nodeclick={(event) => console.log('on node click', event)}
>
	<Controls />
	<Background variant={BackgroundVariant.Dots} />
	<MiniMap />
</SvelteFlow>

<style lang="scss">
	@import '../../styles/theme';

	:global(.svelte-flow, .svelte-flow__pane) {
		background-color: transparent !important;
		border-radius: $border-radius;
		overflow: hidden;
	}

	:global(.svelte-flow__panel) {
		background-color: transparent !important;
		border-radius: $border-radius;
	}

	:global(.svelte-flow__controls-button) {
		border-radius: $border-radius;
		z-index: 200;

		:global(svg) {
			fill: white !important;
			stroke: white !important;
		}
	}

	:global(.svelte-flow__attribution) {
		display: none;
	}

	:global(.svelte-flow__edge) {
		stroke: rgba(255, 255, 255);
	}

	:global(.svelte-flow__handle-left, .svelte-flow__handle-right) {
		top: unset;
		transform: unset;
	}

	:global(.svelte-flow__handle-left) {
		left: -3px;
	}

	:global(.svelte-flow__handle-right) {
		right: -3px;
	}
</style>
