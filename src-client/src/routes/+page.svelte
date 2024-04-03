<script lang="ts">
	import { sendMessage, startListening } from '$lib/ws';
	import { onMount } from 'svelte';
	import { web, webName, workspace } from '$lib/state.svelte';
	import { createWeb, updateWeb } from '$lib/methods';
	import NodeEditor from '$lib/components/NodeEditor.svelte';
	import type { Web } from '$lib/models/workspace';
	import { get } from 'svelte/store';

	onMount(() => {
		startListening();
	});

	function changed(updatedWeb: Web) {
		updateWeb(get(workspace)!, updatedWeb);
	}
</script>

<main>
	<div class="sidebar">
		<div class="active-workspace">
			{$workspace?.name ?? 'No workspace selected'}
		</div>
		<div class="webs">
			{#if $workspace}
				<button class="web create" on:click={() => createWeb($workspace, 'new')}>New web</button>
				{#if $workspace?.webs}
					{#each $workspace?.webs as web}
						<button class="web" on:click={(w) => webName.set(web.name)}>
							{web.name}
						</button>
					{/each}
				{/if}
			{:else}
				<p class="not-selected">No workspace selected</p>
			{/if}
		</div>
	</div>
	<div class="content">
		<div class="top-bar">
			<div class="top-bar-sections">
				<div class="left"></div>
				<div class="centre">
					{#if $workspace?.name && $web?.name}
						<span class="workspace-name">{$workspace?.name}</span>
						<span class="divider">/</span>
						<span class="web-name">{$web?.name}</span>
					{:else}
						<p class="not-selected">No workspace or web selected</p>
					{/if}
				</div>
				<div class="right"></div>
			</div>
			<div class="windows-controls">
				<div class="miny">x</div>
				<div class="maxy">x</div>
				<div class="closy">x</div>
			</div>
		</div>
		<div class="canvas">
			{#if $web}
				<NodeEditor web={$web} on:changed={(w) => changed(w.detail)} />
			{:else}
				<p class="not-selected">No web selected</p>
			{/if}
		</div>
	</div>
</main>

<style lang="scss">
	@import '../styles/theme';

	main {
		display: flex;
		flex-direction: row;
		flex-grow: 1;
	}

	.sidebar {
		display: flex;
		flex-direction: column;
		padding: 10px;
		gap: 20px;

		.active-workspace {
			border: $border;
			border-radius: $border-radius;
			font-weight: 800;
			background-color: rgba(48, 116, 254, 0.25);
			padding: 10px;
			text-align: center;
		}

		.webs {
			display: flex;
			flex-grow: 1;
			flex-direction: column;
			margin: 10px;
			gap: 10px;

			.web {
				border: $border;
				border-radius: $border-radius;
				background-color: rgba(255, 255, 255, 0.1);
				padding: 10px 20px;
				text-align: left;
				cursor: pointer;
				transition: background-color 0.2s;

				&:hover {
					background-color: rgba(255, 255, 255, 0.2);
				}

				&.create {
					border-style: dashed;
					color: rgba(255, 255, 255, 0.5);
				}
			}
		}
	}

	.content {
		display: flex;
		flex-direction: column;
		flex-grow: 1;

		.top-bar {
			display: flex;
			flex-direction: row;

			.web-name {
				font-weight: 800;
			}

			.divider {
				margin: 0 10px;
				color: rgba(255, 255, 255, 0.5);
			}

			.top-bar-sections {
				display: flex;
				flex-direction: row;
				flex-grow: 1;
				gap: 20px;
				padding: 10px;
				align-items: center;
				justify-content: space-between;
			}

			.windows-controls {
				display: flex;
				flex-direction: row;
				align-items: center;
				justify-content: space-evenly;
				gap: 10px;
			}
		}

		.canvas {
			display: flex;
			flex-grow: 1;
			background-color: rgba(0, 0, 0, 0.9);
			border-radius: $border-radius;
			margin: 10px;
		}
	}

	.not-selected {
		display: flex;
		flex-grow: 1;
		align-items: center;
		justify-content: center;
		color: rgba(255, 255, 255, 0.5);
		text-align: center;
	}
</style>
