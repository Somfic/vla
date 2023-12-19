<script lang="ts">
	import { writable } from 'svelte/store';
	import { createEventDispatcher } from 'svelte';
	import { menu, menuResult, show as showMenu } from '$lib/menu';
	import PickInstanceMenu from './PickInstanceMenu.svelte';
	import PickWorkspaceMenu from './PickWorkspace.svelte';

	function picked(e: any) {
		if (e == undefined) {
			menuResult.set(undefined);
			showMenu.set(false);
			return;
		}

		menuResult.set(e.detail);
		showMenu.set(false);
	}

	$: show = $showMenu;
</script>

<div class="menu-wrapper" class:show on:click={() => picked(undefined)}>
	<div class="menu">
		{#if $menu == 'pick-instance'}
			<PickInstanceMenu on:picked={picked} />
		{:else if $menu == 'pick-workspace'}
			<PickWorkspaceMenu on:picked={picked} />
		{/if}
	</div>
</div>

<style lang="scss">
	@import '../../../styles/theme.scss';

	.menu-wrapper {
		position: absolute;
		top: 0;
		left: 0;
		width: 100%;
		height: 100%;
		z-index: 30000;
		background-color: $background-frosted;
		backdrop-filter: brightness(0.5) blur(10px);
		padding-top: 30vh;

		transition: 200ms ease all;

		opacity: 0;
		pointer-events: none;

		&.show {
			opacity: 1;
			pointer-events: all;

			.menu {
				transform: translateY(0px);
			}
		}

		.menu {
			transform: translateY(100px);
			margin: auto;
			min-width: 500px;
			width: 50vw;
			display: flex;
			height: auto;
			flex-shrink: 1;
			flex-direction: column;
			background-color: $background-frosted;
			backdrop-filter: $filter-frosted;
			border: 2px solid $border-color;
			border-radius: 10px;
			overflow: hidden;
			transition: 200ms ease all;
			box-shadow: 0px 0px 100px 0px rgba(0, 0, 0, 1);
		}
	}
</style>
