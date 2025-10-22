use super::events::{ExecutionEvent, FileEventType, HttpRequestData};
use std::cell::RefCell;

/// Represents an execution trigger from a flow node
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Trigger {
    pub source_node: String,
    pub output_id: String,
}

impl Trigger {
    pub fn new(source_node: String, output_id: String) -> Self {
        Self {
            source_node,
            output_id,
        }
    }

    /// Returns the execution output handle for matching edges
    pub fn to_handle(&self) -> String {
        self.output_id.clone()
    }
}

/// Execution context that provides access to event data during brick execution
/// Each variant corresponds to a specific emission type's data
///
/// New emission types can be added here without modifying existing code.
/// Example: SpeechRecognized { transcript: String, confidence: f32 }
#[derive(Debug, Clone)]
pub enum ExecutionContext {
    FlowTriggered,
    HttpRequest(HttpRequestData),
    TimerTick {
        tick_count: u64,
        timestamp: String,
    },
    FileChanged {
        path: String,
        event_type: FileEventType,
    },
    ManualTrigger {
        timestamp: String,
    },
    // Future: Add new variants as needed for new emission types
}

impl Default for ExecutionContext {
    fn default() -> Self {
        Self::FlowTriggered
    }
}

impl ExecutionContext {
    /// Create context from an execution event
    pub fn from_event(event: &ExecutionEvent) -> Self {
        match event {
            ExecutionEvent::NodeTriggered { .. } => ExecutionContext::FlowTriggered,
            ExecutionEvent::HttpRequest { request, .. } => {
                ExecutionContext::HttpRequest(request.clone())
            }
            ExecutionEvent::TimerTick {
                tick_count,
                timestamp,
                ..
            } => ExecutionContext::TimerTick {
                tick_count: *tick_count,
                timestamp: timestamp.clone(),
            },
            ExecutionEvent::FileChanged {
                path, event_type, ..
            } => ExecutionContext::FileChanged {
                path: path.clone(),
                event_type: event_type.clone(),
            },
            ExecutionEvent::ManualTrigger { timestamp, .. } => ExecutionContext::ManualTrigger {
                timestamp: timestamp.clone(),
            },
        }
    }

    /// Get HTTP request data (if this is an HTTP event)
    pub fn http_request(&self) -> Option<&HttpRequestData> {
        match self {
            ExecutionContext::HttpRequest(data) => Some(data),
            _ => None,
        }
    }

    /// Get timer tick data (if this is a timer event)
    pub fn timer_tick(&self) -> Option<(u64, &str)> {
        match self {
            ExecutionContext::TimerTick {
                tick_count,
                timestamp,
            } => Some((*tick_count, timestamp.as_str())),
            _ => None,
        }
    }

    /// Get file change data (if this is a file event)
    pub fn file_change(&self) -> Option<(&str, &FileEventType)> {
        match self {
            ExecutionContext::FileChanged { path, event_type } => Some((path.as_str(), event_type)),
            _ => None,
        }
    }

    /// Get manual trigger timestamp (if this is a manual trigger)
    pub fn manual_trigger_timestamp(&self) -> Option<&str> {
        match self {
            ExecutionContext::ManualTrigger { timestamp } => Some(timestamp.as_str()),
            _ => None,
        }
    }
}

thread_local! {
    static EXECUTION_TRIGGERS: RefCell<Vec<Trigger>> = RefCell::new(Vec::new());
    static CURRENT_NODE_ID: RefCell<Option<String>> = RefCell::new(None);
    static EXECUTION_CONTEXT: RefCell<ExecutionContext> = RefCell::new(ExecutionContext::default());
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

/// Collect and clear the current node ID
/// Returns the current node ID before clearing it
pub fn collect_and_clear_current_node_id() -> Option<String> {
    CURRENT_NODE_ID.with(|current| current.borrow_mut().take())
}

/// Internal function called by trigger! macro
/// Adds an execution output ID to the list of triggers for the current execution
/// Automatically uses the current node ID if available
pub fn add_trigger(output_id: &str) {
    let trigger = CURRENT_NODE_ID.with(|current| {
        if let Some(node_id) = current.borrow().as_ref() {
            Trigger::new(node_id.clone(), output_id.to_string())
        } else {
            // Fallback if no node context (shouldn't happen in normal execution)
            Trigger::new(String::new(), output_id.to_string())
        }
    });

    EXECUTION_TRIGGERS.with(|triggers| {
        triggers.borrow_mut().push(trigger);
    });
}

/// Collect and clear all triggers set during brick execution
/// Called by ExecutionEngine after each brick execution
pub fn collect_and_clear_triggers() -> Vec<Trigger> {
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

/// Set the execution context for the current brick execution
/// Called by ExecutionEngine before executing a brick
pub fn set_execution_context(context: ExecutionContext) {
    EXECUTION_CONTEXT.with(|ctx| {
        *ctx.borrow_mut() = context;
    });
}

/// Get a copy of the current execution context
/// Called by bricks that need access to event data
pub fn get_execution_context() -> ExecutionContext {
    EXECUTION_CONTEXT.with(|ctx| ctx.borrow().clone())
}

/// Clear the execution context
/// Called by ExecutionEngine after executing a brick
pub fn clear_execution_context() {
    EXECUTION_CONTEXT.with(|ctx| {
        *ctx.borrow_mut() = ExecutionContext::default();
    });
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_trigger_functions() {
        // Clear any existing triggers
        clear_triggers();
        clear_current_node_id();
        assert_eq!(trigger_count(), 0);
        assert!(!has_triggers());

        // Add some triggers (with node context)
        set_current_node_id("test_node");
        add_trigger("test1");
        add_trigger("test2");
        assert_eq!(trigger_count(), 2);
        assert!(has_triggers());

        // Collect and clear
        let triggers = collect_and_clear_triggers();
        assert_eq!(
            triggers,
            vec![
                Trigger::new("test_node".to_string(), "test1".to_string()),
                Trigger::new("test_node".to_string(), "test2".to_string())
            ]
        );
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
        assert_eq!(
            triggers,
            vec![Trigger::new(String::new(), "output1".to_string())]
        );

        // Test with node context
        set_current_node_id("node123");
        add_trigger("output1");
        add_trigger("output2");
        let triggers = collect_and_clear_triggers();
        assert_eq!(
            triggers,
            vec![
                Trigger::new("node123".to_string(), "output1".to_string()),
                Trigger::new("node123".to_string(), "output2".to_string())
            ]
        );

        // Test clearing node context
        clear_current_node_id();
        add_trigger("output3");
        let triggers = collect_and_clear_triggers();
        assert_eq!(
            triggers,
            vec![Trigger::new(String::new(), "output3".to_string())]
        );
    }

    #[test]
    fn test_trigger_to_handle() {
        let trigger = Trigger::new("node1".to_string(), "begin".to_string());
        assert_eq!(trigger.to_handle(), "begin");

        let trigger2 = Trigger::new("node2".to_string(), "true_branch".to_string());
        assert_eq!(trigger2.to_handle(), "true_branch");
    }
}
