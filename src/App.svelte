<script lang="ts">
    import { onMount } from "svelte";
    import { partialRecognition, recognition, startListening, messages } from "./lib/ws";
    import { structures } from "./lib/nodes";
    import Editor from "./components/Editor.svelte";

    onMount(() => {
        startListening();
    });
</script>

<Editor />

<p>{$recognition}</p>
<p>{$partialRecognition}</p>
