<script lang="ts">
	import Editor from '$lib/components/editor/Editor.svelte';
	import Menu from '$lib/components/menu/Menu.svelte';
	import Explorer from '$lib/layout/Explorer.svelte';
	import Shortcuts from '$lib/layout/Shortcuts.svelte';
	import type { Workspace } from '$lib/models/workspace';
	import { saveWorkspace } from '$lib/nodes';
	import { webId, web, workspace } from '$lib/state.svelte';
	import { sendMessage, startListening } from '$lib/ws';
	import { onMount } from 'svelte';
	import { get } from 'svelte/store';

	let blurred = true;

	onMount(() => {
		startListening();
	});

	function handleWebChange(e: CustomEvent) {
		let w = get(workspace);
		if (w == undefined) return;
		saveWorkspace(w);
	}
</script>

<Menu />

<main class:blurred>
	<Shortcuts />
	<Explorer />
	<div class="editor">
		<div class="canvas panel">
			{#if $web == undefined}
				<p>No web selected</p>
			{:else}
				<Editor on:change={handleWebChange} web={$web} />
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
		padding: 0.5rem;
		height: 100vh;
		background-color: black;
		transition: background-color 100ms ease-in-out;

		&.blurred {
			background-color: rgba(0, 0, 0, 0);
		}
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
