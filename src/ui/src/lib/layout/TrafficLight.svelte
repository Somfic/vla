<script lang="ts">
	import { createEventDispatcher, onMount } from 'svelte';
	import { appWindow } from '@tauri-apps/api/window';

	import Close from 'lucide-svelte/icons/x';
	import Hide from 'lucide-svelte/icons/minus';
	import Maximize from 'lucide-svelte/icons/square';
	import Minimize from 'lucide-svelte/icons/copy';

	appWindow.onFocusChanged(({ payload: focused }) => {
		active = focused;
	});

	let maximizedThrottle: number;
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

	let active = false;
	let isMaximized = false;
	export let platform: string;
</script>

<div class={`traffic-light ${platform}`}>
	{#if platform === 'macos'}
		<button class="button close" class:active on:click={() => dispatch('close')}></button>
		<button class="button minimize" class:active on:click={() => dispatch('minimize')}></button>
		<button class="button maximize" class:active on:click={() => dispatch('maximize')}></button>
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
				width: 20px;
				transition: 200ms ease;
			}
		}

		&.macos {
			margin-left: 5px;

			button {
				width: 12px;
				height: 12px;
				margin: 10px 4px;
				border-radius: 50%;
				flex-grow: 1;

				&.close {
					background-color: $close-red;

					&:active {
						background-color: $close-red-active;
					}
				}

				&.minimize {
					background-color: $minimize-yellow;

					&:active {
						background-color: $minimize-yellow-active;
					}
				}

				&.maximize {
					background-color: $maximize-green;

					&:active {
						background-color: $maximize-green-active;
					}
				}
			}
		}

		&.windows button {
			padding: 3px 13.6px;
			aspect-ratio: 1;
			border-radius: 5px;

			:global(svg) {
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
