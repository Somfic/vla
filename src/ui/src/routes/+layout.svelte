<script lang="ts">
	import { invoke } from '@tauri-apps/api/tauri';
	import { onMount } from 'svelte';
	import '../styles/global.scss';
	import TitleBar from '$lib/layout/TitleBar.svelte';
	import StatusBar from '$lib/layout/StatusBar.svelte';

	onMount(() => {
		invoke('show_window');
	});
</script>

<main id="application">
	<TitleBar />
	<div id="content">
		<slot />
	</div>
	<div id="status">
		<StatusBar />
	</div>
</main>

<style lang="scss">
	#application {
		max-width: 100vw;
		display: flex;
		background-color: rgba(0, 0, 0, 0.2);
	}

	#content {
		display: flex;
		flex-direction: column;
		flex-grow: 1;
	}

	#status {
		display: flex;
		position: absolute;
		bottom: 0;
		left: 0;
		width: 100vw;
	}
</style>
