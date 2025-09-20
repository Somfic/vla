<script lang="ts">
    import api, { type VlaNode } from "$lib/api";
    import { Handle, Position, type NodeProps } from "@xyflow/svelte";
    import ArgumentInput from "./arguments/ArgumentInput.svelte";

    let { data }: NodeProps<VlaNode> = $props();

    let brick = api.get_brick(data.brick_id);
</script>

{#await brick}
    <div>Loading brick...</div>
{:then brick}
    <div class="node">
        <div class="header">
            {brick.label}
        </div>

        <div class="arguments">
            {#each brick.arguments as argument}
                <ArgumentInput {argument} {data} />
            {/each}
        </div>

        <div class="inputs">
            {#each brick.inputs as input}
                <div class="input">
                    <div class="handle">
                        <Handle
                            type="target"
                            id={input.id}
                            position={Position.Left}
                        />
                    </div>
                    <div class="label">
                        {input.label}
                    </div>
                </div>
            {/each}
        </div>

        <div class="outputs">
            {#each brick.outputs as output}
                <div class="output">
                    <div class="label">
                        {output.label}
                    </div>
                    <div class="handle">
                        <Handle
                            type="source"
                            id={output.id}
                            position={Position.Right}
                        />
                    </div>
                </div>
            {/each}
        </div>

        <div class="preview">
            <img src="https://via.placeholder.com/500" alt="Node Preview" />
        </div>
    </div>
{/await}

<style lang="scss">
    @import "$styles/theme";

    .node {
        display: flex;
        flex-direction: column;
        border: 2px solid $border-color;
        border-radius: $border-radius;
        background-color: $background;
        padding: $gap;
    }

    :global(.svelte-flow__node .selected) {
        border-color: $primary;
    }

    .content {
        flex: 1;
    }

    .inputs,
    .outputs {
        display: flex;
        flex-direction: column;

        .input,
        .output {
            position: relative;
            display: flex;
            align-items: center;
        }

        $handle-size: 10px;

        :global(.svelte-flow__handle) {
            background-color: $primary;
            height: $handle-size;
            width: $handle-size;
            border-radius: 50%;
            z-index: 20000;
        }

        :global(.input .svelte-flow__handle) {
            left: calc(-1 * ($gap + 1px));
        }

        :global(.output .svelte-flow__handle) {
            right: calc(-1 * ($gap + 1px));
        }
    }

    .outputs .output {
        justify-content: flex-end;
    }

    .arguments {
        display: flex;
        flex-direction: column;
        gap: $gap;
    }
</style>
