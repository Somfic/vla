<script lang="ts">
	import Button from '$lib/components/Button.svelte';
	import type { Web } from '$lib/nodes';
	import { workspace, webId } from '$lib/state.svelte';

	function handleWebClicked(web: Web) {
		webId.set(web.id);
	}
</script>

<div class="explorer">
	{#each $workspace?.webs ?? [] as web}
		<button class="file" class:active={$webId == web.id} on:click={() => handleWebClicked(web)}
			>{web.name}</button
		>
	{/each}
</div>

<style lang="scss">
	@import '../../styles/global';

	.explorer {
		background-color: $background;
		border-radius: $border-radius;
		border: $border-width solid $border-color;
		overflow-y: auto;
		padding: $gap calc($gap / 2);
	}

	.file {
		all: unset;
		display: flex;
		justify-content: space-between;
		gap: $gap;
		align-items: center;
		padding: calc($gap / 2) $gap;
		color: $foreground-muted;
		cursor: pointer;
		transition: all ease 200ms;
		border-left: 5px solid transparent;

		&.error {
			color: rgb(255, 49, 49);
		}

		&.warning {
			color: rgb(255, 197, 49);
		}

		&:hover,
		&:active {
			color: $foreground;
		}

		&.active {
			background-color: $background;
			border-left-color: $accent;
		}
	}
</style>
