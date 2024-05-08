<script lang="ts">
	export let progress: number;
	export let fill: string = 'red';

	$: angle = (360 * progress) / 100;

	const getGradient = (angle: number, fill: string) => {
		return `conic-gradient(${fill} ${angle}deg, transparent ${angle}deg)`;
	};
</script>

<div class="progress" style:--background={getGradient(angle, fill)}></div>

<style>
	.progress {
		display: flex;
		border-radius: 200px;
		overflow: hidden;

		height: 60%;
		aspect-ratio: 1;

		overflow: hidden;
		background: var(--background);

		transition: 200ms ease;
	}
</style>
