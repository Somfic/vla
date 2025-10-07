import { shortcuts, type ShortcutOptions, type UnregisterFunction } from '../lib/shortcuts.svelte';
import type { Action } from 'svelte/action';

interface ShortcutActionParams extends ShortcutOptions {
    key: string;
    handler: (event: KeyboardEvent) => void;
}

export const shortcut: Action<HTMLElement, ShortcutActionParams> = (node, params) => {
    let unregister: UnregisterFunction | undefined;

    function update(newParams: ShortcutActionParams) {
        // Unregister previous shortcut if it exists
        if (unregister) {
            unregister();
        }

        // Register new shortcut
        unregister = shortcuts.register(
            newParams.key,
            newParams.handler,
            {
                context: newParams.context,
                priority: newParams.priority,
                description: newParams.description,
                preventDefault: newParams.preventDefault,
                stopPropagation: newParams.stopPropagation,
                enabled: newParams.enabled
            }
        );
    }

    // Initial registration
    update(params);

    return {
        update,
        destroy() {
            if (unregister) {
                unregister();
            }
        }
    };
};
