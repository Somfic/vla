<script lang="ts">
    import type { Graph } from "$lib/core";
    import {
        Background,
        SvelteFlow,
        Controls,
        type OnConnectEnd,
    } from "@xyflow/svelte";
    import "@xyflow/svelte/dist/style.css";g

    const save = () => onSave({ nodes, edges });

    let { graph, onSave }: { graph: Graph; onSave: (graph: Graph) => void } =
        $props();

    $effect(() => {
        nodes = graph.nodes;
        edges = graph.edges;
    });

    let nodes = $state(graph.nodes);
    let edges = $state(graph.edges);

    function handleNodeDoubleClick(event: any) {
        const nodeId = event.detail.node.id;
        // Remove the node and any connected edges
        nodes = nodes.filter((node) => node.id !== nodeId);
        edges = edges.filter(
            (edge) => edge.source !== nodeId && edge.target !== nodeId,
        );
        // Auto-save after deletion
        onSave({ nodes, edges });
    }
</script>

<div style:width="100vw" style:height="100vh">
    <SvelteFlow
        bind:nodes
        bind:edges
        fitView
        onnodedragstop={save}
        onconnectend={save}
    >
        <Background />
        <Controls />
    </SvelteFlow>
</div>
