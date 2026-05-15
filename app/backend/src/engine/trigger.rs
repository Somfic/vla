//! Minimal no-op shim of the execution engine's trigger/context surface.
//!
//! This pre-stages `crate::engine::trigger` so the ported brick files compile
//! without the execution engine. The real engine port will replace this module
//! wholesale; the public surface here mirrors what the bricks expect.
#![allow(dead_code)]

/// The context in which a brick is being executed.
///
/// Bricks read this to adapt their behaviour (e.g. a timer brick reading the
/// tick count). The real engine populates it per execution; the shim always
/// reports `FlowTriggered`.
#[derive(Debug, Clone, Default)]
pub enum ExecutionContext {
    #[default]
    FlowTriggered,
    HttpRequest,
    TimerTick {
        tick_count: u64,
        timestamp: String,
    },
    FileChanged {
        path: String,
        event_type: String,
    },
    ManualTrigger {
        timestamp: String,
    },
}

impl ExecutionContext {
    pub fn timer_tick(&self) -> Option<(u64, &str)> {
        match self {
            ExecutionContext::TimerTick {
                tick_count,
                timestamp,
            } => Some((*tick_count, timestamp.as_str())),
            _ => None,
        }
    }

    pub fn manual_trigger_timestamp(&self) -> Option<&str> {
        match self {
            ExecutionContext::ManualTrigger { timestamp } => Some(timestamp.as_str()),
            _ => None,
        }
    }
}

/// No-op. The real engine records the triggered execution-output handle here.
pub fn add_trigger(_output_id: &str) {}

/// No-op. The real engine records the id of the node currently executing.
pub fn set_current_node_id(_node_id: &str) {}

/// Shim. The real engine returns the context the current brick is running in.
pub fn get_execution_context() -> ExecutionContext {
    ExecutionContext::FlowTriggered
}

/// Shim. The real engine returns how many triggers have been recorded.
pub fn trigger_count() -> usize {
    0
}
