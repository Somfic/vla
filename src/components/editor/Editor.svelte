<script lang="ts">
    import { Svelvet } from "svelvet";
    import EditorNode from "./EditorNode.svelte";
    import { result, structures, type NodeInstance, type NodeInstanceConnection, runWeb } from "../../lib/nodes";
    import { get } from "svelte/store";
    import ContextMenu from "../ContextMenu.svelte";
    import { addNode, type ContextResult } from "../../lib/context";
    import type { Web } from "../../lib/nodes";
    import Topbar from "../topbar/Topbar.svelte";

    let contextMenu: ((query: string) => ContextResult[]) | undefined = undefined;

    export let web: Web;

    function connection(e: any) {
        // FIXME: Make sure this is distinct
        if (web.connections.find((c) => JSON.stringify(c) == JSON.stringify(detailToInstance(e.detail)))) return;
        web.connections = [...web.connections, detailToInstance(e.detail)];
    }

    function disconnection(e: any) {
        web.connections = web.connections.filter((c) => JSON.stringify(c) != JSON.stringify(detailToInstance(e.detail)));
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
            runWeb(web);
            return;
        }

        if (e.key == " ") {
            contextMenu = addNode;
            return;
        }
    }
</script>

<ContextMenu bind:menu={contextMenu} />

<div class="topbar">
    <Topbar />
</div>

<!-- svelte-ignore a11y-no-static-element-interactions -->
<div class="editor" on:keydown={handleKeyPress}>
    <Svelvet minimap theme="dark" on:connection={connection} on:disconnection={disconnection} edgeStyle="bezier">
        {#each web.instances as instance}
            <EditorNode bind:web bind:instance />
        {/each}
    </Svelvet>
</div>

<style lang="scss">
    .editor {
        flex-grow: 1;
        outline: none;
    }

    .topbar {
        position: absolute;
        top: 0;
        left: 0;
        width: 100vw;
        z-index: 100;
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
