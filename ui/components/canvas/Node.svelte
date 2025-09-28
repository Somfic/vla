<script lang="ts">
    import { Position, type NodeProps } from "@xyflow/svelte";
    import ArgumentInput from "./arguments/ArgumentInput.svelte";
    import Handle from "./Handle.svelte";
    import type { CanvasNodeProps } from "$lib/api";

    let node: CanvasNodeProps = $props();
</script>

{#if node.data.brick}
    <div class="node">
        <div class="header">
            {node.data.brick?.label}
        </div>

        <div class="arguments">
            {#each node.data.brick.arguments as argument}
                <ArgumentInput {argument} data={node.data} />
            {/each}
        </div>

        <div class="handles">
            <div class="inputs">
                {#each node.data.brick.inputs as input}
                    <div class="input">
                        <Handle type="input" {node} connection={input} />
                        <div class="label">
                            {input.label}
                        </div>
                    </div>
                {/each}
            </div>

            <div class="outputs">
                {#each node.data.brick.outputs as output}
                    <div class="output">
                        <div class="label">
                            {output.label}
                        </div>
                        <Handle type="output" {node} connection={output} />
                    </div>
                {/each}
            </div>
        </div>

        <div class="preview">
            <img src="https://via.placeholder.com/500" alt="Node Preview" />
        </div>
    </div>
{/if}

<style lang="scss">
    @import "$styles/theme";

    .node {
        display: flex;
        flex-direction: column;
        border: $border;
        border-radius: $border-radius;
        background-color: $background;
        padding: $gap;
        gap: $gap;
        transition: all 0.2s ease;
    }

    :global(.selected > .node) {
        border-color: $primary;
    }

    .handles {
        display: flex;
        justify-content: space-between;
        gap: $gap;
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
