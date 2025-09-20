<script>
    import { MiniMap } from "@xyflow/svelte";
</script>

<MiniMap />

<style lang="scss">
    @import "$styles/theme";

    :global(.svelte-flow__minimap) {
        position: relative;
        background-color: $primary;
        width: auto;
        height: auto;
        display: flex;
        flex-grow: 1;
        margin: 0;
        padding: 0;

        border: 2px solid $border-color;
        border-radius: $border-radius2;
        overflow: hidden;
    }

    :global(.svelte-flow__minimap-mask) {
        fill: none;
        stroke: rgba(255, 255, 255, 0.3);
        stroke-width: 50px;
    }

    :global(.svelte-flow__minimap-node) {
        border-radius: 10px;
        fill: $foreground;
    }
</style>
