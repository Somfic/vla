<script lang="ts">
	import type { NodeData } from "$lib/graph.svelte";
	import type { BrickArgument } from "$lib/schema";
	import Input from "$components/forms/Input.svelte";

	interface Props {
		argument: BrickArgument;
		data: NodeData;
	}

	let { argument, data }: Props = $props();
</script>

<div class="argument">
	<Input
		id={argument.id}
		type={argument.type}
		label={argument.label}
		enumValues={argument.enumOptions ?? undefined}
		bind:value={data.arguments[argument.id]}
	/>
</div>

<style lang="scss">
	.argument {
		display: flex;
		gap: 0.25rem;
		align-items: center;
		justify-content: space-between;
	}
</style>
