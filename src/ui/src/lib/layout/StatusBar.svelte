<script lang="ts">
	import Warning from 'lucide-svelte/icons/triangle-alert';
	import Sync from 'lucide-svelte/icons/git-commit-vertical';
	import Notification from 'lucide-svelte/icons/bell';
	import { tooltip } from '$lib/actions/tooltip';
</script>

<div class="status-bar">
	<div class="ribbon">
		<p class="item" use:tooltip={{ text: 'Hello', position: 'right' }}><Sync />Synced</p>
		<p class="item">12<Warning /></p>
	</div>
	<div class="ribbon"><p class="item"><Notification /></p></div>
</div>

<style lang="scss">
	.status-bar {
		display: flex;
		align-items: center;
		justify-content: space-between;
		margin: 10px;
		font-size: 0.9rem;
	}

	.ribbon {
		display: flex;
		align-items: center;
		justify-content: center;
		background-color: rgba(30, 30, 30, 0.8);
		border: 1px solid rgba(255, 255, 255, 0.1);

		border-radius: 10px;
		color: #8e8e8d;

		.item {
			display: flex;
			align-items: center;
			justify-content: center;
			transition: 200ms ease;
			color: #aaa;
			gap: 5px;
			padding: 5px 15px;
			border-radius: 10px;
			cursor: pointer;
			border: 1px solid transparent;

			&:hover {
				color: #fff;
				background-color: rgba(255, 255, 255, 0.1);
				border: 1px solid rgba(255, 255, 255, 0.1);
			}

			:global(svg) {
				height: 1em;
				width: 1em;
			}
		}
	}
</style>
