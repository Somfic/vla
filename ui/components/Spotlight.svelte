<script lang="ts">
    import { onMount } from "svelte";
    import Shortcuts, { type ShortcutConfig } from "./Shortcuts.svelte";
    import { shortcutContext } from "$actions/shortcutContext";

    let input: HTMLInputElement;
    let spotlight: HTMLDivElement;

    // close event
    let { onClose, show }: { onClose: () => void; show: boolean } = $props();

    onMount(() => {
        // close when spotlight loses focus
        spotlight.addEventListener("focusout", (e) => {
            if (!spotlight.contains(e.relatedTarget as Node)) {
                input.value = "";
                onClose();
            }
        });
    });

    $effect(() => {
        if (show) {
            // focus input when spotlight is shown
            setTimeout(() => {
                input.focus();
            }, 0);
        }
    });

    const shortcuts: ShortcutConfig[] = [
        {
            key: "enter",
            handler: onClose,
            options: {
                context: "spotlight",
                preventDefault: true,
                enabled: show,
            },
        },
    ];
</script>

<Shortcuts {shortcuts} />

<div class="wrapper">
    <div
        class="spotlight"
        use:shortcutContext={"spotlight"}
        bind:this={spotlight}
        class:hide={!show}
    >
        <div class="input">
            <input bind:this={input} type="text" placeholder="Search..." />
        </div>
        <div class="results">
            <!-- Search results will be displayed here -->
            <p>No results found.</p>
        </div>
    </div>
</div>

<style lang="scss">
    @import "$styles/theme";

    .wrapper {
        position: absolute;
        display: flex;
        align-items: center;
        justify-content: center;
        height: 100%;
        width: 100%;
    }

    .spotlight {
        background: $background;
        color: $foreground;

        border: 2px solid $border-color;
        border-radius: $border-radius2;
        width: 400px;
        max-width: 90%;
        z-index: 1000;
        display: flex;
        flex-direction: column;

        &.hide {
            opacity: 0;
        }

        .input {
            border-bottom: 2px solid $border-color;
            padding: $gap;

            input {
                padding: $gap;
                width: 100%;
            }
        }

        .results {
            display: flex;
            padding: $gap;
            gap: $gap;

            max-height: 300px;
            overflow-y: auto;

            p {
                margin: 0;
                color: #666;
                text-align: center;
            }
        }
    }
</style>
