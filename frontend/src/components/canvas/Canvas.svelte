<script lang="ts">
    import type { Graph } from "$lib/core";
    import {
        Background,
        SvelteFlow,
    } from "@xyflow/svelte";
    import "@xyflow/svelte/dist/base.css";
    import Node from "$components/canvas/Node.svelte";
    import Edge from "$components/canvas/Edge.svelte";
    import { setSaveCallback } from "$lib/api";
    import Shortcuts, {
        type ShortcutConfig,
    } from "$components/Shortcuts.svelte";
    import { shortcutContext } from "$actions/shortcutContext";
    import Dock from "./Dock.svelte";

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
        onconnect={save}
        ondelete={save}
    >
        <Background />
    </SvelteFlow>
    <div class="dock">
        <Dock {graph} />
    </div>
</div>

<style lang="scss">
    @import "../../styles/theme";

    .canvas {
        flex-grow: 1;
        position: relative;

        .dock {
            position: absolute;
            left: 50%;
            bottom: $gap;
            transform: translateX(-50%);
        }
    }

    :global(.svelte-flow__attribution) {
        display: none;
    }

    :global(.svelte-flow__background) {
        background: $background-300;
    }
</style>
