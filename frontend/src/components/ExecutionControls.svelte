<script lang="ts">
  import api from "$lib/api";
  import type { Graph, ExecutionMode, ExecutionStateUpdate, NodeExecutionState, ExecutionPhase } from "$lib/core";

  let { graph }: { graph: Graph } = $props();

  // Execution state tracking
  let nodeStates = $state<Map<string, NodeExecutionState>>(new Map());
  let isExecuting = $state(false);
  let executionMode = $state<ExecutionMode>("Normal");
  let totalNodes = $state(0);
  let completedNodes = $state(0);
  let error = $state<string | null>(null);
  let startTime = $state<number | null>(null);
  let elapsedSeconds = $state(0);

  // Timer for elapsed time
  let elapsedTimer: number | null = null;

  // Listen for node execution updates
  api.node_execution_updated.on((update: ExecutionStateUpdate) => {
    nodeStates.set(update.node_id, update.state);
    nodeStates = new Map(nodeStates); // Trigger reactivity

    // Update completed count
    completedNodes = Array.from(nodeStates.values()).filter(
      state => state.phase === "Completed"
    ).length;

    // Handle execution completion
    if (update.state.phase === "Completed" || update.state.phase === "Errored") {
      if (completedNodes >= totalNodes || update.state.phase === "Errored") {
        stopExecution();
      }
    }
  });

  function startElapsedTimer() {
    startTime = Date.now();
    elapsedSeconds = 0;
    elapsedTimer = setInterval(() => {
      if (startTime) {
        elapsedSeconds = Math.floor((Date.now() - startTime) / 1000);
      }
    }, 1000);
  }

  function stopElapsedTimer() {
    if (elapsedTimer) {
      clearInterval(elapsedTimer);
      elapsedTimer = null;
    }
  }

  function stopExecution() {
    isExecuting = false;
    stopElapsedTimer();
  }

  async function executeGraph(mode: ExecutionMode) {
    try {
      error = null;
      nodeStates.clear();
      nodeStates = new Map();
      completedNodes = 0;
      totalNodes = graph.nodes.length;
      executionMode = mode;
      isExecuting = true;

      startElapsedTimer();

      const result = await api.execute_graph(graph, mode);

      if (!result.success && result.error) {
        error = result.error;
      }
    } catch (e) {
      error = `Execution failed: ${e}`;
      console.error(e);
      stopExecution();
    }
  }

  async function startNormalExecution() {
    await executeGraph("Normal");
  }

  async function startSteppedExecution() {
    await executeGraph("Stepped");
  }

  function resetExecution() {
    stopExecution();
    nodeStates.clear();
    nodeStates = new Map();
    completedNodes = 0;
    error = null;
    elapsedSeconds = 0;
    startTime = null;
  }

  // Clean up timer on component destroy
  $effect(() => {
    return () => {
      stopElapsedTimer();
    };
  });

  // Reactive computed values
  const progress = $derived(totalNodes > 0 ? (completedNodes / totalNodes) * 100 : 0);
  const hasErrors = $derived(Array.from(nodeStates.values()).some(state => state.phase === "Errored"));
  const runningNodes = $derived(Array.from(nodeStates.values()).filter(state => state.phase === "Running").length);
  const queuedNodes = $derived(Array.from(nodeStates.values()).filter(state => state.phase === "Queued").length);
</script>

<div class="execution-controls">
  <div class="header">
    <h3>Execution Controls</h3>
    {#if elapsedSeconds > 0}
      <div class="elapsed-time">{elapsedSeconds}s</div>
    {/if}
  </div>

  <div class="buttons">
    {#if !isExecuting}
      <button onclick={startNormalExecution} class="start normal">
        ▶️ Run Normal
      </button>
      <button onclick={startSteppedExecution} class="start stepped">
        👣 Run Stepped
      </button>
    {:else}
      <button onclick={resetExecution} class="reset">
        ⏹️ Stop
      </button>
    {/if}
  </div>

  {#if error}
    <div class="status error">
      ❌ {error}
    </div>
  {/if}

  {#if isExecuting || completedNodes > 0}
    <div class="execution-status">
      <div class="progress-bar">
        <div class="progress-fill" style="width: {progress}%"></div>
        <div class="progress-text">
          {completedNodes} / {totalNodes} nodes
          {#if hasErrors}
            <span class="error-indicator">⚠️</span>
          {/if}
        </div>
      </div>

      {#if runningNodes > 0 || queuedNodes > 0}
        <div class="node-counts">
          {#if runningNodes > 0}
            <span class="running">🔄 {runningNodes} running</span>
          {/if}
          {#if queuedNodes > 0}
            <span class="queued">⏳ {queuedNodes} queued</span>
          {/if}
        </div>
      {/if}

      <div class="mode-indicator">
        Mode: {executionMode}
      </div>
    </div>
  {/if}

  {#if nodeStates.size > 0}
    <div class="node-states">
      <div class="states-header">Node States:</div>
      <div class="states-grid">
        {#each Array.from(nodeStates.entries()) as [nodeId, state]}
          <div class="node-state {state.phase.toLowerCase()}">
            <div class="node-id">{nodeId.slice(0, 8)}...</div>
            <div class="phase">{state.phase}</div>
            {#if state.elapsedMs > 0}
              <div class="elapsed">{state.elapsedMs}ms</div>
            {/if}
            {#if state.errorMessage}
              <div class="error-message" title={state.errorMessage}>
                ❌ {state.errorMessage.slice(0, 30)}...
              </div>
            {/if}
          </div>
        {/each}
      </div>
    </div>
  {/if}
</div>

<style lang="scss">
  @import "$styles/theme";

  .execution-controls {
    position: absolute;
    top: 10px;
    left: 10px;
    z-index: 1000;
    background: $background;
    padding: $gap;
    border-radius: $border-radius;
    border: $border;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    min-width: 280px;
    max-width: 400px;
  }

  .header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 12px;

    h3 {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
      color: #333;
    }

    .elapsed-time {
      font-size: 14px;
      font-weight: 500;
      color: #666;
      background: #f0f0f0;
      padding: 4px 8px;
      border-radius: 4px;
    }
  }

  .buttons {
    display: flex;
    gap: 8px;
    margin-bottom: 12px;
  }

  button {
    flex: 1;
    padding: 10px 16px;
    border: none;
    border-radius: $border-radius;
    cursor: pointer;
    font-size: 14px;
    font-weight: 500;
    transition: all 0.2s;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 6px;

    &:hover {
      transform: translateY(-1px);
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
    }

    &:active {
      transform: translateY(0);
    }
  }

  .start {
    &.normal {
      background: #28a745;
      color: white;

      &:hover {
        background: #218838;
      }
    }

    &.stepped {
      background: #007bff;
      color: white;

      &:hover {
        background: #0056b3;
      }
    }
  }

  .reset {
    background: #dc3545;
    color: white;

    &:hover {
      background: #c82333;
    }
  }

  .status {
    margin-bottom: 12px;
    padding: 8px 12px;
    border-radius: $border-radius;
    font-size: 13px;

    &.error {
      background: #f8d7da;
      color: #721c24;
      border: 1px solid #f5c6cb;
    }
  }

  .execution-status {
    margin-bottom: 12px;
  }

  .progress-bar {
    position: relative;
    background: #e9ecef;
    border-radius: 4px;
    height: 24px;
    overflow: hidden;
    margin-bottom: 8px;

    .progress-fill {
      background: linear-gradient(90deg, #28a745, #20c997);
      height: 100%;
      transition: width 0.3s ease;
    }

    .progress-text {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      font-size: 12px;
      font-weight: 500;
      color: #333;
      display: flex;
      align-items: center;
      gap: 4px;

      .error-indicator {
        color: #dc3545;
      }
    }
  }

  .node-counts {
    display: flex;
    gap: 12px;
    margin-bottom: 6px;
    font-size: 12px;

    .running {
      color: #007bff;
      font-weight: 500;
    }

    .queued {
      color: #6c757d;
      font-weight: 500;
    }
  }

  .mode-indicator {
    font-size: 12px;
    color: #6c757d;
    font-weight: 500;
  }

  .node-states {
    .states-header {
      font-size: 12px;
      font-weight: 600;
      color: #333;
      margin-bottom: 8px;
    }

    .states-grid {
      display: grid;
      grid-template-columns: 1fr;
      gap: 4px;
      max-height: 200px;
      overflow-y: auto;
    }

    .node-state {
      padding: 6px 8px;
      border-radius: 4px;
      font-size: 11px;
      border-left: 3px solid transparent;

      .node-id {
        font-weight: 500;
        color: #333;
      }

      .phase {
        font-size: 10px;
        text-transform: uppercase;
        font-weight: 600;
        margin-top: 2px;
      }

      .elapsed {
        font-size: 10px;
        color: #6c757d;
        margin-top: 2px;
      }

      .error-message {
        font-size: 10px;
        color: #dc3545;
        margin-top: 2px;
        font-weight: 500;
      }

      &.waiting {
        background: #f8f9fa;
        border-left-color: #6c757d;
        .phase { color: #6c757d; }
      }

      &.queued {
        background: #fff3cd;
        border-left-color: #ffc107;
        .phase { color: #856404; }
      }

      &.running {
        background: #cce5ff;
        border-left-color: #007bff;
        .phase { color: #004085; }
      }

      &.completed {
        background: #d4edda;
        border-left-color: #28a745;
        .phase { color: #155724; }
      }

      &.errored {
        background: #f8d7da;
        border-left-color: #dc3545;
        .phase { color: #721c24; }
      }
    }
  }
</style>