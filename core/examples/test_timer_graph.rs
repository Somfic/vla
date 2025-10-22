/// Test the actual timer graph from graph.json
///
/// Run with: cargo run --example test_timer_graph

use std::fs;
use vla_lib::canvas::Graph;
use vla_lib::engine::Engine;

fn main() {
    println!("🧪 Testing Timer Graph from graph.json\n");

    // Load the graph
    let graph_json = fs::read_to_string("/Users/lucas/vla/graph.json")
        .expect("Failed to read graph.json");

    let graph = Graph::from_json(graph_json)
        .expect("Failed to parse graph.json");

    println!("✓ Loaded graph with {} nodes and {} edges",
        graph.nodes.len(), graph.edges.len());

    // Create and start the engine
    let mut engine: Engine = Engine::new(graph);
    engine.start();

    println!("✓ Engine started\n");
    println!("🎧 Running engine for 1 second (should execute ~5 times)...\n");

    let start = std::time::Instant::now();
    let mut execution_count = 0;

    while start.elapsed() < std::time::Duration::from_secs(1) {
        if let Some(result) = engine.next() {
            match result {
                Ok(node_id) => {
                    execution_count += 1;
                    println!("  ✓ Executed node: {}", node_id);
                }
                Err(e) => {
                    println!("  ✗ Error: {}", e);
                }
            }
        }
    }

    println!("\n📊 Results:");
    println!("  Total executions: {}", execution_count);
    println!("  Expected: ~10 (5 timer ticks × 2 nodes: timer + print)");

    if execution_count >= 8 && execution_count <= 12 {
        println!("\n✅ SUCCESS: Graph is executing continuously at 5 Hz!");
    } else {
        println!("\n⚠️  Warning: Execution count is outside expected range");
    }
}
