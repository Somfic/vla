<script lang="ts">
	import { notifications } from '$lib/core/notifications';
	import Notification from '$lib/components/Notification.svelte';
</script>

<div class="notifications">
	{#each notifications as notification (notification.id)}
		<Notification {notification} />
	{/each}
</div>

<style lang="scss">
	.notifications {
		position: absolute;
		display: flex;
		bottom: 0;
		right: 0;
		width: 300px;
		padding-bottom: 2rem;
	}
</style>
