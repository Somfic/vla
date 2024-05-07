<script lang="ts">
	import { invoke } from '@tauri-apps/api/tauri';
	import { onMount } from 'svelte';
	import TrafficLight from './TrafficLight.svelte';

	let platform = 'windows';

	onMount(async () => {
		platform = await invoke('get_platform');
	});
</script>

<div class="topbar" data-tauri-drag-region>
	<div class="left">
		{#if platform === 'macos'}
			<TrafficLight />
		{/if}
	</div>
	<div class="center">center</div>
	<div class="right">right</div>
</div>

<style lang="scss">
	.topbar {
		display: flex;
		flex-direction: row;
		align-items: center;
		justify-content: space-between;
		height: 35px;
		border: 2px solid red;
	}
</style>
