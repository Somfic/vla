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

            activeIndex += delta;

            santiseIndex();

            e.preventDefault();
        }

        if (e.key == "Enter") {
            execute(activeIndex);
            close();
        }

        setTimeout(() => {
            search();
            santiseIndex();
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

    function handleMouseOver(i: number) {
        activeIndex = i;
    }

    function handleMouseClick(i: number) {
        execute(i);
        close();
    }

    function handleScroll(e: Event) {
        let delta = (e as WheelEvent).deltaY > 0 ? 1 : -1;

        activeIndex += delta;

        santiseIndex();
    }

    function execute(i: number) {
        menu?.(inputValue ?? "")?.[i]?.action();
    }

    function santiseIndex() {
        activeIndex = Math.min(activeIndex, results.length - 1);
        activeIndex = Math.max(activeIndex, 0);
    }
</script>

<!-- svelte-ignore a11y-no-static-element-interactions -->
<div class="context-wrapper" on:keydown={handleKeyPress} on:click={handleClick} on:scroll={handleScroll} class:show>
    <!-- svelte-ignore a11y-click-events-have-key-events -->
    <div class="context" on:click={(e) => e.stopPropagation()}>
        <input class="input" placeholder="Search for anything" on:input={santiseIndex} bind:value={inputValue} bind:this={input} type="text" />
        {#if results.length > 0}
            {#each results as result, i}
                <!-- svelte-ignore a11y-mouse-events-have-key-events -->
                <div class="result" class:active={activeIndex == i} on:mouseover={() => handleMouseOver(i)} on:click={() => handleMouseClick(i)}>
                    <div class="name">
                        {result.name}
                    </div>
                    <div class="context">
                        {#if result.category}
                            <div class="category">
                                {result.category}
                            </div>
                        {/if}
                        {#if result.sourcePlugin}
                            <div class="plugin">
                                {result.sourcePlugin}
                            </div>
                        {/if}
                    </div>
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
        background-color: $background-frosted;
        backdrop-filter: brightness(0.5) blur(10px);
        padding-top: 30vh;

        transition: 200ms ease all;

        opacity: 0;
        pointer-events: none;

        &.show {
            opacity: 1;
            pointer-events: all;

            > .context {
                transform: translateY(0px);
            }
        }

        > .context {
            transform: translateY(100px);
            margin: auto;
            min-width: 500px;
            width: 50vw;
            display: flex;
            height: auto;
            flex-shrink: 1;
            flex-direction: column;
            background-color: $background-frosted;
            backdrop-filter: $filter-frosted;
            border: 2px solid $border-color;
            border-radius: 10px;
            overflow: hidden;
            transition: 200ms ease all;
            box-shadow: 0px 0px 100px 0px rgba(0, 0, 0, 1);
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
        cursor: pointer;

        .context {
            display: flex;
            align-items: center;
            gap: 1rem;
        }

        .category {
            opacity: 0.5;
        }

        .plugin {
            opacity: 0.25;
        }

        &.active {
            background-color: $accent;
            color: $foreground;
        }
    }
</style>
