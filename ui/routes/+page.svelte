<script lang="ts">
  import Canvas from "$components/canvas/Canvas.svelte";
  import SideBar from "$components/sidebar/SideBar.svelte";
  import MenuBar from "$components/menubar/MenuBar.svelte";
  import { SvelteFlowProvider } from "@xyflow/svelte";
  import Spotlight from "$components/Spotlight.svelte";
  import Shortcuts, { type ShortcutConfig } from "$components/Shortcuts.svelte";
  import { graphStore } from "$lib/graph.svelte";

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

  $effect(() => {
    console.log(graphStore.graph);
  });

  // Set up auto-save in the store
  graphStore.setupAutoSave();
</script>

<Shortcuts {shortcuts} />
<Spotlight onClose={() => (showSpotlight = false)} open={showSpotlight} />

<main>
  <SvelteFlowProvider>
    <div class="menubar">
      <MenuBar />
    </div>

    <div class="content">
      {#if graphStore.isLoading}
        <p>Loading graph...</p>
      {:else if graphStore.error}
        <p>Error: {graphStore.error}</p>
      {:else if graphStore.graph}
        <Canvas bind:graph={graphStore.graph} />
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
    border: $border;
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
