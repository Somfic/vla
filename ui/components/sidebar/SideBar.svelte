<script lang="ts">
    import Minimap from "$components/canvas/Minimap.svelte";
    import { useNodes } from "@xyflow/svelte";
    import type { CanvasNode } from "$lib/api";

    const nodesStore = useNodes();

    const selectedNode = $derived(
        nodesStore.current.find((n) => n.selected) as CanvasNode | undefined,
    );
</script>

<div class="sidebar">
    {#if selectedNode?.data}
        <div class="arguments">
            <h3>{selectedNode.data.brickId}</h3>
            <h2>{selectedNode.data.brick?.description}</h2>
            <div class="node-data">
                <h4>Selected Node: {selectedNode.id}</h4>
                <pre>{JSON.stringify(selectedNode.data, null, 2)}</pre>
            </div>
        </div>
    {/if}

    <Minimap />
</div>

<style lang="scss">
    @import "$styles/theme";

    .sidebar {
        display: flex;
        flex-direction: column;
        gap: $gap;
    }

    .arguments {
        background-color: $background-secondary;
        border: $border;
        border-radius: $border-radius;
        padding: $gap;

        h3 {
            margin: 0 0 $gap 0;
            color: $foreground;
            font-size: 16px;
        }

        h4 {
            margin: 0 0 8px 0;
            color: $primary;
            font-size: 14px;
        }

        p {
            color: $foreground;
            margin: 0;
        }

        pre {
            background-color: $background;
            color: $foreground;
            padding: 8px;
            border-radius: 4px;
            font-size: 12px;
            overflow-x: auto;
            margin: 0;
            white-space: pre-wrap;
            word-break: break-all;
        }
    }
</style>
