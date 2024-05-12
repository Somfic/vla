<script lang="ts">
	import ErrorIcon from 'lucide-svelte/icons/circle-x';
	import WarningIcon from 'lucide-svelte/icons/circle-alert';
	import InfoIcon from 'lucide-svelte/icons/circle-dot';
	import SuccessIcon from 'lucide-svelte/icons/circle-check';
	import CloseIcon from 'lucide-svelte/icons/x';
	import type { TrackedNotification } from '$lib/models/trackedNotification';

	export let notification: TrackedNotification;
</script>

<div class="notification">
	<div class="top">
		<div class="title">
			<div class="icon">
				{#if notification.level == 'error'}
					<ErrorIcon />
				{:else if notification.level == 'warning'}
					<WarningIcon />
				{:else if notification.level == 'information'}
					<InfoIcon />
				{:else if notification.level == 'success'}
					<SuccessIcon />
				{/if}
			</div>
			<div class="text">{notification.title}</div>
		</div>
		<button class="close" on:click={() => (notification.hadRead = true)}><CloseIcon /></button>
	</div>
</div>

<style lang="scss">
	.notification {
		display: flex;
		flex-grow: 1;
		background-color: rgba(30, 30, 30, 0.8);
		padding: 1rem 2rem;
		border-radius: 10px;
		border: 1px solid rgba(255, 255, 255, 0.1);
	}

	.top,
	.title {
		display: flex;
		flex-grow: 1;
		align-items: center;
	}

	.top {
		justify-content: space-between;
	}

	.close {
		display: flex;
		align-items: center;
		justify-content: center;
	}
</style>
