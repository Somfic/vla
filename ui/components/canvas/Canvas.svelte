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
    import { setSaveCallback } from "$lib/api";
    import Spotlight from "$components/Spotlight.svelte";
    import Shortcuts, {
        type ShortcutConfig,
    } from "$components/Shortcuts.svelte";
    import { shortcutContext } from "$actions/shortcutContext";

    const save = () => onSave({ nodes, edges });

    let { graph, onSave }: { graph: Graph; onSave: (graph: Graph) => void } =
        $props();

    $effect(() => {
        nodes = graph.nodes;
        edges = graph.edges;
    });

    let nodes = $state(graph.nodes);
    let edges = $state(graph.edges);

    // Set up save callback through api
    setSaveCallback(save);

    let nodeTypes = { v1: Node };

    let showSpotlight = $state(false);

    let shortcuts: ShortcutConfig[] = [
        {
            key: "space",
            options: { context: "canvas" },
            handler: () => {
                showSpotlight = true;
            },
        },
    ];
</script>

<Shortcuts {shortcuts} />

<div class="canvas" use:shortcutContext={"canvas"}>
    <Spotlight onClose={() => (showSpotlight = false)} show={showSpotlight} />

    <SvelteFlow
        bind:nodes
        bind:edges
        {nodeTypes}
        fitView
        snapGrid={[20, 20]}
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
        background: $background-secondary;
    }
</style>
