<script lang="ts">
	import { Svelvet } from 'svelvet';
	import EditorNode from './EditorNode.svelte';
	import { type NodeInstanceConnection, runWeb } from '$lib/nodes';
	import type { Web } from '$lib/nodes';
	import { invokeMenu } from '$lib/menu';
	import { createEventDispatcher } from 'svelte';

	let { web } = $props<{ web: Web }>();

	const dispatch = createEventDispatcher();

	function connection(e: any) {
		// FIXME: Make sure this is distinct
		// FIXME: Make sure we don't connect to ourselves
		// FIXME: Make sure we don't connect in a loop
		// FIXME: Make sure we only connect types that can be casted to each other
		if (
			web.connections.find((c) => JSON.stringify(c) == JSON.stringify(detailToInstance(e.detail)))
		)
			return;
		web.connections = [...web.connections, detailToInstance(e.detail)];
		dispatch('change');
	}

	function disconnection(e: any) {
		web.connections = web.connections.filter(
			(c) => JSON.stringify(c) != JSON.stringify(detailToInstance(e.detail))
		);
		dispatch('change');
	}

	function detailToInstance(detail: any): NodeInstanceConnection {
		return {
			from: {
				instanceId: detail.sourceNode.id.substring(2), // remove "n-"
				propertyId: detail.sourceAnchor.id.split('-')[1].split('/')[0] // go from "a-id/2" to "id"
			},
			to: {
				instanceId: detail.targetNode.id.substring(2), // remove "n-"
				propertyId: detail.targetAnchor.id.split('-')[1].split('/')[0] // go from "a-id/2" to "id"
			}
		};
	}

	function handleKeyPress(e: KeyboardEvent) {
		if (e.key == 'Enter') {
			runWeb(web);
			return;
		}

		if (e.key == ' ') {
			invokeMenu('pick-instance', (i) => {
				if (i == undefined) return;
				web.instances = [...web.instances, i];
				dispatch('change');
			});
			return;
		}
	}
</script>

<!-- svelte-ignore a11y-no-static-element-interactions -->
<div class="editor" on:keydown={handleKeyPress}>
	<Svelvet
		minimap
		theme="dark"
		on:connection={connection}
		on:disconnection={disconnection}
		edgeStyle="bezier"
		modifier="meta"
	>
		{#each web.instances as instance}
			<EditorNode bind:web bind:instance on:change={() => dispatch('change')} />
		{/each}
	</Svelvet>
</div>

<style lang="scss">
	.editor {
		flex-grow: 1;
		outline: none;
	}

	:root[svelvet-theme='dark'] {
		outline: none;
		height: 0px;

		--background-color: $background;
		--text-color: $foreground;

		--node-border-radius: 10px;
		--node-shadow: none;
	}
</style>
