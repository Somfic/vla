/// Emission contexts - each self-emitting brick type has its own context
/// that runs independently and emits events when ready

use super::events::ExecutionEvent;
use std::sync::mpsc::Sender;

/// Trait for emission contexts - each self-emitting node type implements this
pub trait EmissionContext: Send {
    /// Start the context (spawn threads, set up listeners, etc.)
    /// The context will send ExecutionEvents through the sender when ready
    fn start(&mut self, node_id: String, event_sender: Sender<ExecutionEvent>) -> Result<(), String>;

    /// Stop the context (cleanup, join threads, etc.)
    fn stop(&mut self) -> Result<(), String>;

    /// Check if the context is active
    fn is_active(&self) -> bool;

    /// Get context type name for debugging
    fn context_type(&self) -> &'static str;
}

/// Timer emission context - emits tick events at regular intervals
pub struct TimerContext {
    interval_ms: u64,
    active: std::sync::Arc<std::sync::Mutex<bool>>,
    thread_handle: Option<std::thread::JoinHandle<()>>,
}

impl TimerContext {
    pub fn new(interval_ms: u64) -> Self {
        Self {
            interval_ms,
            active: std::sync::Arc::new(std::sync::Mutex::new(false)),
            thread_handle: None,
        }
    }
}

impl EmissionContext for TimerContext {
    fn start(&mut self, node_id: String, event_sender: Sender<ExecutionEvent>) -> Result<(), String> {
        if *self.active.lock().unwrap() {
            return Err("Timer context already active".to_string());
        }

        *self.active.lock().unwrap() = true;

        let interval_ms = self.interval_ms;
        let active = std::sync::Arc::clone(&self.active);

        let handle = std::thread::spawn(move || {
            let mut tick_count = 0u64;

            while *active.lock().unwrap() {
                // Create timestamp
                let timestamp = {
                    use std::time::{SystemTime, UNIX_EPOCH};
                    let duration = SystemTime::now().duration_since(UNIX_EPOCH).unwrap();
                    format!("{}.{:03}", duration.as_secs(), duration.subsec_millis())
                };

                // Send event
                let event = ExecutionEvent::TimerTick {
                    node_id: node_id.clone(),
                    tick_count,
                    timestamp,
                };

                if event_sender.send(event).is_err() {
                    // Receiver dropped, stop the timer
                    break;
                }

                tick_count += 1;
                std::thread::sleep(std::time::Duration::from_millis(interval_ms));
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
            handle.join().map_err(|_| "Failed to join timer thread".to_string())?;
        }

        Ok(())
    }

    fn is_active(&self) -> bool {
        *self.active.lock().unwrap()
    }

    fn context_type(&self) -> &'static str {
        "Timer"
    }
}

impl Drop for TimerContext {
    fn drop(&mut self) {
        let _ = self.stop();
    }
}

/// Manual trigger context - can be triggered programmatically
pub struct ManualTriggerContext {
    active: bool,
    event_sender: Option<Sender<ExecutionEvent>>,
    node_id: Option<String>,
}

impl ManualTriggerContext {
    pub fn new() -> Self {
        Self {
            active: false,
            event_sender: None,
            node_id: None,
        }
    }

    /// Manually trigger this context
    pub fn trigger(&self) -> Result<(), String> {
        if !self.active {
            return Err("Manual trigger context not active".to_string());
        }

        let timestamp = {
            use std::time::{SystemTime, UNIX_EPOCH};
            let duration = SystemTime::now().duration_since(UNIX_EPOCH).unwrap();
            format!("{}.{:03}", duration.as_secs(), duration.subsec_millis())
        };

        let event = ExecutionEvent::ManualTrigger {
            node_id: self.node_id.clone().unwrap(),
            timestamp,
        };

        self.event_sender
            .as_ref()
            .ok_or("Event sender not initialized")?
            .send(event)
            .map_err(|e| format!("Failed to send event: {}", e))
    }
}

impl EmissionContext for ManualTriggerContext {
    fn start(&mut self, node_id: String, event_sender: Sender<ExecutionEvent>) -> Result<(), String> {
        if self.active {
            return Err("Manual trigger context already active".to_string());
        }

        self.node_id = Some(node_id);
        self.event_sender = Some(event_sender);
        self.active = true;

        Ok(())
    }

    fn stop(&mut self) -> Result<(), String> {
        self.active = false;
        self.event_sender = None;
        self.node_id = None;
        Ok(())
    }

    fn is_active(&self) -> bool {
        self.active
    }

    fn context_type(&self) -> &'static str {
        "ManualTrigger"
    }
}

impl Default for ManualTriggerContext {
    fn default() -> Self {
        Self::new()
    }
}

/// Example: Future speech recognition context (just a skeleton to show extensibility)
///
/// ```ignore
/// pub struct SpeechRecognitionContext {
///     recognizer: Option<SpeechRecognizer>,
///     active: bool,
/// }
///
/// impl EmissionContext for SpeechRecognitionContext {
///     fn start(&mut self, node_id: String, event_sender: Sender<ExecutionEvent>) -> Result<(), String> {
///         // Initialize speech recognizer
///         // Set up callback that sends ExecutionEvent::SpeechDetected when speech is recognized
///         // The recognizer runs asynchronously on its own thread
///         Ok(())
///     }
///
///     fn stop(&mut self) -> Result<(), String> {
///         // Stop and cleanup recognizer
///         Ok(())
///     }
///
///     // ... other trait methods
/// }
/// ```

#[cfg(test)]
mod tests {
    use super::*;
    use std::sync::mpsc;
    use std::time::Duration;

    #[test]
    fn test_timer_context() {
        let (sender, receiver) = mpsc::channel();
        let mut context = TimerContext::new(50);

        context.start("test_node".to_string(), sender).unwrap();
        assert!(context.is_active());

        // Wait for a few ticks
        std::thread::sleep(Duration::from_millis(200));

        // Check we received multiple events
        let mut count = 0;
        while let Ok(event) = receiver.try_recv() {
            match event {
                ExecutionEvent::TimerTick { node_id, tick_count, .. } => {
                    assert_eq!(node_id, "test_node");
                    assert_eq!(tick_count, count);
                    count += 1;
                }
                _ => panic!("Expected TimerTick event"),
            }
        }

        assert!(count >= 3, "Expected at least 3 ticks, got {}", count);

        context.stop().unwrap();
        assert!(!context.is_active());
    }

    #[test]
    fn test_manual_trigger_context() {
        let (sender, receiver) = mpsc::channel();
        let mut context = ManualTriggerContext::new();

        context.start("test_node".to_string(), sender).unwrap();
        assert!(context.is_active());

        // Trigger multiple times
        context.trigger().unwrap();
        context.trigger().unwrap();

        // Check we received exactly 2 events
        let mut count = 0;
        while let Ok(event) = receiver.try_recv() {
            match event {
                ExecutionEvent::ManualTrigger { node_id, .. } => {
                    assert_eq!(node_id, "test_node");
                    count += 1;
                }
                _ => panic!("Expected ManualTrigger event"),
            }
        }

        assert_eq!(count, 2);

        context.stop().unwrap();
        assert!(!context.is_active());
    }
}
