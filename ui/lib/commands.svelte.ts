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

        // Return unregister function
        return () => this.unregister(command.id);
    }

    // Unregister a command
    unregister(commandId: string): void {
        this.commands.delete(commandId);
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

        const results: CommandWithScore[] = [];
        const normalizedQuery = query.toLowerCase().trim();

        for (const command of this.commands.values()) {
            const score = this.calculateScore(command, normalizedQuery);
            if (score > 0) {
                results.push({ ...command, score });
            }
        }

        return results.sort((a, b) => b.score - a.score).slice(0, 8); // Limit results
    }

    private calculateScore(command: Command, query: string): number {
        const title = command.title.toLowerCase();
        const description = (command.description || '').toLowerCase();
        const keywords = (command.keywords || []).join(' ').toLowerCase();
        const category = (command.category || '').toLowerCase();

        let score = 0;

        // Title matches (highest priority)
        if (title === query) score += 100;
        else if (title.startsWith(query)) score += 80;
        else if (title.includes(query)) score += 60;

        // Description matches
        if (description.includes(query)) score += 30;

        // Keywords matches
        if (keywords.includes(query)) score += 40;

        // Category matches
        if (category.includes(query)) score += 20;

        // Fuzzy matching for partial words
        if (this.fuzzyMatch(title, query)) score += 25;

        // Recent usage boost
        if (command.lastUsed) {
            const daysSinceUsed = (Date.now() - command.lastUsed) / (1000 * 60 * 60 * 24);
            if (daysSinceUsed < 1) score += 15;
            else if (daysSinceUsed < 7) score += 10;
        }

        return score;
    }

    private fuzzyMatch(text: string, query: string): boolean {
        let textIndex = 0;
        let queryIndex = 0;

        while (textIndex < text.length && queryIndex < query.length) {
            if (text[textIndex].toLowerCase() === query[queryIndex].toLowerCase()) {
                queryIndex++;
            }
            textIndex++;
        }

        return queryIndex === query.length;
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
