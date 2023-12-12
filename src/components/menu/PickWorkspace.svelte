<script lang="ts">
    import { createEventDispatcher, onDestroy, onMount } from "svelte";
    import { workspaces } from "../../lib/nodes";

    const dispatch = createEventDispatcher();

    let workspaceIndex: number = -1;

    onMount(() => {
        window.addEventListener("keydown", handleKeyPress);
    });

    onDestroy(() => {
        window.removeEventListener("keydown", handleKeyPress);
    });

    function handleKeyPress(e: KeyboardEvent) {
        console.log(e.key);

        if (e.key == "Escape") {
            dispatch("picked", { undefined });
        }

        if (e.key == "ArrowDown" || e.key == "ArrowUp") {
            let delta = e.key == "ArrowDown" ? 1 : -1;
            delta = e.shiftKey || e.ctrlKey ? delta * 5 : delta;

            workspaceIndex += delta;

            santiseIndex();
        }

        if (e.key == "Enter") {
            pick(workspaceIndex);
        }
    }

    function santiseIndex() {
        if (workspaceIndex < 0) workspaceIndex = $workspaces.length - 1;
        if (workspaceIndex >= $workspaces.length) workspaceIndex = 0;
    }

    function handleClickWorkspace(i: number) {
        workspaceIndex = i;
        pick(workspaceIndex);
    }

    function pick(i: number) {
        dispatch("picked", $workspaces[workspaceIndex]);
    }
</script>

<a class="picker" tabindex="1" on:click={(e) => e.stopPropagation()}>
    <h1>Pick workspace</h1>
    <div class="workspaces">
        {#each $workspaces as workspace, i}
            <div class="workspace" class:active={workspaceIndex == i} on:click={() => handleClickWorkspace(i)}>
                <div class="name">{workspace.name}</div>
                <div class="path">{workspace.webs.length} {workspace.webs.length == 1 ? "web" : "webs"}</div>
            </div>
        {/each}
    </div>
</a>

<style lang="scss">
    @import "../../theme.scss";

    .picker {
        display: flex;
        flex-direction: column;

        h1 {
            padding: 1rem;
            padding-top: 2rem;
            margin: 0;
            font-size: 1.5rem;
            font-weight: bold;
            border-bottom: 2px solid $border-color;
        }

        .workspaces {
            display: flex;
            flex-direction: column;
            border-right: 2px solid $border-color;

            .workspace {
                padding: 1rem;
                border-bottom: 2px solid $border-color;
                min-width: 200px;
                transition: 200ms ease all;
                cursor: pointer;

                &:last-child {
                    border-bottom: none;
                }

                &.active,
                &:hover {
                    background-color: $accent;
                    color: $foreground;

                    .path {
                        color: $foreground;
                    }
                }

                .name {
                    font-weight: bold;
                }

                .path {
                    font-size: 0.8rem;
                    color: $foreground-muted;
                }
            }
        }

        .webs {
            display: flex;
            align-items: center;
            justify-content: center;
            flex-grow: 1;

            .web {
                padding: 1rem;
                border-bottom: 2px solid $border-color;
                min-width: 200px;
                transition: 200ms ease all;

                &:last-child {
                    border-bottom: none;
                }

                &.active,
                &:hover {
                    background-color: $accent;
                    color: $foreground;

                    .path {
                        color: $foreground;
                    }
                }

                .name {
                    font-weight: bold;
                }

                .path {
                    font-size: 0.8rem;
                    color: $foreground-muted;
                }
            }
        }
    }
</style>
