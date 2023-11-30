<script lang="ts">
    import type { ContextResult } from "../lib/context";

    export let menu: ((query: string) => ContextResult[]) | undefined;

    let input: HTMLInputElement | undefined = undefined;
    let inputValue: string = "";
    let results: ContextResult[] = [];
    let activeIndex: number = 0;

    $: show = menu != undefined;
    $: if (show) {
        search();
        setTimeout(() => {
            input?.focus();
        }, 100);
    }

    function handleKeyPress(e: KeyboardEvent) {
        if (e.key == "Escape") {
            console.log("ESC");
            close();
        }

        if (e.key == "ArrowDown" || e.key == "ArrowUp") {
            let delta = e.key == "ArrowDown" ? 1 : -1;
            delta = e.shiftKey || e.ctrlKey ? delta * 5 : delta;

            activeIndex = Math.min(activeIndex + delta, results.length - 1);
            activeIndex = Math.max(activeIndex, 0);

            e.preventDefault();
        }

        if (e.key == "Enter") {
            menu?.(inputValue ?? "")?.[activeIndex]?.action();
            close();
        }

        setTimeout(() => {
            inputValue = inputValue.trim();
            search();
        }, 1);
    }

    function handleClick(e: MouseEvent) {
        close();
    }

    function close() {
        menu = undefined;

        setTimeout(() => {
            inputValue = "";
            results = [];
        }, 500);
    }

    function search() {
        results = menu?.(inputValue ?? "") ?? [];
    }
</script>

<div class="context-wrapper" on:keydown={handleKeyPress} on:click={handleClick} class:show>
    <div class="context" on:click={(e) => e.stopPropagation()}>
        <input class="input" bind:value={inputValue} bind:this={input} type="text" />
        {#if results.length > 0}
            {#each results as result, i}
                <div class="result" class:active={activeIndex == i}>
                    <div class="name">
                        {result.name}
                    </div>
                    {#if result.context}
                        <div class="context">
                            {result.context}
                        </div>
                    {/if}
                </div>
            {/each}
        {/if}
    </div>
</div>

<style lang="scss">
    @import "../theme.scss";

    .context-wrapper {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        z-index: 30000;
        background-color: rgba(0, 0, 0, 0.5);
        padding-top: 30vh;

        transition: 200ms ease all;

        opacity: 0;
        pointer-events: none;

        &.show {
            opacity: 1;
            pointer-events: all;
        }

        > .context {
            margin: auto;
            min-width: 300px;
            width: 50vw;
            display: flex;
            height: auto;
            flex-shrink: 1;
            flex-direction: column;
            background-color: rgba(0, 0, 0, 0.25);
            border: 2px solid $border-color;
            border-radius: 10px;
            backdrop-filter: blur(5px);
            overflow: hidden;
        }
    }

    .input {
        all: unset;
        padding: 1rem 2rem;
        width: 100%;
        font-size: 2.5rem;

        background-color: inherit;
        color: $foreground;
        transition: all ease 200ms;
    }

    .result {
        padding: 1rem 2rem;

        display: flex;
        align-items: center;
        justify-content: space-between;

        background-color: inherit;
        color: $foreground;
        transition: all ease 200ms;

        .context {
            opacity: 0.5;
        }

        &.active {
            background-color: $accent;
            color: $foreground;
        }
    }
</style>
