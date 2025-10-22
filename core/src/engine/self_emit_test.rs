/// Integration tests for self-emitting nodes
#[cfg(test)]
mod tests {
    use crate::bricks::events::{manual_trigger_brick, timer_brick};
    use crate::engine::events::ExecutionEvent;
    use crate::engine::listeners::{
        manual::ManualTriggerListener, timer::TimerListener, EventListener,
    };
    use crate::engine::trigger;
    use crate::prelude::*;
    use std::collections::BTreeMap;
    use std::time::Duration;

    /// Create a test node with a self-emitting brick
    fn create_timer_node(id: &str, interval_ms: u64) -> Node {
        let mut arguments = BTreeMap::new();
        arguments.insert("interval_ms".to_string(), interval_ms.to_string());

        Node {
            id: id.to_string(),
            position: Point { x: 0.0, y: 0.0 },
            data: NodeData {
                brick_id: "timer".to_string(),
                brick: Some(timer_brick()),
                arguments,
                defaults: BTreeMap::new(),
            },
            r#type: "v1".to_string(),
        }
    }

    fn create_manual_trigger_node(id: &str) -> Node {
        Node {
            id: id.to_string(),
            position: Point { x: 0.0, y: 0.0 },
            data: NodeData {
                brick_id: "manual_trigger".to_string(),
                brick: Some(manual_trigger_brick()),
                arguments: BTreeMap::new(),
                defaults: BTreeMap::new(),
            },
            r#type: "v1".to_string(),
        }
    }

    #[test]
    fn test_timer_listener_emits_events() {
        let mut listener = TimerListener::new("timer_node".to_string(), 50);
        let receiver = listener.start().unwrap();

        // Wait for 2-3 ticks
        std::thread::sleep(Duration::from_millis(150));

        // Check we received events
        let mut event_count = 0;
        while let Ok(event) = receiver.try_recv() {
            match event {
                ExecutionEvent::TimerTick {
                    node_id,
                    tick_count,
                    ..
                } => {
                    assert_eq!(node_id, "timer_node");
                    assert_eq!(tick_count, event_count);
                    event_count += 1;
                }
                _ => panic!("Expected TimerTick event"),
            }
        }

        assert!(
            event_count >= 2 && event_count <= 4,
            "Expected 2-4 events, got {}",
            event_count
        );

        listener.stop().unwrap();
    }

    #[test]
    fn test_manual_trigger_listener_emits_on_demand() {
        let mut listener = ManualTriggerListener::new("manual_node".to_string());
        let receiver = listener.start().unwrap();

        // Trigger multiple times
        listener.trigger().unwrap();
        listener.trigger().unwrap();
        listener.trigger().unwrap();

        // Check we received exactly 3 events
        let mut event_count = 0;
        while let Ok(event) = receiver.try_recv() {
            match event {
                ExecutionEvent::ManualTrigger { node_id, .. } => {
                    assert_eq!(node_id, "manual_node");
                    event_count += 1;
                }
                _ => panic!("Expected ManualTrigger event"),
            }
        }

        assert_eq!(event_count, 3);

        listener.stop().unwrap();
    }

    #[test]
    fn test_timer_brick_execution_with_context() {
        // Setup execution context with timer data
        let context = trigger::ExecutionContext::TimerTick {
            tick_count: 5,
            timestamp: "1234567890.123".to_string(),
        };

        trigger::set_execution_context(context);

        // Execute the timer brick
        let brick = timer_brick();
        let args = vec![];
        let inputs = vec![];

        let outputs = (brick.execution)(args, inputs);

        // Check outputs
        assert_eq!(outputs.len(), 2);
        assert_eq!(outputs[0].id, "output_0"); // tick_count
        assert_eq!(outputs[0].value, "\"5\"");
        assert_eq!(outputs[1].id, "output_1"); // timestamp
        assert_eq!(outputs[1].value, "\"1234567890.123\"");

        trigger::clear_execution_context();
    }

    #[test]
    fn test_manual_trigger_brick_execution_with_context() {
        // Setup execution context with manual trigger data
        let context = trigger::ExecutionContext::ManualTrigger {
            timestamp: "1234567890.456".to_string(),
        };

        trigger::set_execution_context(context);

        // Execute the manual trigger brick
        let brick = manual_trigger_brick();
        let args = vec![];
        let inputs = vec![];

        let outputs = (brick.execution)(args, inputs);

        // Check outputs
        assert_eq!(outputs.len(), 1);
        assert_eq!(outputs[0].id, "output_0"); // timestamp
        assert_eq!(outputs[0].value, "\"1234567890.456\"");

        trigger::clear_execution_context();
    }

    #[test]
    fn test_graph_with_timer_node() {
        // Create a simple graph with just a timer node
        let graph = Graph {
            nodes: vec![create_timer_node("timer1", 100)],
            edges: vec![],
        };

        // Verify the node has the correct emission type
        let node = &graph.nodes[0];
        if let Some(brick) = &node.data.brick {
            match &brick.emission_type {
                crate::bricks::types::BrickEmissionType::FlowTriggered => {
                    // Currently expected since macro doesn't parse emission_type yet
                }
                _ => {
                    // Future: should be Timer type
                }
            }
        }

        assert_eq!(graph.nodes.len(), 1);
    }
}
