<script lang="ts">
	import { invoke } from '@tauri-apps/api/tauri';
	import { onMount } from 'svelte';
	import '../styles/global.scss';
	import TitleBar from '$lib/layout/TitleBar.svelte';

	onMount(() => {
		invoke('show_window');
	});
</script>

<main id="application">
	<TitleBar />
</main>

<style lang="scss">
	#application {
		max-width: 100vw;
		//background-color: blue;
	}
</style>
