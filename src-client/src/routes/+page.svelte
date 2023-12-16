<script lang="ts">
	import { startListening } from '$lib/ws';
	import { onMount } from 'svelte';
	import Panel from '../components/layout/Panel.svelte';
	import Editor from '../components/editor/Editor.svelte';

	onMount(() => {
		startListening();
	});
</script>

<main>
	<div class="shortcuts panel">
		<p>Hello</p>
		<div>
			<p>Hey</p>
			<p>Hey 2</p>
		</div>
	</div>
	<div class="explorer panel">
		<p>Workspace items</p>
		<!-- Placeholder items-->
		{#each Array(200) as _}
			<p>Item</p>
		{/each}
	</div>
	<div class="editor">
		<div class="canvas panel"></div>
		<div class="output">
			<div class="console panel"></div>
			<div class="overview panel"></div>
		</div>
	</div>
</main>

<style lang="scss">
	@import '../styles/global';

	main {
		display: flex;
		flex-direction: row;
		flex-grow: 1;
		gap: $gap;
		padding: $gap;
		height: 100vh;
	}

	.panel {
		padding: $gap;
		background-color: $background;
		border-radius: $border-radius;
		border: $border-width solid $border-color;
		overflow-y: auto;
	}

	.editor {
		display: flex;
		flex-direction: column;
		flex-grow: 1;
		gap: $gap;
	}

	.canvas {
		flex-grow: 1;
	}

	.output {
		display: flex;
		flex-direction: row;
		gap: $gap;

		.console {
			flex-grow: 1;
		}
	}
</style>
