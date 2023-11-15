<script lang="ts">
    import { Anchor, Node, type CSSColorString } from "svelvet";
    import { types, type NodeStructure } from "../lib/nodes";
    import EditorProperty from "./EditorProperty.svelte";
    import { get, type Readable } from "svelte/store";
    import NodeAnchor from "./EditorAnchor.svelte";
    import EditorAnchor from "./EditorAnchor.svelte";

    export let structure: NodeStructure;

    function getColorFromType(type: string): CSSColorString | null {
        let rgb = get(types).find((t) => t.Name === type)?.Color;

        if (rgb) {
            return `rgba(${rgb})` as CSSColorString;
        }

        return null;
    }
</script>

<Node let:grabHandle let:selected>
    <div use:grabHandle class:selected class="node">
        <div class="title">{structure.Type.split(",")[0].split(".").slice(-1)}</div>
        <div class="properties">
            {#each structure.Properties as property}
                <EditorProperty {property} />
            {/each}
        </div>
        <div class="input-output">
            {#if structure.Inputs.length > 0}
                <div class="inputs">
                    {#each structure.Inputs as input}
                        <div class="input">
                            <div class="anchor">
                                <Anchor let:linked let:hovering let:connecting input id={input.Id} nodeConnect>
                                    <EditorAnchor parameter={input} {linked} {hovering} {connecting} />
                                </Anchor>
                            </div>
                            <div class="name">{input.Name}</div>
                        </div>
                    {/each}
                </div>
            {/if}
            {#if structure.Outputs.length > 0}
                <div class="outputs">
                    {#each structure.Outputs as output}
                        <div class="output">
                            <div class="name">{output.Name}</div>
                            <div class="anchor">
                                <Anchor let:linked let:hovering let:connecting output id={output.Id} nodeConnect>
                                    <EditorAnchor parameter={output} {linked} {hovering} {connecting} />
                                </Anchor>
                            </div>
                        </div>
                    {/each}
                </div>
            {/if}
        </div>
    </div>
</Node>

<style lang="scss">
    .node {
        display: flex;
        flex-direction: column;
        background-color: #343434;
    }

    .title {
        background-color: #ad50a3;
        padding: 12px;
    }

    .input-output {
        display: flex;

        .inputs,
        .outputs {
            display: flex;
            flex-direction: column;
            flex-grow: 1;
        }

        .inputs {
            align-items: left;
        }

        .outputs {
            align-items: end;
            background-color: #1f1f1f;
        }

        .input,
        .output {
            display: flex;
            align-items: center;

            .name {
                padding: 12px;
                font-weight: bold;
            }
        }
    }
</style>
