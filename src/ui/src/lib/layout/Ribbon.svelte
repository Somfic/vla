<script lang="ts">
	import Terminal from 'lucide-svelte/icons/terminal';
	import Extensions from 'lucide-svelte/icons/blocks';
</script>

<div class="ribbon-wrapper">
	<div class="ribbon">
		<div class="item"><Terminal /></div>
		<div class="divider"></div>
		<div class="item"><Extensions /></div>
	</div>
</div>

<style lang="scss">
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
				transition: 200ms ease;
				border: 1px solid transparent;

				:global(svg) {
					width: 20px;
					height: 20px;
					stroke: #aaa;
					transition: 200ms ease;
				}

				&:hover {
					background-color: rgba(255, 255, 255, 0.1);
					border: 1px solid rgba(255, 255, 255, 0.1);

					:global(svg) {
						stroke: #fff;
					}
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
