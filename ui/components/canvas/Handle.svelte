<script lang="ts">
    import { Handle, Position, useEdges, type NodeProps } from "@xyflow/svelte";
    import type { BrickInput, BrickOutput } from "../../lib/core";
    import type { CanvasNodeProps } from "$lib/api";

    let {
        type,
        connection,
        node,
    }: {
        type: "input" | "output";
        connection: BrickInput | BrickOutput;
        node: CanvasNodeProps;
    } = $props();

    let edges = useEdges();

    let edge = $derived(() =>
        edges.current.find((edge) =>
            type === "input"
                ? edge.target === node.id && edge.targetHandle === connection.id
                : edge.source === node.id &&
                  edge.sourceHandle === connection.id,
        ),
    );
</script>

<div
    class={`handle ${type} type-${connection.type}`}
    class:connected={!!edge()}
>
    <Handle
        type={type === "input" ? "target" : "source"}
        id={connection.id}
        position={type === "input" ? Position.Left : Position.Right}
    />
</div>

<style lang="scss">
    @import "$styles/theme";

    $handle-size: 10px;

    :global(.svelte-flow__handle) {
        border: 1px solid $border-color;
        overflow: hidden;
        height: $handle-size;
        width: $handle-size;
        border-radius: 50%;
        z-index: 20000;
        border-width: 2px;
        background-color: $background;
        transition:
            background-color 0.3s,
            border-color 0.3s;
    }

    :global(.input .svelte-flow__handle) {
        left: calc(-1 * ($gap + 1px));
    }

    :global(.output .svelte-flow__handle) {
        right: calc(-1 * ($gap + 1px));
    }

    // types
    .type-String :global(.svelte-flow__handle) {
        border-color: $string-color;
    }

    .type-Number :global(.svelte-flow__handle) {
        border-color: $number-color;
    }

    .type-Boolean :global(.svelte-flow__handle) {
        border-color: $boolean-color;
    }

    .connected {
        &.type-String :global(.svelte-flow__handle) {
            background-color: $string-color;
        }

        &.type-Number :global(.svelte-flow__handle) {
            background-color: $number-color;
        }

        &.type-Boolean :global(.svelte-flow__handle) {
            background-color: $boolean-color;
        }
    }
</style>
