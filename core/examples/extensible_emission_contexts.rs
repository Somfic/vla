/// Example: Demonstrating how to add new emission contexts
///
/// This example shows how easy it is to create a new self-emitting node type
/// without modifying the core engine code. Just:
/// 1. Implement the EmissionContext trait
/// 2. Add a new ExecutionEvent variant
/// 3. Add a new ExecutionContext variant
/// 4. Create your brick
///
/// Run with: cargo run --example extensible_emission_contexts

use std::sync::mpsc::{self, Sender};
use std::thread;
use std::time::Duration;
use vla_lib::engine::emission_contexts::{EmissionContext, TimerContext, ManualTriggerContext};
use vla_lib::engine::events::ExecutionEvent;

/// Example: Custom RandomNumberContext that emits random numbers at intervals
/// This demonstrates how easy it is to add new emission types!
struct RandomNumberContext {
    interval_ms: u64,
    min: i32,
    max: i32,
    active: std::sync::Arc<std::sync::Mutex<bool>>,
    thread_handle: Option<thread::JoinHandle<()>>,
}

impl RandomNumberContext {
    fn new(interval_ms: u64, min: i32, max: i32) -> Self {
        Self {
            interval_ms,
            min,
            max,
            active: std::sync::Arc::new(std::sync::Mutex::new(false)),
            thread_handle: None,
        }
    }
}

impl EmissionContext for RandomNumberContext {
    fn start(&mut self, node_id: String, event_sender: Sender<ExecutionEvent>) -> Result<(), String> {
        if *self.active.lock().unwrap() {
            return Err("Random number context already active".to_string());
        }

        *self.active.lock().unwrap() = true;

        let interval_ms = self.interval_ms;
        let min = self.min;
        let max = self.max;
        let active = std::sync::Arc::clone(&self.active);

        let handle = thread::spawn(move || {
            // Simple pseudo-random using system time (no rand dependency needed)
            let mut counter = 0u64;

            while *active.lock().unwrap() {
                use std::time::{SystemTime, UNIX_EPOCH};
                let duration = SystemTime::now().duration_since(UNIX_EPOCH).unwrap();

                // Simple pseudo-random: mix timestamp with counter
                let pseudo_random = ((duration.as_millis() as i32 + counter as i32) % (max - min + 1)) + min;

                let timestamp = format!("{}.{:03}", duration.as_secs(), duration.subsec_millis());

                // For now, we'll use ManualTrigger as a placeholder
                // In a real implementation, you'd add ExecutionEvent::RandomNumber
                let event = ExecutionEvent::ManualTrigger {
                    node_id: format!("{}:{}", node_id, pseudo_random),
                    timestamp,
                };

                if event_sender.send(event).is_err() {
                    break;
                }

                counter += 1;
                thread::sleep(Duration::from_millis(interval_ms));
            }
        });

        self.thread_handle = Some(handle);
        Ok(())
    }

    fn stop(&mut self) -> Result<(), String> {
        if !*self.active.lock().unwrap() {
            return Ok(());
        }

        *self.active.lock().unwrap() = false;

        if let Some(handle) = self.thread_handle.take() {
            handle.join().map_err(|_| "Failed to join random number thread".to_string())?;
        }

        Ok(())
    }

    fn is_active(&self) -> bool {
        *self.active.lock().unwrap()
    }

    fn context_type(&self) -> &'static str {
        "RandomNumber"
    }
}

impl Drop for RandomNumberContext {
    fn drop(&mut self) {
        let _ = self.stop();
    }
}

fn main() {
    println!("🚀 Extensible Emission Contexts Example");
    println!("=========================================\n");

    println!("Demonstrating 3 different emission context types:\n");

    // 1. Timer context
    println!("1️⃣  Timer Context (built-in)");
    println!("   Emits tick events every 200ms");
    let (timer_sender, timer_receiver) = mpsc::channel();
    let mut timer_ctx = TimerContext::new(200);
    timer_ctx.start("timer_node".to_string(), timer_sender).unwrap();

    // 2. Manual trigger context
    println!("2️⃣  Manual Trigger Context (built-in)");
    println!("   Can be triggered on demand");
    let (manual_sender, manual_receiver) = mpsc::channel();
    let mut manual_ctx = ManualTriggerContext::new();
    manual_ctx.start("manual_node".to_string(), manual_sender).unwrap();

    // 3. Custom random number context
    println!("3️⃣  Random Number Context (custom!)");
    println!("   Emits random numbers (1-100) every 300ms\n");
    let (random_sender, random_receiver) = mpsc::channel();
    let mut random_ctx = RandomNumberContext::new(300, 1, 100);
    random_ctx.start("random_node".to_string(), random_sender).unwrap();

    println!("🎧 Listening for events...\n");

    // Listen for events for 1 second
    let start = std::time::Instant::now();
    let mut timer_count = 0;
    let mut manual_count = 0;
    let mut random_count = 0;

    // Trigger manual context a few times
    manual_ctx.trigger().unwrap();
    thread::sleep(Duration::from_millis(100));
    manual_ctx.trigger().unwrap();

    while start.elapsed() < Duration::from_secs(1) {
        // Check all receivers
        if let Ok(event) = timer_receiver.try_recv() {
            match event {
                ExecutionEvent::TimerTick { tick_count, .. } => {
                    println!("   ⏰ Timer tick #{}", tick_count);
                    timer_count += 1;
                }
                _ => {}
            }
        }

        if let Ok(event) = manual_receiver.try_recv() {
            match event {
                ExecutionEvent::ManualTrigger { .. } => {
                    println!("   👆 Manual trigger");
                    manual_count += 1;
                }
                _ => {}
            }
        }

        if let Ok(event) = random_receiver.try_recv() {
            match event {
                ExecutionEvent::ManualTrigger { node_id, .. } => {
                    // Parse the number from the node_id (hacky, just for demo)
                    if let Some(number_str) = node_id.split(':').nth(1) {
                        println!("   🎲 Random number: {}", number_str);
                        random_count += 1;
                    }
                }
                _ => {}
            }
        }

        thread::sleep(Duration::from_millis(10));
    }

    // Cleanup
    timer_ctx.stop().unwrap();
    manual_ctx.stop().unwrap();
    random_ctx.stop().unwrap();

    println!("\n📊 Summary:");
    println!("   Timer events:   {} (~5 expected)", timer_count);
    println!("   Manual events:  {} (2 triggered)", manual_count);
    println!("   Random events:  {} (~3 expected)", random_count);
    println!("\n✅ All contexts cleaned up!");
    println!("\n💡 Key Takeaway:");
    println!("   Adding a new emission type requires:");
    println!("   1. Implement EmissionContext trait (~50 lines)");
    println!("   2. Add ExecutionEvent variant (1 line)");
    println!("   3. Add ExecutionContext variant (1 line)");
    println!("   4. Create your brick using brick! macro");
    println!("   That's it! No engine modifications needed! 🎉");
}
