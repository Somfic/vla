# VLA Execution Engine Design

## Architecture Overview

The VLA execution engine implements a **sequential flow execution model** with automatic data dependency resolution. This document describes the design and implementation.

## Core Concepts

### Node Types

1. **Data Nodes**: Pure computation nodes
   - No `execution_inputs` or `execution_outputs`
   - Executed lazily when their data is needed
   - Results are cached to avoid re-execution
   - Example: `add`, `multiply`, `constant`

2. **Flow Nodes**: Execution control nodes
   - Have `execution_inputs` and/or `execution_outputs`
   - Control the execution flow using `trigger!()` macro
   - Execute sequentially based on trigger flow
   - Example: `start`, `if_else`, `print`, `end`

### Execution Model

The engine uses a **single-node stepping model**:

```
┌─────────────────────────────────────────────┐
│  Each Iteration Executes EXACTLY ONE Node  │
└─────────────────────────────────────────────┘
         │
         ├─► Data Node (if pending dependencies)
         │   └─► Execute and cache output
         │
         └─► Flow Node (when dependencies ready)
             ├─► Execute the node
             ├─► Collect triggers
             └─► Queue next flow nodes
```

## Engine State

```rust
pub struct Engine {
    graph: Graph,
    queue: VecDeque<String>,              // Flow nodes waiting to execute
    cache: HashMap<String, Vec<...>>,     // Cached data node outputs
    current_flow_node: Option<String>,     // Current flow node being processed
    pending_data_deps: VecDeque<String>,  // Data dependencies to execute
}
```

## Execution Algorithm

### Initialization

```rust
engine.start()
```

1. Find start nodes (flow nodes with execution outputs but no inputs)
2. Or find nodes with no incoming edges
3. Queue them for execution

### Iterator Step (`next()`)

Each call to `next()` performs ONE of these actions:

#### Case 1: Pending Data Dependency

```rust
if let Some(data_node) = pending_data_deps.pop_front() {
    execute_data_node(data_node)
    return Some(Ok(data_node))
}
```

- Execute the data node
- Cache its output
- Return the node ID

#### Case 2: Current Flow Node Ready

```rust
if let Some(flow_node) = current_flow_node.take() {
    execute_node_internal(flow_node)
    collect_triggers()
    queue_next_nodes()
    return Some(Ok(flow_node))
}
```

- Execute the flow node
- Collect `trigger!()` calls
- Find edges matching the triggers
- Queue target nodes for execution
- Return the node ID

#### Case 3: Start New Flow Node

```rust
let next_flow = queue.pop_front()?;
let data_deps = resolve_data_dependencies(next_flow);

if data_deps.is_empty() {
    current_flow_node = Some(next_flow)
    next()  // Execute it immediately
} else {
    pending_data_deps.extend(data_deps)
    current_flow_node = Some(next_flow)
    next()  // Execute first data dependency
}
```

- Dequeue next flow node
- Use DFS to find data dependencies
- Queue all data dependencies
- Execute first dependency in next iteration

## Data Dependency Resolution

Uses depth-first search via `DataNodeDfsIterator`:

```rust
fn resolve_data_dependencies(node_id) -> Vec<String> {
    let cached = get_cached_node_ids();
    DataNodeDfsIterator::new(&graph, node_id, &cached).collect()
}
```

The DFS iterator:
- Traverses data edges (not execution edges)
- Returns nodes in post-order (dependencies before dependents)
- Skips already-cached nodes
- Only yields data nodes (skips flow nodes)

## Execution Flow Example

### Simple Linear Flow

```
Graph:
  [Start] --exec--> [Print] --exec--> [End]
             ^
             |
           data
             |
          [Add 5+3]

Execution Steps:
1. Start   (flow node)
2. Add     (data node - dependency of Print)
3. Print   (flow node)
4. End     (flow node)
```

### Conditional Flow

```
Graph:
  [Start] --exec--> [If]
                     ├--exec(true)--> [TrueHandler]
                     └--exec(false)--> [FalseHandler]

Execution Steps (condition = true):
1. Start        (flow node)
2. If           (flow node, triggers "true_branch")
3. TrueHandler  (flow node)

Note: FalseHandler is NOT executed
```

### Complex Data Dependencies

```
Graph:
  [Start] --exec--> [Print]
                       ^
                       |
                     data
                       |
                    [Multiply]
                     /      \
                   data    data
                   /          \
              [Add A+B]    [Sub C-D]
                /  \         /  \
              data data    data data
              /      \     /      \
           [A=5]  [B=3] [C=10] [D=2]

Execution Steps:
1. Start    (flow node)
2. A        (data node)
3. B        (data node)
4. Add      (data node)
5. C        (data node)
6. D        (data node)
7. Sub      (data node)
8. Multiply (data node)
9. Print    (flow node)
```

## Trigger System

Flow nodes use `trigger!("output_id")` to activate execution outputs:

```rust
brick! {
    #[execution_input("execute")]
    #[execution_output("true_branch")]
    #[execution_output("false_branch")]
    fn if_else(#[input] condition: bool) -> (bool) {
        if condition {
            trigger!("true_branch");  // Activates exec_true_branch
        } else {
            trigger!("false_branch"); // Activates exec_false_branch
        }
        (condition,)
    }
}
```

Triggers are collected and matched against edges:
- Trigger format: `"node_id:output_id"`
- Edge handle: `"exec_output_id"`
- Matching edges queue their target nodes

## Handle Naming Convention

### Execution Handles (Flow Control)
- Source: `exec_<output_id>` (e.g., `exec_begin`, `exec_true_branch`)
- Target: `exec_<input_id>` (e.g., `exec_execute`)

### Data Handles (Data Flow)
- Source: `data_<output_id>` (e.g., `data_result`, `data_value`)
- Target: `data_<input_id>` (e.g., `data_a`, `data_b`)

## Benefits of This Design

### 1. **Simplicity**
- Clear separation: flow nodes control execution, data nodes provide values
- Sequential flow execution is easy to reason about
- No complex scheduling or parallelism

### 2. **Lazy Evaluation**
- Data nodes only execute when needed
- Unused branches don't compute their data dependencies
- Efficient for conditional flows

### 3. **Step-Through Debugging**
- Each iteration = one node execution
- Perfect for visual debugging
- Can pause/inspect between any two nodes

### 4. **Correct Dependency Order**
- DFS ensures dependencies execute before dependents
- Cached nodes never re-execute
- Diamond dependencies handled correctly

### 5. **Explicit Flow Control**
- `trigger!()` makes execution flow visible in code
- Easy to add complex control flow (loops, conditions)
- Predictable execution order

## Future Extensions

### Potential Enhancements

1. **Breakpoints**: Pause execution at specific nodes
2. **Variable Inspection**: View cached values during execution
3. **Execution Visualization**: Highlight active execution path in UI
4. **Performance Profiling**: Track execution time per node
5. **Parallel Data Execution**: Execute independent data nodes concurrently
6. **Subgraphs**: Reusable graph components
7. **Error Handling**: Try/catch flow control
8. **Loops**: While/for loop constructs

### Implementation Notes

For parallel data execution:
- Could execute independent data nodes concurrently
- Would need to track which data nodes are independent
- Requires locking/synchronization for cache
- Trade-off: complexity vs performance

For subgraphs:
- Treat subgraph as a single node
- Map inputs/outputs to subgraph nodes
- Could cache entire subgraph results
- Enable modularity and reuse

## Testing

Run all engine tests:
```bash
cd core
cargo test --lib engine
```

Specific test suites:
- `engine::data_dfs::tests` - DFS iterator tests
- `engine::flow_test::tests` - Flow execution tests
- `engine::trigger::tests` - Trigger system tests
- `engine::tests` - Basic engine tests

## API Reference

### Engine

```rust
// Create new engine
let mut engine = Engine::new(graph);

// Start execution (finds and queues start nodes)
engine.start();

// Manually enqueue a flow node
engine.enqueue(node_id);

// Iterate through execution (one node per step)
for result in engine {
    match result {
        Ok(node_id) => println!("Executed: {}", node_id),
        Err(e) => eprintln!("Error: {}", e),
    }
}
```

### DataNodeDfsIterator

```rust
use vla::engine::data_dfs::DataNodeDfsIterator;

// Create iterator for resolving dependencies
let cached = HashSet::new();
let iter = DataNodeDfsIterator::new(&graph, target_node_id, &cached);

// Get all dependencies in execution order
let deps: Vec<String> = iter.collect();
```

### Trigger System

```rust
// In a brick implementation
trigger!("output_id");  // Trigger an execution output

// System functions (internal use)
trigger::set_current_node_id(node_id);
trigger::collect_and_clear_triggers();
trigger::clear_current_node_id();
```
