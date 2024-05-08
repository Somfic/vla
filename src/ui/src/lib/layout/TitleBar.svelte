<script lang="ts">
	import { invoke } from '@tauri-apps/api/tauri';
	import { onMount } from 'svelte';
	import TrafficLight from './TrafficLight.svelte';
	import { appWindow } from '@tauri-apps/api/window';

	let platform = 'macos';

	onMount(async () => {
		platform = await invoke('get_platform');
	});

	let rightWidth = 0;
	let leftWidth = 0;
</script>

<div class="topbar" data-tauri-drag-region>
	<div class="left" bind:clientWidth={leftWidth} style:margin-right={`-${leftWidth}px`}>
		{#if platform === 'macos'}
			<TrafficLight
				{platform}
				updateStatus={'available'}
				updateProgress={100}
				on:hide={async () => await appWindow.minimize()}
				on:close={async () => await appWindow.close()}
				on:maximize={async () => await appWindow.maximize()}
				on:minimize={async () => await appWindow.unmaximize()}
			/>
		{/if}
	</div>
	<div class="center">vla</div>
	<div class="right" bind:clientWidth={rightWidth} style:margin-left={`-${rightWidth}px`}>
		{#if platform === 'windows'}
			<TrafficLight
				{platform}
				updateStatus={'unavailable'}
				updateProgress={50}
				on:hide={async () => await appWindow.minimize()}
				on:close={async () => await appWindow.close()}
				on:fullscreen={async () => await appWindow.setFullscreen(!(await appWindow.isFullscreen()))}
				on:maximize={async () => await appWindow.maximize()}
				on:minimize={async () => await appWindow.unmaximize()}
			/>
		{/if}
	</div>
</div>

<style lang="scss">
	.topbar {
		display: flex;
		width: 100vw;
		flex-direction: row;
		align-items: center;
		justify-content: space-between;
		background-color: rgba(30, 30, 30, 0.5);
		border-bottom: 1px solid rgba(255, 255, 255, 0.1);
		z-index: 200;

		.left,
		.center,
		.right {
			height: 100%;
		}
	}
</style>
