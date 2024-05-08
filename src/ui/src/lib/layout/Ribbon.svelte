<script lang="ts">
	import Terminal from 'lucide-svelte/icons/terminal';
	import Extensions from 'lucide-svelte/icons/blocks';
	import { tooltip } from '$lib/actions/tooltip';
</script>

<div class="ribbon-wrapper">
	<div class="ribbon">
		<div class="item-wrapper" use:tooltip={{ text: 'Command pallet', position: 'right' }}>
			<div class="item">
				<Terminal />
			</div>
		</div>
		<div class="divider"></div>
		<div class="item-wrapper" use:tooltip={{ text: 'Extensions', position: 'right' }}>
			<div class="item"><Extensions /></div>
		</div>
	</div>
</div>

<style lang="scss">
	@import '../../styles/theme.scss';

	.ribbon-wrapper {
		display: flex;
		align-items: center;
		justify-content: center;
		margin: 10px;

		.ribbon {
			display: flex;
			flex-direction: column;
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
				padding: 12px;
				border-radius: 10px;
				cursor: pointer;
				transition: $transition;
				border: 1px solid transparent;

				:global(svg) {
					width: 20px;
					height: 20px;
					stroke: #aaa;
					transition: $transition;
				}

				&:hover {
					background-color: rgba(255, 255, 255, 0.1);
					border: 1px solid rgba(255, 255, 255, 0.1);
					transform: scale(1.2);

					:global(svg) {
						stroke: #ddd;
					}
				}

				&:active {
					transform: scale(1.1);
					background-color: rgba(255, 255, 255, 0.05);
				}
			}

			.divider {
				height: 1px;
				width: 40%;
				background-color: rgba(170, 170, 170, 0.2);
			}
		}
	}
</style>
