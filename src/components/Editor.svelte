<script lang="ts">
    import { onMount } from "svelte";
    import { structures, types } from "../lib/nodes";
    import { createEditor, loadStructures, loadTypes } from "../lib/editor";

    let el: HTMLElement;

    structures.subscribe(async (nodes) => {
        await loadStructures(nodes);
    });

    types.subscribe(async (types) => {
        await loadTypes(types);
    });

    onMount(async () => {
        await createEditor(el);
        await loadStructures($structures);
    });
</script>

<div class="rete" bind:this={el} />

<style>
    .rete {
        position: relative;
        width: 100vw;
        height: 100vh;
        font-size: 1rem;
        text-align: left;
        background-color: #131313;
    }

    :global(body) {
        margin: 0;
        overflow: hidden;
    }
</style>
