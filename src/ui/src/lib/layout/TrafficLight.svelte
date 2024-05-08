<script lang="ts">
	import { createEventDispatcher, onMount } from 'svelte';
	import { appWindow } from '@tauri-apps/api/window';

	import Close from 'lucide-svelte/icons/x';
	import Hide from 'lucide-svelte/icons/minus';
	import Maximize from 'lucide-svelte/icons/square';
	import Minimize from 'lucide-svelte/icons/copy';
	import FullScreen from 'lucide-svelte/icons/chevrons-up-down';

	import UpdateAvailable from 'lucide-svelte/icons/arrow-down';
	import UpdateDownloading from '$lib/icons/Progress.svelte';
	import UpdateInstalling from 'lucide-svelte/icons/loader-circle';
	import UpdatePendingRestart from 'lucide-svelte/icons/rotate-cw';
	import { tooltip } from '$lib/actions/tooltip';

	appWindow.onFocusChanged(({ payload: focused }) => {
		active = focused;
	});

	let maximizedThrottle: NodeJS.Timeout;
	let lastSize = { width: 0, height: 0 };
	appWindow.onResized(async ({ payload }) => {
		if (payload.width === lastSize.width && payload.height === lastSize.height) return;
		lastSize = payload;

		console.log('resized');
		clearTimeout(maximizedThrottle);
		maximizedThrottle = setTimeout(async () => {
			console.log('calling isMaximized');
		}, 100);
	});

	const dispatch = createEventDispatcher();

	let active = true;
	let isMaximized = false;

	export let platform: string;
	export let updateStatus:
		| 'available'
		| 'downloading'
		| 'installing'
		| 'pending-restart'
		| 'unavailable' = 'unavailable';
	export let updateProgress: number = 0;
</script>

<div class={`traffic-light ${platform}`}>
	{#if platform === 'macos'}
		<button class="button close" class:active on:click={() => dispatch('close')}>
			<Close absoluteStrokeWidth strokeWidth={3} size={20} />
		</button>
		<button class="button minimize" class:active on:click={() => dispatch('hide')}>
			<Hide absoluteStrokeWidth strokeWidth={3} size={20} />
		</button>
		<button class="button fullscreen" class:active on:click={() => dispatch('fullscreen')}>
			<FullScreen absoluteStrokeWidth strokeWidth={3} size={20} />
		</button>
		{#if updateStatus != 'unavailable'}
			<button
				class={`button update ${updateStatus}`}
				class:active
				on:click={() => dispatch('update')}
			>
				{#if updateStatus === 'available'}
					<UpdateAvailable absoluteStrokeWidth strokeWidth={3} size={20} />
				{:else if updateStatus === 'downloading'}
					<UpdateDownloading progress={updateProgress} fill="#0000004d" />
				{:else if updateStatus === 'installing'}
					<UpdateInstalling absoluteStrokeWidth strokeWidth={3} size={20} />
				{:else if updateStatus === 'pending-restart'}
					<UpdatePendingRestart absoluteStrokeWidth strokeWidth={3} size={20} />
				{/if}
			</button>
		{/if}
	{:else}
		<button class="button minimize" class:active on:click={() => dispatch('hide')}>
			<Hide absoluteStrokeWidth strokeWidth={1} size={15} />
		</button>
		{#if !isMaximized}
			<button class="button maximize" class:active on:click={() => dispatch('maximize')}>
				<Maximize absoluteStrokeWidth strokeWidth={1} size={12} />
			</button>
		{:else}
			<button class="button minimize" class:active on:click={() => dispatch('minimize')}>
				<Minimize absoluteStrokeWidth strokeWidth={1} size={12} />
			</button>
		{/if}
		<button class="button close" class:active on:click={() => dispatch('close')}>
			<Close absoluteStrokeWidth strokeWidth={1} size={18} />
		</button>
	{/if}
</div>

<style lang="scss">
	$close-red: #ff6159;
	$close-red-active: #bf4942;
	$close-red-icon: #4d0000;
	$close-red-icon-active: #190000;

	$minimize-yellow: #ffbd2e;
	$minimize-yellow-active: #bf8e22;
	$minimize-yellow-icon: #995700;
	$minimize-yellow-icon-active: #592800;

	$maximize-green: #28c941;
	$maximize-green-active: #1d9730;
	$maximize-green-icon: #006500;
	$maximize-green-icon-active: #003200;

	$update-blue: #2e9fff;
	$update-blue-active: #1d6f9f;

	$active: #fff;
	$hover: rgba(255, 255, 255, 0.1);
	$hover-active: rgba(255, 255, 255, 0.05);
	$disabled: #aaa;

	.traffic-light {
		display: flex;
		flex-direction: row;
		align-items: center;
		justify-content: space-between;
		overflow: hidden;
		flex-grow: 1;

		button {
			all: unset;
			display: flex;
			flex-grow: 1;
			align-items: center;
			justify-content: center;
			transition: 200ms ease;

			:global(svg) {
				transition: 200ms ease;
			}
		}

		&.macos {
			padding: 0 8px;

			button {
				display: flex;
				align-items: center;
				justify-content: center;
				width: 12px;
				height: 12px;
				margin: 10px 4px;
				border-radius: 50%;
				flex-grow: 1;
				background-color: $disabled;

				:global(svg) {
					stroke: transparent;
					transform: scale(0.8);
				}

				:global(.progress) {
					opacity: 0;
				}

				&.close {
					&.active,
					&:hover {
						background-color: $close-red;
					}

					&:active {
						background-color: $close-red-active;
					}
				}

				&.minimize {
					&.active,
					&:hover {
						background-color: $minimize-yellow;
					}

					&:active {
						background-color: $minimize-yellow-active;
					}
				}

				&.fullscreen {
					transform: rotate(45deg);

					&.active,
					&:hover {
						background-color: $maximize-green;
					}

					&:active {
						background-color: $maximize-green-active;
					}
				}

				&.update {
					display: flex;
					align-items: center;
					justify-content: center;

					&.active,
					&:hover {
						background-color: $update-blue;
					}

					&:active {
						background-color: $update-blue-active;
					}

					&.installing {
						animation: spin 1s linear infinite;
					}
				}

				@keyframes spin {
					0% {
						transform: rotate(0deg);
					}
					100% {
						transform: rotate(360deg);
					}
				}
			}

			&:hover {
				.close {
					background-color: $close-red;
				}

				.minimize {
					background-color: $minimize-yellow;
				}

				.fullscreen {
					background-color: $maximize-green;
				}

				.update {
					background-color: $update-blue;
				}

				:global(svg) {
					stroke: #0000004d !important;
				}

				:global(.progress) {
					opacity: 1 !important;
				}
			}
		}

		&.windows button {
			padding: 3px 13.6px;
			aspect-ratio: 1;
			border-radius: 5px;

			:global(svg) {
				width: 20px;
				stroke: $disabled;
			}

			&.active :global(svg) {
				stroke: $active;
			}

			&:hover {
				background-color: $hover;

				:global(svg) {
					stroke: $active;
				}
			}

			&:active {
				background-color: $hover-active;
			}

			&.close {
				&:hover {
					background-color: $close-red;
				}

				&:active {
					background-color: $close-red-active;
				}
			}

			&.minimize {
				transform: scaleX(-1);
			}
		}
	}
</style>
