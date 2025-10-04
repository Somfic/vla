<script lang="ts">
    import type { Graph } from "$lib/core";
    import {
        Background,
        SvelteFlow,
        Controls,
        MiniMap,
        SvelteFlowProvider,
    } from "@xyflow/svelte";
    import "@xyflow/svelte/dist/base.css";
    import Node from "$components/canvas/Node.svelte";
    import Edge from "$components/canvas/Edge.svelte";
    import { setSaveCallback } from "$lib/api";
    import Shortcuts, {
        type ShortcutConfig,
    } from "$components/Shortcuts.svelte";
    import { shortcutContext } from "$actions/shortcutContext";

    let {
        graph = $bindable(),
        onSave,
    }: { graph: Graph; onSave: (graph: Graph) => void } = $props();

    const save = () => onSave(graph);

    // Set up save callback through api
    setSaveCallback(save);

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
        onnodedragstop={save}
        onconnectend={save}
        ondelete={save}
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
        background: $bg-bottom;
    }
</style>
