<script lang="ts">
    import Minimap from "$components/canvas/Minimap.svelte";
    import { useNodes } from "@xyflow/svelte";
    import type { VlaNode } from "$lib/api";

    const nodesStore = useNodes();
    let selectedNode: VlaNode | undefined = $state(undefined);

    // Use a different approach - track changes manually
    let lastNodesString = $state('');

    $effect(() => {
        const currentNodes = nodesStore.current;
        const nodesString = JSON.stringify(currentNodes.map(n => ({ id: n.id, selected: n.selected })));

        if (nodesString !== lastNodesString) {
            lastNodesString = nodesString;
            selectedNode = currentNodes.find((n) => n.selected) as VlaNode | undefined;
            console.log('Selection update:', selectedNode?.id || 'none');
        }
    });
</script>

<div class="sidebar">
    <div class="debug-panel">
        <h3>Debug Panel</h3>
        {#if selectedNode}
            <div class="node-data">
                <h4>Selected Node: {selectedNode.id}</h4>
                <pre>{JSON.stringify(selectedNode.data, null, 2)}</pre>
            </div>
        {:else}
            <p>No node selected</p>
        {/if}
    </div>

    <Minimap />
</div>

<style lang="scss">
    @import "$styles/theme";

    .sidebar {
        display: flex;
        flex-direction: column;
        gap: $gap;
    }

    .debug-panel {
        background-color: $background-secondary;
        border: 2px solid $border-color;
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
