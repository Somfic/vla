use crate::bricks::macros::brick;
use crate::engine::trigger;
use crate::prelude::*;
use crate::trigger;

pub fn all_bricks() -> Vec<Brick> {
    vec![manual_trigger_brick(), timer_brick()]
}

// Manual trigger brick - can be triggered from UI
brick! {
    #[id("manual_trigger")]
    #[label("Manual Trigger")]
    #[description("Triggers execution when manually activated from the UI")]
    #[keywords(&["trigger", "manual", "start", "button"])]
    #[category("Events")]
    #[emission_type(ManualTrigger)]
    #[execution_output("triggered", "Triggered")]
    fn manual_trigger() -> (
        #[label("Timestamp")] String
    ) {
        let ctx = trigger::get_execution_context();
        let timestamp = match ctx.manual_trigger_timestamp() {
            Some(ts) => ts.to_string(),
            None => {
                // Use system time as fallback
                use std::time::{SystemTime, UNIX_EPOCH};
                let duration = SystemTime::now().duration_since(UNIX_EPOCH).unwrap();
                format!("{}.{:03}", duration.as_secs(), duration.subsec_millis())
            }
        };

        trigger!("triggered");
        (timestamp,)
    }
}

// Timer brick - emits at regular intervals
brick! {
    #[id("timer")]
    #[label("Timer")]
    #[description("Triggers execution at regular intervals")]
    #[keywords(&["timer", "interval", "repeat", "periodic"])]
    #[category("Events")]
    #[emission_type(Timer { default_interval_ms: 1000 })]
    #[execution_output("tick", "Tick")]
    fn timer(
        #[argument] #[label("Interval (ms)")] interval_ms: String = "1000"
    ) -> (
        #[label("Tick Count")] String,
        #[label("Timestamp")] String
    ) {
        let ctx = trigger::get_execution_context();
        let (tick_count, timestamp) = match ctx.timer_tick() {
            Some((count, ts)) => (count.to_string(), ts.to_string()),
            None => {
                // Use system time as fallback
                use std::time::{SystemTime, UNIX_EPOCH};
                let duration = SystemTime::now().duration_since(UNIX_EPOCH).unwrap();
                let ts = format!("{}.{:03}", duration.as_secs(), duration.subsec_millis());
                ("0".to_string(), ts)
            }
        };

        trigger!("tick");
        (tick_count, timestamp)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_manual_trigger_brick_metadata() {
        let brick = manual_trigger_brick();
        assert_eq!(brick.id, "manual_trigger");
        assert_eq!(brick.label, "Manual Trigger");
        assert_eq!(brick.category, "Events");

        // Check emission_type is ManualTrigger
        assert!(matches!(
            &brick.emission_type,
            crate::bricks::types::BrickEmissionType::ManualTrigger
        ));

        // Should have one execution output
        assert_eq!(brick.execution_outputs.len(), 1);
        assert_eq!(brick.execution_outputs[0].id, "triggered");

        // Should have no execution inputs (self-emitting)
        assert_eq!(brick.execution_inputs.len(), 0);

        // Should have one output
        assert_eq!(brick.outputs.len(), 1);
    }

    #[test]
    fn test_timer_brick_metadata() {
        let brick = timer_brick();
        assert_eq!(brick.id, "timer");
        assert_eq!(brick.label, "Timer");
        assert_eq!(brick.category, "Events");

        // Check emission_type is Timer with correct interval
        assert!(matches!(
            &brick.emission_type,
            crate::bricks::types::BrickEmissionType::Timer { default_interval_ms: 1000 }
        ));

        // Should have one execution output
        assert_eq!(brick.execution_outputs.len(), 1);
        assert_eq!(brick.execution_outputs[0].id, "tick");

        // Should have no execution inputs (self-emitting)
        assert_eq!(brick.execution_inputs.len(), 0);

        // Should have two outputs (tick_count and timestamp)
        assert_eq!(brick.outputs.len(), 2);
    }
}
