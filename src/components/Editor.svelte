<script lang="ts">
    import { Svelvet } from "svelvet";
    import EditorNode from "./EditorNode.svelte";
    import { result, structures, type NodeInstance, type NodeInstanceConnection, connections, instances, runWeb } from "../lib/nodes";
    import { get } from "svelte/store";
    import ContextMenu from "./ContextMenu.svelte";
    import { addNode, type ContextResult } from "../lib/context";

    let contextMenu: ((query: string) => ContextResult[]) | undefined = undefined;

    connections.subscribe((c) => runWeb());
    instances.subscribe((i) => runWeb());

    function connection(e: any) {
        // FIXME: Make sure this is distinct
        if (get(connections).find((c) => JSON.stringify(c) == JSON.stringify(detailToInstance(e.detail)))) return;
        connections.update((c) => [...c, detailToInstance(e.detail)]);
    }

    function disconnection(e: any) {
        let connection = detailToInstance(e.detail);

        connections.update((c) =>
            c.filter((c) => {
                return JSON.stringify(c) != JSON.stringify(connection);
            })
        );
    }

    function detailToInstance(detail: any): NodeInstanceConnection {
        return {
            from: {
                instanceId: detail.sourceNode.id.substring(2), // remove "n-"
                propertyId: detail.sourceAnchor.id.split("-")[1].split("/")[0], // go from "a-id/2" to "id"
            },
            to: {
                instanceId: detail.targetNode.id.substring(2), // remove "n-"
                propertyId: detail.targetAnchor.id.split("-")[1].split("/")[0], // go from "a-id/2" to "id"
            },
        };
    }

    function handleKeyPress(e: KeyboardEvent) {
        console.log(e);

        if (e.key == "Enter") {
            runWeb();
            return;
        }

        if (e.key == " ") {
            contextMenu = addNode;
            return;
        }
    }
</script>

<ContextMenu bind:menu={contextMenu} />

<!-- svelte-ignore a11y-no-static-element-interactions -->
<div class="editor" on:keydown={handleKeyPress}>
    <Svelvet minimap theme="dark" on:connection={connection} on:disconnection={disconnection} edgeStyle="step">
        {#each $instances as instance, i}
            <EditorNode bind:instance={$instances[i]} />
        {/each}
    </Svelvet>
</div>

<style lang="scss">
    .editor {
        flex-grow: 1;
        outline: none;
    }

    :root[svelvet-theme="dark"] {
        outline: none;
        height: 0px;

        --background-color: $background;
        --text-color: $foreground;

        --node-border-radius: 10px;
        --node-shadow: none;
    }
</style>
