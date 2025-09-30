import type { Graph } from "$lib/core";
import api from "$lib/api";

class GraphStore {
  #graph = $state<Graph | null>(null);
  #isLoading = $state(true);
  #error = $state<string | null>(null);
  #isInitialLoad = true;

  constructor() {
    this.loadGraph();
    this.setupApiListeners();
  }

  setupAutoSave() {
    $effect(() => {
      if (this.#graph && !this.#isLoading) {
        if (this.#isInitialLoad) {
          this.#isInitialLoad = false;
          return;
        }
        console.log("Graph changed, auto-saving...");
        this.saveGraph(this.#graph);
      }
    });
  }

  get graph() {
    return this.#graph;
  }

  set graph(newGraph: Graph | null) {
    this.#graph = newGraph;
  }

  get isLoading() {
    return this.#isLoading;
  }

  get error() {
    return this.#error;
  }

  private async loadGraph() {
    try {
      this.#isLoading = true;
      this.#error = null;
      const graph = await api.load_graph("../graph.json");
      this.#graph = graph;
    } catch (e) {
      this.#error = e instanceof Error ? e.message : "Failed to load graph";
      console.error("Failed to load graph:", e);
    } finally {
      this.#isLoading = false;
    }
  }

  private setupApiListeners() {
    api.graph_updated.on((updatedGraph) => {
      this.#graph = updatedGraph;
    });
  }

  async saveGraph(graph: Graph) {
    try {
      await api.save_graph(graph, "../graph.json");
    } catch (e) {
      console.error("Auto-save failed:", e);
    }
  }
}

export const graphStore = new GraphStore();
