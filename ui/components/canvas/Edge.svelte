<script lang="ts">
    import type { CanvasNode } from "$lib/api";
    import {
        BaseEdge,
        getBezierPath,
        getStraightPath,
        Position,
        useEdges,
        useNodes,
        type EdgeProps,
    } from "@xyflow/svelte";

    let { id, sourceX, sourceY, targetX, targetY, source, target }: EdgeProps =
        $props();

    let edges = useEdges();
    let edge = $derived(() => edges.current.find((e) => e.id === id));
    let nodes = useNodes();

    let sourceNode = $derived(
        () =>
            nodes.current.find((n) => n.id === source) as
                | CanvasNode
                | undefined,
    );

    let targetNode = $derived(
        () =>
            nodes.current.find((n) => n.id === target) as
                | CanvasNode
                | undefined,
    );

    let sourceHandle = $derived(() =>
        sourceNode()
            ? sourceNode()!.data.brick?.outputs.find(
                  (o) => o.id === edge()?.sourceHandle,
              )
            : null,
    );

    let targetHandle = $derived(() =>
        targetNode()
            ? targetNode()!.data.brick?.inputs.find(
                  (i) => i.id === edge()?.targetHandle,
              )
            : null,
    );

    const [path, labelX, labelY, offsetX, offsetY] = $derived(
        getBezierPath({
            sourceX: sourceX,
            sourceY: sourceY,
            sourcePosition: Position.Right,
            targetX: targetX,
            targetY: targetY,
            targetPosition: Position.Left,
        }),
    );
</script>

<g
    class={`source-type-${sourceHandle()?.type} target-type-${targetHandle()?.type}`}
>
    <BaseEdge {id} {path} />
</g>

<style lang="scss">
    @import "$styles/theme";

    .source-type-String :global(.svelte-flow__edge-path) {
        stroke: $string-color;
    }

    .source-type-Number :global(.svelte-flow__edge-path) {
        stroke: $number-color;
    }

    .source-type-Boolean :global(.svelte-flow__edge-path) {
        stroke: $boolean-color;
    }

    :global(.svelte-flow__edge-path) {
        stroke-width: 2px;
    }
</style>
