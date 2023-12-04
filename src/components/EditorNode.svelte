<script lang="ts">
    import { Anchor, Node } from "svelvet";
    import { result as r, type NodeInstance, type ParameterInstance, instances, typeToDefinition, type ParameterStructure } from "../lib/nodes";
    import EditorProperty from "./EditorProperty.svelte";
    import EditorAnchor from "./EditorAnchor.svelte";
    import { get } from "svelte/store";
    import { structures, connections } from "../lib/nodes";
    import ComputedValue from "./ComputedValue.svelte";
    import EditorEdge from "./EditorEdge.svelte";
    import EditorAnchorDefaultValue from "./EditorAnchorDefaultValue.svelte";

    export let instance: NodeInstance;
    $: structure = get(structures)?.find((s) => s.nodeType == instance.nodeType)!;
    $: result = get(r)?.instances?.find((i) => i.id == instance.id);

    function getConnections(input: ParameterInstance | ParameterStructure): [string, string][] {
        let array: [string, string][] = [];

        get(connections)
            .filter((c) => c.from.instanceId == instance.id && c.from.propertyId == input.id)
            .forEach((c) => array.push([`${c.to.instanceId}`, `${c.to.propertyId}`]));

        return array;
    }

    function handleKeyUp(e: KeyboardEvent) {
        if (e.key == "Delete") {
            console.log("delete", instance.id)
            instances.update((i) => i.filter((i) => i.id != instance.id));
        }
    }
</script>

<Node let:grabHandle let:selected id={instance.id} on:nodeClicked edge={EditorEdge}>
    <!-- svelte-ignore a11y-no-static-element-interactions -->
    <div use:grabHandle class:selected class="node" on:keyup={handleKeyUp}>
        <div class="title">{result?.value?.name ?? structure.nodeType.split(",")[0].split(".").slice(-1)[0].replace("Node", "")}</div>
        <div class="properties">
            {#each structure.properties as property, i}
                <EditorProperty {property} bind:value={instance.properties[i].value} />
            {/each}
        </div>
        <div class="input-output">
            {#if structure.inputs.length > 0}
                <div class="inputs">
                    {#each instance.inputs as input}
                        <div class="input">
                            <div class="anchor-wrapper">
                                <Anchor let:linked let:hovering let:connecting input id={input.id} nodeConnect connections={getConnections(input)}>
                                    {#if !linked}
                                        <div class="default">
                                            <EditorAnchorDefaultValue {structure} bind:parameter={input} {linked} {hovering} {connecting} />
                                        </div>
                                    {/if}
                                    <EditorAnchor {structure} parameter={input} {linked} {hovering} {connecting} input />
                                </Anchor>
                            </div>
                            <div class="name">{structure.inputs.concat(structure.outputs).find((i) => i.id == input.id)?.name}</div>
                            <!-- <ComputedValue id={`${instance.Id}.${input.Id}`} input /> -->
                        </div>
                    {/each}
                </div>
            {/if}
            {#if structure.outputs.length > 0}
                <div class="outputs">
                    {#each structure.outputs as output}
                        <div class="output">
                            <div class="value">
                                <ComputedValue type={typeToDefinition(output.type)} id={`${instance.id}.${output.id}`} output />
                            </div>
                            <div class="name">{output.name}</div>
                            <div class="anchor">
                                <Anchor let:linked let:hovering let:connecting output id={output.id} multiple={false} connections={getConnections(output)}>
                                    <EditorAnchor {structure} parameter={output} {linked} {hovering} {connecting} output />
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
            justify-content: center;
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
            position: relative;
            .name {
                text-align: left;
                flex-grow: 1;
            }
        }

        .anchor-wrapper {
            position: relative;
        }
    }

</style>
