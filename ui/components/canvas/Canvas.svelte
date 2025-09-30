<script lang="ts">
    import type { Graph } from "$lib/core";
    import { Background, SvelteFlow } from "@xyflow/svelte";
    import "@xyflow/svelte/dist/base.css";
    import Node from "$components/canvas/Node.svelte";
    import Edge from "$components/canvas/Edge.svelte";
    import Shortcuts, {
        type ShortcutConfig,
    } from "$components/Shortcuts.svelte";
    import { shortcutContext } from "$actions/shortcutContext";

    let { graph = $bindable() }: { graph: Graph } = $props();

    function updateGraph() {
        // Create new graph object to trigger reactivity
        graph = { ...graph, nodes: [...graph.nodes], edges: [...graph.edges] };
    }

    let nodeTypes = { v1: Node };
    let edgeTypes = { default: Edge };

    let shortcuts: ShortcutConfig[] = [];
</script>

<Shortcuts {shortcuts} />

<div class="canvas" use:shortcutContext={"canvas"}>
    <SvelteFlow
        bind:nodes={graph.nodes}
        bind:edges={graph.edges}
        {edgeTypes}
        {nodeTypes}
        fitView
        snapGrid={[20, 20]}
        onnodedragstop={updateGraph}
        onconnectend={updateGraph}
        ondelete={updateGraph}
    >
        <Background />
    </SvelteFlow>
</div>

<style lang="scss">
    @import "$styles/theme";

    .canvas {
        flex-grow: 1;
        position: relative;
    }

    :global(.svelte-flow__attribution) {
        display: none;
    }

    :global(.svelte-flow__background) {
        background: $background-secondary;
    }
</style>
