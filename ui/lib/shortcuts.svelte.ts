export interface ShortcutOptions {
    context?: string;
    priority?: number;
    description?: string;
    preventDefault?: boolean;
    stopPropagation?: boolean;
    enabled?: boolean;
}

export interface Shortcut {
    key: string;
    handler: (event: KeyboardEvent) => void;
    context: string;
    priority: number;
    description: string;
    preventDefault: boolean;
    stopPropagation: boolean;
    enabled: boolean;
}

export interface Command {
    id: string;
    title: string;
    description?: string;
    category?: string;
    icon?: string;
    keywords?: string[];
    lastUsed?: number;
    action: () => void | Promise<void>;
}

export interface CommandWithScore extends Command {
    score: number;
}

export type UnregisterFunction = () => void;

class ShortcutManager {
    private shortcuts = new Map<string, Map<string, Shortcut>>();
    private activeContexts = $state(new Set<string>(['global']));

    constructor() {
        // Bind the handler to maintain context
        this.handleKeydown = this.handleKeydown.bind(this);

        // Add global listener
        if (typeof window !== 'undefined') {
            window.addEventListener('keydown', this.handleKeydown);
        }
    }

    // Normalize key combinations
    private normalizeKey(key: string): string {
        const parts = key.toLowerCase().split('+');
        const modifiers: string[] = [];
        let mainKey = '';

        parts.forEach(part => {
            part = part.trim();
            if (['ctrl', 'cmd', 'meta'].includes(part)) {
                modifiers.push('meta');
            } else if (part === 'shift') {
                modifiers.push('shift');
            } else if (part === 'alt') {
                modifiers.push('alt');
            } else {
                mainKey = part;
            }
        });

        return [...modifiers.sort(), mainKey].join('+');
    }

    // Get key from event
    private getEventKey(event: KeyboardEvent): string {
        const modifiers: string[] = [];
        if (event.metaKey || event.ctrlKey) modifiers.push('meta');
        if (event.shiftKey) modifiers.push('shift');
        if (event.altKey) modifiers.push('alt');

        const key = event.key.toLowerCase();
        return [...modifiers, key].join('+');
    }

    // Register a shortcut
    register(key: string, handler: (event: KeyboardEvent) => void, options: ShortcutOptions = {}): UnregisterFunction {
        const normalizedKey = this.normalizeKey(key);
        const context = options.context || 'global';

        if (!this.shortcuts.has(context)) {
            this.shortcuts.set(context, new Map());
        }

        const shortcut: Shortcut = {
            key: normalizedKey,
            handler,
            context,
            priority: options.priority || 0,
            description: options.description || '',
            preventDefault: options.preventDefault !== false,
            stopPropagation: options.stopPropagation !== false,
            enabled: options.enabled !== false
        };

        this.shortcuts.get(context)!.set(normalizedKey, shortcut);

        // Return unregister function
        return () => this.unregister(key, context);
    }

    // Unregister a shortcut
    unregister(key: string, context = 'global'): void {
        const normalizedKey = this.normalizeKey(key);
        const contextMap = this.shortcuts.get(context);
        if (contextMap) {
            contextMap.delete(normalizedKey);
        }
    }

    // Set active contexts (contexts have priority order)
    setActiveContexts(contexts: string[]): void {
        this.activeContexts = new Set(['global', ...contexts]);
    }

    // Add context
    pushContext(context: string): void {
        this.activeContexts.add(context);
    }

    // Remove context
    popContext(context: string): void {
        this.activeContexts.delete(context);
    }

    // Handle keydown events
    private handleKeydown(event: KeyboardEvent): void {
        const eventKey = this.getEventKey(event);

        // Find matching shortcuts in active contexts (reverse order for priority)
        const activeContextsList = Array.from(this.activeContexts).reverse();

        for (const context of activeContextsList) {
            const contextMap = this.shortcuts.get(context);
            if (!contextMap) continue;

            const shortcut = contextMap.get(eventKey);
            if (shortcut && shortcut.enabled) {
                if (shortcut.preventDefault) {
                    event.preventDefault();
                }
                if (shortcut.stopPropagation) {
                    event.stopPropagation();
                }

                shortcut.handler(event);
                return; // Stop at first match
            }
        }
    }

    // Get all shortcuts for display/debugging
    getAllShortcuts(): Shortcut[] {
        const all: Shortcut[] = [];
        for (const [context, shortcuts] of this.shortcuts) {
            for (const [key, shortcut] of shortcuts) {
                all.push({ ...shortcut, context });
            }
        }
        return all;
    }

    // Clean up
    destroy(): void {
        if (typeof window !== 'undefined') {
            window.removeEventListener('keydown', this.handleKeydown);
        }
        this.shortcuts.clear();
    }
}

export const shortcuts = new ShortcutManager();
