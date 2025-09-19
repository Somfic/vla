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

    const save = () => onSave({ nodes, edges });

    let { graph, onSave }: { graph: Graph; onSave: (graph: Graph) => void } =
        $props();

    $effect(() => {
        nodes = graph.nodes;
        edges = graph.edges;
    });

    let nodes = $state(graph.nodes);
    let edges = $state(graph.edges);
    let nodeTypes = { vla: Node };
</script>

<div class="canvas">
    <SvelteFlow
        bind:nodes
        bind:edges
        {nodeTypes}
        fitView
        onnodedragstop={save}
        onconnectend={save}
    >
        <Background />
    </SvelteFlow>
</div>

<style lang="scss">
    .canvas {
        flex-grow: 1;
    }

    :global(.svelte-flow__attribution) {
        display: none;
    }

    :global(.svelte-flow__background, .svelte-flow__container) {
        background-color: transparent;
    }
</style>
