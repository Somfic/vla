<script lang="ts">
    import { onMount } from "svelte";
    import {
        type ShortcutOptions,
        type UnregisterFunction,
        shortcuts as shortcutManager,
    } from "../lib/shortcuts.svelte";

    export interface ShortcutConfig {
        key: string;
        handler: (event: KeyboardEvent) => void;
        options?: ShortcutOptions;
    }

    interface Props {
        shortcuts: ShortcutConfig[];
        children: any;
    }

    let { shortcuts, children }: Props = $props();

    onMount(() => {
        const unregisterFunctions: UnregisterFunction[] = [];

        // Register all shortcuts
        shortcuts.forEach(({ key, handler, options }) => {
            const unregister = shortcutManager.register(
                key,
                handler,
                options || {},
            );
            unregisterFunctions.push(unregister);
        });

        // Cleanup function
        return () => {
            unregisterFunctions.forEach((fn) => fn());
        };
    });
</script>

{@render children()}
