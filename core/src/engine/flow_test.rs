use crate::bricks::macros::brick;
use crate::engine::Engine;
use crate::prelude::*;
use crate::trigger;
use std::collections::BTreeMap;

// Data node: simple addition
brick! {
    #[id("add_numbers")]
    #[category("Math")]
    fn add_numbers(
        #[input] a: f32 = 5.0,
        #[input] b: f32 = 3.0
    ) -> (
        #[label("Sum")] f32
    ) {
        (a + b,)
    }
}

// Flow node: Start execution
brick! {
    #[id("start_flow")]
    #[category("Control Flow")]
    #[execution_output("begin")]
    fn start_flow() -> (
        #[label("Started")] bool
    ) {
        trigger!("begin");
        (true,)
    }
}

// Flow node: Print and continue
brick! {
    #[id("print_and_continue")]
    #[category("Control Flow")]
    #[execution_input("execute")]
    #[execution_output("done")]
    fn print_and_continue(
        #[input] value: f32 = 0.0
    ) -> (
        #[label("Value")] f32
    ) {
        println!("  [Print] Value: {}", value);
        trigger!("done");
        (value,)
    }
}

// Flow node: End execution
brick! {
    #[id("end_flow")]
    #[category("Control Flow")]
    #[execution_input("execute")]
    fn end_flow(
        #[input] final_value: f32 = 0.0
    ) -> (
        #[label("Result")] f32
    ) {
        println!("  [End] Final value: {}", final_value);
        (final_value,)
    }
}

#[test]
fn test_sequential_flow_with_data_dependencies() {
    println!("\n=== Sequential Flow Execution Test ===\n");

    // Create graph:
    //
    // Data flow:
    //   add_numbers (data node) → computes 5 + 3 = 8
    //       ↓ (data connection)
    //   print_node
    //       ↓ (data connection)
    //   end_node
    //
    // Execution flow:
    //   start_node → print_node → end_node
    //
    let graph = Graph {
        nodes: vec![
            Node {
                id: "start".to_string(),
                position: Point { x: 0.0, y: 0.0 },
                data: NodeData {
                    brick_id: "start_flow".to_string(),
                    brick: Some(start_flow_brick()),
                    arguments: BTreeMap::new(),
                    defaults: BTreeMap::new(),
                },
                r#type: "v1".to_string(),
            },
            Node {
                id: "add".to_string(),
                position: Point { x: 100.0, y: 0.0 },
                data: NodeData {
                    brick_id: "add_numbers".to_string(),
                    brick: Some(add_numbers_brick()),
                    arguments: BTreeMap::new(),
                    defaults: BTreeMap::new(),
                },
                r#type: "v1".to_string(),
            },
            Node {
                id: "print".to_string(),
                position: Point { x: 200.0, y: 0.0 },
                data: NodeData {
                    brick_id: "print_and_continue".to_string(),
                    brick: Some(print_and_continue_brick()),
                    arguments: BTreeMap::new(),
                    defaults: BTreeMap::new(),
                },
                r#type: "v1".to_string(),
            },
            Node {
                id: "end".to_string(),
                position: Point { x: 300.0, y: 0.0 },
                data: NodeData {
                    brick_id: "end_flow".to_string(),
                    brick: Some(end_flow_brick()),
                    arguments: BTreeMap::new(),
                    defaults: BTreeMap::new(),
                },
                r#type: "v1".to_string(),
            },
        ],
        edges: vec![
            // Execution flow edges
            Edge {
                id: "exec1".to_string(),
                source: "start".to_string(),
                target: "print".to_string(),
                source_handle: "begin".to_string(),
                target_handle: "execute".to_string(),
            },
            Edge {
                id: "exec2".to_string(),
                source: "print".to_string(),
                target: "end".to_string(),
                source_handle: "done".to_string(),
                target_handle: "execute".to_string(),
            },
            // Data flow edges
            Edge {
                id: "data1".to_string(),
                source: "add".to_string(),
                target: "print".to_string(),
                source_handle: "data_output_0".to_string(),
                target_handle: "data_value".to_string(),
            },
            Edge {
                id: "data2".to_string(),
                source: "print".to_string(),
                target: "end".to_string(),
                source_handle: "data_output_0".to_string(),
                target_handle: "data_final_value".to_string(),
            },
        ],
    };

    let mut engine = Engine::with_debug_test(graph, true);
    engine.start();

    println!("Starting execution...\n");

    let mut step_count = 0;
    let mut executed_nodes = Vec::new();

    for result in engine {
        step_count += 1;
        match result {
            Ok(node_id) => {
                println!("  Step {}: Executed {}", step_count, node_id);
                executed_nodes.push(node_id);
            }
            Err(e) => panic!("Step {} failed: {}", step_count, e),
        }
    }

    println!("\n✓ Execution completed in {} steps", step_count);
    println!("  Execution order: {:?}", executed_nodes);

    // Expected execution order:
    // 1. start (flow node)
    // 2. add (data node - dependency of print)
    // 3. print (flow node)
    // 4. end (flow node)
    assert_eq!(
        step_count, 4,
        "Expected 4 node executions (1 data + 3 flow)"
    );
    assert_eq!(executed_nodes, vec!["start", "add", "print", "end"]);
}

#[test]
fn test_conditional_flow() {
    println!("\n=== Conditional Flow Test ===\n");

    // Flow node: Conditional branch
    brick! {
        #[id("if_else")]
        #[category("Control Flow")]
        #[execution_input("execute")]
        #[execution_output("true_branch")]
        #[execution_output("false_branch")]
        fn if_else(
            #[input] condition: bool = true
        ) -> (
            #[label("Condition")] bool
        ) {
            println!("  [If] Condition: {}", condition);
            if condition {
                trigger!("true_branch");
            } else {
                trigger!("false_branch");
            }
            (condition,)
        }
    }

    brick! {
        #[id("true_handler")]
        #[category("Control Flow")]
        #[execution_input("execute")]
        fn true_handler() -> (#[label("Result")] bool) {
            println!("  [True Branch] Executed!");
            (true,)
        }
    }

    brick! {
        #[id("false_handler")]
        #[category("Control Flow")]
        #[execution_input("execute")]
        fn false_handler() -> (#[label("Result")] bool) {
            println!("  [False Branch] Executed!");
            (false,)
        }
    }

    // Graph: start → if_else → true_handler (or false_handler based on condition)
    let graph = Graph {
        nodes: vec![
            Node {
                id: "start".to_string(),
                position: Point { x: 0.0, y: 0.0 },
                data: NodeData {
                    brick_id: "start_flow".to_string(),
                    brick: Some(start_flow_brick()),
                    arguments: BTreeMap::new(),
                    defaults: BTreeMap::new(),
                },
                r#type: "v1".to_string(),
            },
            Node {
                id: "if".to_string(),
                position: Point { x: 100.0, y: 0.0 },
                data: NodeData {
                    brick_id: "if_else".to_string(),
                    brick: Some(if_else_brick()),
                    arguments: BTreeMap::new(),
                    defaults: BTreeMap::new(),
                },
                r#type: "v1".to_string(),
            },
            Node {
                id: "true_node".to_string(),
                position: Point { x: 200.0, y: -50.0 },
                data: NodeData {
                    brick_id: "true_handler".to_string(),
                    brick: Some(true_handler_brick()),
                    arguments: BTreeMap::new(),
                    defaults: BTreeMap::new(),
                },
                r#type: "v1".to_string(),
            },
            Node {
                id: "false_node".to_string(),
                position: Point { x: 200.0, y: 50.0 },
                data: NodeData {
                    brick_id: "false_handler".to_string(),
                    brick: Some(false_handler_brick()),
                    arguments: BTreeMap::new(),
                    defaults: BTreeMap::new(),
                },
                r#type: "v1".to_string(),
            },
        ],
        edges: vec![
            Edge {
                id: "exec1".to_string(),
                source: "start".to_string(),
                target: "if".to_string(),
                source_handle: "begin".to_string(),
                target_handle: "execute".to_string(),
            },
            Edge {
                id: "exec_true".to_string(),
                source: "if".to_string(),
                target: "true_node".to_string(),
                source_handle: "true_branch".to_string(),
                target_handle: "execute".to_string(),
            },
            Edge {
                id: "exec_false".to_string(),
                source: "if".to_string(),
                target: "false_node".to_string(),
                source_handle: "false_branch".to_string(),
                target_handle: "execute".to_string(),
            },
        ],
    };

    let mut engine = Engine::with_debug_test(graph, true);
    engine.start();

    println!("Starting conditional execution...\n");

    let mut step_count = 0;
    let mut executed_nodes = Vec::new();

    for result in engine {
        step_count += 1;
        match result {
            Ok(node_id) => {
                println!("  Step {}: Executed {}", step_count, node_id);
                executed_nodes.push(node_id);
            }
            Err(e) => panic!("Step {} failed: {}", step_count, e),
        }
    }

    println!(
        "\n✓ Conditional execution completed in {} steps",
        step_count
    );
    println!("  Execution order: {:?}", executed_nodes);

    // Should execute: start, if, true_node (false_node is NOT executed)
    assert_eq!(step_count, 3, "Expected 3 node executions");
    assert_eq!(executed_nodes, vec!["start", "if", "true_node"]);
    assert!(
        !executed_nodes.contains(&"false_node".to_string()),
        "False branch should not execute"
    );
}
