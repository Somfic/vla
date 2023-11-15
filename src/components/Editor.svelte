<script lang="ts">
    import { onMount } from "svelte";
    import { nodeStructures } from "../lib/nodes";
    import { createEditor } from "../lib/editor";

    let el: HTMLElement;

    nodeStructures.subscribe((nodes) => {
        createEditor(el, nodes);
    });

    onMount(async () => {});
</script>

<div class="rete" bind:this={el} />

<style>
    .rete {
        position: relative;
        width: 100vw;
        height: 100vh;
        font-size: 1rem;
        text-align: left;
        border: 1px solid #cbd0db;
    }

    :global(body) {
        margin: 0;
        overflow: hidden;
    }
</style>
