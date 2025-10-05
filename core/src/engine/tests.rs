use std::collections::{BTreeMap, HashMap};

use crate::bricks::macros::brick;
use crate::bricks::types::BrickInputValue;
use crate::engine::Engine;
use crate::prelude::*;

brick! {
    #[id("logical_and")]
    #[category("Boolean Logic")]
    fn logical_and(
        #[input] a: bool,
        #[input] b: bool
    ) -> (
        #[label("A & B")] bool
    )
    {
        (a && b,)
    }
}

fn sample_graph() -> Graph {
    let node = Node {
        id: "node1".to_string(),
        position: Point { x: 0.0, y: 0.0 },
        data: NodeData {
            brick_id: "logical_and".to_string(),
            brick: Some(logical_and_brick()),
            arguments: BTreeMap::new(),
            defaults: ({
                let mut map = BTreeMap::new();
                map.insert("a".to_string(), "true".to_string());
                map.insert("b".to_string(), "false".to_string());
                map
            }),
        },
        r#type: "v1".to_string(),
    };

    Graph {
        nodes: vec![node],
        edges: vec![],
    }
}

#[test]
fn test_engine_execution() {
    let graph = sample_graph();

    let mut engine = Engine::new(graph);
    engine.start();

    while let Some(result) = engine.next() {
        assert!(result.is_ok());
    }
}

#[test]
fn test_json_execution() {
    // Test the add brick execution with JSON values
    let args = vec![]; // Use inputs instead of arguments for this test
    let inputs = vec![
        BrickInputValue {
            id: "a".to_string(),
            value: "5.0".to_string(), // JSON encoded f32
        },
        BrickInputValue {
            id: "b".to_string(),
            value: "3.0".to_string(), // JSON encoded f32
        },
    ];

    // Call the execution function directly
    let brick = crate::bricks::arithmetics::add_brick();
    let outputs = (brick.execution)(args, inputs);

    println!("Outputs: {:?}", outputs);

    // Verify we got one output
    assert_eq!(outputs.len(), 1);

    // Check the output value is JSON encoded
    let result_output = &outputs[0];
    assert_eq!(result_output.id, "output_0");

    // Parse the JSON value back to verify it's the expected result
    let result_value: f32 = serde_json::from_str(&result_output.value).unwrap();
    assert_eq!(result_value, 8.0); // 5.0 + 3.0 = 8.0
}

#[test]
fn test_json_execution_with_default_values() {
    // Test with missing inputs (should use defaults)
    let args = vec![];
    let inputs = vec![]; // No inputs provided

    let brick = crate::bricks::arithmetics::add_brick();
    let outputs = (brick.execution)(args, inputs);

    println!("Outputs with defaults: {:?}", outputs);

    // Verify we got one output
    assert_eq!(outputs.len(), 1);

    // Check the output value
    let result_output = &outputs[0];
    let result_value: f32 = serde_json::from_str(&result_output.value).unwrap();
    assert_eq!(result_value, 2.0); // 1.0 + 1.0 = 2.0 (default values)
}
