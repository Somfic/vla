<script lang="ts">
	import { createEventDispatcher, onDestroy, onMount } from 'svelte';
	import { saveWorkspace, type Web, type Workspace } from '$lib/nodes';
	import Editor from '../editor/Editor.svelte';
	import Topbar from '../topbar/Topbar.svelte';

	const dispatch = createEventDispatcher();
	export let workspace: Workspace;
	let activeWeb = workspace.webs[0];
</script>

<div class="workspace">
	<div class="topbar">
		<Topbar
			{workspace}
			bind:web={activeWeb}
			on:changeWorkspace={(e) => dispatch('changeWorkspace', e.detail)}
		/>
	</div>

	<div class="web-editor">
		<div class="editor">
			{#if activeWeb != undefined}
				<Editor bind:web={activeWeb} on:change={() => saveWorkspace(workspace)} />
			{:else}
				<div class="placeholder">
					<p>Select a web to edit</p>
				</div>
			{/if}
		</div>
	</div>
</div>

<style lang="scss">
	@import '../../../styles/theme.scss';

	.workspace {
		display: flex;
		flex-direction: column;
		flex-grow: 1;
	}

	.topbar {
		position: absolute;
		top: 0;
		left: 0;
		width: 100vw;
		z-index: 100;
	}

	.web-editor {
		display: flex;
		flex-grow: 1;
	}

	.webs {
		background-color: $background-ui;
		margin-top: 20px;
		padding-top: 3rem;
	}

	.editor {
		display: flex;
		flex-grow: 1;
	}
</style>
