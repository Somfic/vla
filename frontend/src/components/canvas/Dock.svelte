<script lang="ts">
    import api from "$lib/api";
    import type { Graph } from "$lib/core";

    let { graph }: { graph: Graph } = $props();

    function onRun() {
        api.execute_graph(graph, "Normal");
    }
</script>

<div class="dock">
    <button onclick={() => onRun()}> Run </button>
</div>

<style lang="scss">
    @import "../../styles/theme.scss";

    .dock {
        display: flex;
        align-items: center;
        justify-content: space-evenly;
        border: $border;
        border-radius: $border-radius2;
        padding: $gap;
        background-color: $blur-background;
        backdrop-filter: $blur;
        transition: all $transition;

        &:hover {
            padding: $gap $gap2;
        }

        button {
            aspect-ratio: 1;
            border-radius: $border-radius;
            border: $border;
            background-color: $blur-background;
            color: $foreground;
            transition: all $transition;
            cursor: pointer;

            &:hover,
            &:focus {
                border-color: $primary;
            }
        }
    }
</style>
