<script lang="ts">
    import { onMount } from "svelte";
    import { partialRecognition, recognition, startListening, messages } from "./lib/ws";
    import { structures } from "./lib/nodes";
    import Editor from "./components/Editor.svelte";

    onMount(() => {
        startListening();
    });
</script>

<main class="container">
    <Editor />

    <p>{$recognition}</p>
    <p>{$partialRecognition}</p>

    {#each $structures as node}
        <div>{node.Type.split(",")[0].split(".").slice(-1)[0].replace("Node", "")}</div>
    {/each}
</main>

<style>
</style>
