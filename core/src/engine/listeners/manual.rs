use super::EventListener;
use crate::engine::events::ExecutionEvent;
use std::sync::mpsc::{self, Receiver, Sender};
use std::sync::{Arc, Mutex};

/// Listener for manual trigger events
/// Can be triggered programmatically via the trigger() method
pub struct ManualTriggerListener {
    node_id: String,
    active: bool,
    sender: Option<Arc<Mutex<Sender<ExecutionEvent>>>>,
}

impl ManualTriggerListener {
    pub fn new(node_id: String) -> Self {
        Self {
            node_id,
            active: false,
            sender: None,
        }
    }

    /// Manually trigger an event for this node
    pub fn trigger(&self) -> Result<(), String> {
        if !self.active {
            return Err("Listener not active".to_string());
        }

        if let Some(sender) = &self.sender {
            let timestamp = Self::get_timestamp();
            let event = ExecutionEvent::ManualTrigger {
                node_id: self.node_id.clone(),
                timestamp,
            };

            sender
                .lock()
                .unwrap()
                .send(event)
                .map_err(|e| format!("Failed to send event: {}", e))?;

            Ok(())
        } else {
            Err("Sender not initialized".to_string())
        }
    }

    fn get_timestamp() -> String {
        use std::time::{SystemTime, UNIX_EPOCH};
        let duration = SystemTime::now().duration_since(UNIX_EPOCH).unwrap();
        format!("{}.{:03}", duration.as_secs(), duration.subsec_millis())
    }
}

impl EventListener for ManualTriggerListener {
    fn start(&mut self) -> Result<Receiver<ExecutionEvent>, String> {
        if self.active {
            return Err("Listener already active".to_string());
        }

        let (sender, receiver) = mpsc::channel();
        self.sender = Some(Arc::new(Mutex::new(sender)));
        self.active = true;

        Ok(receiver)
    }

    fn stop(&mut self) -> Result<(), String> {
        if !self.active {
            return Ok(());
        }

        self.sender = None;
        self.active = false;
        Ok(())
    }

    fn is_active(&self) -> bool {
        self.active
    }

    fn node_id(&self) -> &str {
        &self.node_id
    }

    fn listener_type(&self) -> &str {
        "ManualTrigger"
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_manual_trigger_listener() {
        let mut listener = ManualTriggerListener::new("test_node".to_string());

        // Should not be active initially
        assert!(!listener.is_active());

        // Start the listener
        let receiver = listener.start().unwrap();
        assert!(listener.is_active());

        // Trigger an event
        listener.trigger().unwrap();

        // Should receive the event
        let event = receiver.try_recv().unwrap();
        match event {
            ExecutionEvent::ManualTrigger { node_id, .. } => {
                assert_eq!(node_id, "test_node");
            }
            _ => panic!("Expected ManualTrigger event"),
        }

        // Stop the listener
        listener.stop().unwrap();
        assert!(!listener.is_active());
    }

    #[test]
    fn test_trigger_when_inactive() {
        let listener = ManualTriggerListener::new("test_node".to_string());

        // Should fail to trigger when not active
        assert!(listener.trigger().is_err());
    }
}
