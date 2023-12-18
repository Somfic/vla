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
	import { workspace } from '$lib/state.svelte';

	export let instance: NodeInstance;
	export let web: Web;

	const dispatch = createEventDispatcher();

	$: structure = get(workspace)?.structures?.find((s) => s.nodeType == instance.nodeType)!;
	$: result = get(workspace)
		?.webs.find((w) => w.id == web.id)
		?.result.instances.find((i) => i.id == instance.id);

	function getConnections(input: ParameterInstance | ParameterStructure): [string, string][] {
		let array: [string, string][] = [];

		web.connections
			.filter((c) => c.from.instanceId == instance.id && c.from.propertyId == input.id)
			.forEach((c) => array.push([`${c.to.instanceId}`, `${c.to.propertyId}`]));

		return array;
	}

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

	function typeToDefinition(type: string) {
		return get(workspace)?.types.find((t) => t.name.replace('&', '') == type.replace('&', ''))!;
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
				structure.nodeType.split(',')[0].split('.').slice(-1)[0].replace('Node', '')}
		</div>

		{#if structure.properties.length > 0}
			<div class="properties">
				{#each structure.properties as property, i}
					<EditorProperty
						on:change={() => dispatch('change')}
						{property}
						bind:value={instance.properties[i].value}
					/>
				{/each}
			</div>
		{/if}
		<div class="input-output">
			{#if structure.inputs.length > 0}
				<div class="inputs">
					{#each instance.inputs as input}
						<div class="input">
							<div class="anchor-wrapper">
								<Anchor
									let:linked
									let:hovering
									let:connecting
									input
									id={input.id}
									nodeConnect
									connections={getConnections(input)}
								>
									<EditorAnchorDefaultValue
										on:change={() => dispatch('change')}
										{structure}
										bind:parameter={input}
										{linked}
										{connecting}
									/>
									<EditorAnchor
										{structure}
										parameter={input}
										{linked}
										{hovering}
										{connecting}
										input
									/>
								</Anchor>
							</div>
							<div class="name">
								{structure.inputs.concat(structure.outputs).find((i) => i.id == input.id)?.name}
							</div>
							<!-- <ComputedValue id={`${instance.Id}.${input.Id}`} input /> -->
						</div>
					{/each}
				</div>
			{/if}
			{#if structure.outputs.length > 0}
				<div class="outputs">
					{#each structure.outputs as output}
						<div class="output">
							<div class="value">
								<ComputedValue
									type={typeToDefinition(output.type)}
									id={`${instance.id}.${output.id}`}
									output
								/>
							</div>
							<div class="name">{output.name}</div>
							<div class="anchor">
								<Anchor
									let:linked
									let:hovering
									let:connecting
									output
									id={output.id}
									multiple={false}
									connections={getConnections(output)}
								>
									<EditorAnchor
										{structure}
										parameter={output}
										{linked}
										{hovering}
										{connecting}
										output
									/>
								</Anchor>
							</div>
						</div>
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
