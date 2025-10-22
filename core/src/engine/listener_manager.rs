use super::listeners::{manual::ManualTriggerListener, ListenerRegistry};
use std::collections::HashMap;
use std::sync::{Arc, Mutex};

/// Global manager for listener registries across different graph executions
pub struct ListenerManager {
    registries: Arc<Mutex<HashMap<String, Arc<Mutex<ListenerRegistry>>>>>,
}

impl ListenerManager {
    pub fn new() -> Self {
        Self {
            registries: Arc::new(Mutex::new(HashMap::new())),
        }
    }

    /// Get or create a registry for a graph execution
    pub fn get_registry(&self, execution_id: &str) -> Arc<Mutex<ListenerRegistry>> {
        let mut registries = self.registries.lock().unwrap();
        registries
            .entry(execution_id.to_string())
            .or_insert_with(|| Arc::new(Mutex::new(ListenerRegistry::new())))
            .clone()
    }

    /// Remove a registry when execution completes
    pub fn remove_registry(&self, execution_id: &str) {
        let mut registries = self.registries.lock().unwrap();
        if let Some(registry) = registries.remove(execution_id) {
            // Stop all listeners
            let _ = registry.lock().unwrap().stop_all();
        }
    }

    /// Manually trigger a node in a specific execution
    pub fn trigger_node(&self, execution_id: &str, node_id: &str) -> Result<(), String> {
        let registries = self.registries.lock().unwrap();
        let registry = registries
            .get(execution_id)
            .ok_or_else(|| format!("No active execution with id: {}", execution_id))?;

        let registry = registry.lock().unwrap();

        // Find the manual trigger listener for this node
        for listener in &registry.listeners {
            if listener.node_id() == node_id && listener.listener_type() == "ManualTrigger" {
                // We need to downcast to ManualTriggerListener to call trigger()
                // This is a limitation of our current design - we'll need to add trigger to the trait
                // or handle it differently
                return Err("Manual triggering not yet fully implemented".to_string());
            }
        }

        Err(format!("No manual trigger listener found for node: {}", node_id))
    }

    /// Get list of active executions
    pub fn active_executions(&self) -> Vec<String> {
        let registries = self.registries.lock().unwrap();
        registries.keys().cloned().collect()
    }
}

impl Default for ListenerManager {
    fn default() -> Self {
        Self::new()
    }
}

// Global singleton instance
lazy_static::lazy_static! {
    pub static ref LISTENER_MANAGER: ListenerManager = ListenerManager::new();
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_listener_manager() {
        let manager = ListenerManager::new();

        // Get a registry for a new execution
        let registry = manager.get_registry("exec_1");
        assert_eq!(manager.active_executions().len(), 1);

        // Remove the registry
        manager.remove_registry("exec_1");
        assert_eq!(manager.active_executions().len(), 0);
    }
}
