<script>
	// @ts-nocheck

	import Button from '$lib/components/Button.svelte';
	import Workspaces from '$lib/icons/Workspaces.svelte';
</script>

<div class="shortcuts">
	<Button padding>
		<Workspaces />
	</Button>
</div>

<style lang="scss">
	@import '../../styles/global';

	.shortcuts {
		font-size: 1.5rem;
		padding: $gap;
		background-color: $background;
		border-radius: $border-radius;
		border: $border-width solid $border-color;
	}
</style>
