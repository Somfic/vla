<script lang="ts">
	export let text: string;
	export let position: 'left' | 'top' | 'right' | 'bottom';
	export let show: boolean = false;
	export let parentSize: { width: number; height: number } = { width: 0, height: 0 };
</script>

<p
	class={`tooltip ${position}`}
	class:show
	style:--parentWidth={parentSize.width}
	style:--parentHeight={parentSize.height}
>
	{text}
</p>

<style lang="scss">
	@import '../../styles/theme.scss';

	.tooltip {
		position: absolute;
		pointer-events: none;
		transition: $transition;
		opacity: 0;
		background-color: rgba(0, 0, 0, 0.2);
		border: 1px solid rgba(255, 255, 255, 0.1);
		padding: 5px 10px;
		border-radius: 10px;
		font-size: 0.9rem;
		display: flex;
		align-items: center;
		justify-content: center;
		text-align: center;
		max-width: 200px;
		text-wrap: nowrap;
		color: #eee;

		&.show {
			opacity: 1;
		}

		&.top {
			bottom: calc(100% + 10px);
			left: 50%;
			transform: translateX(-50%);
		}

		&.right {
			top: 50%;
			left: calc(100% + 10px);
			transform: translateY(-50%);
		}

		&.bottom {
			top: calc(100% + 10px);
			left: 50%;
			transform: translateX(-50%);
		}

		&.left {
			top: 50%;
			right: calc(100% + 10px);
			transform: translateY(-50%);
		}
	}
</style>
