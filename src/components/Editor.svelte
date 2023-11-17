<script lang="ts">
    import { Svelvet } from "svelvet";
    import EditorNode from "./EditorNode.svelte";
    import { structures, type NodeInstance, type NodeInstanceConnection, connections, instances } from "../lib/nodes";

    function connection(e: any) {
        connections.update((c) => [...c, detailToInstance(e.detail)]);
    }

    function disconnection(e: any) {
        let connection = detailToInstance(e.detail);

        connections.update((c) =>
            c.filter((c) => {
                return !(c.From.InstanceId == connection.From.InstanceId && c.From.PropertyId == connection.From.PropertyId && c.To.InstanceId == connection.To.InstanceId && c.To.PropertyId == connection.To.PropertyId);
            })
        );
    }

    function detailToInstance(detail: any): NodeInstanceConnection {
        return {
            From: {
                InstanceId: detail.sourceNode.id,
                PropertyId: detail.sourceAnchor.id.split("-")[1].split("/")[0],
            },
            To: {
                InstanceId: detail.targetNode.id,
                PropertyId: detail.targetAnchor.id.split("-")[1].split("/")[0],
            },
        };
    }
</script>

<div class="editor">
    <Svelvet theme="dark" on:connection={connection} on:disconnection={disconnection} controls>
        {#each $instances as instance}
            <EditorNode {instance} />
        {/each}
    </Svelvet>
    <p>{JSON.stringify($connections)}</p>
</div>

<style lang="scss">
    .editor {
        height: 400px;
    }
</style>
