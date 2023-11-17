<script lang="ts">
    import { Svelvet } from "svelvet";
    import EditorNode from "./EditorNode.svelte";
    import { structures } from "../lib/nodes";

    function connection(e: any) {
        console.log(e);
    }
</script>

<div class="editor">
    <Svelvet theme="dark" on:connection={connection} controls>
        {#each $structures as structure}
            <EditorNode {structure} />
        {/each}
    </Svelvet>
</div>

<style lang="scss">
    .editor {
        height: 400px;
    }
</style>
