<script lang="ts">
    import { fly, fade } from "svelte/transition";
    import { flip } from "svelte/animate";
    import Shortcuts, { type ShortcutConfig } from "./Shortcuts.svelte";
    import { shortcutContext } from "$actions/shortcutContext";
    import { commands, type CommandWithScore } from "$lib/commands.svelte";
    import api from "$lib/api";

    let input: HTMLInputElement;
    let spotlight: HTMLDivElement;
    let query = $state("");
    let selectedIndex = $state(0);
    let isPulsing = $state(false);
    let pulseTimeout: number;
    let visibleResults = $state<CommandWithScore[]>([]);

    // close event
    let { onClose, open }: { onClose: () => void; open: boolean } = $props();

    $effect(() => {
        if (spotlight && open) {
            const handleFocusOut = (e: FocusEvent) => {
                if (!spotlight.contains(e.relatedTarget as Node)) {
                    close();
                }
            };

            input.addEventListener("focusout", handleFocusOut);

            return () => {
                input.removeEventListener("focusout", handleFocusOut);
            };
        }
    });

    $effect(() => {
        if (open) {
            setTimeout(() => {
                input.focus();
            }, 0);
        }
    });

    const allResults = $derived(() => {
        return commands.search(query);
    });

    const results = $derived(() => {
        const searchResults = allResults();
        const relevantResults = searchResults
            .filter((r) => r.score > 0)
            .slice(0, 5);

        // If we don't have enough relevant results, pad with irrelevant ones
        if (relevantResults.length < 5 && query.trim() === "") {
            const recentResults = commands
                .getAllCommands()
                .filter((cmd) => cmd.lastUsed)
                .sort((a, b) => (b.lastUsed || 0) - (a.lastUsed || 0))
                .slice(0, 5)
                .map((cmd) => ({ ...cmd, score: 0 }));

            return recentResults.slice(0, 5);
        }

        if (relevantResults.length < 5) {
            const irrelevantResults = commands
                .getAllCommands()
                .filter((cmd) => !relevantResults.find((r) => r.id === cmd.id))
                .slice(0, 5 - relevantResults.length)
                .map((cmd) => ({ ...cmd, score: 0 }));

            // Combine and sort by score to maintain proper ordering
            return [...relevantResults, ...irrelevantResults].sort(
                (a, b) => b.score - a.score,
            );
        }

        return relevantResults;
    });

    // Calculate dynamic height based on number of results
    const spotlightHeight = $derived(() => {
        const baseHeight = 60; // Input height + padding
        const resultHeight = 50; // Each result height + gap
        const numResults = Math.min(visibleResults.length, 5);
        const resultsHeight = numResults * resultHeight;
        return baseHeight + resultsHeight;
    });

    // Manage visible results to allow proper exit animations
    $effect(() => {
        const newResults = results();

        // Always update to new results for smooth transitions
        selectedIndex = 0;
        visibleResults = newResults;
    });

    $effect(() => {
        if (
            selectedIndex >= visibleResults.length &&
            visibleResults.length > 0
        ) {
            selectedIndex = visibleResults.length - 1;
        }
    });

    // Pulse animation when typing (debounced for fast typing)
    $effect(() => {
        if (query.length > 0) {
            // Clear any existing timeout
            if (pulseTimeout) {
                clearTimeout(pulseTimeout);
            }

            // Only pulse if not already pulsing
            if (!isPulsing) {
                isPulsing = true;
            }

            // Set a new timeout to stop pulsing
            pulseTimeout = setTimeout(() => {
                isPulsing = false;
            }, 300);
        } else {
            // Clear pulse immediately when query is empty
            if (pulseTimeout) {
                clearTimeout(pulseTimeout);
            }
            isPulsing = false;
        }
    });

    const navigateUp = () => {
        if (selectedIndex > 0) {
            selectedIndex--;
        }
    };

    const navigateDown = () => {
        if (selectedIndex < visibleResults.length - 1) {
            selectedIndex++;
        }
    };

    const executeIndex = async (index: number) => {
        const selected = visibleResults[index];
        if (selected) {
            await commands.execute(selected.id);
            onClose();
        }
    };

    const executeSelected = async () => {
        await executeIndex(selectedIndex);
    };

    const close = () => {
        query = "";
        selectedIndex = 0;
        onClose();
    };

    const handleInputKeydown = (event: KeyboardEvent) => {
        switch (event.key) {
            case "ArrowUp":
                event.preventDefault();
                event.stopPropagation();
                navigateUp();
                break;
            case "ArrowDown":
                event.preventDefault();
                event.stopPropagation();
                navigateDown();
                break;
            case "Enter":
                event.preventDefault();
                event.stopPropagation();
                executeSelected();
                break;
            case "Escape":
                event.preventDefault();
                event.stopPropagation();
                close();
                break;
            case "Backspace":
                if (query.length === 0) {
                    event.preventDefault();
                    event.stopPropagation();
                    close();
                }
                break;
            // For all other keys (including space), stop propagation to prevent global shortcuts
            default:
                event.stopPropagation();
                break;
        }
    };

    const shortcuts: ShortcutConfig[] = [];

    api.get_bricks().then((bricks) => {
        console.log("Loaded bricks:", bricks);

        bricks.forEach((brick) => {
            commands.register({
                id: brick.id,
                title: brick.label,
                description: brick.description,
                category: brick.category || "Uncategorized",
                keywords: [brick.category, ...brick.keywords],
                action: async () => {
                    await api.insert_node("../graph.json", brick.id, {
                        x: 0,
                        y: 0,
                    });
                },
            });
        });
    });
</script>

<Shortcuts {shortcuts} />

{#if open}
    <!-- svelte-ignore a11y_no_static_element_interactions -->
    <div class="wrapper">
        <!-- svelte-ignore a11y_click_events_have_key_events -->
        <div
            class="spotlight"
            use:shortcutContext={"spotlight"}
            bind:this={spotlight}
            onclick={close}
            style="height: {spotlightHeight()}px"
            in:fly={{ y: -20, duration: 200 }}
            out:fly={{ y: -20, duration: 150 }}
        >
            <div
                class="input"
                class:pulsing={isPulsing}
                class:has-content={query.length > 0}
            >
                <input
                    bind:value={query}
                    bind:this={input}
                    type="text"
                    placeholder="Search commands..."
                    onkeydown={handleInputKeydown}
                />
            </div>
            <div class="results">
                {#each visibleResults as result, index (result.id)}
                    <div
                        class="result {index === selectedIndex
                            ? 'selected'
                            : ''} {result.score === 0 ? 'irrelevant' : ''}"
                        onclick={() => {
                            executeIndex(index);
                        }}
                        in:fly={{ y: 20, duration: 200, delay: index * 50 }}
                        out:fade={{ duration: 150 }}
                        animate:flip={{ duration: 200 }}
                    >
                        {result.title}
                        <span
                            style="opacity: 0.5; font-size: 12px; margin-left: 8px;"
                            >({result.score})</span
                        >
                    </div>
                {/each}
            </div>
        </div>
    </div>
{/if}

<style lang="scss">
    @import "$styles/theme";

    .wrapper {
        position: fixed;
        top: 0;
        left: 0;
        display: flex;
        align-items: flex-start;
        justify-content: center;
        height: 100vh;
        width: 100vw;
        padding-top: 30vh;
        backdrop-filter: blur(24px) brightness(1.05) saturate(1.2);
        background: rgba(0, 0, 0, 0.01);
        z-index: 9999;
    }

    .spotlight {
        color: $foreground;
        width: 600px;
        max-width: 90%;
        z-index: 10000;
        display: flex;
        flex-direction: column;
        transition: height 0.3s ease;
        backdrop-filter: blur(32px) brightness(1.1) saturate(1.3);
        -webkit-backdrop-filter: blur(32px) brightness(1.1) saturate(1.3);
        background: rgba(30, 30, 30, 0.95);
        border: 1px solid rgba(255, 255, 255, 0.2);
        border-radius: 20px;
        box-shadow:
            0 16px 48px rgba(0, 0, 0, 0.1),
            0 8px 24px rgba(0, 0, 0, 0.06),
            0 4px 8px rgba(0, 0, 0, 0.04),
            inset 0 1px 0 rgba(255, 255, 255, 0.8),
            inset 0 -1px 0 rgba(0, 0, 0, 0.05);

        .input {
            border: 1px solid rgba(255, 255, 255, 0.15);
            border-radius: 16px;
            padding: $gap;
            background: rgba(40, 40, 40, 0.8);
            backdrop-filter: blur(16px);
            -webkit-backdrop-filter: blur(16px);
            transition: all 0.3s ease;
            box-shadow:
                0 2px 8px rgba(0, 0, 0, 0.2),
                inset 0 1px 0 rgba(255, 255, 255, 0.1);

            &.has-content {
                border-color: $primary;
                background: rgba(50, 50, 50, 0.9);
                box-shadow:
                    0 0 0 3px rgba($primary, 0.3),
                    0 2px 12px rgba(0, 0, 0, 0.3),
                    inset 0 1px 0 rgba(255, 255, 255, 0.2);
            }

            &.pulsing {
                border-color: $primary;
                animation: pulse 0.4s ease-in-out infinite;
            }

            input {
                padding: $gap;
                width: 100%;
                background: transparent;
                border: none;
                outline: none;
                color: inherit;
                font-size: inherit;

                &::placeholder {
                    color: rgba($foreground, 0.6);
                }
            }
        }

        @keyframes pulse {
            0%,
            100% {
                box-shadow: 0 0 0 2px rgba($primary, 0.15);
            }
            50% {
                box-shadow: 0 0 0 4px rgba($primary, 0.25);
            }
        }

        .results {
            display: flex;
            flex-direction: column;
            padding: $gap;
            gap: $gap;

            overflow-y: auto;
            scroll-behavior: smooth;

            .result {
                border: 1px solid rgba(255, 255, 255, 0.1);
                border-radius: 12px;
                padding: $gap;
                background: rgba(45, 45, 45, 0.7);
                cursor: pointer;
                transition: all 0.25s ease;
                backdrop-filter: blur(8px);
                -webkit-backdrop-filter: blur(8px);
                box-shadow:
                    0 1px 4px rgba(0, 0, 0, 0.3),
                    inset 0 1px 0 rgba(255, 255, 255, 0.1);

                &.selected {
                    border-color: $primary;
                    background: rgba($primary, 0.12);
                    transform: translateY(-1px);
                    box-shadow:
                        0 0 0 2px rgba($primary, 0.2),
                        0 4px 12px rgba(0, 0, 0, 0.08),
                        inset 0 1px 0 rgba(255, 255, 255, 0.8);
                }

                &.irrelevant {
                    opacity: 0.5;
                    background: rgba(35, 35, 35, 0.5);

                    &.selected {
                        opacity: 0.85;
                        border-color: rgba($primary, 0.6);
                        background: rgba($primary, 0.08);
                    }
                }
            }
        }
    }
</style>
