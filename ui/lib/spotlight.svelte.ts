import { commands } from "./commands.svelte";

class SpotlightStore {
    isOpen = $state(false);
    query = $state('');
    selectedIndex = $state(0);

    // Derived state for search results
    searchResults = $derived(() => {
        const results = commands.search(this.query);
        // Reset selection if results changed
        if (this.selectedIndex >= results.length) {
            this.selectedIndex = 0;
        }
        return results;
    });

    open(): void {
        this.isOpen = true;
        this.query = '';
        this.selectedIndex = 0;
    }

    close(): void {
        this.isOpen = false;
        this.query = '';
        this.selectedIndex = 0;
    }

    setQuery(query: string): void {
        this.query = query;
        this.selectedIndex = 0; // Reset selection on new query
    }

    navigateUp(): void {
        if (this.selectedIndex > 0) {
            this.selectedIndex--;
        }
    }

    navigateDown(): void {
        if (this.selectedIndex < this.searchResults.length - 1) {
            this.selectedIndex++;
        }
    }

    async executeSelected(): Promise<void> {
        const results = this.searchResults();
        const selected = results[this.selectedIndex];
        if (selected) {
            await commands.execute(selected.id);
            this.close();
        }
    }

    selectIndex(index: number): void {
        this.selectedIndex = Math.max(0, Math.min(index, this.searchResults.length - 1));
    }
}

export const spotlight = new SpotlightStore();
