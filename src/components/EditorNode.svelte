<script lang="ts">
    import { Anchor, Node } from "svelvet";
    import { result as r, type NodeInstance, type Parameter } from "../lib/nodes";
    import EditorProperty from "./EditorProperty.svelte";
    import EditorAnchor from "./EditorAnchor.svelte";
    import { get } from "svelte/store";
    import { structures, connections } from "../lib/nodes";
    import ComputedValue from "./ComputedValue.svelte";

    export let instance: NodeInstance;
    $: structure = get(structures)?.find((s) => s.NodeType == instance.NodeType)!;
    $: result = get(r)?.Instances?.find((i) => i.Id == instance.Id);

    function getConnections(input: Parameter): [string, string][] {
        let array: [string, string][] = [];

        get(connections)
            .filter((c) => c.From.InstanceId == instance.Id && c.From.PropertyId == input.Id)
            .forEach((c) => array.push([`${c.To.InstanceId}`, `${c.To.PropertyId}`]));

        return array;
    }
</script>

<Node let:grabHandle let:selected id={instance.Id}>
    <div use:grabHandle class:selected class="node">
        <div class="title">{result?.Value?.Name ?? structure.NodeType.split(",")[0].split(".").slice(-1)[0].replace("Node", "")}</div>
        <div class="properties">
            {#each instance.Properties as property, i}
                <EditorProperty {property} {structure} bind:value={instance.Properties[i].Value} />
            {/each}
        </div>
        <div class="input-output">
            {#if structure.Inputs.length > 0}
                <div class="inputs">
                    {#each structure.Inputs as input}
                        <div class="input">
                            <div class="anchor">
                                <Anchor let:linked let:hovering let:connecting input id={input.Id} nodeConnect connections={getConnections(input)}>
                                    <EditorAnchor parameter={input} {linked} {hovering} {connecting} input />
                                </Anchor>
                            </div>
                            <div class="name">{input.Name}</div>
                            <!-- <ComputedValue id={`${instance.Id}.${input.Id}`} input /> -->
                        </div>
                    {/each}
                </div>
            {/if}
            {#if structure.Outputs.length > 0}
                <div class="outputs">
                    {#each structure.Outputs as output}
                        <div class="output">
                            <div class="value">
                                <ComputedValue id={`${instance.Id}.${output.Id}`} output />
                            </div>
                            <div class="name">{output.Name}</div>
                            <div class="anchor">
                                <Anchor let:linked let:hovering let:connecting output id={output.Id} multiple={false} connections={getConnections(output)}>
                                    <EditorAnchor parameter={output} {linked} {hovering} {connecting} output />
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
    @import "../theme.scss";

    .node {
        display: flex;
        flex-direction: column;
        background-color: #343434;
        box-shadow: 0px 0px 50px 0px rgba(0, 0, 0, 0.75);
        border-radius: 11px;
        transition: 200ms ease all;
        outline: 2px solid transparent;

        &.selected {
            outline: 2px solid transparentize($accent, 0.5);
        }
    }

    .title {
        background-color: #ad50a3;
        padding: 12px;
        border-radius: 10px 10px 0 0;
    }

    .input-output {
        display: flex;

        :first-child {
            border-bottom-left-radius: 10px;
        }

        :last-child {
            border-bottom-right-radius: 10px;
        }

        .inputs {
            flex-grow: 1;
            align-items: left;
            padding-right: 12px;
        }

        .outputs {
            flex-grow: 1;
            align-items: end;
            background-color: #1f1f1f;
            padding-left: 12px;
        }

        .input,
        .output {
            display: flex;
            flex-grow: 1;
            align-items: center;
            margin: 6px 0px;
            min-height: 2rem;
            width: 100%;

            .name {
                margin: 0 8px;
                font-weight: bold;
                text-align: left;
            }

            .value {
                flex-grow: 1;
            }
        }

        .input {
            .name {
                text-align: left;
                flex-grow: 1;
            }
        }
    }
</style>
