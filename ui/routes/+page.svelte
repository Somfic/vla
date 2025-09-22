<script lang="ts">
  import api from "$lib/api";
  import Canvas from "$components/canvas/Canvas.svelte";
  import type { Graph, Node } from "$lib/core";
  import SideBar from "$components/sidebar/SideBar.svelte";
  import MenuBar from "$components/menubar/MenuBar.svelte";
  import { SvelteFlowProvider } from "@xyflow/svelte";

  let graph = $state<Graph | null>(null);
  api.load_graph("../graph.json").then((g) => {
    graph = g;
    console.log("Graph loaded:", graph);
  });

  api.get_bricks().then((bricks) => {
    console.log("Available bricks:", bricks);
  });

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
      position: { x: 0, y: 0 },
      data: { brick_id: "testBrick", brick: null, arguments: {} },
      type: "vla",
    };
    graph = {
      ...graph,
      nodes: [...graph.nodes, newNode],
    };
    await handleAutoSave(graph);
  }
</script>

<main>
  <SvelteFlowProvider>
    <div class="menubar">
      <MenuBar />
    </div>

    <div class="content">
      {#if !graph}
        <p>Loading graph...</p>
      {:else}
        <Canvas {graph} onSave={handleAutoSave} />
      {/if}
    </div>

    <div class="sidebar">
      <SideBar />
    </div>
  </SvelteFlowProvider>
</main>

<div class="controls">
  <button onclick={addNode}>Add Node</button>
  {#if status}
    <div class="status">{status}</div>
  {/if}
</div>

<style lang="scss">
  @import "$styles/theme";

  main {
    display: flex;
  }

  .menubar,
  .sidebar {
    padding: $gap;
  }

  .content {
    display: flex;
    flex-grow: 1;
    background-color: $background-secondary;
    margin: $gap 0;
    overflow: hidden;
    border: 2px solid $border-color;
    border-radius: $border-radius2;
  }

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
    border-radius: $border-radius;
    cursor: pointer;
  }

  button:hover {
    background: $primary;
  }

  .status {
    padding: 8px 12px;
    background: #f0f0f0;
    border-radius: $border-radius;
    font-size: 14px;
    margin-top: 8px;
  }
</style>
