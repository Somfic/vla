<script lang="ts">
    import { Handle, Position, useEdges } from "@xyflow/svelte";
    import type { BrickInput, BrickOutput } from "../../lib/core";
    import type { CanvasNodeProps } from "$lib/api";
    import Input from "$components/forms/Input.svelte";

    let {
        input = $bindable(),
        output = $bindable(),
        node,
        onchange,
    }: {
        input?: BrickInput;
        output?: BrickOutput;
        node: CanvasNodeProps;
        onchange?: () => void;
    } = $props();

    const id = input ? input!.id : output!.id;
    const connectionType = input ? input!.type : output!.type;

    let edges = useEdges();

    let edge = $derived(() =>
        edges.current.find((edge) =>
            input
                ? edge.target === node.id && edge.targetHandle === id
                : edge.source === node.id && edge.sourceHandle === id,
        ),
    );
</script>

<div
    class={`handle type-${connectionType}`}
    class:input
    class:output
    class:connected={!!edge()}
>
    {#if input && !edge() && input.type !== "flow"}
        <div class="default">
            <Input
                type={input!.type}
                bind:value={node.data.defaults[input.id]}
                onchange={() => onchange?.()}
            />
        </div>
    {/if}

    <Handle
        {id}
        type={input ? "target" : "source"}
        position={input ? Position.Left : Position.Right}
    />
</div>

<style lang="scss">
    @import "$styles/theme";

    $handle-size: 10px;
    $handle-size-gap: 15px;

    .handle {
        position: relative;
        padding: $handle-size-gap 0;
    }

    :global(.svelte-flow__handle) {
        border: $border;
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
    .type-string :global(.svelte-flow__handle) {
        border-color: $string-color;
    }

    .type-number :global(.svelte-flow__handle) {
        border-color: $number-color;
    }

    .type-boolean :global(.svelte-flow__handle) {
        border-color: $boolean-color;
    }

    .type-flow :global(.svelte-flow__handle) {
        border-color: $flow-color;
        border-radius: 0;
    }

    .connected {
        &.type-string :global(.svelte-flow__handle) {
            background-color: $string-color;
        }

        &.type-number :global(.svelte-flow__handle) {
            background-color: $number-color;
        }

        &.type-boolean :global(.svelte-flow__handle) {
            background-color: $boolean-color;
        }

        &.type-flow :global(.svelte-flow__handle) {
            background-color: $flow-color;
        }
    }

    .default {
        position: absolute;
        right: $handle-size + $gap;
        top: calc(50% - $handle-size / 2);
        height: $handle-size;
        display: flex;
        align-items: center;
    }
</style>
