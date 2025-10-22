use super::emission_contexts::EmissionContext;
use super::events::ExecutionEvent;
use std::sync::mpsc;

pub mod manual;
pub mod timer;

/// Trait for event listeners that can trigger node execution
pub trait EventListener: Send {
    /// Start listening for events
    /// Returns a receiver for ExecutionEvents
    fn start(&mut self) -> Result<mpsc::Receiver<ExecutionEvent>, String>;

    /// Stop listening for events
    fn stop(&mut self) -> Result<(), String>;

    /// Check if the listener is currently active
    fn is_active(&self) -> bool;

    /// Get the node ID this listener is associated with
    fn node_id(&self) -> &str;

    /// Get the listener type name
    fn listener_type(&self) -> &str;
}

/// Registry for managing all active emission contexts for a graph
pub struct ListenerRegistry {
    pub listeners: Vec<Box<dyn EmissionContext>>,
    event_receiver: Option<mpsc::Receiver<ExecutionEvent>>,
}

impl ListenerRegistry {
    pub fn new() -> Self {
        Self {
            listeners: Vec::new(),
            event_receiver: None,
        }
    }

    /// Create a new registry with a manual trigger channel
    pub fn new_with_trigger_channel() -> (Self, mpsc::Sender<ExecutionEvent>) {
        let (sender, receiver) = mpsc::channel();

        let registry = Self {
            listeners: Vec::new(),
            event_receiver: Some(receiver),
        };

        (registry, sender)
    }

    /// Create a new registry with an existing event receiver
    pub fn new_with_receiver(event_receiver: mpsc::Receiver<ExecutionEvent>) -> Self {
        Self {
            listeners: Vec::new(),
            event_receiver: Some(event_receiver),
        }
    }

    // NOTE: Old EventListener-based register method - kept for reference
    // The new architecture uses EmissionContext directly
    /*
    /// Register a listener and start it
    pub fn register(&mut self, mut listener: Box<dyn EventListener>) -> Result<(), String> {
        let receiver = listener.start()?;

        // Store the receiver (we'll merge multiple receivers later)
        if self.event_receiver.is_none() {
            self.event_receiver = Some(receiver);
        } else {
            // For now, we'll implement a simple approach
            // In a real system, we'd merge multiple receivers
            return Err("Multiple listeners not yet supported - need receiver merging".to_string());
        }

        self.listeners.push(listener);
        Ok(())
    }
    */

    /// Stop all emission contexts
    pub fn stop_all(&mut self) -> Result<(), String> {
        for context in &mut self.listeners {
            context.stop()?;
        }
        self.event_receiver = None;
        Ok(())
    }

    /// Get the event receiver (if any contexts are active)
    pub fn event_receiver(&mut self) -> Option<&mpsc::Receiver<ExecutionEvent>> {
        self.event_receiver.as_ref()
    }

    /// Get count of active contexts
    pub fn active_count(&self) -> usize {
        self.listeners.iter().filter(|c| c.is_active()).count()
    }

    /// Check if any contexts are active
    pub fn has_active_listeners(&self) -> bool {
        self.listeners.iter().any(|c| c.is_active())
    }
}

impl Default for ListenerRegistry {
    fn default() -> Self {
        Self::new()
    }
}

// NOTE: Old EventListener-based function - kept for reference
// The new architecture creates EmissionContexts directly in Engine::start()
/*
/// Create listeners for all self-emitting nodes in a graph
pub fn create_listeners_for_graph(graph: &Graph) -> Result<ListenerRegistry, String> {
    let mut registry = ListenerRegistry::new();

    for node in &graph.nodes {
        if let Some(brick) = &node.data.brick {
            match &brick.emission_type {
                crate::bricks::types::BrickEmissionType::ManualTrigger => {
                    let listener = Box::new(manual::ManualTriggerListener::new(node.id.clone()));
                    registry.register(listener)?;
                }
                crate::bricks::types::BrickEmissionType::Timer { default_interval_ms } => {
                    let interval_ms = node.data.arguments
                        .get("interval_ms")
                        .and_then(|v| v.parse::<u64>().ok())
                        .unwrap_or(*default_interval_ms as u64);

                    let listener = Box::new(timer::TimerListener::new(
                        node.id.clone(),
                        interval_ms,
                    ));
                    registry.register(listener)?;
                }
                crate::bricks::types::BrickEmissionType::FlowTriggered => {
                    // No listener needed for flow-triggered bricks
                }
                _ => {
                    // Other emission types not yet implemented
                }
            }
        }
    }

    Ok(registry)
}
*/

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_listener_registry_creation() {
        let registry = ListenerRegistry::new();
        assert_eq!(registry.active_count(), 0);
        assert!(!registry.has_active_listeners());
    }
}
