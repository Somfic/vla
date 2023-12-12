<script lang="ts">
    import { createEventDispatcher } from "svelte";
    import type { Web, Workspace } from "../../lib/nodes";

    export let workspace: Workspace;
    export let web: Web;

    const dispatch = createEventDispatcher();
</script>

<div class="topbar" style:--workspace-accent={workspace.color.hex + "66"}>
    <div class="left"></div>
    <div class="center">
        <div class="name">
            <p class="workspace" on:click={() => dispatch("changeWorkspace", workspace)}>
                {workspace.name}
            </p>
            <div class="divider">/</div>
            <p class="name">{web.name}</p>
        </div>
    </div>
    <div class="right"></div>
</div>

<style lang="scss">
    @import "../../theme.scss";

    .topbar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 1rem;
        height: 3rem;
        // Transparent version of --workspace-accent
        background-color: var(--workspace-accent);
        backdrop-filter: $filter-frosted;
        border-bottom: 1px solid $border-color;
        color: $foreground;
    }

    .name {
        display: flex;
        align-items: center;
        gap: 0.5rem;
    }

    .left,
    .center,
    .right {
        width: 0px;
        display: flex;
        align-items: center;
        gap: 1rem;
        flex-grow: 10000000;
        flex-shrink: 1000000;
    }

    .left {
        justify-content: flex-start;
    }

    .center {
        justify-content: center;
    }

    .right {
        justify-content: flex-end;
    }
</style>
