<script lang="ts">
    import type { Graph } from "$lib/core";
    import { Background, SvelteFlow, Controls, MiniMap } from "@xyflow/svelte";
    import "@xyflow/svelte/dist/base.css";
    import Node from "$components/Node.svelte";

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

<div style:width="100vw" style:height="100vh">
    <SvelteFlow
        bind:nodes
        bind:edges
        {nodeTypes}
        fitView
        onnodedragstop={save}
        onconnectend={save}
    >
        <Background bgColor="#111" />
        <Controls />
        <MiniMap />
    </SvelteFlow>
</div>
