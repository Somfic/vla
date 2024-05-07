<script lang="ts">
	import { invoke } from '@tauri-apps/api/tauri';
	import { onMount } from 'svelte';

	onMount(() => {
		invoke('show_window');
	});
</script>

</main>
