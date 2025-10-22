/// Example: Self-emitting timer node that prints tick count
///
/// This example demonstrates how self-emitting nodes work:
/// 1. Create a timer node that emits events every 100ms
/// 2. Connect it to a debug brick that prints the tick count
/// 3. Run for a few seconds and observe the output
///
/// Run with: cargo run --example self_emitting_timer

use std::time::Duration;
use vla_lib::bricks::events::timer_brick;
use vla_lib::engine::events::ExecutionEvent;
use vla_lib::engine::listeners::timer::TimerListener;
use vla_lib::engine::listeners::EventListener;
use vla_lib::engine::trigger;

fn main() {
    println!("🚀 Self-Emitting Timer Example");
    println!("================================\n");

    // Create a timer listener for a hypothetical timer node
    let mut listener = TimerListener::new("timer_node_1".to_string(), 100);

    println!("Starting timer listener (100ms interval)...");
    let receiver = listener.start().unwrap();

    println!("Listening for timer ticks...\n");

    // Listen for events for 1 second
    let start = std::time::Instant::now();
    let mut tick_count = 0;

    while start.elapsed() < Duration::from_secs(1) {
        // Try to receive events (non-blocking)
        if let Ok(event) = receiver.try_recv() {
            match event {
                ExecutionEvent::TimerTick {
                    node_id,
                    tick_count: count,
                    timestamp,
                } => {
                    println!("⏰ Tick #{}: node={}, time={}", count, node_id, timestamp);

                    // Simulate executing the timer brick with this event context
                    let context = trigger::ExecutionContext::TimerTick {
                        tick_count: count,
                        timestamp: timestamp.clone(),
                    };
                    trigger::set_execution_context(context);

                    // Execute the brick
                    let brick = timer_brick();
                    let outputs = (brick.execution)(vec![], vec![]);

                    println!("   📤 Outputs: tick_count={}, timestamp={}",
                        outputs[0].value, outputs[1].value);

                    trigger::clear_execution_context();
                    tick_count += 1;
                }
                _ => {}
            }
        }

        // Small sleep to avoid busy-waiting
        std::thread::sleep(Duration::from_millis(10));
    }

    println!("\n✅ Received {} ticks in 1 second", tick_count);
    println!("   Expected: ~10 ticks (100ms interval)");

    // Stop the listener
    listener.stop().unwrap();
    println!("\n🛑 Timer listener stopped");
}
