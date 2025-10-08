<script lang="ts">
    import ArgumentInput from "./arguments/ArgumentInput.svelte";
    import Handle from "./Handle.svelte";
    import api, { saveNodeChanges, type CanvasNodeProps } from "$lib/api";
    import Input from "$components/forms/Input.svelte";
    import type { NodeExecutionState } from "$lib/core";

    let node: CanvasNodeProps = $props();

    let executionState: NodeExecutionState | null = $state(null);

    api.node_execution_updated.on((state) => {
        if (state.node_id === node.id) {
            executionState = state.state;
        }
    });
</script>

{#if node.data.brick}
    <div class="node phase-{executionState?.phase.toLowerCase() ?? 'waiting'}">
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
                {#each node.data.brick.execution_inputs as input}
                    <div class="input">
                        <Handle
                            input={{
                                ...input,
                                type: "flow",
                                defaultValue: null,
                            }}
                            {node}
                            onchange={() => saveNodeChanges()}
                        />
                    </div>
                {/each}
                {#each node.data.brick.inputs as input, i}
                    <div class="input">
                        <Handle
                            bind:input={node.data.brick.inputs[i]}
                            {node}
                            onchange={() => saveNodeChanges()}
                        />
                        <div class="label">
                            {input.label}
                        </div>
                    </div>
                {/each}
            </div>

            <div class="outputs">
                {#each node.data.brick.execution_outputs as output}
                    <div class="output">
                        <div class="label">
                            {output.label}
                        </div>
                        <Handle
                            output={{ ...output, type: "flow" }}
                            {node}
                            onchange={() => saveNodeChanges()}
                        />
                    </div>
                {/each}
                {#each node.data.brick.outputs as output}
                    <div class="output">
                        {#if output.type !== "flow"}
                            <Input type={output.type} value={"0"} disabled />
                        {/if}
                        <div class="label">
                            {output.label}
                        </div>
                        <Handle {node} {output} />
                    </div>
                {/each}
            </div>
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
        outline: 2px solid transparent;

        &.phase-queued {
            outline-color: rgba($primary, 0.3);
            outline-style: dashed;
            outline-offset: 4px;
        }

        &.phase-running {
            outline-color: $primary;
        }

        &.phase-completed {
            outline-color: rgba($primary, 0.3);
        }
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
            flex-grow: 1;
            gap: $gap;
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
