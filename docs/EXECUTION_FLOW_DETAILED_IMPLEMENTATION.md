# VLA Execution Flow Implementation Plan

## Overview

This document provides a comprehensive implementation plan for adding execution flow control to VLA's visual programming system. The execution flow model separates **data connections** (what values flow between nodes) from **execution connections** (which nodes actually run and in what order).

## Architecture Decisions

### Core Design Philosophy

1. **Dual Connection Types**:
   - Data connections: Blue circular handles, connect outputs to inputs for data flow
   - Execution connections: Green triangular handles, control when and which bricks execute

2. **Execution as Metadata**:
   - Execution inputs/outputs are declared as attributes above the function, not as function parameters
   - This keeps function signatures clean and focused on data processing logic

3. **Declarative Execution Control**:
   - Use `trigger!("output_id")` macro calls within brick functions to activate execution outputs
   - Thread-local storage collects triggers after function execution

4. **Big Loop Execution Model**:
   - Main execution loop processes one node per step
   - Lazy data dependency resolution - only execute data nodes when needed by active execution path

5. **Edge Type Inference**:
   - Cannot modify third-party Edge struct
   - Infer connection type from handle naming convention: `exec_*` vs `data_*`

## Current Architecture Analysis

### What We Have Now
- **Data-only system**: Nodes store brick configurations and arguments
- **Static connections**: Edges only represent data flow relationships
- **No execution**: Graphs are purely descriptive, not executable
- **Simple brick system**: Functions with inputs/outputs but no execution control
- **Flow type exists**: `ConnectionType::Flow` exists in types but unused

### What We Need
- **Execution metadata**: Additional fields on Brick struct for execution inputs/outputs
- **Execution engine**: Interpreter that follows execution flow and evaluates nodes
- **Conditional execution**: Nodes that control which execution paths are taken
- **Visual distinction**: UI that clearly shows data vs execution connections
- **Mixed execution model**: Some nodes participate in execution flow, others are pure data processors

## Detailed Implementation Plan

### Phase 1: Core Infrastructure (Week 1-2)

#### 1.1 Update Brick Type System

**File**: `core/src/bricks/types.rs`

Add execution fields to existing Brick struct:

```rust
#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct Brick {
    pub id: String,
    pub label: String,
    pub description: String,
    pub keywords: Vec<String>,
    pub category: String,
    pub arguments: Vec<BrickArgument>,
    pub inputs: Vec<BrickInput>,
    pub outputs: Vec<BrickOutput>,
    pub execution_inputs: Vec<BrickExecutionInput>,   // NEW
    pub execution_outputs: Vec<BrickExecutionOutput>, // NEW
    #[serde(skip, default = "default_execution_fn")]
    pub execution: fn(Vec<BrickArgument>, Vec<BrickInput>) -> Vec<BrickOutput>,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickExecutionInput {
    pub id: String,
    pub label: String,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickExecutionOutput {
    pub id: String,
    pub label: String,
}
```

#### 1.2 Implement `trigger!()` Macro

**File**: `core/src/bricks/execution.rs` (new file)

```rust
use std::cell::RefCell;

// Thread-local storage for execution triggers
thread_local! {
    static EXECUTION_TRIGGERS: RefCell<Vec<String>> = RefCell::new(Vec::new());
}

/// Macro to trigger execution outputs from within brick functions
///
/// Usage: `trigger!("output_id");`
#[macro_export]
macro_rules! trigger {
    ($output_id:expr) => {
        $crate::bricks::execution::add_trigger($output_id);
    };
}

/// Internal function called by trigger! macro
pub fn add_trigger(output_id: &str) {
    EXECUTION_TRIGGERS.with(|triggers| {
        triggers.borrow_mut().push(output_id.to_string());
    });
}

/// Collect and clear all triggers set during brick execution
/// Called by ExecutionEngine after each brick execution
pub fn collect_and_clear_triggers() -> Vec<String> {
    EXECUTION_TRIGGERS.with(|triggers| {
        triggers.borrow_mut().drain(..).collect()
    })
}

/// Clear triggers without collecting (for cleanup)
pub fn clear_triggers() {
    EXECUTION_TRIGGERS.with(|triggers| {
        triggers.borrow_mut().clear();
    });
}
```

#### 1.3 Extend Brick Macro

**File**: `core/src/bricks/macros.rs`

Add support for execution attributes in the main brick macro:

```rust
// Add new patterns for execution attributes
// Pattern with execution inputs and outputs
(
    #[id($id:expr)]
    $(#[label($label:expr)])?
    $(#[description($description:expr)])?
    $(#[keywords($keywords:expr)])?
    #[category($category:expr)]
    $(#[execution_input($exec_input_id:expr)])*
    $(#[execution_output($exec_output_id:expr)])*
    fn $fn_name:ident(
        $($(#[$param_attr:ident$(($($param_attr_content:tt)*))? ])+ $param_name:ident: $param_type:ident $(= $default:expr)?),*
    ) -> (
        $($(#[$output_attr:ident$(($($output_attr_content:tt)*))? ])+ $output_type:ident),+
    )
    $body:block
) => {
    // Generate brick with execution metadata
    // ... (detailed macro implementation)
};

// Helper macros for execution metadata
(@add_execution_inputs $exec_vec:ident, [$($exec_input_id:expr),*]) => {
    $(
        $exec_vec.push(crate::bricks::types::BrickExecutionInput {
            id: $exec_input_id.to_string(),
            label: $exec_input_id.to_string(), // TODO: Allow custom labels
        });
    )*
};

(@add_execution_outputs $exec_vec:ident, [$($exec_output_id:expr),*]) => {
    $(
        $exec_vec.push(crate::bricks::types::BrickExecutionOutput {
            id: $exec_output_id.to_string(),
            label: $exec_output_id.to_string(), // TODO: Allow custom labels
        });
    )*
};
```

#### 1.4 Handle Generation Strategy

**Convention**: Generate handles with prefixes to distinguish types:
- **Execution handles**: `exec_{input/output_id}`
- **Data handles**: `data_{input/output_id}`

Example for if/else brick:
- Execution input: `exec_execute`
- Execution outputs: `exec_true_branch`, `exec_false_branch`
- Data input: `data_condition`
- Data output: `data_result`

### Phase 2: Execution Engine (Week 2-3)

#### 2.1 Core Execution Engine

**File**: `core/src/execution/engine.rs` (new file)

```rust
use std::collections::{HashMap, HashSet, VecDeque};
use crate::{Graph, Node, Edge};
use crate::bricks::execution::{collect_and_clear_triggers, clear_triggers};

pub struct ExecutionEngine {
    graph: Graph,
    execution_queue: VecDeque<String>,           // Nodes waiting to execute
    data_cache: HashMap<String, HashMap<String, String>>, // node_id -> output_id -> value
    execution_state: HashMap<String, NodeExecutionState>,
    step_count: u64,
}

#[derive(Debug, Clone, PartialEq)]
pub enum NodeExecutionState {
    NotStarted,
    Executing,
    Completed,
    Error(String),
}

impl ExecutionEngine {
    pub fn new(graph: Graph) -> Self {
        let mut engine = Self {
            graph,
            execution_queue: VecDeque::new(),
            data_cache: HashMap::new(),
            execution_state: HashMap::new(),
            step_count: 0,
        };

        // Initialize all nodes to NotStarted
        for node in &engine.graph.nodes {
            engine.execution_state.insert(node.id.clone(), NodeExecutionState::NotStarted);
        }

        engine
    }

    /// Start execution from nodes that have no execution inputs (start nodes)
    pub fn start_execution(&mut self) {
        let start_nodes = self.find_start_nodes();
        for node_id in start_nodes {
            self.execution_queue.push_back(node_id);
        }
    }

    /// Execute one step of the execution flow
    /// Returns true if execution should continue, false if finished
    pub fn execute_step(&mut self) -> Result<bool, String> {
        if let Some(node_id) = self.execution_queue.pop_front() {
            self.execute_node(&node_id)?;
            self.step_count += 1;
            Ok(!self.execution_queue.is_empty())
        } else {
            Ok(false) // No more nodes to execute
        }
    }

    /// Execute a complete graph until no more nodes are queued
    pub fn execute_full(&mut self) -> Result<ExecutionResult, String> {
        self.start_execution();

        while self.execute_step()? {
            // Continue until finished
            if self.step_count > 10000 {
                return Err("Execution exceeded maximum steps - possible infinite loop".to_string());
            }
        }

        Ok(ExecutionResult {
            final_outputs: self.data_cache.clone(),
            step_count: self.step_count,
            success: true,
        })
    }

    fn execute_node(&mut self, node_id: &str) -> Result<(), String> {
        // Clear any previous triggers
        clear_triggers();

        // 1. Resolve data dependencies for this node
        self.resolve_data_dependencies(node_id)?;

        // 2. Get the node and its brick
        let node = self.graph.nodes.iter()
            .find(|n| n.id == node_id)
            .ok_or_else(|| format!("Node {} not found", node_id))?;

        let brick = node.data.brick.as_ref()
            .ok_or_else(|| format!("Node {} has no brick", node_id))?;

        // 3. Prepare inputs and arguments
        let brick_inputs = self.prepare_brick_inputs(node, brick)?;
        let brick_arguments = self.prepare_brick_arguments(node, brick)?;

        // 4. Execute the brick function
        self.execution_state.insert(node_id.to_string(), NodeExecutionState::Executing);

        let outputs = (brick.execution)(brick_arguments, brick_inputs);

        // 5. Cache the outputs
        let mut output_cache = HashMap::new();
        for (i, output) in outputs.iter().enumerate() {
            let output_id = if i < brick.outputs.len() {
                brick.outputs[i].id.clone()
            } else {
                format!("output_{}", i)
            };
            output_cache.insert(output_id, format!("{:?}", output)); // TODO: Better serialization
        }
        self.data_cache.insert(node_id.to_string(), output_cache);

        // 6. Collect execution triggers and queue next nodes
        let triggered_outputs = collect_and_clear_triggers();
        self.queue_triggered_nodes(node_id, &triggered_outputs)?;

        // 7. Mark node as completed
        self.execution_state.insert(node_id.to_string(), NodeExecutionState::Completed);

        Ok(())
    }

    fn resolve_data_dependencies(&mut self, node_id: &str) -> Result<(), String> {
        // Find all data edges that target this node
        let data_edges: Vec<_> = self.graph.edges.iter()
            .filter(|edge| {
                edge.target == node_id &&
                self.is_data_edge(edge)
            })
            .collect();

        // For each required input, ensure the source node has been executed
        for edge in data_edges {
            if !self.data_cache.contains_key(&edge.source) {
                // Source hasn't been executed yet - execute it now
                self.execute_data_path(&edge.source)?;
            }
        }

        Ok(())
    }

    fn execute_data_path(&mut self, node_id: &str) -> Result<(), String> {
        // Recursively execute all data dependencies
        self.resolve_data_dependencies(node_id)?;

        // Execute this data node if not already executed
        if !self.data_cache.contains_key(node_id) {
            let node = self.graph.nodes.iter()
                .find(|n| n.id == node_id)
                .ok_or_else(|| format!("Node {} not found", node_id))?;

            let brick = node.data.brick.as_ref()
                .ok_or_else(|| format!("Node {} has no brick", node_id))?;

            let brick_inputs = self.prepare_brick_inputs(node, brick)?;
            let brick_arguments = self.prepare_brick_arguments(node, brick)?;

            let outputs = (brick.execution)(brick_arguments, brick_inputs);

            // Cache outputs
            let mut output_cache = HashMap::new();
            for (i, output) in outputs.iter().enumerate() {
                let output_id = if i < brick.outputs.len() {
                    brick.outputs[i].id.clone()
                } else {
                    format!("output_{}", i)
                };
                output_cache.insert(output_id, format!("{:?}", output));
            }
            self.data_cache.insert(node_id.to_string(), output_cache);
        }

        Ok(())
    }

    fn queue_triggered_nodes(&mut self, source_node_id: &str, triggered_outputs: &[String]) -> Result<(), String> {
        for trigger_id in triggered_outputs {
            // Find execution edges from this trigger
            let source_handle = format!("exec_{}", trigger_id);

            for edge in &self.graph.edges {
                if edge.source == source_node_id &&
                   edge.source_handle.as_deref() == Some(&source_handle) &&
                   self.is_execution_edge(edge) {

                    // Queue the target node for execution
                    self.execution_queue.push_back(edge.target.clone());
                }
            }
        }

        Ok(())
    }

    fn find_start_nodes(&self) -> Vec<String> {
        // Find nodes that have execution outputs but no execution inputs
        self.graph.nodes.iter()
            .filter(|node| {
                if let Some(brick) = &node.data.brick {
                    !brick.execution_outputs.is_empty() && brick.execution_inputs.is_empty()
                } else {
                    false
                }
            })
            .map(|node| node.id.clone())
            .collect()
    }

    fn is_execution_edge(&self, edge: &Edge) -> bool {
        // Infer from handle names
        edge.source_handle.as_ref().map_or(false, |h| h.starts_with("exec_")) ||
        edge.target_handle.as_ref().map_or(false, |h| h.starts_with("exec_"))
    }

    fn is_data_edge(&self, edge: &Edge) -> bool {
        !self.is_execution_edge(edge)
    }

    // TODO: Implement prepare_brick_inputs, prepare_brick_arguments methods
}

#[derive(Debug, Clone)]
pub struct ExecutionResult {
    pub final_outputs: HashMap<String, HashMap<String, String>>,
    pub step_count: u64,
    pub success: bool,
}
```

#### 2.2 API Integration

**File**: `core/src/lib.rs`

Add execution endpoints to the API:

```rust
use crate::execution::engine::{ExecutionEngine, ExecutionResult};

#[taurpc::procedures(export_to = "../ui/lib/core.ts")]
pub trait Api {
    // ... existing methods
    async fn execute_graph(graph: Graph) -> Result<ExecutionResult, String>;
    async fn execute_graph_step(graph: Graph, step_count: u64) -> Result<(ExecutionResult, bool), String>;
}

#[taurpc::resolvers]
impl Api for ApiImpl {
    // ... existing implementations

    async fn execute_graph(self, graph: Graph) -> Result<ExecutionResult, String> {
        let mut engine = ExecutionEngine::new(graph);
        engine.execute_full()
    }

    async fn execute_graph_step(self, graph: Graph, step_count: u64) -> Result<(ExecutionResult, bool), String> {
        let mut engine = ExecutionEngine::new(graph);

        for _ in 0..step_count {
            if !engine.execute_step()? {
                break; // Execution finished
            }
        }

        let result = ExecutionResult {
            final_outputs: engine.data_cache.clone(),
            step_count: engine.step_count,
            success: true,
        };

        let has_more = !engine.execution_queue.is_empty();
        Ok((result, has_more))
    }
}
```

### Phase 3: UI Updates (Week 3-4)

#### 3.1 Enhanced Node Component

**File**: `ui/components/canvas/Node.svelte`

Update to show separate sections for execution vs data handles:

```svelte
<script lang="ts">
    import { Handle, Position } from "@xyflow/svelte";
    import type { NodeProps } from "@xyflow/svelte";
    import type { NodeData } from "$lib/api";

    type $$Props = NodeProps<NodeData>;

    export let id: $$Props["id"];
    export let data: $$Props["data"];
    export let selected: $$Props["selected"] = false;

    $: brick = data.brick;

    function getHandleStyle(type: 'execution' | 'data') {
        const baseStyle = "width: 12px; height: 12px; border: 2px solid;";

        if (type === 'execution') {
            return baseStyle + `
                border-radius: 0;
                transform: rotate(45deg);
                border-color: #4CAF50;
                background: #4CAF50;
            `;
        } else {
            return baseStyle + `
                border-radius: 50%;
                border-color: #2196F3;
                background: #2196F3;
            `;
        }
    }
</script>

<div class="vla-node" class:selected>
    <div class="node-header">
        <h3>{brick?.label || data.brick_id}</h3>
    </div>

    <div class="node-body">
        <!-- Execution Inputs (top left) -->
        {#if brick?.execution_inputs && brick.execution_inputs.length > 0}
            <div class="execution-inputs">
                {#each brick.execution_inputs as execInput}
                    <div class="handle-group">
                        <Handle
                            type="target"
                            position={Position.Left}
                            id="exec_{execInput.id}"
                            style={getHandleStyle('execution')}
                        />
                        <label class="exec-label">{execInput.label}</label>
                    </div>
                {/each}
            </div>
        {/if}

        <!-- Data Inputs (middle left) -->
        {#if brick?.inputs && brick.inputs.length > 0}
            <div class="data-inputs">
                {#each brick.inputs as input}
                    <div class="handle-group">
                        <Handle
                            type="target"
                            position={Position.Left}
                            id="data_{input.id}"
                            style={getHandleStyle('data')}
                        />
                        <label class="data-label">{input.label}</label>
                    </div>
                {/each}
            </div>
        {/if}

        <!-- Arguments (center) -->
        {#if brick?.arguments && brick.arguments.length > 0}
            <div class="arguments">
                {#each brick.arguments as arg}
                    <div class="argument">
                        <label>{arg.label}:</label>
                        <input
                            type={arg.type === 'number' ? 'number' :
                                  arg.type === 'boolean' ? 'checkbox' : 'text'}
                            bind:value={data.arguments[arg.id]}
                        />
                    </div>
                {/each}
            </div>
        {/if}

        <!-- Data Outputs (middle right) -->
        {#if brick?.outputs && brick.outputs.length > 0}
            <div class="data-outputs">
                {#each brick.outputs as output}
                    <div class="handle-group">
                        <label class="data-label">{output.label}</label>
                        <Handle
                            type="source"
                            position={Position.Right}
                            id="data_{output.id}"
                            style={getHandleStyle('data')}
                        />
                    </div>
                {/each}
            </div>
        {/if}

        <!-- Execution Outputs (bottom right) -->
        {#if brick?.execution_outputs && brick.execution_outputs.length > 0}
            <div class="execution-outputs">
                {#each brick.execution_outputs as execOutput}
                    <div class="handle-group">
                        <label class="exec-label">{execOutput.label}</label>
                        <Handle
                            type="source"
                            position={Position.Right}
                            id="exec_{execOutput.id}"
                            style={getHandleStyle('execution')}
                        />
                    </div>
                {/each}
            </div>
        {/if}
    </div>
</div>

<style>
    .vla-node {
        background: white;
        border: 2px solid #ddd;
        border-radius: 8px;
        padding: 12px;
        min-width: 200px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .vla-node.selected {
        border-color: #007acc;
        box-shadow: 0 0 0 2px rgba(0, 122, 204, 0.3);
    }

    .node-header h3 {
        margin: 0;
        font-size: 14px;
        font-weight: bold;
        text-align: center;
        color: #333;
    }

    .node-body {
        display: flex;
        flex-direction: column;
        gap: 8px;
        margin-top: 8px;
    }

    .handle-group {
        display: flex;
        align-items: center;
        gap: 8px;
        margin: 4px 0;
        font-size: 12px;
    }

    .execution-inputs .handle-group,
    .execution-outputs .handle-group {
        border-top: 1px solid #4CAF50;
        padding-top: 4px;
    }

    .exec-label {
        color: #4CAF50;
        font-weight: bold;
    }

    .data-label {
        color: #2196F3;
    }

    .arguments {
        padding: 8px;
        background: #f9f9f9;
        border-radius: 4px;
    }

    .argument {
        display: flex;
        flex-direction: column;
        gap: 4px;
        margin: 4px 0;
    }

    .argument input {
        padding: 4px 8px;
        border: 1px solid #ccc;
        border-radius: 4px;
        font-size: 12px;
    }
</style>
```

#### 3.2 Execution Controls

**File**: `ui/components/canvas/ExecutionControls.svelte` (new file)

```svelte
<script lang="ts">
    import api from "$lib/api";
    import type { Graph, ExecutionResult } from "$lib/core";

    export let graph: Graph;

    let isExecuting = false;
    let executionResult: ExecutionResult | null = null;
    let error: string | null = null;

    async function executeGraph() {
        if (isExecuting) return;

        isExecuting = true;
        error = null;

        try {
            executionResult = await api().execute_graph(graph);
        } catch (e) {
            error = e instanceof Error ? e.message : String(e);
        } finally {
            isExecuting = false;
        }
    }

    async function stepExecute() {
        if (isExecuting) return;

        isExecuting = true;
        error = null;

        try {
            const [result, hasMore] = await api().execute_graph_step(graph, 1);
            executionResult = result;
        } catch (e) {
            error = e instanceof Error ? e.message : String(e);
        } finally {
            isExecuting = false;
        }
    }

    function clearResults() {
        executionResult = null;
        error = null;
    }
</script>

<div class="execution-controls">
    <div class="buttons">
        <button on:click={executeGraph} disabled={isExecuting} class="execute-btn">
            {isExecuting ? "Executing..." : "▶ Execute"}
        </button>

        <button on:click={stepExecute} disabled={isExecuting} class="step-btn">
            ⏭ Step
        </button>

        <button on:click={clearResults} class="clear-btn">
            🗑 Clear
        </button>
    </div>

    {#if error}
        <div class="error">
            <strong>Error:</strong> {error}
        </div>
    {/if}

    {#if executionResult}
        <div class="results">
            <h4>Execution Result</h4>
            <p><strong>Steps:</strong> {executionResult.step_count}</p>
            <p><strong>Success:</strong> {executionResult.success}</p>

            {#if Object.keys(executionResult.final_outputs).length > 0}
                <details>
                    <summary>Final Outputs</summary>
                    <pre>{JSON.stringify(executionResult.final_outputs, null, 2)}</pre>
                </details>
            {/if}
        </div>
    {/if}
</div>

<style>
    .execution-controls {
        padding: 12px;
        background: #f5f5f5;
        border-bottom: 1px solid #ddd;
        display: flex;
        flex-direction: column;
        gap: 12px;
    }

    .buttons {
        display: flex;
        gap: 8px;
    }

    button {
        padding: 8px 16px;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-weight: bold;
        font-size: 14px;
    }

    .execute-btn {
        background: #4CAF50;
        color: white;
    }

    .step-btn {
        background: #FF9800;
        color: white;
    }

    .clear-btn {
        background: #f44336;
        color: white;
    }

    button:disabled {
        background: #ccc;
        cursor: not-allowed;
    }

    .error {
        padding: 8px;
        background: #ffebee;
        border: 1px solid #f44336;
        border-radius: 4px;
        color: #d32f2f;
    }

    .results {
        padding: 8px;
        background: #e8f5e8;
        border: 1px solid #4CAF50;
        border-radius: 4px;
        font-size: 12px;
    }

    .results h4 {
        margin: 0 0 8px 0;
        color: #2e7d32;
    }

    .results p {
        margin: 4px 0;
    }

    pre {
        background: white;
        padding: 8px;
        border-radius: 4px;
        overflow-x: auto;
        font-size: 11px;
    }
</style>
```

### Phase 4: Control Flow Bricks (Week 4)

#### 4.1 If/Else Brick

**File**: `core/src/bricks/control_flow.rs` (new file)

```rust
use crate::bricks::macros::brick;
use crate::prelude::*;
use crate::trigger;

pub fn all_bricks() -> Vec<Brick> {
    vec![
        if_else_brick(),
        start_brick(),
        end_brick(),
        merge_brick(),
    ]
}

brick! {
    #[id("if_else")]
    #[label("If/Else")]
    #[description("Conditional execution - runs true branch if condition is true, false branch otherwise")]
    #[category("Control Flow")]
    #[execution_input("execute")]
    #[execution_output("true_branch")]
    #[execution_output("false_branch")]
    fn if_else(
        #[input] #[label("Condition")] condition: bool
    ) -> (
        #[label("Condition Value")] bool
    ) {
        if condition {
            trigger!("true_branch");
        } else {
            trigger!("false_branch");
        }

        (condition,)
    }
}

brick! {
    #[id("start")]
    #[label("Start")]
    #[description("Starts execution flow - use this to begin your program")]
    #[category("Control Flow")]
    #[execution_output("begin")]
    fn start() -> (
        #[label("Started")] bool
    ) {
        trigger!("begin");
        (true,)
    }
}

brick! {
    #[id("end")]
    #[label("End")]
    #[description("Ends execution flow - use this to terminate your program")]
    #[category("Control Flow")]
    #[execution_input("execute")]
    fn end(
        #[input] #[label("Final Value")] value: String = "completed"
    ) -> (
        #[label("Result")] String
    ) {
        // No trigger!() call - execution ends here
        (value,)
    }
}

brick! {
    #[id("merge")]
    #[label("Merge")]
    #[description("Merges multiple execution paths into one")]
    #[category("Control Flow")]
    #[execution_input("execute")]
    #[execution_output("continue")]
    fn merge() -> (
        #[label("Merged")] bool
    ) {
        trigger!("continue");
        (true,)
    }
}
```

#### 4.2 Update Module Registration

**File**: `core/src/bricks/mod.rs`

```rust
pub mod arithmetics;
pub mod boolean_logic;
pub mod constants;
pub mod control_flow;  // NEW
pub mod execution;     // NEW
pub mod macros;

#[cfg(test)]
mod tests;
pub mod types;

use crate::prelude::*;

pub fn all_bricks() -> Vec<Brick> {
    let mut bricks = vec![];
    bricks.extend(arithmetics::all_bricks());
    bricks.extend(boolean_logic::all_bricks());
    bricks.extend(constants::all_bricks());
    bricks.extend(control_flow::all_bricks());  // NEW
    bricks
}
```

## Example Usage Patterns

### Simple Linear Flow
```
[Start] → [Process Data] → [Log Result] → [End]
```

### Conditional Branch
```
[Start] → [If/Else] → [True: Process A] → [Merge]
                   └→ [False: Process B] → [Merge] → [End]
```

### Data Dependencies
```
[Pure Data: Add Numbers] ← [Start] → [Log Sum] → [End]
     ↑ (data flow)               ↑ (execution flow)
```

## Testing Strategy

### Unit Tests
1. **Macro generation**: Test brick macro generates correct execution metadata
2. **Trigger collection**: Test `trigger!()` macro and collection functions
3. **Engine logic**: Test execution ordering, data dependency resolution
4. **Edge inference**: Test handle-based edge type detection

### Integration Tests
1. **Simple execution**: Linear flow with data dependencies
2. **Conditional execution**: If/else with both branches
3. **Complex flows**: Multiple conditions, merges, loops
4. **Error handling**: Invalid graphs, missing data, infinite loops

### Example Test Graph
```rust
#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_if_else_execution() {
        // Create a graph: Start → If/Else → (True: Add | False: Subtract) → End
        let graph = create_test_graph();
        let mut engine = ExecutionEngine::new(graph);

        let result = engine.execute_full().unwrap();

        assert!(result.success);
        assert_eq!(result.step_count, 4); // Start, If/Else, Math op, End
    }
}
```

## Migration Strategy

### Backward Compatibility
- Existing bricks continue to work as pure data processors
- No execution attributes = no participation in execution flow
- Existing graphs load and function normally (data-only mode)

### Gradual Adoption
1. **Phase 1**: Add execution flow to new graphs only
2. **Phase 2**: Users can optionally add Start/End nodes to existing graphs
3. **Phase 3**: Full execution flow becomes the standard workflow

## Future Extensions

### Advanced Control Flow
- **While/For loops**: Conditional repetition
- **Break/Continue**: Loop control
- **Try/Catch**: Error handling in execution flow
- **Parallel execution**: Multiple execution paths simultaneously

### Debugging Features
- **Breakpoints**: Pause execution at specific nodes
- **Variable inspection**: View data values during execution
- **Execution visualization**: Highlight active execution path
- **Performance profiling**: Execution time per node

### External Integration
- **Webhooks**: HTTP triggers start execution
- **Timers**: Schedule-based execution
- **File watchers**: File system event triggers
- **Database triggers**: Data change notifications

This comprehensive plan provides the foundation for implementing execution flow while maintaining the simplicity and flexibility of the current VLA system.