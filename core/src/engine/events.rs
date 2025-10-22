use serde::{Deserialize, Serialize};

/// Represents all types of events that can trigger node execution
#[derive(Debug, Clone, Serialize, Deserialize, specta::Type)]
pub enum ExecutionEvent {
    /// Traditional flow-based execution trigger
    /// Occurs when a node completes and triggers its execution output
    NodeTriggered {
        node_id: String,
        trigger_output: String,
    },

    /// HTTP webhook request received
    HttpRequest {
        node_id: String,
        request: HttpRequestData,
    },

    /// Timer/interval tick
    TimerTick {
        node_id: String,
        tick_count: u64,
        timestamp: String,
    },

    /// File system change detected
    FileChanged {
        node_id: String,
        path: String,
        event_type: FileEventType,
    },

    /// Manual trigger from UI
    ManualTrigger {
        node_id: String,
        timestamp: String,
    },

    // Future event types (speech detection, database triggers, etc.)
    // SpeechDetected { node_id: String, transcript: String },
    // DatabaseChanged { node_id: String, table: String, operation: String },
}

impl ExecutionEvent {
    /// Get the target node ID for this event
    pub fn target_node_id(&self) -> &str {
        match self {
            ExecutionEvent::NodeTriggered { node_id, .. } => node_id,
            ExecutionEvent::HttpRequest { node_id, .. } => node_id,
            ExecutionEvent::TimerTick { node_id, .. } => node_id,
            ExecutionEvent::FileChanged { node_id, .. } => node_id,
            ExecutionEvent::ManualTrigger { node_id, .. } => node_id,
        }
    }

    /// Check if this is a flow-based trigger (from another node)
    pub fn is_flow_trigger(&self) -> bool {
        matches!(self, ExecutionEvent::NodeTriggered { .. })
    }

    /// Check if this is a self-emission trigger (external event)
    pub fn is_self_emission(&self) -> bool {
        !self.is_flow_trigger()
    }
}

/// HTTP request data for webhook events
#[derive(Debug, Clone, Serialize, Deserialize, specta::Type)]
pub struct HttpRequestData {
    pub method: String,
    pub path: String,
    pub body: String,
    pub headers: String, // JSON-encoded headers
    pub query: String,   // JSON-encoded query parameters
}

impl Default for HttpRequestData {
    fn default() -> Self {
        Self {
            method: "GET".to_string(),
            path: "/".to_string(),
            body: String::new(),
            headers: "{}".to_string(),
            query: "{}".to_string(),
        }
    }
}

/// File system event types
#[derive(Debug, Clone, Serialize, Deserialize, specta::Type)]
pub enum FileEventType {
    Created,
    Modified,
    Deleted,
    Renamed,
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_event_target_node() {
        let event = ExecutionEvent::ManualTrigger {
            node_id: "test_node".to_string(),
            timestamp: "2024-01-01T00:00:00Z".to_string(),
        };
        assert_eq!(event.target_node_id(), "test_node");

        let event2 = ExecutionEvent::NodeTriggered {
            node_id: "flow_node".to_string(),
            trigger_output: "output1".to_string(),
        };
        assert_eq!(event2.target_node_id(), "flow_node");
    }

    #[test]
    fn test_event_type_detection() {
        let flow_event = ExecutionEvent::NodeTriggered {
            node_id: "node1".to_string(),
            trigger_output: "out".to_string(),
        };
        assert!(flow_event.is_flow_trigger());
        assert!(!flow_event.is_self_emission());

        let self_emit_event = ExecutionEvent::ManualTrigger {
            node_id: "node2".to_string(),
            timestamp: "2024-01-01T00:00:00Z".to_string(),
        };
        assert!(!self_emit_event.is_flow_trigger());
        assert!(self_emit_event.is_self_emission());
    }
}
