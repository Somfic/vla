# VLA Engine Execution Model

## Overview

The VLA engine implements a **sequential flow execution model** with automatic data dependency resolution. Key features:

1. **Sequential Flow Execution**: Flow nodes execute one at a time based on execution flow
2. **Lazy Data Evaluation**: Data nodes are only executed when needed by flow nodes
3. **Single-Node Stepping**: Each iteration executes exactly one node (data or flow)
4. **Automatic DFS Resolution**: Data dependencies are resolved depth-first automatically

## Key Concepts

### Data Nodes vs Flow Nodes

- **Data Nodes**: Pure data processors with no `execution_inputs` or `execution_outputs`. They compute values and are executed lazily when their data is needed.

- **Flow Nodes**: Have `execution_inputs` and/or `execution_outputs`. They control the execution flow using the `trigger!()` macro.

### DFS Iterator

The `DataNodeDfsIterator` traverses data dependencies depth-first:

```rust
use vla::engine::data_dfs::DataNodeDfsIterator;

let cached = HashSet::new();
let iter = DataNodeDfsIterator::new(&graph, "target_node_id", &cached);

// Collect all dependencies in execution order
let deps: Vec<String> = iter.collect();
```

## Usage Examples

### Example 1: Basic Step-Through Execution

```rust
use vla::engine::Engine;
use vla::prelude::*;

// Create your graph
let graph = Graph {
    nodes: vec![/* ... */],
    edges: vec![/* ... */],
};

// Create engine and start execution
let mut engine = Engine::new(graph);
engine.start();

// Step through execution one node at a time
// Each step executes EXACTLY ONE node (either data or flow)
for result in engine {
    match result {
        Ok(node_id) => println!("✓ Executed: {}", node_id),
        Err(e) => eprintln!("✗ Error: {}", e),
    }
}
```

### Example 2: Using the Iterator Pattern

```rust
// The Engine implements Iterator<Item = Result<String, String>>
// Each step returns the ID of the executed node
let mut engine = Engine::new(graph);
engine.start();

// Collect all executed node IDs
let executed_nodes: Vec<String> = engine
    .filter_map(|result| result.ok())
    .collect();

println!("Executed nodes: {:?}", executed_nodes);

// Or check if all steps succeeded
let mut engine = Engine::new(graph);
engine.start();
let success = engine.all(|result| result.is_ok());
```

### Example 3: Manual Data Dependency Resolution

```rust
use vla::engine::data_dfs::DataNodeDfsIterator;
use std::collections::HashSet;

let graph = /* your graph */;
let cached = HashSet::new();

// Find all data dependencies for a specific node
let node_id = "target_node";
let dfs_iter = DataNodeDfsIterator::new(&graph, node_id, &cached);

for dep_node_id in dfs_iter {
    println!("Need to execute: {}", dep_node_id);
    // Execute the node...
}
```

### Example 4: Graph Structure for DFS

```
Data flow example:

    A (data: constant 5)
     |
     v
    B (data: constant 3)
     |
     v
    C (data: add A + B)
     |
     v
    D (flow: print C)
     |
     v
    E (flow: end)

When D is triggered:
1. DFS resolves: C needs to be computed
2. DFS resolves: C depends on A and B
3. Execution order: A → B → C → D → E
```

### Example 5: Diamond Dependency

```
    A (data)
   / \
  B   C (both data nodes)
   \ /
    D (flow node)

When D is triggered:
1. DFS finds B and C as dependencies
2. Both B and C depend on A
3. Execution order: A → B → C → D (or A → C → B → D)
4. A is only executed once (cached)
```

## API Reference

### `Engine`

- `Engine::new(graph: Graph) -> Self`: Create a new engine
- `engine.start(&mut self)`: Initialize execution by finding start nodes
- `engine.step(&mut self) -> Option<Result<String, String>>`: Execute one step, returns node ID
- `engine.next(&mut self) -> Option<Result<(), String>>`: Iterator implementation
- `engine.enqueue(&mut self, node_id: String)`: Manually queue a node for execution

### `DataNodeDfsIterator`

- `DataNodeDfsIterator::new(&graph, node_id, cached) -> Self`: Create DFS iterator
- Implements `Iterator<Item = String>`: Returns node IDs in DFS order
- Only yields data nodes (skips flow nodes)
- Skips nodes that are already in the cache

## How It Works

### Execution Flow

1. **Start**: `engine.start()` finds entry nodes:
   - Flow nodes with execution outputs but no inputs (e.g., "Start" node)
   - Or nodes with no incoming edges

2. **Iteration**: Each call to `next()`:
   - Dequeues a node to execute
   - Resolves its data dependencies using DFS
   - Executes all data dependencies in order
   - Executes the node itself
   - Collects triggers and queues next nodes

3. **Data Resolution**: For each node:
   - `DataNodeDfsIterator` finds all upstream data nodes
   - Returns them in post-order (dependencies before dependents)
   - Cached nodes are skipped

### Handle Naming Convention

- **Execution handles**: `exec_<output_id>` (e.g., `exec_execute`, `exec_true_branch`)
- **Data handles**: `data_<input/output_id>` (e.g., `data_value`, `data_result`)

### Trigger System

Flow nodes use `trigger!("output_id")` to activate execution outputs:

```rust
brick! {
    #[id("if_branch")]
    #[execution_input("execute")]
    #[execution_output("true_branch")]
    #[execution_output("false_branch")]
    fn if_branch(
        #[input] condition: bool
    ) -> (#[label("Value")] bool) {
        if condition {
            trigger!("true_branch");
        } else {
            trigger!("false_branch");
        }
        (condition,)
    }
}
```

## Testing

Run the tests with:

```bash
cd core
cargo test --lib engine::data_dfs
```

This will run all DFS iterator tests including:
- Linear dependencies
- Diamond dependencies
- Cached node handling
- Flow node filtering
