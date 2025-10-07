<script lang="ts">
  import api from "$lib/api";
  import type { Graph, ExecutionStep, ExecutionState } from "$lib/core";

  let { graph }: { graph: Graph } = $props();

  let executionState = $state<ExecutionState | null>(null);
  let currentStep = $state<ExecutionStep | null>(null);
  let executionSteps = $state<ExecutionStep[]>([]);
  let isExecuting = $state(false);
  let error = $state<string | null>(null);

  async function startExecution() {
    try {
      error = null;
      executionSteps = [];
      currentStep = null;

      executionState = await api.start_execution(graph);
      isExecuting = true;
    } catch (e) {
      error = `Failed to start: ${e}`;
      console.error(e);
    }
  }

  async function stepExecution() {
    try {
      error = null;

      const step = await api.step_execution();
      currentStep = step;
      executionSteps = [...executionSteps, step];

      if (!step.success) {
        error = step.error || "Execution failed";
        isExecuting = false;
      }
    } catch (e) {
      // Execution complete
      isExecuting = false;
      error = null;
      currentStep = null;
    }
  }

  async function runToEnd() {
    while (isExecuting) {
      await stepExecution();
      // Small delay to see the steps
      await new Promise((resolve) => setTimeout(resolve, 100));
    }
  }

  async function resetExecution() {
    try {
      await api.reset_execution();
      executionState = null;
      currentStep = null;
      executionSteps = [];
      isExecuting = false;
      error = null;
    } catch (e) {
      error = `Failed to reset: ${e}`;
      console.error(e);
    }
  }
</script>

<div class="execution-controls">
  <div class="buttons">
    {#if !isExecuting}
      <button onclick={startExecution} class="start">Start</button>
    {:else}
      <button onclick={stepExecution} class="step">Step</button>
      <button onclick={runToEnd} class="run">Run to End</button>
      <button onclick={resetExecution} class="reset">Reset</button>
    {/if}
  </div>

  {#if error}
    <div class="status error">{error}</div>
  {/if}

  {#if currentStep}
    <div class="status success">
      Last: {currentStep.node_id}
      {#if !currentStep.success}
        <span class="error-text">❌ {currentStep.error}</span>
      {:else}
        <span class="success-text">✓</span>
      {/if}
    </div>
  {/if}

  {#if executionSteps.length > 0}
    <div class="steps-count">Steps executed: {executionSteps.length}</div>
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
  }

  .buttons {
    display: flex;
    gap: 8px;
  }

  button {
    padding: 8px 16px;
    border: none;
    border-radius: $border-radius;
    cursor: pointer;
    font-size: 14px;
    font-weight: 500;
    transition: all 0.2s;

    &:hover {
      transform: translateY(-1px);
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
    }

    &:active {
      transform: translateY(0);
    }
  }

  .start {
    background: #28a745;
    color: white;

    &:hover {
      background: #218838;
    }
  }

  .step {
    background: #007bff;
    color: white;

    &:hover {
      background: #0056b3;
    }
  }

  .run {
    background: #17a2b8;
    color: white;

    &:hover {
      background: #138496;
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
    margin-top: 8px;
    padding: 8px 12px;
    border-radius: $border-radius;
    font-size: 13px;

    &.error {
      background: #f8d7da;
      color: #721c24;
      border: 1px solid #f5c6cb;
    }

    &.success {
      background: #d4edda;
      color: #155724;
      border: 1px solid #c3e6cb;
    }
  }

  .error-text {
    color: #dc3545;
    font-weight: 500;
  }

  .success-text {
    color: #28a745;
    font-weight: 500;
  }

  .steps-count {
    margin-top: 6px;
    font-size: 12px;
    color: #6c757d;
  }
</style>
