import { shortcuts } from '../lib/shortcuts.svelte';
import type { Action } from 'svelte/action';

export const shortcutContext: Action<HTMLElement, string> = (node, context) => {
    let currentContext = context;

    function enter() {
        shortcuts.pushContext(currentContext);
    }

    function leave() {
        shortcuts.popContext(currentContext);
    }

    // Add event listeners
    node.addEventListener('focusin', enter);
    node.addEventListener('click', enter);
    node.addEventListener('mouseenter', enter);
    node.addEventListener('focusout', leave);
    node.addEventListener('mouseleave', leave);

    // If element is already focused, activate context immediately
    if (document.activeElement && node.contains(document.activeElement)) {
        enter();
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
            node.removeEventListener('focusin', enter);
            node.removeEventListener('click', enter);
            node.removeEventListener('mouseenter', enter);
            node.removeEventListener('focusout', leave);
            node.removeEventListener('mouseleave', leave);
            shortcuts.popContext(currentContext);
        }
    };
};
