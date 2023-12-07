<script lang="ts">
    import { onMount } from "svelte";
    import { partialRecognition, recognition, startListening, messages, isReady } from "./lib/ws";
    import { result, structures, types } from "./lib/nodes";
    import Editor from "./components/Editor.svelte";
    import Splashscreen from "./components/Splashscreen.svelte";
    import Topbar from "./components/topbar/Topbar.svelte";

    onMount(() => {
        startListening();
    });
</script>

<Splashscreen />

<div class="topbar">
    <Topbar />
</div>
<Editor />

<style lang="scss">
    .topbar {
        position: absolute;
        top: 0;
        left: 0;
        width: 100vw;
        z-index: 100;
    }
</style>
