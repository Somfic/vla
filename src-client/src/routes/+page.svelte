<script lang="ts">
	import { startListening } from '$lib/ws';
	import { onMount } from 'svelte';
	import { web, webName, workspace } from '$lib/state.svelte';
	import { createWeb } from '$lib/methods';

	onMount(() => {
		startListening();
	});
</script>

<main>
	<div class="sidebar">
		<div class="active-workspace">
			{$workspace?.name}
		</div>
		<div class="webs">
			<div class="web create" on:click={() => createWeb($workspace, 'new')}>New web</div>
			{#if $workspace?.webs}
				{#each $workspace?.webs as web}
					<div class="web" on:click={(w) => webName.set(web.name)}>
						{web.name}
					</div>
				{/each}
			{/if}
		</div>
	</div>
	<div class="content">
		<div class="top-bar">
			<div class="top-bar-sections">
				<div class="left"></div>
				<div class="centre">
					<span class="workspace-name">{$workspace?.name}</span>
					<span class="divider">/</span>
					<span class="web-name">{$web?.name}</span>
				</div>
				<div class="right"></div>
			</div>
			<div class="windows-controls">
				<div class="miny">x</div>
				<div class="maxy">x</div>
				<div class="closy">x</div>
			</div>
		</div>
	</div>
</main>

<style lang="scss">
	main {
		display: flex;
		flex-direction: row;
		flex-grow: 1;
		gap: 20px;
	}

	.sidebar {
		display: flex;
		flex-direction: column;
		padding: 10px;
		gap: 20px;

		.active-workspace {
			border-radius: 10px;
			border: 2px solid rgba(255, 255, 255, 0.25);
			font-weight: 800;
			background-color: rgba(48, 116, 254, 0.25);
			padding: 10px;
			text-align: center;
		}

		.webs {
			display: flex;
			flex-direction: column;
			margin: 10px;
			gap: 10px;

			.web {
				border-radius: 10px;
				border: 2px solid rgba(255, 255, 255, 0.25);
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
</style>
