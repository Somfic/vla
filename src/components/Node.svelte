<script lang="ts">
    import api from "$lib/api";
    import type { NodeData } from "$lib/core";
    import {
        Handle,
        Position,
        type Node,
        type NodeProps,
    } from "@xyflow/svelte";

    let { id, data }: NodeProps<Node<NodeData>> = $props();

    let brick = api.get_brick(data.brick_id);

    console.log(brick);
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
                        <input type="checkbox" id={argument.id} />
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
    .node {
        display: flex;
        flex-direction: column;
        border: 1px solid #222;
        background-color: #eee;
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
    }

    .outputs .output {
        justify-content: flex-end;
    }
</style>
