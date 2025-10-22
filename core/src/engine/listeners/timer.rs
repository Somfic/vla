use super::EventListener;
use crate::engine::events::ExecutionEvent;
use std::sync::mpsc::{self, Receiver, Sender};
use std::sync::{Arc, Mutex};
use std::thread;
use std::time::Duration;

/// Listener that emits timer tick events at regular intervals
pub struct TimerListener {
    node_id: String,
    interval_ms: u64,
    active: Arc<Mutex<bool>>,
    thread_handle: Option<thread::JoinHandle<()>>,
}

impl TimerListener {
    pub fn new(node_id: String, interval_ms: u64) -> Self {
        Self {
            node_id,
            interval_ms,
            active: Arc::new(Mutex::new(false)),
            thread_handle: None,
        }
    }

    fn get_timestamp() -> String {
        use std::time::{SystemTime, UNIX_EPOCH};
        let duration = SystemTime::now().duration_since(UNIX_EPOCH).unwrap();
        format!("{}.{:03}", duration.as_secs(), duration.subsec_millis())
    }
}

impl EventListener for TimerListener {
    fn start(&mut self) -> Result<Receiver<ExecutionEvent>, String> {
        if *self.active.lock().unwrap() {
            return Err("Timer listener already active".to_string());
        }

        let (sender, receiver) = mpsc::channel();
        *self.active.lock().unwrap() = true;

        let node_id = self.node_id.clone();
        let interval_ms = self.interval_ms;
        let active = Arc::clone(&self.active);

        // Spawn a thread that sends tick events at intervals
        let handle = thread::spawn(move || {
            let mut tick_count: u64 = 0;

            while *active.lock().unwrap() {
                // Send tick event
                let timestamp = Self::get_timestamp();
                let event = ExecutionEvent::TimerTick {
                    node_id: node_id.clone(),
                    tick_count,
                    timestamp,
                };

                if sender.send(event).is_err() {
                    // Receiver dropped, stop the timer
                    break;
                }

                tick_count += 1;

                // Sleep for the interval
                thread::sleep(Duration::from_millis(interval_ms));
            }
        });

        self.thread_handle = Some(handle);

        Ok(receiver)
    }

    fn stop(&mut self) -> Result<(), String> {
        if !*self.active.lock().unwrap() {
            return Ok(());
        }

        // Signal the thread to stop
        *self.active.lock().unwrap() = false;

        // Wait for the thread to finish
        if let Some(handle) = self.thread_handle.take() {
            handle.join().map_err(|_| "Failed to join timer thread".to_string())?;
        }

        Ok(())
    }

    fn is_active(&self) -> bool {
        *self.active.lock().unwrap()
    }

    fn node_id(&self) -> &str {
        &self.node_id
    }

    fn listener_type(&self) -> &str {
        "Timer"
    }
}

impl Drop for TimerListener {
    fn drop(&mut self) {
        let _ = self.stop();
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::time::Instant;

    #[test]
    fn test_timer_listener() {
        let mut listener = TimerListener::new("test_node".to_string(), 100);

        // Should not be active initially
        assert!(!listener.is_active());

        // Start the listener
        let receiver = listener.start().unwrap();
        assert!(listener.is_active());

        // Wait for a few ticks
        thread::sleep(Duration::from_millis(350));

        // Should have received 3-4 events (accounting for timing variations)
        let mut event_count = 0;
        while let Ok(event) = receiver.try_recv() {
            match event {
                ExecutionEvent::TimerTick {
                    node_id,
                    tick_count,
                    ..
                } => {
                    assert_eq!(node_id, "test_node");
                    assert_eq!(tick_count, event_count);
                    event_count += 1;
                }
                _ => panic!("Expected TimerTick event"),
            }
        }

        assert!(event_count >= 3 && event_count <= 4, "Expected 3-4 ticks, got {}", event_count);

        // Stop the listener
        listener.stop().unwrap();
        assert!(!listener.is_active());

        // Wait a bit to ensure no more events are sent
        thread::sleep(Duration::from_millis(200));
        assert!(receiver.try_recv().is_err(), "Should not receive events after stop");
    }

    #[test]
    fn test_timer_interval_accuracy() {
        let mut listener = TimerListener::new("test_node".to_string(), 50);
        let receiver = listener.start().unwrap();

        let start = Instant::now();

        // Wait for exactly 5 ticks
        let mut ticks = 0;
        while ticks < 5 {
            if let Ok(event) = receiver.recv_timeout(Duration::from_millis(100)) {
                match event {
                    ExecutionEvent::TimerTick { .. } => {
                        ticks += 1;
                    }
                    _ => {}
                }
            }
        }

        let elapsed = start.elapsed();

        // Should take approximately 250ms (5 ticks * 50ms interval)
        // Allow 50ms tolerance for thread scheduling
        assert!(
            elapsed.as_millis() >= 200 && elapsed.as_millis() <= 300,
            "Expected ~250ms, got {}ms",
            elapsed.as_millis()
        );

        listener.stop().unwrap();
    }
}
