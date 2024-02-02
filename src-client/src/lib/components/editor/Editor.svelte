<script lang="ts">
	import { Svelvet } from 'svelvet';
	import EditorNode from './EditorNode.svelte';
	import { runWeb } from '$lib/nodes';
	import { invokeMenu, menu } from '$lib/menu';
	import { createEventDispatcher } from 'svelte';
	import type { Web } from '$lib/models/web';
	import type { NodeConnection } from '$lib/models/connection';

	export let web: Web;

	const dispatch = createEventDispatcher();

	function connection(e: any) {
		let sourceId = detailToInstance(e.detail).from.instanceId;
		let targetId = detailToInstance(e.detail).to.instanceId;

		if (
			web.connections.find(
				(c) =>
					c.from.instanceId == sourceId &&
					c.to.instanceId == targetId &&
					c.from.propertyId == detailToInstance(e.detail).from.propertyId &&
					c.to.propertyId == detailToInstance(e.detail).to.propertyId
			)
		) {
			return;
		}
		web.connections = [...web.connections, detailToInstance(e.detail)];
		dispatchChange();
	}

	function disconnection(e: any) {
		web.connections = web.connections.filter(
			(c) => JSON.stringify(c) != JSON.stringify(detailToInstance(e.detail))
		);
		dispatchChange();
	}

	function detailToInstance(detail: any): NodeConnection {
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
				dispatchChange();
			});
			return;
		}
	}

	function dispatchChange() {
		dispatch('change', { web });
	}
</script>

<!-- svelte-ignore a11y-no-static-element-interactions -->
<div class="editor" on:keydown={handleKeyPress}>
	<Svelvet
		theme="dark"
		on:connection={connection}
		on:disconnection={disconnection}
		edgeStyle="bezier"
	>
		{#each web.instances as instance}
			<EditorNode bind:web bind:instance on:change={dispatchChange} />
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

		--background-color: $background;
		--text-color: $foreground;

		--node-border-radius: 10px;
		--node-shadow: none;
	}
</style>
