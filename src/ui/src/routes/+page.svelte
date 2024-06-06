<script lang="ts">
	import NodeCanvas from '$lib/components/NodeCanvas.svelte';
	import Ribbon from '$lib/layout/Ribbon.svelte';

	let height: number;
</script>

<div class="content" bind:clientHeight={height} style:height={`${height}px`}>
	<NodeCanvas />
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
