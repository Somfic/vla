<script lang="ts">
    import api, { type VlaNode } from "$lib/api";
    import { Handle, Position, type NodeProps } from "@xyflow/svelte";

    let { data }: NodeProps<VlaNode> & { onArgumentChange?: () => void } =
        $props();

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
                <div class="argument">
                    <label for={argument.id}>{argument.label}</label>
                    {#if argument.type === "Boolean"}
                        <input type="checkbox" />
                    {/if}
                </div>
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

        .argument {
            display: flex;
            align-items: center;
            justify-content: space-between;

            input[type="checkbox"] {
                width: 16px;
                height: 16px;
                cursor: pointer;
                appearance: none;
                background-color: $background-secondary;
                border: 2px solid $border-color;
                border-radius: 3px;
                position: relative;

                &:checked {
                    background-color: $primary;
                    border-color: $primary;

                    &::after {
                        content: "✓";
                        position: absolute;
                        top: 50%;
                        left: 50%;
                        transform: translate(-50%, -50%);
                        color: $foreground;
                        font-size: 12px;
                        font-weight: bold;
                    }
                }

                &:hover {
                    border-color: $primary;
                }
            }
        }
    }
</style>
