<script>
    import { MiniMap } from "@xyflow/svelte";
</script>

<div class="sidebar">
    <MiniMap />
</div>

<style lang="scss">
    .sidebar {
        display: flex;
    }

    :global(.svelte-flow__minimap) {
        position: relative;
        background-color: #f0f0f0;
        width: auto;
        height: auto;
        display: flex;
        flex-grow: 1;
        margin: 0;
        padding: 0;
    }

    :global(.svelte-flow__minimap-node) {
        border-radius: 10px;
    }

    :global(.svelte-flow) {
        --xy-minimap-background-color-default: red;
    }
</style>
