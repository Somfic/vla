<script lang="ts">
	import { SvelteFlowProvider } from "@xyflow/svelte";
	import { CommandPalette, commands } from "glow";
	import { onMount } from "svelte";
	import Canvas from "$components/canvas/Canvas.svelte";
	import { graph } from "$lib/graph.svelte";
	import { api } from "$lib/schema";

	// Register every brick from the catalog as a glow command. Selecting one
	// drops a node onto the in-memory canvas.
	onMount(() => {
		let unregister: (() => void) | undefined;
		api.bricks.getBricks().then((bricks) => {
			unregister = commands.registerMany(
				bricks.map((brick) => ({
					id: brick.id,
					label: brick.label,
					description: brick.description,
					group: brick.category || "Uncategorized",
					keywords: [brick.category, ...brick.keywords],
					perform: () => {
						// Cascade so stacked inserts don't fully overlap.
						const n = graph.nodes.length;
						graph.addNode(brick, { x: 60 * n, y: 40 * n });
					},
				})),
			);
		});
		return () => unregister?.();
	});
</script>

<CommandPalette hotkey=" " placeholder="Search bricks..." />

<div class="editor">
	<SvelteFlowProvider>
		<Canvas />
	</SvelteFlowProvider>
</div>

<style lang="scss">
	.editor {
		display: flex;
		flex-grow: 1;
		width: 100%;
		height: 100vh;
		overflow: hidden;
	}
</style>
