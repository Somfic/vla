<script lang="ts">
  import api from "$lib/api";
  import Canvas from "$components/canvas/Canvas.svelte";
  import type { Graph } from "$lib/core";
  import SideBar from "$components/sidebar/SideBar.svelte";
  import MenuBar from "$components/menubar/MenuBar.svelte";
  import { SvelteFlowProvider } from "@xyflow/svelte";
  import Spotlight from "$components/Spotlight.svelte";
  import Shortcuts, { type ShortcutConfig } from "$components/Shortcuts.svelte";

  let graph = $state<Graph | null>(null);
  api.load_graph("../graph.json").then((g) => {
    graph = g;
  });

  api.graph_updated.on((updatedGraph) => {
    graph = updatedGraph;
  });

  async function handleAutoSave(updatedGraph: Graph) {
    graph = updatedGraph;
    try {
      console.log("Auto-saving graph...", updatedGraph);
      await api.save_graph(updatedGraph, "../graph.json");
    } catch (e) {
      console.error("Auto-save failed:", e);
    }
  }

  let showSpotlight = $state(false);

  let shortcuts: ShortcutConfig[] = [
    {
      key: "space",
      options: { context: "global" },
      handler: () => {
        showSpotlight = true;
      },
    },
  ];
</script>

<Shortcuts {shortcuts} />
<Spotlight onClose={() => (showSpotlight = false)} open={showSpotlight} />

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
