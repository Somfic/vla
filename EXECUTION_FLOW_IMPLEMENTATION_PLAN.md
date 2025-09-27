# VLA Execution Flow Implementation Plan

## Overview

This document provides a comprehensive implementation plan for adding execution flow control to VLA's visual programming system. The execution flow model separates **data connections** (what values flow between nodes) from **execution connections** (which nodes actually run and in what order).

## Current State Analysis

### What We Have Now
- **Data-only system**: Nodes store brick configurations and arguments
- **Static connections**: Edges only represent data flow relationships
- **No execution**: Graphs are purely descriptive, not executable
- **Simple brick system**: Functions with inputs/outputs but no execution control

### What We Need
- **Dual connection types**: Data connections + execution flow connections
- **Execution engine**: Interpreter that follows execution flow and evaluates nodes
- **Conditional execution**: Nodes that control which execution paths are taken
- **Visual distinction**: UI that clearly shows data vs execution connections

## Core Architecture Changes

### 1. Enhanced Edge System

#### Current Edge Structure
```rust
#[taurpc::ipc_type]
pub struct Edge {
    pub id: String,
    pub source: String,
    pub target: String,
}
```

#### New Enhanced Edge Structure
```rust
#[taurpc::ipc_type]
pub struct Edge {
    pub id: String,
    pub source: String,
    pub target: String,
    pub edge_type: EdgeType,
}

#[taurpc::ipc_type]
pub enum EdgeType {
    // Data connection: from output handle to input handle
    Data {
        source_output: String,  // e.g., "result", "sum", "output_0"
        target_input: String,   // e.g., "a", "b", "input_1"
    },
    // Execution flow: from execution output to execution input
    ExecutionFlow {
        source_trigger: Option<String>,  // e.g., "true_branch", "false_branch", None for default
        target_trigger: Option<String>,  // e.g., "execute", None for default
    },
}
```

### 2. Enhanced Brick System

#### Current Brick Structure
```rust
pub struct Brick {
    pub id: String,
    pub label: String,
    pub description: String,
    pub arguments: Vec<BrickArgument>,
    pub inputs: Vec<BrickInput>,
    pub outputs: Vec<BrickOutput>,
    pub execution: fn(Vec<BrickArgument>, Vec<BrickInput>) -> Vec<BrickOutput>,
}
```

#### New Enhanced Brick Structure
```rust
pub struct Brick {
    pub id: String,
    pub label: String,
    pub description: String,
    pub arguments: Vec<BrickArgument>,
    pub inputs: Vec<BrickInput>,
    pub outputs: Vec<BrickOutput>,
    pub execution_inputs: Vec<BrickExecutionInput>,   // NEW
    pub execution_outputs: Vec<BrickExecutionOutput>, // NEW
    pub execution: fn(Vec<BrickArgument>, Vec<BrickInput>) -> BrickExecutionResult,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickExecutionInput {
    pub id: String,
    pub label: String,
    pub is_required: bool,  // If true, node won't execute without this trigger
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickExecutionOutput {
    pub id: String,
    pub label: String,
    pub condition: Option<String>,  // Optional condition for when this trigger fires
}

pub struct BrickExecutionResult {
    pub data_outputs: Vec<BrickOutput>,
    pub execution_signals: Vec<String>,  // Which execution outputs to trigger
}
```

### 3. Node Execution State

```rust
#[derive(Debug, Clone)]
pub struct NodeExecutionState {
    pub node_id: String,
    pub status: ExecutionStatus,
    pub data_cache: HashMap<String, String>,  // Cached output values
    pub pending_triggers: HashSet<String>,    // Execution inputs waiting for signals
    pub execution_count: u64,                 // How many times this node has executed
}

#[derive(Debug, Clone, PartialEq)]
pub enum ExecutionStatus {
    NotStarted,
    WaitingForTrigger,
    WaitingForData,
    Executing,
    Completed,
    Error(String),
}
```

## Detailed Implementation Plan

### Phase 1: Core Infrastructure (Week 1-2)

#### Step 1.1: Update Type Definitions

**File**: `core/src/lib.rs`

```rust
// Add to existing structs
#[taurpc::ipc_type]
pub struct Edge {
    pub id: String,
    pub source: String,
    pub target: String,
    pub edge_type: EdgeType,
}

#[taurpc::ipc_type]
pub enum EdgeType {
    Data {
        source_output: String,
        target_input: String,
    },
    ExecutionFlow {
        source_trigger: Option<String>,
        target_trigger: Option<String>,
    },
}

// Add execution context to API
#[taurpc::procedures(export_to = "../ui/lib/core.ts")]
pub trait Api {
    // ... existing methods
    async fn execute_graph(graph: Graph, start_node_ids: Vec<String>) -> Result<ExecutionResult, String>;
    async fn step_execute_graph(graph: Graph, execution_state: ExecutionState) -> Result<ExecutionStepResult, String>;
}

#[taurpc::ipc_type]
pub struct ExecutionResult {
    pub final_outputs: HashMap<String, HashMap<String, String>>,  // node_id -> output_id -> value
    pub execution_trace: Vec<ExecutionStep>,
    pub success: bool,
    pub error: Option<String>,
}

#[taurpc::ipc_type]
pub struct ExecutionStep {
    pub node_id: String,
    pub timestamp: u64,
    pub inputs_received: HashMap<String, String>,
    pub outputs_produced: HashMap<String, String>,
    pub execution_signals_sent: Vec<String>,
}

#[taurpc::ipc_type]
pub struct ExecutionState {
    pub nodes: HashMap<String, NodeExecutionState>,
    pub pending_signals: Vec<ExecutionSignal>,
    pub step_count: u64,
}

#[taurpc::ipc_type]
pub struct ExecutionSignal {
    pub from_node: String,
    pub to_node: String,
    pub trigger_id: Option<String>,
    pub timestamp: u64,
}

#[taurpc::ipc_type]
pub struct ExecutionStepResult {
    pub new_state: ExecutionState,
    pub completed_steps: Vec<ExecutionStep>,
    pub is_finished: bool,
}
```

#### Step 1.2: Create Execution Engine

**File**: `core/src/execution/mod.rs`

```rust
use std::collections::{HashMap, HashSet, VecDeque};
use crate::{Graph, Node, Edge, EdgeType, ExecutionResult, ExecutionState, ExecutionStep};

pub struct ExecutionEngine {
    graph: Graph,
    state: ExecutionState,
    data_cache: HashMap<String, HashMap<String, String>>,  // node_id -> output_id -> value
}

impl ExecutionEngine {
    pub fn new(graph: Graph, start_nodes: Vec<String>) -> Self {
        let mut state = ExecutionState {
            nodes: HashMap::new(),
            pending_signals: Vec::new(),
            step_count: 0,
        };

        // Initialize all nodes to NotStarted
        for node in &graph.nodes {
            state.nodes.insert(node.id.clone(), NodeExecutionState {
                node_id: node.id.clone(),
                status: ExecutionStatus::NotStarted,
                data_cache: HashMap::new(),
                pending_triggers: HashSet::new(),
                execution_count: 0,
            });
        }

        // Send initial execution signals to start nodes
        for start_node_id in start_nodes {
            state.pending_signals.push(ExecutionSignal {
                from_node: "system".to_string(),
                to_node: start_node_id,
                trigger_id: None,  // Default trigger
                timestamp: 0,
            });
        }

        Self {
            graph,
            state,
            data_cache: HashMap::new(),
        }
    }

    pub fn execute_full(&mut self) -> ExecutionResult {
        let mut execution_trace = Vec::new();
        let mut step_count = 0;
        const MAX_STEPS: u64 = 10000;  // Prevent infinite loops

        while !self.state.pending_signals.is_empty() && step_count < MAX_STEPS {
            match self.execute_step() {
                Ok(step_result) => {
                    execution_trace.extend(step_result.completed_steps);
                    self.state = step_result.new_state;
                    if step_result.is_finished {
                        break;
                    }
                }
                Err(e) => {
                    return ExecutionResult {
                        final_outputs: self.collect_outputs(),
                        execution_trace,
                        success: false,
                        error: Some(e),
                    };
                }
            }
            step_count += 1;
        }

        ExecutionResult {
            final_outputs: self.collect_outputs(),
            execution_trace,
            success: step_count < MAX_STEPS,
            error: if step_count >= MAX_STEPS {
                Some("Execution exceeded maximum steps - possible infinite loop".to_string())
            } else {
                None
            },
        }
    }

    pub fn execute_step(&mut self) -> Result<ExecutionStepResult, String> {
        if self.state.pending_signals.is_empty() {
            return Ok(ExecutionStepResult {
                new_state: self.state.clone(),
                completed_steps: Vec::new(),
                is_finished: true,
            });
        }

        let mut completed_steps = Vec::new();
        let signal = self.state.pending_signals.remove(0);  // FIFO execution

        // Find the target node
        let target_node = self.graph.nodes.iter()
            .find(|n| n.id == signal.to_node)
            .ok_or_else(|| format!("Target node {} not found", signal.to_node))?;

        // Get node's execution state
        let node_state = self.state.nodes.get_mut(&signal.to_node)
            .ok_or_else(|| format!("Node state for {} not found", signal.to_node))?;

        // Mark trigger as received
        if let Some(trigger_id) = &signal.trigger_id {
            node_state.pending_triggers.insert(trigger_id.clone());
        } else {
            node_state.pending_triggers.insert("default".to_string());
        }

        // Check if node is ready to execute
        if self.is_node_ready_to_execute(target_node)? {
            let step = self.execute_node(target_node)?;
            completed_steps.push(step);
        }

        self.state.step_count += 1;

        Ok(ExecutionStepResult {
            new_state: self.state.clone(),
            completed_steps,
            is_finished: self.state.pending_signals.is_empty(),
        })
    }

    fn is_node_ready_to_execute(&self, node: &Node) -> Result<bool, String> {
        let brick = node.data.brick.as_ref()
            .ok_or_else(|| format!("Node {} has no brick data", node.id))?;

        // Check if all required execution inputs have been triggered
        for exec_input in &brick.execution_inputs {
            if exec_input.is_required {
                let node_state = self.state.nodes.get(&node.id).unwrap();
                if !node_state.pending_triggers.contains(&exec_input.id) {
                    return Ok(false);
                }
            }
        }

        // Check if all required data inputs are available
        for input in &brick.inputs {
            if !self.is_data_input_satisfied(node, &input.id)? {
                return Ok(false);
            }
        }

        Ok(true)
    }

    fn is_data_input_satisfied(&self, node: &Node, input_id: &str) -> Result<bool, String> {
        // Look for data edges that provide this input
        for edge in &self.graph.edges {
            if let EdgeType::Data { source_output, target_input } = &edge.edge_type {
                if edge.target == node.id && target_input == input_id {
                    // Check if source node has produced this output
                    if let Some(cached_data) = self.data_cache.get(&edge.source) {
                        if cached_data.contains_key(source_output) {
                            return Ok(true);
                        }
                    }
                    return Ok(false);  // Input connected but data not available
                }
            }
        }

        // If no connection, check if input has a default value
        let brick = node.data.brick.as_ref().unwrap();
        let input = brick.inputs.iter().find(|i| i.id == input_id).unwrap();
        Ok(input.default_value.is_some() || node.data.arguments.contains_key(input_id))
    }

    fn execute_node(&mut self, node: &Node) -> Result<ExecutionStep, String> {
        let brick = node.data.brick.as_ref().unwrap();

        // Gather input values
        let mut input_values = HashMap::new();
        for input in &brick.inputs {
            let value = self.resolve_input_value(node, &input.id)?;
            input_values.insert(input.id.clone(), value);
        }

        // Convert to brick input format
        let brick_inputs: Vec<crate::bricks::types::BrickInput> = brick.inputs.iter().map(|input| {
            crate::bricks::types::BrickInput {
                id: input.id.clone(),
                label: input.label.clone(),
                r#type: input.r#type.clone(),
                default_value: input_values.get(&input.id).cloned(),
            }
        }).collect();

        let brick_arguments: Vec<crate::bricks::types::BrickArgument> = brick.arguments.iter().map(|arg| {
            let value = node.data.arguments.get(&arg.id)
                .or(arg.default_value.as_ref())
                .cloned();

            crate::bricks::types::BrickArgument {
                id: arg.id.clone(),
                label: arg.label.clone(),
                r#type: arg.r#type.clone(),
                enum_options: arg.enum_options.clone(),
                default_value: value,
            }
        }).collect();

        // Execute the brick
        let outputs = (brick.execution)(brick_arguments, brick_inputs);

        // Cache output values
        let mut output_values = HashMap::new();
        for (i, output) in outputs.iter().enumerate() {
            output_values.insert(
                brick.outputs.get(i).map(|o| o.id.clone())
                    .unwrap_or_else(|| format!("output_{}", i)),
                format!("{:?}", output),  // TODO: Better serialization
            );
        }

        self.data_cache.insert(node.id.clone(), output_values.clone());

        // Send execution signals to downstream nodes
        let execution_signals = self.send_execution_signals(node)?;

        // Update node state
        let node_state = self.state.nodes.get_mut(&node.id).unwrap();
        node_state.status = ExecutionStatus::Completed;
        node_state.execution_count += 1;
        node_state.pending_triggers.clear();

        Ok(ExecutionStep {
            node_id: node.id.clone(),
            timestamp: self.state.step_count,
            inputs_received: input_values,
            outputs_produced: output_values,
            execution_signals_sent: execution_signals,
        })
    }

    fn resolve_input_value(&self, node: &Node, input_id: &str) -> Result<String, String> {
        // First check if it's connected via data edge
        for edge in &self.graph.edges {
            if let EdgeType::Data { source_output, target_input } = &edge.edge_type {
                if edge.target == node.id && target_input == input_id {
                    if let Some(cached_data) = self.data_cache.get(&edge.source) {
                        if let Some(value) = cached_data.get(source_output) {
                            return Ok(value.clone());
                        }
                    }
                    return Err(format!("Data not available from upstream node"));
                }
            }
        }

        // Check if it's provided as an argument
        if let Some(value) = node.data.arguments.get(input_id) {
            return Ok(value.clone());
        }

        // Use default value
        let brick = node.data.brick.as_ref().unwrap();
        let input = brick.inputs.iter().find(|i| i.id == input_id).unwrap();
        if let Some(default) = &input.default_value {
            Ok(default.clone())
        } else {
            Err(format!("No value available for input {}", input_id))
        }
    }

    fn send_execution_signals(&mut self, node: &Node) -> Result<Vec<String>, String> {
        let mut signals_sent = Vec::new();

        // For now, send default execution signal to all downstream execution flow edges
        for edge in &self.graph.edges {
            if let EdgeType::ExecutionFlow { source_trigger, target_trigger } = &edge.edge_type {
                if edge.source == node.id {
                    // TODO: Implement conditional execution based on source_trigger
                    // For now, always send signal
                    self.state.pending_signals.push(ExecutionSignal {
                        from_node: node.id.clone(),
                        to_node: edge.target.clone(),
                        trigger_id: target_trigger.clone(),
                        timestamp: self.state.step_count,
                    });

                    signals_sent.push(target_trigger.clone().unwrap_or_else(|| "default".to_string()));
                }
            }
        }

        Ok(signals_sent)
    }

    fn collect_outputs(&self) -> HashMap<String, HashMap<String, String>> {
        self.data_cache.clone()
    }
}

#[derive(Debug, Clone, PartialEq)]
pub enum ExecutionStatus {
    NotStarted,
    WaitingForTrigger,
    WaitingForData,
    Executing,
    Completed,
    Error(String),
}

#[derive(Debug, Clone)]
pub struct NodeExecutionState {
    pub node_id: String,
    pub status: ExecutionStatus,
    pub data_cache: HashMap<String, String>,
    pub pending_triggers: HashSet<String>,
    pub execution_count: u64,
}
```

#### Step 1.3: Update API Implementation

**File**: `core/src/lib.rs` (Add to ApiImpl)

```rust
use crate::execution::ExecutionEngine;

#[taurpc::resolvers]
impl Api for ApiImpl {
    // ... existing methods

    async fn execute_graph(self, graph: Graph, start_node_ids: Vec<String>) -> Result<ExecutionResult, String> {
        let mut engine = ExecutionEngine::new(graph, start_node_ids);
        Ok(engine.execute_full())
    }

    async fn step_execute_graph(self, graph: Graph, execution_state: ExecutionState) -> Result<ExecutionStepResult, String> {
        let mut engine = ExecutionEngine::new(graph, Vec::new());
        engine.set_state(execution_state);
        engine.execute_step()
    }
}
```

### Phase 2: Conditional Execution Bricks (Week 2-3)

#### Step 2.1: Update Brick Macro for Execution Control

**File**: `core/src/bricks/macros.rs`

Add support for execution inputs/outputs in the brick macro:

```rust
// Add new attribute types for execution control
#[derive(Debug, Clone)]
pub enum BrickAttribute {
    Input,
    Output,
    Argument,
    ExecutionInput,
    ExecutionOutput,
    Label(String),
    Id(String),
    Description(String),
    Required(bool),
    Condition(String),
}

// Update macro to support execution attributes
brick! {
    #[id("if_branch")]
    #[label("If Branch")]
    #[description("Executes different paths based on condition")]
    fn if_branch(
        #[execution_input] #[label("Execute")] execute: (),
        #[input] #[label("Condition")] condition: bool
    ) -> (
        #[execution_output] #[label("True Branch")] #[condition("condition")] (),
        #[execution_output] #[label("False Branch")] #[condition("!condition")] (),
        #[output] #[label("Condition Value")] bool
    ) {
        // Return the condition value as data output
        (condition,)
    }
}
```

#### Step 2.2: Implement Core Conditional Bricks

**File**: `core/src/bricks/control_flow.rs`

```rust
use crate::bricks::macros::brick;

// Basic branching brick
brick! {
    #[id("if_branch")]
    #[label("If Branch")]
    #[description("Executes true or false branch based on condition")]
    fn if_branch(
        #[input] #[label("Condition")] condition: bool
    ) -> (
        #[label("Condition Value")] bool
    ) {
        (condition,)
    }
}

// Execution merge brick
brick! {
    #[id("merge")]
    #[label("Merge")]
    #[description("Merges multiple execution paths into one")]
    fn merge() -> (
        #[label("Continue")] bool
    ) {
        (true,)
    }
}

// Start node - begins execution
brick! {
    #[id("start")]
    #[label("Start")]
    #[description("Starts graph execution")]
    fn start() -> (
        #[label("Started")] bool
    ) {
        (true,)
    }
}

// End node - terminates execution path
brick! {
    #[id("end")]
    #[label("End")]
    #[description("Ends graph execution")]
    fn end(
        #[input] #[label("Value")] value: String = "completed"
    ) -> (
        #[label("Final Value")] String
    ) {
        (value,)
    }
}

// While loop brick
brick! {
    #[id("while_loop")]
    #[label("While Loop")]
    #[description("Repeats execution while condition is true")]
    fn while_loop(
        #[input] #[label("Condition")] condition: bool,
        #[input] #[label("Loop Body")] body_result: String = ""
    ) -> (
        #[label("Should Continue")] bool,
        #[label("Current Value")] String
    ) {
        (condition, body_result)
    }
}
```

### Phase 3: UI Updates (Week 3-4)

#### Step 3.1: Update Edge Types in UI

**File**: `ui/lib/core.ts` (This will be auto-generated by Specta)

The types will automatically include the new `EdgeType` enum.

#### Step 3.2: Enhanced Node Component

**File**: `ui/components/canvas/Node.svelte`

```svelte
<script lang="ts">
    import { Handle, Position, type NodeProps } from "@xyflow/svelte";
    import type { NodeData, Brick } from "$lib/core";

    type $$Props = NodeProps<NodeData>;

    export let id: $$Props["id"];
    export let data: $$Props["data"];
    export let selected: $$Props["selected"] = false;

    $: brick = data.brick;

    // Helper to determine handle style
    function getHandleStyle(type: 'data' | 'execution', isSource: boolean) {
        const baseStyle = "width: 12px; height: 12px; border: 2px solid #333;";

        if (type === 'execution') {
            // Triangular handles for execution flow
            return baseStyle + `
                border-radius: 0;
                transform: rotate(45deg);
                background: ${isSource ? '#4CAF50' : '#FF9800'};
            `;
        } else {
            // Circular handles for data flow
            return baseStyle + `
                border-radius: 50%;
                background: ${isSource ? '#2196F3' : '#9C27B0'};
            `;
        }
    }
</script>

<div class="vla-node" class:selected>
    <div class="node-header">
        <h3>{brick?.label || data.brick_id}</h3>
    </div>

    <div class="node-body">
        <!-- Execution Inputs (left side, top) -->
        {#if brick?.execution_inputs}
            <div class="execution-inputs">
                {#each brick.execution_inputs as execInput}
                    <div class="handle-group execution-input">
                        <Handle
                            type="target"
                            position={Position.Left}
                            id="exec_in_{execInput.id}"
                            style={getHandleStyle('execution', false)}
                        />
                        <label>{execInput.label}</label>
                    </div>
                {/each}
            </div>
        {/if}

        <!-- Data Inputs (left side, middle) -->
        {#if brick?.inputs}
            <div class="data-inputs">
                {#each brick.inputs as input}
                    <div class="handle-group data-input">
                        <Handle
                            type="target"
                            position={Position.Left}
                            id="data_in_{input.id}"
                            style={getHandleStyle('data', false)}
                        />
                        <label>{input.label}</label>
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
                            type={arg.type === 'Number' ? 'number' :
                                  arg.type === 'Boolean' ? 'checkbox' : 'text'}
                            bind:value={data.arguments[arg.id]}
                            placeholder={arg.default_value || ""}
                        />
                    </div>
                {/each}
            </div>
        {/if}

        <!-- Data Outputs (right side, middle) -->
        {#if brick?.outputs}
            <div class="data-outputs">
                {#each brick.outputs as output}
                    <div class="handle-group data-output">
                        <label>{output.label}</label>
                        <Handle
                            type="source"
                            position={Position.Right}
                            id="data_out_{output.id}"
                            style={getHandleStyle('data', true)}
                        />
                    </div>
                {/each}
            </div>
        {/if}

        <!-- Execution Outputs (right side, bottom) -->
        {#if brick?.execution_outputs}
            <div class="execution-outputs">
                {#each brick.execution_outputs as execOutput}
                    <div class="handle-group execution-output">
                        <label>{execOutput.label}</label>
                        <Handle
                            type="source"
                            position={Position.Right}
                            id="exec_out_{execOutput.id}"
                            style={getHandleStyle('execution', true)}
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
        display: grid;
        grid-template-columns: auto 1fr auto;
        grid-template-rows: auto auto auto auto;
        gap: 8px;
        margin-top: 8px;
    }

    .execution-inputs { grid-column: 1; grid-row: 1; }
    .data-inputs { grid-column: 1; grid-row: 2; }
    .arguments { grid-column: 2; grid-row: 2; }
    .data-outputs { grid-column: 3; grid-row: 2; }
    .execution-outputs { grid-column: 3; grid-row: 3; }

    .handle-group {
        display: flex;
        align-items: center;
        gap: 8px;
        margin: 4px 0;
        font-size: 12px;
    }

    .execution-input, .data-input {
        justify-content: flex-start;
    }

    .execution-output, .data-output {
        justify-content: flex-end;
    }

    .arguments {
        padding: 0 16px;
    }

    .argument {
        display: flex;
        flex-direction: column;
        gap: 4px;
        margin: 4px 0;
    }

    .argument label {
        font-size: 11px;
        font-weight: bold;
        color: #666;
    }

    .argument input {
        padding: 4px 8px;
        border: 1px solid #ccc;
        border-radius: 4px;
        font-size: 12px;
    }

    /* Execution flow visual distinction */
    .execution-inputs, .execution-outputs {
        border-top: 2px solid #4CAF50;
        padding-top: 4px;
    }

    .execution-input label, .execution-output label {
        color: #4CAF50;
        font-weight: bold;
    }

    .data-input label, .data-output label {
        color: #2196F3;
    }
</style>
```

#### Step 3.3: Enhanced Canvas with Execution Controls

**File**: `ui/components/canvas/Canvas.svelte`

```svelte
<script lang="ts">
    import {
        SvelteFlow,
        Controls,
        Background,
        type Node,
        type Edge,
        type Connection,
    } from "@xyflow/svelte";
    import "@xyflow/svelte/dist/base.css";
    import VlaNode from "./Node.svelte";
    import api, { type VlaNode } from "$lib/api";
    import type { EdgeType } from "$lib/core";

    export let nodes: VlaNode[];
    export let edges: Edge[];

    // Add execution controls
    let isExecuting = false;
    let executionResult: any = null;

    const nodeTypes = {
        vla: VlaNode,
    };

    async function executeGraph() {
        isExecuting = true;
        try {
            // Find start nodes (nodes with no incoming execution edges)
            const startNodes = nodes
                .filter(node => !edges.some(edge =>
                    edge.target === node.id &&
                    edge.data?.edge_type?.ExecutionFlow
                ))
                .map(node => node.id);

            const graph = {
                nodes: nodes.map(node => ({
                    id: node.id,
                    position: node.position,
                    data: node.data,
                    type: node.type || "vla"
                })),
                edges: edges.map(edge => ({
                    id: edge.id,
                    source: edge.source,
                    target: edge.target,
                    edge_type: edge.data?.edge_type || {
                        Data: {
                            source_output: edge.sourceHandle?.replace("data_out_", "") || "result",
                            target_input: edge.targetHandle?.replace("data_in_", "") || "input"
                        }
                    }
                }))
            };

            executionResult = await api().execute_graph(graph, startNodes);
            console.log("Execution result:", executionResult);
        } catch (error) {
            console.error("Execution failed:", error);
        } finally {
            isExecuting = false;
        }
    }

    function onConnect(connection: Connection) {
        // Determine edge type based on handle types
        const sourceHandle = connection.sourceHandle || "";
        const targetHandle = connection.targetHandle || "";

        let edgeType: EdgeType;

        if (sourceHandle.startsWith("exec_") || targetHandle.startsWith("exec_")) {
            // Execution flow connection
            edgeType = {
                ExecutionFlow: {
                    source_trigger: sourceHandle.startsWith("exec_out_")
                        ? sourceHandle.replace("exec_out_", "")
                        : null,
                    target_trigger: targetHandle.startsWith("exec_in_")
                        ? targetHandle.replace("exec_in_", "")
                        : null
                }
            };
        } else {
            // Data connection
            edgeType = {
                Data: {
                    source_output: sourceHandle.replace("data_out_", "") || "result",
                    target_input: targetHandle.replace("data_in_", "") || "input"
                }
            };
        }

        const newEdge: Edge = {
            id: `${connection.source}-${connection.target}-${Date.now()}`,
            source: connection.source!,
            target: connection.target!,
            sourceHandle: connection.sourceHandle,
            targetHandle: connection.targetHandle,
            data: { edge_type: edgeType }
        };

        edges = [...edges, newEdge];
    }
</script>

<div class="canvas-container">
    <div class="execution-controls">
        <button
            on:click={executeGraph}
            disabled={isExecuting}
            class="execute-button"
        >
            {isExecuting ? "Executing..." : "Execute Graph"}
        </button>

        {#if executionResult}
            <div class="execution-result">
                <h4>Execution Result:</h4>
                <p>Success: {executionResult.success}</p>
                {#if executionResult.error}
                    <p class="error">Error: {executionResult.error}</p>
                {/if}
                <details>
                    <summary>Execution Trace ({executionResult.execution_trace.length} steps)</summary>
                    <pre>{JSON.stringify(executionResult.execution_trace, null, 2)}</pre>
                </details>
            </div>
        {/if}
    </div>

    <SvelteFlow
        {nodes}
        {edges}
        {nodeTypes}
        {onConnect}
        fitView
    >
        <Controls />
        <Background />
    </SvelteFlow>
</div>

<style>
    .canvas-container {
        height: 100vh;
        display: flex;
        flex-direction: column;
    }

    .execution-controls {
        padding: 12px;
        background: #f5f5f5;
        border-bottom: 1px solid #ddd;
        display: flex;
        gap: 16px;
        align-items: flex-start;
    }

    .execute-button {
        background: #4CAF50;
        color: white;
        border: none;
        padding: 8px 16px;
        border-radius: 4px;
        cursor: pointer;
        font-weight: bold;
    }

    .execute-button:disabled {
        background: #ccc;
        cursor: not-allowed;
    }

    .execution-result {
        flex: 1;
        font-size: 12px;
    }

    .execution-result h4 {
        margin: 0 0 8px 0;
        color: #333;
    }

    .error {
        color: #f44336;
        font-weight: bold;
    }

    .canvas-container :global(.svelte-flow) {
        flex: 1;
    }

    /* Style execution flow edges differently */
    .canvas-container :global(.svelte-flow__edge[data-edge-type="ExecutionFlow"]) {
        stroke: #4CAF50;
        stroke-width: 3px;
    }

    .canvas-container :global(.svelte-flow__edge[data-edge-type="ExecutionFlow"] .svelte-flow__edge-path) {
        stroke: #4CAF50;
        stroke-width: 3px;
        marker-end: url(#execution-arrow);
    }

    .canvas-container :global(.svelte-flow__edge[data-edge-type="Data"]) {
        stroke: #2196F3;
        stroke-width: 2px;
    }
</style>
```

### Phase 4: Testing and Validation (Week 4-5)

#### Step 4.1: Create Test Graphs

Create example graphs that test different execution patterns:

1. **Simple Linear Flow**: Start → Math → End
2. **Conditional Branch**: Start → If → (True Path | False Path) → Merge → End
3. **Loop Flow**: Start → While → Body → (Continue | Exit) → End
4. **Complex Flow**: Multiple conditions, nested branches

#### Step 4.2: Add Debug Features

- Step-by-step execution mode
- Node highlighting during execution
- Data flow visualization
- Execution trace viewer

### Phase 5: Advanced Features (Week 5-6)

#### Step 5.1: Loop Control Bricks

```rust
brick! {
    #[id("for_loop")]
    #[label("For Loop")]
    fn for_loop(
        #[input] start: i32 = 0,
        #[input] end: i32 = 10,
        #[input] step: i32 = 1,
        #[input] current: i32 = 0
    ) -> (
        #[label("Index")] i32,
        #[label("Should Continue")] bool
    ) {
        let next = current + step;
        (current, next < end)
    }
}

brick! {
    #[id("break")]
    #[label("Break")]
    fn break_loop() -> (#[label("Broken")] bool) {
        (true,)
    }
}

brick! {
    #[id("continue")]
    #[label("Continue")]
    fn continue_loop() -> (#[label("Continued")] bool) {
        (true,)
    }
}
```

#### Step 5.2: Error Handling

```rust
brick! {
    #[id("try_catch")]
    #[label("Try-Catch")]
    fn try_catch(
        #[input] success_value: String = "",
        #[input] error_occurred: bool = false
    ) -> (
        #[label("Result")] String,
        #[label("Has Error")] bool
    ) {
        (success_value, error_occurred)
    }
}
```

## Implementation Timeline

### Week 1-2: Core Infrastructure
- [ ] Update type definitions (EdgeType, execution structures)
- [ ] Implement basic ExecutionEngine
- [ ] Add API endpoints for execution
- [ ] Basic execution flow (no conditionals yet)

### Week 2-3: Conditional Bricks
- [ ] Update brick macro for execution control
- [ ] Implement if_branch, merge, start, end bricks
- [ ] Add conditional execution logic to engine
- [ ] Test basic conditional flows

### Week 3-4: UI Updates
- [ ] Update Node component with execution handles
- [ ] Add execution controls to Canvas
- [ ] Implement edge type detection
- [ ] Visual distinction for execution vs data flow

### Week 4-5: Testing & Polish
- [ ] Create comprehensive test cases
- [ ] Add debugging features
- [ ] Performance optimization
- [ ] Error handling improvements

### Week 5-6: Advanced Features
- [ ] Loop control bricks
- [ ] Error handling bricks
- [ ] Complex execution patterns
- [ ] Documentation and examples

## Success Criteria

1. **Functional Execution**: Graphs execute correctly following both data and execution flow
2. **Conditional Branching**: False branches do not execute
3. **Visual Clarity**: Users can clearly distinguish data vs execution connections
4. **Performance**: Execution completes in reasonable time for complex graphs
5. **Debugging**: Users can step through execution and see intermediate results
6. **Error Handling**: Graceful handling of execution errors and infinite loops

## Risk Mitigation

### Technical Risks
- **Complexity**: Start with simple linear execution, add features incrementally
- **Performance**: Profile execution engine, optimize hot paths
- **Memory**: Implement proper cleanup of execution state

### User Experience Risks
- **Confusion**: Provide clear visual distinction between connection types
- **Learning Curve**: Create tutorial graphs and documentation
- **Debugging**: Add step-by-step execution mode

### Implementation Risks
- **Scope Creep**: Focus on core execution first, advanced features later
- **Testing**: Create automated tests for execution engine
- **Migration**: Ensure backward compatibility with existing graphs

## Future Extensions

After successful implementation:

1. **Parallel Execution**: Multiple execution paths running simultaneously
2. **Event-Driven Execution**: Nodes that respond to external events
3. **Subgraphs**: Reusable graph components
4. **Distributed Execution**: Execution across multiple machines
5. **Real-time Execution**: Streaming data processing

This execution flow system provides the foundation for a full-featured visual programming environment while maintaining the simplicity and usability that makes VLA appealing to users.
