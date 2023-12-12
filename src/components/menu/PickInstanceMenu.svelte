<script lang="ts">
    import Fuse from "fuse.js";
    import { get } from "svelte/store";
    import { structures } from "../../lib/nodes";
    import { createEventDispatcher } from "svelte";
    import { generateGuid, getDefaultValueForType } from "../../lib/context";

    const dispatch = createEventDispatcher();

    let inputValue: string = "";
    let results: Array<{
        name: string;
        sourcePlugin: string;
        category: string;
        action: () => void;
    }> = [];
    let activeIndex: number = 0;

    function handleKeyPress(e: KeyboardEvent) {
        if (e.key == "ArrowDown" || e.key == "ArrowUp") {
            let delta = e.key == "ArrowDown" ? 1 : -1;
            delta = e.shiftKey || e.ctrlKey ? delta * 5 : delta;

            activeIndex += delta;

            santiseIndex();

            e.preventDefault();
        }

        if (e.key == "Enter") {
            execute(activeIndex);
        }

        setTimeout(() => {
            search();
            santiseIndex();
        }, 1);
    }

    function search() {
        results = addNode(inputValue ?? "") ?? [];
    }

    function handleMouseOver(i: number) {
        activeIndex = i;
    }

    function handleMouseClick(i: number) {
        execute(i);
    }

    function handleScroll(e: Event) {
        let delta = (e as WheelEvent).deltaY > 0 ? 1 : -1;

        activeIndex += delta;

        santiseIndex();
    }

    function execute(i: number) {
        results[i].action();
    }

    function santiseIndex() {
        activeIndex = Math.min(activeIndex, results.length - 1);
        activeIndex = Math.max(activeIndex, 0);
    }

    function addNode(query: string) {
        const search = new Fuse(get(structures), {
            keys: ["searchTerms"],
            threshold: 0.3,
        });

        const result = search.search(query);

        return result.map((r) => {
            return {
                name: r.item.name,
                sourcePlugin: r.item.nodeType.split(".")[0],
                category: r.item.category,
                action: () => {
                    dispatch("picked", {
                        id: generateGuid(),
                        nodeType: r.item.nodeType,
                        metadata: {
                            position: {
                                x: 0,
                                y: 0,
                            },
                        },
                        inputs: r.item.inputs.map((p) => {
                            return {
                                id: p.id,
                                value: p.defaultValue,
                            };
                        }),
                        properties: r.item.properties.map((p) => {
                            return {
                                name: p.name,
                                type: p.type,
                                value: getDefaultValueForType(p.type),
                            };
                        }),
                    });
                },
            };
        });
    }
</script>

<!-- svelte-ignore a11y-no-static-element-interactions -->
<div class="context" tabindex="1" on:keydown={handleKeyPress} on:scroll={handleScroll} on:click={(e) => e.stopPropagation()}>
    <input class="input" placeholder="Search for anything" on:input={santiseIndex} bind:value={inputValue} type="text" />
    {#if results.length > 0}
        {#each results as result, i}
            <!-- svelte-ignore a11y-mouse-events-have-key-events -->
            <!-- svelte-ignore a11y-click-events-have-key-events -->
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

<style lang="scss">
    @import "../../theme.scss";

    .input {
        all: unset;
        padding: 1rem;
        padding-top: 2rem;
        margin: 0;
        font-size: 1.5rem;
        font-weight: bold;
        width: 100%;

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
