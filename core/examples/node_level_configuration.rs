/// Example: Node-level configuration for emission contexts
///
/// This demonstrates how each node instance can have its own configuration
/// that gets passed to its emission context. For example, three timer nodes
/// can each have different intervals.
///
/// Run with: cargo run --example node_level_configuration

use std::collections::BTreeMap;
use std::sync::mpsc;
use std::time::Duration;
use vla_lib::bricks::events::timer_brick;
use vla_lib::canvas::{Graph, Node, NodeData, Point};
use vla_lib::engine::events::ExecutionEvent;

// Import EmissionContext trait and TimerContext
use vla_lib::engine::emission_contexts::EmissionContext;
use vla_lib::engine::emission_contexts::TimerContext;

fn create_timer_node_with_interval(id: &str, interval_ms: u64) -> Node {
    let mut arguments = BTreeMap::new();
    arguments.insert("interval_ms".to_string(), interval_ms.to_string());

    Node {
        id: id.to_string(),
        position: Point { x: 0.0, y: 0.0 },
        data: NodeData {
            brick_id: "timer".to_string(),
            brick: Some(timer_brick()),
            arguments,
            defaults: BTreeMap::new(),
        },
        r#type: "v1".to_string(),
    }
}

fn main() {
    println!("🎛️  Node-Level Configuration Example");
    println!("====================================\n");

    println!("Creating a graph with 3 timer nodes:");
    println!("  • timer_fast:   100ms interval");
    println!("  • timer_medium: 250ms interval");
    println!("  • timer_slow:   500ms interval\n");

    // Create graph with 3 timer nodes, each with different intervals
    let graph = Graph {
        nodes: vec![
            create_timer_node_with_interval("timer_fast", 100),
            create_timer_node_with_interval("timer_medium", 250),
            create_timer_node_with_interval("timer_slow", 500),
        ],
        edges: vec![],
    };

    // Simulate what the engine would do: create emission contexts per node
    println!("Starting emission contexts...\n");

    let mut contexts: Vec<(String, Box<dyn EmissionContext>)> = Vec::new();
    let (event_sender, event_receiver) = mpsc::channel();

    // For demo purposes, we'll manually create contexts since the brick macro
    // doesn't yet parse emission_type attributes (it defaults to FlowTriggered).
    // In the final system, this would be automatic based on brick.emission_type.

    for node in &graph.nodes {
        // Read interval from node arguments - this is the key part!
        let interval_ms = node
            .data
            .arguments
            .get("interval_ms")
            .and_then(|v| v.parse::<u64>().ok())
            .unwrap_or(1000); // Default if not specified

        println!(
            "  ✓ Node '{}': interval={}ms (from node arguments)",
            node.id, interval_ms
        );

        // Create context with node-specific configuration
        let mut context = Box::new(TimerContext::new(interval_ms));
        context
            .start(node.id.clone(), event_sender.clone())
            .unwrap();

        contexts.push((node.id.clone(), context as Box<dyn EmissionContext>));
    }

    println!("\n🎧 Listening for events (1 second)...\n");

    // Track events per node
    let mut event_counts: BTreeMap<String, u64> = BTreeMap::new();

    let start = std::time::Instant::now();
    while start.elapsed() < Duration::from_secs(1) {
        if let Ok(event) = event_receiver.try_recv() {
            match event {
                ExecutionEvent::TimerTick {
                    node_id,
                    tick_count,
                    ..
                } => {
                    println!("  ⏰ {} - Tick #{}", node_id, tick_count);
                    *event_counts.entry(node_id).or_insert(0) += 1;
                }
                _ => {}
            }
        }
        std::thread::sleep(Duration::from_millis(10));
    }

    // Stop all contexts
    for (_, mut context) in contexts {
        context.stop().unwrap();
    }

    println!("\n📊 Results:");
    println!("  timer_fast   (100ms): {} ticks (expected: ~10)", event_counts.get("timer_fast").unwrap_or(&0));
    println!("  timer_medium (250ms): {} ticks (expected: ~4)", event_counts.get("timer_medium").unwrap_or(&0));
    println!("  timer_slow   (500ms): {} ticks (expected: ~2)", event_counts.get("timer_slow").unwrap_or(&0));

    println!("\n✨ Key Takeaway:");
    println!("  Each node instance configures its own emission context!");
    println!("  The brick defines defaults, but nodes override with arguments.");
    println!("\n  Flow:");
    println!("  1. Brick defines: emission_type(Timer {{ default_interval_ms: 1000 }})");
    println!("  2. Node provides: arguments {{ \"interval_ms\": \"250\" }}");
    println!("  3. Context uses: 250ms (node argument takes precedence)");
}
