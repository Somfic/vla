<script lang="ts">
	import { invoke } from '@tauri-apps/api/tauri';
	import { onMount } from 'svelte';
	import TrafficLight from './TrafficLight.svelte';
	import { appWindow } from '@tauri-apps/api/window';

	let platform = 'macos';

	onMount(async () => {
		platform = await invoke('get_platform');
	});
</script>

<div class="topbar" data-tauri-drag-region>
	<div class="left">
		{#if platform === 'macos'}
			<TrafficLight
				{platform}
				on:hide={async () => await appWindow.minimize()}
				on:close={async () => await appWindow.close()}
				on:maximize={async () => await appWindow.maximize()}
				on:minimize={async () => await appWindow.unmaximize()}
			/>
		{/if}
	</div>
	<div class="center">center</div>
	<div class="right">
		{#if platform === 'windows'}
			<TrafficLight
				{platform}
				on:hide={async () => await appWindow.minimize()}
				on:close={async () => await appWindow.close()}
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

		.left,
		.center,
		.right {
			height: 100%;
		}
	}
</style>
