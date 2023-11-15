<script>
    import { Node, Svelvet, Minimap, Controls } from "svelvet";
    import EditorNode from "./EditorNode.svelte";
    import { structures } from "../lib/nodes";
</script>

<div class="editor">
    <Svelvet theme="dark">
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
