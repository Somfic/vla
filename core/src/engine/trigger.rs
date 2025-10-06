use std::cell::RefCell;

/// Thread-local storage for execution triggers
/// This stores the execution output IDs that should be triggered when a brick executes
thread_local! {
    static EXECUTION_TRIGGERS: RefCell<Vec<String>> = RefCell::new(Vec::new());
    static CURRENT_NODE_ID: RefCell<Option<String>> = RefCell::new(None);
}

/// Set the current node ID for trigger context
/// Called by ExecutionEngine before executing a brick
pub fn set_current_node_id(node_id: &str) {
    CURRENT_NODE_ID.with(|current| {
        *current.borrow_mut() = Some(node_id.to_string());
    });
}

/// Clear the current node ID
/// Called by ExecutionEngine after executing a brick
pub fn clear_current_node_id() {
    CURRENT_NODE_ID.with(|current| {
        *current.borrow_mut() = None;
    });
}

/// Internal function called by trigger! macro
/// Adds an execution output ID to the list of triggers for the current execution
/// Automatically prefixes with the current node ID if available
pub fn add_trigger(output_id: &str) {
    let prefixed_id = CURRENT_NODE_ID.with(|current| {
        if let Some(node_id) = current.borrow().as_ref() {
            format!("{}:{}", node_id, output_id)
        } else {
            output_id.to_string()
        }
    });

    EXECUTION_TRIGGERS.with(|triggers| {
        triggers.borrow_mut().push(prefixed_id);
    });
}

/// Collect and clear all triggers set during brick execution
/// Called by ExecutionEngine after each brick execution
pub fn collect_and_clear_triggers() -> Vec<String> {
    EXECUTION_TRIGGERS.with(|triggers| triggers.borrow_mut().drain(..).collect())
}

/// Clear triggers without collecting (for cleanup)
pub fn clear_triggers() {
    EXECUTION_TRIGGERS.with(|triggers| {
        triggers.borrow_mut().clear();
    });
}

/// Check if any triggers are currently set (for testing/debugging)
pub fn has_triggers() -> bool {
    EXECUTION_TRIGGERS.with(|triggers| !triggers.borrow().is_empty())
}

/// Get the number of triggers currently set (for testing/debugging)
pub fn trigger_count() -> usize {
    EXECUTION_TRIGGERS.with(|triggers| triggers.borrow().len())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_trigger_functions() {
        // Clear any existing triggers
        clear_triggers();
        assert_eq!(trigger_count(), 0);
        assert!(!has_triggers());

        // Add some triggers
        add_trigger("test1");
        add_trigger("test2");
        assert_eq!(trigger_count(), 2);
        assert!(has_triggers());

        // Collect and clear
        let triggers = collect_and_clear_triggers();
        assert_eq!(triggers, vec!["test1", "test2"]);
        assert_eq!(trigger_count(), 0);
        assert!(!has_triggers());
    }

    #[test]
    fn test_clear_triggers() {
        add_trigger("test");
        assert!(has_triggers());

        clear_triggers();
        assert!(!has_triggers());
    }

    #[test]
    fn test_node_prefixed_triggers() {
        // Clear any existing state
        clear_triggers();
        clear_current_node_id();

        // Test without node context
        add_trigger("output1");
        let triggers = collect_and_clear_triggers();
        assert_eq!(triggers, vec!["output1"]);

        // Test with node context
        set_current_node_id("node123");
        add_trigger("output1");
        add_trigger("output2");
        let triggers = collect_and_clear_triggers();
        assert_eq!(triggers, vec!["node123:output1", "node123:output2"]);

        // Test clearing node context
        clear_current_node_id();
        add_trigger("output3");
        let triggers = collect_and_clear_triggers();
        assert_eq!(triggers, vec!["output3"]);
    }
}
