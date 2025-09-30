use std::cell::RefCell;

/// Thread-local storage for execution triggers
/// This stores the execution output IDs that should be triggered when a brick executes
thread_local! {
    static EXECUTION_TRIGGERS: RefCell<Vec<String>> = RefCell::new(Vec::new());
}

/// Internal function called by trigger! macro
/// Adds an execution output ID to the list of triggers for the current execution
pub fn add_trigger(output_id: &str) {
    EXECUTION_TRIGGERS.with(|triggers| {
        triggers.borrow_mut().push(output_id.to_string());
    });
}

/// Collect and clear all triggers set during brick execution
/// Called by ExecutionEngine after each brick execution
pub fn collect_and_clear_triggers() -> Vec<String> {
    EXECUTION_TRIGGERS.with(|triggers| {
        triggers.borrow_mut().drain(..).collect()
    })
}

/// Clear triggers without collecting (for cleanup)
pub fn clear_triggers() {
    EXECUTION_TRIGGERS.with(|triggers| {
        triggers.borrow_mut().clear();
    });
}

/// Check if any triggers are currently set (for testing/debugging)
pub fn has_triggers() -> bool {
    EXECUTION_TRIGGERS.with(|triggers| {
        !triggers.borrow().is_empty()
    })
}

/// Get the number of triggers currently set (for testing/debugging)
pub fn trigger_count() -> usize {
    EXECUTION_TRIGGERS.with(|triggers| {
        triggers.borrow().len()
    })
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
}