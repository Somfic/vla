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
	<StatusBar />
</main>

<style lang="scss">
	#application {
		max-width: 100vw;
		display: flex;
	}

	#content {
		display: flex;
		flex-direction: column;
		flex-grow: 1;
	}
</style>
