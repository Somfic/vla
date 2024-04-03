<script>
	import Vla from '$lib/icons/Vla.svelte';
	import { invoke } from '@tauri-apps/api/tauri';

	import { onMount } from 'svelte';

	onMount(() => {
		invoke('show_splashscreen');
	});
</script>

<div class="splash">
	<div class="icon">
		<Vla />
	</div>
</div>

<style lang="scss">
	.splash {
		display: flex;
		flex-grow: 1;
		flex-direction: column;
		align-items: center;
		justify-content: center;
	}

	.icon {
		width: 200px;
	}
</style>
