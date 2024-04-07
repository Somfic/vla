<script lang="ts">
	export let value: any;
	export let type: string;
</script>

<div class="value">
	{value} ({type})
</div>

<style lang="scss">
	@import '../../styles/theme.scss';

	.value {
		border: $border;
		border-radius: $border-radius;
		background-color: $background-light;
		padding: calc($gap / 4) calc($gap / 2);
		min-width: 50px;
		text-align: right;
		font-family: $font-monospace;
		font-size: 0.8em;
		color: rgba(255, 255, 255, 0.5);
	}
</style>
