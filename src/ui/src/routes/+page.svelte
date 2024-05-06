<script lang="ts">
	import { invoke } from '@tauri-apps/api/tauri';
	import { onMount } from 'svelte';

	let date = new Date();
	let plugin: any = null;

	onMount(async () => {
		const plugin_sources = (await invoke('get_all_plugins')) as string[];
		const plugin_source = plugin_sources[0];

		plugin = await import(plugin_source);

		console.log(plugin);
	});

	setInterval(() => {
		date = new Date();
	}, 10);
</script>

<main>
	<p>plugins: {JSON.stringify(plugin)}</p>
	<p>Hello {date}</p>
</main>
