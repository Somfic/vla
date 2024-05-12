<script lang="ts">
	import NodeCanvas from '$lib/components/NodeCanvas.svelte';
	import Ribbon from '$lib/layout/Ribbon.svelte';
	import { invoke } from '@tauri-apps/api/tauri';

	let height: number;

	async function onNodesChanged(e) {
		await invoke('on_nodes_changed', e);
	}

	async function onConnectionsChanged(e) {
		await invoke('on_connections_changed', e);
	}
</script>

<div class="content" bind:clientHeight={height} style:height={`${height}px`}>
	<NodeCanvas on:nodeschanged={onNodesChanged} on:connectionschanged={onConnectionsChanged} />
</div>

<div id="ribbon">
	<Ribbon />
</div>

<style lang="scss">
	.content {
		flex-grow: 1;
	}

	#ribbon {
		position: absolute;
		bottom: 0;
		left: 0;
		height: 100vh;
		display: flex;
		align-items: center;
		justify-content: center;
	}
</style>
