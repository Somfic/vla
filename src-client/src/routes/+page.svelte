<script lang="ts">
	import Editor from '$lib/components/editor/Editor.svelte';
	import Menu from '$lib/components/menu/Menu.svelte';
	import Explorer from '$lib/layout/Explorer.svelte';
	import Shortcuts from '$lib/layout/Shortcuts.svelte';
	import { webId, web } from '$lib/state.svelte';
	import { startListening } from '$lib/ws';
	import { onMount } from 'svelte';

	onMount(() => {
		startListening();
	});
</script>

<Menu />

<main>
	<Shortcuts />
	<Explorer />
	<div class="editor">
		<div class="canvas panel">
			{#if $web == undefined}
				<p>No web selected</p>
			{:else}
				<Editor web={$web} />
			{/if}
		</div>
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

	.shortcuts {
		padding: 0;
	}

	.editor {
		display: flex;
		flex-direction: column;
		flex-grow: 1;
		gap: $gap;
	}

	.canvas {
		flex-grow: 1;
		display: flex;
		background: $background;
		border: $border-width solid $border-color;
		border-radius: $border-radius;
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
