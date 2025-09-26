import { shortcuts } from '$lib/shortcuts.svelte';
import type { Action } from 'svelte/action';

export const shortcutContext: Action<HTMLElement, string> = (node, context) => {
    let currentContext = context;

    function handleFocusIn() {
        shortcuts.pushContext(currentContext);
    }

    function handleFocusOut() {
        shortcuts.popContext(currentContext);
    }

    // Add event listeners
    node.addEventListener('focusin', handleFocusIn);
    node.addEventListener('focusout', handleFocusOut);

    // If element is already focused, activate context immediately
    if (document.activeElement && node.contains(document.activeElement)) {
        handleFocusIn();
    }

    return {
        update(newContext: string) {
            // Remove old context and add new one
            shortcuts.popContext(currentContext);
            currentContext = newContext;

            // If still focused, activate new context
            if (document.activeElement && node.contains(document.activeElement)) {
                shortcuts.pushContext(currentContext);
            }
        },
        destroy() {
            node.removeEventListener('focusin', handleFocusIn);
            node.removeEventListener('focusout', handleFocusOut);
            shortcuts.popContext(currentContext);
        }
    };
};
