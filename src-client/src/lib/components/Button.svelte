<script lang="ts">
	export let active = false;
	export let padding = false;
</script>

<button class:active class:padding>
	<slot />
</button>

<style lang="scss">
	@import '../../styles/global';

	button {
		all: unset;
		border: $border-width solid transparent;
		border-radius: $border-radius;
		display: flex;
		align-items: center;
		justify-content: center;
		transition: all ease 200ms;
		cursor: pointer;

		&.active,
		&:hover,
		&:active {
			border: $border-width solid $border-color;
			background-color: $background;
		}

		&.padding {
			padding: $gap;
		}
	}
</style>
