<script lang="ts">
    import { onMount } from "svelte";
    import { hasConnected, startListening } from "./lib/ws";
    import Splashscreen from "./components/Splashscreen.svelte";
    import Menu from "./components/menu/Menu.svelte";
    import { invokeMenu } from "./lib/menu";
    import type { Web, Workspace } from "./lib/nodes";
    import Editor from "./components/editor/Editor.svelte";
    import WorkspaceEditor from "./components/workspace/Workspace.svelte";

    let workspace: Workspace | undefined;

    onMount(() => {
        startListening();
        triggerChangeWorkspace();
    });

    function triggerChangeWorkspace() {
        invokeMenu("pick-workspace", (i) => {
            if (i == undefined) return;
            workspace = i;
        });
    }
</script>

<div class="vla">
    {#if $hasConnected}
        <Menu />
    {/if}

    <Splashscreen />

    {#if workspace != undefined}
        <WorkspaceEditor bind:workspace on:changeWorkspace={(e) => triggerChangeWorkspace()} />
    {/if}
</div>

<style lang="scss">
    .vla {
        display: flex;
        flex-grow: 1;

        // Blurred vibrant colors background
        background: radial-gradient(rgb(0, 0, 0), rgb(19, 19, 19));
    }
</style>
