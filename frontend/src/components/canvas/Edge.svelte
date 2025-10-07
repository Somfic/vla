<script lang="ts">
    import type { CanvasNode } from "$lib/api";
    import {
        BaseEdge,
        getBezierPath,
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

    const typeColors: Record<string, string> = {
        string: "hsl(66, 100%, 67%)",
        number: "hsl(240, 100%, 68%)",
        boolean: "hsl(0, 100%, 71%)",
        flow: "hsl(0, 0%, 49%)",
    };

    let sourceColor = $derived(
        typeColors[sourceHandle()?.type ?? "flow"] ?? typeColors.flow,
    );
    let targetColor = $derived(
        typeColors[targetHandle()?.type ?? "flow"] ?? typeColors.flow,
    );

    let gradientId = $derived(`edge-gradient-${id}`);
    let blurId = $derived(`edge-blur-${id}`);
    let isFlowEdge = $derived(!sourceHandle());
</script>

<svg>
    <defs>
        <linearGradient id={gradientId} x1="0%" y1="0%" x2="100%" y2="0%">
            <stop offset="20%" stop-color={sourceColor} />
            <stop offset="80%" stop-color={targetColor} />
        </linearGradient>
        <filter id={blurId} x="-50%" y="-50%" width="200%" height="200%">
            <feGaussianBlur in="SourceGraphic" stdDeviation="6" />
        </filter>
    </defs>
</svg>

<g class={isFlowEdge ? "flow-edge" : "data-edge"}>
    <g class="glow-layer" filter="url(#{blurId})">
        <BaseEdge {id} {path} />
    </g>

    <BaseEdge {id} {path} style="stroke: url(#{gradientId})" />
</g>

<style lang="scss">
    :global(.svelte-flow__edge-path) {
        stroke-width: 2px;
    }

    .flow-edge :global(.svelte-flow__edge-path) {
        stroke-dasharray: 5, 5;
        animation: dash 2s linear infinite;

        @keyframes dash {
            to {
                stroke-dashoffset: -20;
            }
        }
    }

    .glow-layer :global(.svelte-flow__edge-path) {
        stroke: hsla(0, 0%, 100%, 1);
        stroke-width: 0.5px;
        stroke-dasharray: 60, 20;
        animation: glow 3s linear infinite;
        stroke-linecap: round;

        @keyframes glow {
            to {
                stroke-dashoffset: -80;
            }
        }
    }
</style>
