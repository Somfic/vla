<script lang="ts">
  import api from "$lib/api";
  import Canvas from "$components/Canvas.svelte";
  import type { Graph, Node } from "$lib/core";

  let graph = $state<Graph | null>(null);
  let loading = $state(true);
  let error = $state<string | null>(null);
  let status = $state<string>("");

  async function handleAutoSave(updatedGraph: Graph) {
    graph = updatedGraph;
    try {
      await api.save_graph(updatedGraph, "../graph.json");
    } catch (e) {
      console.error("Auto-save failed:", e);
    }
  }

  async function addNode() {
    if (!graph) return;
    const newNode: Node = {
      id: (graph.nodes.length + 1).toString(),
      position: { x: Math.random() * 200 - 100, y: Math.random() * 200 - 100 },
      data: { label: `Node ${graph.nodes.length + 1}` },
      type: "default",
    };
    graph = {
      ...graph,
      nodes: [...graph.nodes, newNode],
    };
    await handleAutoSave(graph);
  }

  async function initApp() {
    try {
      graph = await api.load_graph("../graph.json");
    } catch (e) {
      // If loading from file fails, create empty graph
      graph = {
        nodes: [],
        edges: [],
      };
    } finally {
      loading = false;
    }
  }

  initApp();
</script>

<div class="controls">
  <button onclick={addNode}>Add Node</button>
  {#if status}
    <div class="status">{status}</div>
  {/if}
</div>

{#if loading}
  <p>Loading...</p>
{:else if error}
  <p>Error: {error}</p>
{:else if graph}
  <Canvas {graph} onSave={handleAutoSave} />
{/if}

<style>
  .controls {
    position: absolute;
    top: 10px;
    left: 10px;
    z-index: 1000;
    display: flex;
    gap: 10px;
  }

  button {
    padding: 8px 16px;
    background: #007acc;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
  }

  button:hover {
    background: #005a9e;
  }

  .status {
    padding: 8px 12px;
    background: #f0f0f0;
    border-radius: 4px;
    font-size: 14px;
    margin-top: 8px;
  }
</style>
