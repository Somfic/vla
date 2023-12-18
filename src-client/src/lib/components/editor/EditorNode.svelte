<script lang="ts">
	import { Anchor, Node } from 'svelvet';
	import {
		type NodeInstance,
		type ParameterInstance,
		type ParameterStructure,
		type Web
	} from '$lib/nodes';
	import EditorProperty from './EditorProperty.svelte';
	import EditorAnchor from './EditorAnchor.svelte';
	import { get } from 'svelte/store';
	import ComputedValue from './ComputedValue.svelte';
	import EditorEdge from './EditorEdge.svelte';
	import EditorAnchorDefaultValue from './EditorAnchorDefaultValue.svelte';
	import { createEventDispatcher } from 'svelte';
	import { state } from '$lib/state.svelte';
	import NodeInput from './NodeInput.svelte';
	import NodeOutput from './NodeOutput.svelte';

	let { instance, web } = $props<{ instance: NodeInstance; web: Web }>();

	const dispatch = createEventDispatcher();

	// FIXME: Structure *could* be undefined, but it shouldn't be. We should probably handle this better
	let structure = $derived(
		state.workspace?.structures.find((s) => s.nodeType == instance.nodeType)
	)!;
	// FIXME: Should we store each web result seperately? Right now we just store the "active" one
	let result = $derived(web?.result.instances.find((i) => i.id == instance.id));

	function handleKeyPress(e: KeyboardEvent) {
		if (e.key == 'Delete') {
			console.log('delete', instance.id);
			web.instances = web.instances.filter((i) => i.id != instance.id);
			dispatch('change');
		}
	}

	function handleClick(e: MouseEvent) {
		web.instances = web.instances.filter((i) => i.id != instance.id);
		web.connections = web.connections.filter(
			(c) => c.from.instanceId != instance.id && c.to.instanceId != instance.id
		);
		e.preventDefault();
		dispatch('change');
	}
</script>

<Node let:grabHandle let:selected id={instance.id} on:nodeClicked edge={EditorEdge}>
	<!-- svelte-ignore a11y-no-static-element-interactions -->
	<div
		use:grabHandle
		class:selected
		class="node"
		on:keydown={handleKeyPress}
		on:contextmenu={handleClick}
	>
		<div class="title">
			{result?.value?.name ??
				structure?.nodeType.split(',')[0].split('.').slice(-1)[0].replace('Node', '')}
		</div>

		{#if structure?.properties.length ?? 0 > 0}
			<div class="properties">
				{#each structure?.properties! as property, i}
					<EditorProperty
						on:change={() => dispatch('change')}
						{property}
						bind:value={instance.properties[i].value}
					/>
				{/each}
			</div>
		{/if}
		<div class="input-output">
			{#if structure?.inputs?.length ?? 0 > 0}
				<div class="inputs">
					{#each instance.inputs! as input}
						<NodeInput {structure} bind:input />
					{/each}
				</div>
			{/if}
			{#if structure.outputs.length > 0}
				<div class="outputs">
					{#each structure.outputs as output}
						<NodeOutput {instance} {structure} bind:output />
					{/each}
				</div>
			{/if}
		</div>
	</div>
</Node>

<style lang="scss">
	@import '../../../styles/theme.scss';

	$border: 2px solid $border-color;

	.node {
		display: flex;
		flex-direction: column;
		background-color: $background-light;
		box-shadow: 0px 0px 50px 0px rgba(0, 0, 0, 0.75);
		border-radius: 11px;
		transition: 200ms ease all;
		outline: 2px solid transparent;
		border: $border;

		&.selected {
			outline: 10px solid transparentize($accent, 0.8);
		}
	}

	.title {
		background-color: $accent;
		padding: 12px;
		border-radius: 10px 10px 0 0;
		border-bottom: $border;
	}

	.properties {
		border-bottom: $border;
	}

	.input-output {
		display: flex;

		:first-child {
			border-bottom-left-radius: 10px;
		}

		:last-child {
			border-bottom-right-radius: 10px;
		}

		.inputs {
			flex-grow: 1;
			align-items: left;
			padding-right: 12px;
			border-right: $border;
		}

		.outputs {
			flex-grow: 1;
			align-items: end;
			background-color: $background-dark;
			padding-left: 12px;
		}

		.input,
		.output {
			display: flex;
			flex-grow: 1;
			align-items: center;
			justify-content: center;
			margin: 6px 0px;
			min-height: 2rem;
			width: 100%;

			.name {
				margin: 0 8px;
				font-weight: bold;
				text-align: left;
			}

			.value {
				flex-grow: 1;
			}
		}

		.input {
			position: relative;
			.name {
				text-align: left;
				flex-grow: 1;
			}
		}

		.anchor-wrapper {
			position: relative;
		}
	}
</style>
