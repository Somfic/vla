<script lang="ts">
	import { startListening } from '$lib/ws';
	import { onMount } from 'svelte';

	onMount(() => {
		startListening();
	});
</script>
