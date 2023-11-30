<script lang="ts">
    import { onMount } from "svelte";
    import { partialRecognition, recognition, startListening, messages, isReady } from "./lib/ws";
    import { result, structures, types } from "./lib/nodes";
    import Editor from "./components/Editor.svelte";
    import Splashscreen from "./components/Splashscreen.svelte";

    onMount(() => {
        startListening();
    });
</script>

<Splashscreen />

<Editor />
