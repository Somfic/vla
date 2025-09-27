import Fuse from 'fuse.js';

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

export interface CommandCategory {
    id: string;
    name: string;
    icon?: string;
    priority?: number;
}

class CommandRegistry {
    private commands = $state(new Map<string, Command>());
    private categories = $state(new Map<string, CommandCategory>());
    private fuse: Fuse<Command> | null = null;

    // Register a new command
    register(command: Command): () => void {
        this.commands.set(command.id, { ...command });

        // Auto-create category if it doesn't exist
        if (command.category && !this.categories.has(command.category)) {
            this.categories.set(command.category, {
                id: command.category,
                name: command.category.charAt(0).toUpperCase() + command.category.slice(1)
            });
        }

        // Recreate Fuse index when commands change
        this.initializeFuse();

        // Return unregister function
        return () => this.unregister(command.id);
    }

    // Unregister a command
    unregister(commandId: string): void {
        this.commands.delete(commandId);
        this.initializeFuse();
    }

    // Initialize Fuse.js instance
    private initializeFuse(): void {
        const commandsList = Array.from(this.commands.values());

        this.fuse = new Fuse(commandsList, {
            keys: [
                { name: 'title', weight: 0.6 },
                { name: 'description', weight: 0.2 },
                { name: 'keywords', weight: 0.15 },
                { name: 'category', weight: 0.05 }
            ],
            threshold: 0.4, // Lower = more strict matching
            distance: 100,
            minMatchCharLength: 1,
            includeScore: true,
            includeMatches: true,
            ignoreLocation: true,
            useExtendedSearch: true
        });
    }

    // Register a category
    registerCategory(category: CommandCategory): void {
        this.categories.set(category.id, category);
    }

    // Search commands with scoring
    search(query: string): CommandWithScore[] {
        if (!query.trim()) {
            // Return recent commands when no query
            return this.getRecentCommands();
        }

        if (!this.fuse) {
            this.initializeFuse();
        }

        if (!this.fuse) {
            return [];
        }

        const results = this.fuse.search(query, { limit: 8 });

        return results.map(result => {
            const command = result.item;
            // Convert Fuse score (lower is better) to our score (higher is better)
            const score = Math.round((1 - (result.score || 0)) * 1000);

            // Add recent usage boost
            let finalScore = score;
            if (command.lastUsed) {
                const daysSinceUsed = (Date.now() - command.lastUsed) / (1000 * 60 * 60 * 24);
                if (daysSinceUsed < 1) finalScore += 150;
                else if (daysSinceUsed < 7) finalScore += 100;
                else if (daysSinceUsed < 30) finalScore += 50;
            }

            return { ...command, score: finalScore };
        }).sort((a, b) => b.score - a.score);
    }


    // Get recently used commands
    private getRecentCommands(): CommandWithScore[] {
        return Array.from(this.commands.values())
            .filter(cmd => cmd.lastUsed)
            .sort((a, b) => (b.lastUsed || 0) - (a.lastUsed || 0))
            .slice(0, 5)
            .map(cmd => ({ ...cmd, score: 0 }));
    }

    // Execute a command
    async execute(commandId: string): Promise<void> {
        const command = this.commands.get(commandId);
        if (!command) return;

        // Track usage
        command.lastUsed = Date.now();

        try {
            await command.action();
        } catch (error) {
            console.error('Error executing command:', error);
        }
    }

    // Get all commands
    getAllCommands(): Command[] {
        return Array.from(this.commands.values());
    }

    // Get commands by category
    getCommandsByCategory(categoryId: string): Command[] {
        return Array.from(this.commands.values())
            .filter(cmd => cmd.category === categoryId);
    }

    // Get all categories
    getCategories(): CommandCategory[] {
        return Array.from(this.categories.values())
            .sort((a, b) => (a.priority || 0) - (b.priority || 0));
    }
}

export const commands = new CommandRegistry();
