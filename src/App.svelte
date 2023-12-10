<script lang="ts">
    import { onMount } from "svelte";
    import { partialRecognition, recognition, startListening, messages, isReady } from "./lib/ws";
    import { result, structures, types } from "./lib/nodes";
    import Editor from "./components/editor/EditorValue.svelte";
    import Splashscreen from "./components/Splashscreen.svelte";
    import Topbar from "./components/topbar/Topbar.svelte";

    onMount(() => {
        startListening();
    });
</script>

<Splashscreen />

<style lang="scss">
</style>
