/// Integration test for timer-based self-emitting nodes
///
/// Run with: cargo test --test timer_integration_test

use std::collections::BTreeMap;
use vla::canvas::{Edge, Graph, Node, NodeData, Point};
use vla::engine::Engine;
use vla::bricks::events::timer_brick;
use vla::bricks::debug::print_brick;

#[test]
fn test_timer_graph_continuous_execution() {
    // Create a graph with timer -> print
    let mut timer_args = BTreeMap::new();
    timer_args.insert("interval_ms".to_string(), "100".to_string()); // 100ms = 10 Hz

    let timer_node = Node {
        id: "timer_1".to_string(),
        position: Point { x: 0.0, y: 0.0 },
        data: NodeData {
            brick_id: "timer".to_string(),
            brick: Some(timer_brick()),
            arguments: timer_args,
            defaults: BTreeMap::new(),
        },
        r#type: "v1".to_string(),
    };

    let print_node = Node {
        id: "print_1".to_string(),
        position: Point { x: 100.0, y: 0.0 },
        data: NodeData {
            brick_id: "print".to_string(),
            brick: Some(print_brick()),
            arguments: BTreeMap::new(),
            defaults: BTreeMap::new(),
        },
        r#type: "v1".to_string(),
    };

    // Connect timer tick -> print execute
    let tick_edge = Edge {
        id: "timer_tick_to_print".to_string(),
        source: "timer_1".to_string(),
        source_handle: "tick".to_string(),
        target: "print_1".to_string(),
        target_handle: "execute".to_string(),
    };

    // Connect timer output_1 (timestamp) -> print value
    let data_edge = Edge {
        id: "timer_timestamp_to_print".to_string(),
        source: "timer_1".to_string(),
        source_handle: "output_1".to_string(),
        target: "print_1".to_string(),
        target_handle: "value".to_string(),
    };

    let graph = Graph {
        nodes: vec![timer_node, print_node],
        edges: vec![tick_edge, data_edge],
    };

    // Create and start engine
    let mut engine = Engine::new_test(graph);
    engine.start();

    // Run for 500ms, should get ~5 timer ticks = 10 total executions (timer + print each time)
    let start = std::time::Instant::now();
    let mut execution_count = 0;
    let mut timer_executions = 0;
    let mut print_executions = 0;

    while start.elapsed() < std::time::Duration::from_millis(500) {
        if let Some(result) = engine.next() {
            match result {
                Ok(node_id) => {
                    execution_count += 1;
                    if node_id == "timer_1" {
                        timer_executions += 1;
                    } else if node_id == "print_1" {
                        print_executions += 1;
                    }
                }
                Err(e) => {
                    panic!("Execution error: {}", e);
                }
            }
        }
    }

    println!("Total executions: {}", execution_count);
    println!("Timer executions: {}", timer_executions);
    println!("Print executions: {}", print_executions);

    // We expect ~5 timer ticks in 500ms at 100ms interval
    // Each tick should execute timer node + print node = 2 executions per tick
    // So total should be around 10 executions (5 ticks × 2 nodes)
    assert!(timer_executions >= 4 && timer_executions <= 6,
        "Expected 4-6 timer executions, got {}", timer_executions);
    assert!(print_executions >= 4 && print_executions <= 6,
        "Expected 4-6 print executions, got {}", print_executions);
    assert!(execution_count >= 8 && execution_count <= 12,
        "Expected 8-12 total executions, got {}", execution_count);
}
