<script lang="ts">
	import WarningIcon from 'lucide-svelte/icons/triangle-alert';
	import SyncIcon from 'lucide-svelte/icons/git-commit-vertical';
	import NotificationIcon from 'lucide-svelte/icons/bell';
	import { tooltip } from '$lib/actions/tooltip';
	import { notifications } from '$lib/core/notifications';
	import Notifications from './Notifications.svelte';
</script>

<div class="status-bar">
	<div class="ribbon">
		<p class="item" use:tooltip={{ text: 'changed synced to the cloud', position: 'top' }}>
			<SyncIcon />Synced
		</p>
		<p class="item" use:tooltip={{ text: '12 warnings', position: 'top' }}>
			12<WarningIcon />
		</p>
	</div>
	<div class="ribbon">
		<p
			class="item"
			use:tooltip={{
				text: `${notifications.filter((x) => !x.hasRead).length} new notifications`,
				position: 'top'
			}}
		>
			<Notifications />
			<NotificationIcon />
		</p>
	</div>
</div>

<style lang="scss">
	@import '../../styles/theme.scss';

	.status-bar {
		display: flex;
		flex-grow: 1;
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
			position: relative;
			display: flex;
			align-items: center;
			justify-content: center;
			transition: $transition;
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
