#[cfg(test)]
mod tests {
    use super::super::macros::brick;
    use super::super::types::*;

    // Test bricks with different scenarios
    brick! {
        fn arguments_only(#[argument] value: i32 = 42, #[argument] name: String = "test") -> String {
            id: "arguments_only",
            label: "Arguments Only",
            description: "Test brick with only arguments",
            body: {
                format!("{}: {}", name, value)
            }
        }
    }

    brick! {
        fn inputs_only(#[input] first: String, #[input] second: i32) -> String {
            id: "inputs_only",
            label: "Inputs Only",
            description: "Test brick with only inputs",
            body: {
                format!("{} - {}", first, second)
            }
        }
    }

    brick! {
        fn mixed_params(#[input] data: String, #[argument] multiplier: i32 = 2, #[input] flag: bool) -> String {
            id: "mixed_params",
            label: "Mixed Parameters",
            description: "Test brick with mixed arguments and inputs",
            body: {
                format!("Data: {}, Multiplier: {}, Flag: {}", data, multiplier, flag)
            }
        }
    }

    brick! {
        fn no_params() -> String {
            id: "no_params",
            label: "No Parameters",
            description: "Test brick with no parameters",
            body: {
                "Hello World".to_string()
            }
        }
    }

    brick! {
        fn backward_compatible(value: i32 = 10) -> i32 {
            id: "backward_compatible",
            label: "Backward Compatible",
            description: "Test brick without annotations (should default to arguments)",
            body: {
                value * 2
            }
        }
    }

    brick! {
        fn all_types(#[argument] string_arg: String = "default", #[argument] int_arg: i32 = 0, #[argument] bool_arg: bool = false, #[input] string_input: String, #[input] int_input: i32, #[input] bool_input: bool) -> String {
            id: "all_types",
            label: "All Types",
            description: "Test brick with all supported types",
            body: {
                format!("Args: {}, {}, {} | Inputs: {}, {}, {}", string_arg, int_arg, bool_arg, string_input, int_input, bool_input)
            }
        }
    }

    brick! {
        fn edge_case_defaults(#[argument] no_default: String, #[argument] with_default: i32 = 999) -> String {
            id: "edge_case_defaults",
            label: "Edge Case Defaults",
            description: "Test mixing parameters with and without defaults",
            body: {
                format!("{}-{}", no_default, with_default)
            }
        }
    }

    brick! {
        fn single_input(#[input] data: String) -> String {
            id: "single_input",
            label: "Single Input",
            description: "Test with single input parameter",
            body: {
                format!("Processed: {}", data)
            }
        }
    }

    brick! {
        fn single_argument(#[argument] value: i32 = 123) -> i32 {
            id: "single_argument",
            label: "Single Argument",
            description: "Test with single argument parameter",
            body: {
                value + 100
            }
        }
    }

    brick! {
        fn test_labels(#[argument(label = "Custom Arg Label")] custom_arg: String = "default", #[input(label = "Custom Input Label")] custom_input: i32, #[argument] no_label_arg: bool) -> String {
            id: "test_labels",
            label: "Test Labels",
            description: "Test custom labels and default value handling",
            body: {
                format!("Arg: {}, Input: {}, Bool: {}", custom_arg, custom_input, no_label_arg)
            }
        }
    }

    brick! {
        fn test_no_defaults(#[argument] no_default_string: String, #[input] no_default_input: i32) -> String {
            id: "test_no_defaults",
            label: "Test No Defaults",
            description: "Test parameters without default values should have None",
            body: {
                format!("String: {}, Int: {}", no_default_string, no_default_input)
            }
        }
    }

    brick! {
        fn multiple_outputs(#[argument] value: i32 = 10) -> (String, i32, bool) {
            id: "multiple_outputs",
            label: "Multiple Outputs",
            description: "Test brick with multiple outputs",
            outputs: {
                formatted: String = "Formatted Value",
                doubled: i32 = "Doubled Value",
                is_positive: bool = "Is Positive"
            },
            body: {
                let formatted = format!("Value: {}", value);
                let doubled = value * 2;
                let is_positive = value > 0;
                (formatted, doubled, is_positive)
            }
        }
    }

    brick! {
        fn mixed_inputs_multiple_outputs(#[input] data: String, #[argument(label = "Multiplier")] multiplier: i32 = 2) -> (String, i32) {
            id: "mixed_inputs_multiple_outputs",
            label: "Mixed Inputs Multiple Outputs",
            description: "Test brick with inputs and multiple outputs",
            outputs: {
                processed: String = "Processed Data",
                length: i32 = "Data Length"
            },
            body: {
                let processed = format!("{} x{}", data, multiplier);
                let length = data.len() as i32;
                (processed, length)
            }
        }
    }

    brick! {
        fn test_output_label() -> #[output(label = "Custom Output")] String {
            id: "test_output_label",
            label: "Test Output Label",
            description: "Test brick with custom output label",
            body: {
                "test result".to_string()
            }
        }
    }

    #[test]
    fn test_basic_brick_structure() {
        // Test arguments only
        let brick = arguments_only_brick();
        assert_eq!(brick.id, "arguments_only");
        assert_eq!(brick.arguments.len(), 2);
        assert_eq!(brick.inputs.len(), 0);
        assert_eq!(brick.outputs.len(), 1);

        // Test inputs only
        let brick = inputs_only_brick();
        assert_eq!(brick.id, "inputs_only");
        assert_eq!(brick.arguments.len(), 0);
        assert_eq!(brick.inputs.len(), 2);
        assert_eq!(brick.outputs.len(), 1);

        // Test mixed parameters
        let brick = mixed_params_brick();
        assert_eq!(brick.id, "mixed_params");
        assert_eq!(brick.arguments.len(), 1);
        assert_eq!(brick.inputs.len(), 2);
        assert_eq!(brick.outputs.len(), 1);
    }

    #[test]
    fn test_argument_details() {
        let brick = arguments_only_brick();

        // Check first argument
        assert_eq!(brick.arguments[0].id, "value");
        assert_eq!(brick.arguments[0].label, "value");
        assert_eq!(brick.arguments[0].default_value, Some("42".to_string()));
        match brick.arguments[0].r#type {
            BrickArgumentType::Number => {}
            _ => panic!("Expected Number type for i32 argument"),
        }

        // Check second argument
        assert_eq!(brick.arguments[1].id, "name");
        assert_eq!(brick.arguments[1].label, "name");
        assert_eq!(brick.arguments[1].default_value, Some("test".to_string()));
        match brick.arguments[1].r#type {
            BrickArgumentType::String => {}
            _ => panic!("Expected String type for String argument"),
        }
    }

    #[test]
    fn test_input_details() {
        let brick = inputs_only_brick();

        // Check first input
        assert_eq!(brick.inputs[0].id, "first");
        assert_eq!(brick.inputs[0].label, "first");
        match brick.inputs[0].r#type {
            BrickHandleType::String => {}
            _ => panic!("Expected String type for String input"),
        }

        // Check second input
        assert_eq!(brick.inputs[1].id, "second");
        assert_eq!(brick.inputs[1].label, "second");
        match brick.inputs[1].r#type {
            BrickHandleType::Number => {}
            _ => panic!("Expected Number type for i32 input"),
        }
    }

    #[test]
    fn test_mixed_parameters() {
        let brick = mixed_params_brick();

        // Should have 1 argument
        assert_eq!(brick.arguments.len(), 1);
        assert_eq!(brick.arguments[0].id, "multiplier");
        assert_eq!(brick.arguments[0].default_value, Some("2".to_string()));

        // Should have 2 inputs
        assert_eq!(brick.inputs.len(), 2);
        assert_eq!(brick.inputs[0].id, "data");
        assert_eq!(brick.inputs[1].id, "flag");
    }

    #[test]
    fn test_no_parameters() {
        let brick = no_params_brick();
        assert_eq!(brick.id, "no_params");
        assert_eq!(brick.arguments.len(), 0);
        assert_eq!(brick.inputs.len(), 0);
        assert_eq!(brick.outputs.len(), 1);
    }

    #[test]
    fn test_backward_compatibility() {
        // Test that parameters without annotations default to arguments
        let brick = backward_compatible_brick();
        assert_eq!(brick.id, "backward_compatible");
        assert_eq!(brick.arguments.len(), 1);
        assert_eq!(brick.inputs.len(), 0);

        assert_eq!(brick.arguments[0].id, "value");
        assert_eq!(brick.arguments[0].default_value, Some("10".to_string()));
    }

    #[test]
    fn test_all_types() {
        let brick = all_types_brick();

        // Check arguments (3 total)
        assert_eq!(brick.arguments.len(), 3);

        // String argument
        assert_eq!(brick.arguments[0].id, "string_arg");
        match brick.arguments[0].r#type {
            BrickArgumentType::String => {}
            _ => panic!("Expected String type"),
        }

        // i32 argument
        assert_eq!(brick.arguments[1].id, "int_arg");
        match brick.arguments[1].r#type {
            BrickArgumentType::Number => {}
            _ => panic!("Expected Number type"),
        }

        // bool argument
        assert_eq!(brick.arguments[2].id, "bool_arg");
        match brick.arguments[2].r#type {
            BrickArgumentType::Boolean => {}
            _ => panic!("Expected Boolean type"),
        }

        // Check inputs (3 total)
        assert_eq!(brick.inputs.len(), 3);

        // String input
        assert_eq!(brick.inputs[0].id, "string_input");
        match brick.inputs[0].r#type {
            BrickHandleType::String => {}
            _ => panic!("Expected String type"),
        }

        // i32 input
        assert_eq!(brick.inputs[1].id, "int_input");
        match brick.inputs[1].r#type {
            BrickHandleType::Number => {}
            _ => panic!("Expected Number type"),
        }

        // bool input
        assert_eq!(brick.inputs[2].id, "bool_input");
        match brick.inputs[2].r#type {
            BrickHandleType::Boolean => {}
            _ => panic!("Expected Boolean type"),
        }
    }

    #[test]
    fn test_execution_arguments_only() {
        let brick = arguments_only_brick();

        // Create test arguments
        let args = vec![
            BrickArgument {
                id: "value".to_string(),
                label: "value".to_string(),
                r#type: BrickArgumentType::Number,
                enum_options: None,
                default_value: Some("100".to_string()),
            },
            BrickArgument {
                id: "name".to_string(),
                label: "name".to_string(),
                r#type: BrickArgumentType::String,
                enum_options: None,
                default_value: Some("custom".to_string()),
            },
        ];

        let inputs = vec![];

        // Execute the brick
        let outputs = (brick.execution)(args, inputs);
        assert_eq!(outputs.len(), 1);
        assert_eq!(outputs[0].id, "result");
    }

    #[test]
    fn test_execution_inputs_only() {
        let brick = inputs_only_brick();

        let args = vec![];
        let inputs = vec![
            BrickInput {
                id: "first".to_string(),
                label: "first".to_string(),
                r#type: BrickHandleType::String,
                default_value: Some("Hello".to_string()),
            },
            BrickInput {
                id: "second".to_string(),
                label: "second".to_string(),
                r#type: BrickHandleType::Number,
                default_value: Some("42".to_string()),
            },
        ];

        // Execute the brick
        let outputs = (brick.execution)(args, inputs);
        assert_eq!(outputs.len(), 1);
        assert_eq!(outputs[0].id, "result");
    }

    #[test]
    fn test_execution_mixed_parameters() {
        let brick = mixed_params_brick();

        let args = vec![BrickArgument {
            id: "multiplier".to_string(),
            label: "multiplier".to_string(),
            r#type: BrickArgumentType::Number,
            enum_options: None,
            default_value: Some("5".to_string()),
        }];

        let inputs = vec![
            BrickInput {
                id: "data".to_string(),
                label: "data".to_string(),
                r#type: BrickHandleType::String,
                default_value: Some("test_data".to_string()),
            },
            BrickInput {
                id: "flag".to_string(),
                label: "flag".to_string(),
                r#type: BrickHandleType::Boolean,
                default_value: Some("true".to_string()),
            },
        ];

        // Execute the brick
        let outputs = (brick.execution)(args, inputs);
        assert_eq!(outputs.len(), 1);
        assert_eq!(outputs[0].id, "result");
    }

    #[test]
    fn test_execution_no_parameters() {
        let brick = no_params_brick();

        let args = vec![];
        let inputs = vec![];

        // Execute the brick
        let outputs = (brick.execution)(args, inputs);
        assert_eq!(outputs.len(), 1);
        assert_eq!(outputs[0].id, "result");
    }

    #[test]
    fn test_execution_with_defaults() {
        let brick = backward_compatible_brick();

        // Test with empty arguments (should use default)
        let args = vec![];
        let inputs = vec![];

        let outputs = (brick.execution)(args, inputs);
        assert_eq!(outputs.len(), 1);
        assert_eq!(outputs[0].id, "result");

        // Test with provided argument
        let args = vec![BrickArgument {
            id: "value".to_string(),
            label: "value".to_string(),
            r#type: BrickArgumentType::Number,
            enum_options: None,
            default_value: Some("20".to_string()),
        }];

        let outputs = (brick.execution)(args, vec![]);
        assert_eq!(outputs.len(), 1);
        assert_eq!(outputs[0].id, "result");
    }

    #[test]
    fn test_execution_missing_parameters() {
        let brick = mixed_params_brick();

        // Test with missing parameters (should use defaults)
        let args = vec![];
        let inputs = vec![];

        let outputs = (brick.execution)(args, inputs);
        assert_eq!(outputs.len(), 1);
        assert_eq!(outputs[0].id, "result");
    }

    #[test]
    fn test_type_mapping_consistency() {
        // Test that argument and input types map consistently
        let brick = all_types_brick();

        // Verify String types
        match brick.arguments[0].r#type {
            BrickArgumentType::String => {}
            _ => panic!("String argument type mismatch"),
        }
        match brick.inputs[0].r#type {
            BrickHandleType::String => {}
            _ => panic!("String input type mismatch"),
        }

        // Verify Number types
        match brick.arguments[1].r#type {
            BrickArgumentType::Number => {}
            _ => panic!("Number argument type mismatch"),
        }
        match brick.inputs[1].r#type {
            BrickHandleType::Number => {}
            _ => panic!("Number input type mismatch"),
        }

        // Verify Boolean types
        match brick.arguments[2].r#type {
            BrickArgumentType::Boolean => {}
            _ => panic!("Boolean argument type mismatch"),
        }
        match brick.inputs[2].r#type {
            BrickHandleType::Boolean => {}
            _ => panic!("Boolean input type mismatch"),
        }
    }

    #[test]
    fn test_edge_case_defaults() {
        let brick = edge_case_defaults_brick();
        assert_eq!(brick.id, "edge_case_defaults");
        assert_eq!(brick.arguments.len(), 2);
        assert_eq!(brick.inputs.len(), 0);

        // Parameter without default should have empty default_value or use type default
        assert_eq!(brick.arguments[0].id, "no_default");

        // Parameter with default should have the specified default
        assert_eq!(brick.arguments[1].id, "with_default");
        assert_eq!(brick.arguments[1].default_value, Some("999".to_string()));
    }

    #[test]
    fn test_single_parameter_bricks() {
        // Test single input
        let brick = single_input_brick();
        assert_eq!(brick.id, "single_input");
        assert_eq!(brick.arguments.len(), 0);
        assert_eq!(brick.inputs.len(), 1);
        assert_eq!(brick.inputs[0].id, "data");

        // Test single argument
        let brick = single_argument_brick();
        assert_eq!(brick.id, "single_argument");
        assert_eq!(brick.arguments.len(), 1);
        assert_eq!(brick.inputs.len(), 0);
        assert_eq!(brick.arguments[0].id, "value");
        assert_eq!(brick.arguments[0].default_value, Some("123".to_string()));
    }

    #[test]
    fn test_execution_edge_cases() {
        // Test execution with partial arguments
        let brick = edge_case_defaults_brick();

        let args = vec![
            BrickArgument {
                id: "no_default".to_string(),
                label: "no_default".to_string(),
                r#type: BrickArgumentType::String,
                enum_options: None,
                default_value: Some("test_value".to_string()),
            },
            // Omit the second argument to test default behavior
        ];

        let outputs = (brick.execution)(args, vec![]);
        assert_eq!(outputs.len(), 1);

        // Test single parameter executions
        let single_input = single_input_brick();
        let inputs = vec![BrickInput {
            id: "data".to_string(),
            label: "data".to_string(),
            r#type: BrickHandleType::String,
            default_value: Some("input_data".to_string()),
        }];

        let outputs = (single_input.execution)(vec![], inputs);
        assert_eq!(outputs.len(), 1);

        let single_arg = single_argument_brick();
        let args = vec![BrickArgument {
            id: "value".to_string(),
            label: "value".to_string(),
            r#type: BrickArgumentType::Number,
            enum_options: None,
            default_value: Some("456".to_string()),
        }];

        let outputs = (single_arg.execution)(args, vec![]);
        assert_eq!(outputs.len(), 1);
    }

    #[test]
    fn test_parameter_order_independence() {
        // Test that parameters can be provided in different orders
        let brick = mixed_params_brick();

        // Provide arguments in different order than defined
        let args = vec![BrickArgument {
            id: "multiplier".to_string(),
            label: "multiplier".to_string(),
            r#type: BrickArgumentType::Number,
            enum_options: None,
            default_value: Some("7".to_string()),
        }];

        // Provide inputs in different order than defined
        let inputs = vec![
            BrickInput {
                id: "flag".to_string(),
                label: "flag".to_string(),
                r#type: BrickHandleType::Boolean,
                default_value: Some("false".to_string()),
            },
            BrickInput {
                id: "data".to_string(),
                label: "data".to_string(),
                r#type: BrickHandleType::String,
                default_value: Some("reordered".to_string()),
            },
        ];

        let outputs = (brick.execution)(args, inputs);
        assert_eq!(outputs.len(), 1);
        assert_eq!(outputs[0].id, "result");
    }

    #[test]
    fn test_custom_labels() {
        let brick = test_labels_brick();
        assert_eq!(brick.id, "test_labels");

        // Check custom argument label
        assert_eq!(brick.arguments.len(), 2);
        assert_eq!(brick.arguments[0].id, "custom_arg");
        assert_eq!(brick.arguments[0].label, "Custom Arg Label");
        assert_eq!(
            brick.arguments[0].default_value,
            Some("default".to_string())
        );

        // Check argument without custom label (should use parameter name)
        assert_eq!(brick.arguments[1].id, "no_label_arg");
        assert_eq!(brick.arguments[1].label, "no_label_arg");

        // Check custom input label
        assert_eq!(brick.inputs.len(), 1);
        assert_eq!(brick.inputs[0].id, "custom_input");
        assert_eq!(brick.inputs[0].label, "Custom Input Label");
    }

    #[test]
    fn test_none_default_values() {
        let brick = test_no_defaults_brick();
        assert_eq!(brick.id, "test_no_defaults");

        // Check argument without default value
        assert_eq!(brick.arguments.len(), 1);
        assert_eq!(brick.arguments[0].id, "no_default_string");
        assert_eq!(brick.arguments[0].default_value, None); // Should be None, not Some("")

        // Check input without default value
        assert_eq!(brick.inputs.len(), 1);
        assert_eq!(brick.inputs[0].id, "no_default_input");
        assert_eq!(brick.inputs[0].default_value, None); // Should be None, not Some("0")
    }

    #[test]
    fn test_mixed_label_and_default_scenarios() {
        let brick = test_labels_brick();

        // Test execution with the new syntax
        let args = vec![
            BrickArgument {
                id: "custom_arg".to_string(),
                label: "Custom Arg Label".to_string(),
                r#type: BrickArgumentType::String,
                enum_options: None,
                default_value: Some("custom_value".to_string()),
            },
            BrickArgument {
                id: "no_label_arg".to_string(),
                label: "no_label_arg".to_string(),
                r#type: BrickArgumentType::Boolean,
                enum_options: None,
                default_value: None, // Testing None default
            },
        ];

        let inputs = vec![BrickInput {
            id: "custom_input".to_string(),
            label: "Custom Input Label".to_string(),
            r#type: BrickHandleType::Number,
            default_value: Some("42".to_string()),
        }];

        let outputs = (brick.execution)(args, inputs);
        assert_eq!(outputs.len(), 1);
        assert_eq!(outputs[0].id, "result");
    }

    #[test]
    fn test_multiple_outputs_structure() {
        let brick = multiple_outputs_brick();
        assert_eq!(brick.id, "multiple_outputs");

        // Should have 1 argument
        assert_eq!(brick.arguments.len(), 1);
        assert_eq!(brick.arguments[0].id, "value");
        assert_eq!(brick.arguments[0].default_value, Some("10".to_string()));

        // Should have 0 inputs
        assert_eq!(brick.inputs.len(), 0);

        // Should have 3 outputs
        assert_eq!(brick.outputs.len(), 3);

        // Check first output
        assert_eq!(brick.outputs[0].id, "formatted");
        assert_eq!(brick.outputs[0].label, "Formatted Value");
        match brick.outputs[0].r#type {
            BrickHandleType::String => {}
            _ => panic!("Expected String type for formatted output"),
        }

        // Check second output
        assert_eq!(brick.outputs[1].id, "doubled");
        assert_eq!(brick.outputs[1].label, "Doubled Value");
        match brick.outputs[1].r#type {
            BrickHandleType::Number => {}
            _ => panic!("Expected Number type for doubled output"),
        }

        // Check third output
        assert_eq!(brick.outputs[2].id, "is_positive");
        assert_eq!(brick.outputs[2].label, "Is Positive");
        match brick.outputs[2].r#type {
            BrickHandleType::Boolean => {}
            _ => panic!("Expected Boolean type for is_positive output"),
        }
    }

    #[test]
    fn test_mixed_inputs_multiple_outputs() {
        let brick = mixed_inputs_multiple_outputs_brick();
        assert_eq!(brick.id, "mixed_inputs_multiple_outputs");

        // Should have 1 argument with custom label
        assert_eq!(brick.arguments.len(), 1);
        assert_eq!(brick.arguments[0].id, "multiplier");
        assert_eq!(brick.arguments[0].label, "Multiplier");
        assert_eq!(brick.arguments[0].default_value, Some("2".to_string()));

        // Should have 1 input
        assert_eq!(brick.inputs.len(), 1);
        assert_eq!(brick.inputs[0].id, "data");
        assert_eq!(brick.inputs[0].label, "data");

        // Should have 2 outputs
        assert_eq!(brick.outputs.len(), 2);
        assert_eq!(brick.outputs[0].id, "processed");
        assert_eq!(brick.outputs[0].label, "Processed Data");
        assert_eq!(brick.outputs[1].id, "length");
        assert_eq!(brick.outputs[1].label, "Data Length");
    }

    #[test]
    fn test_multiple_outputs_execution() {
        let brick = multiple_outputs_brick();

        let args = vec![BrickArgument {
            id: "value".to_string(),
            label: "value".to_string(),
            r#type: BrickArgumentType::Number,
            enum_options: None,
            default_value: Some("5".to_string()),
        }];

        let inputs = vec![];

        let outputs = (brick.execution)(args, inputs);
        assert_eq!(outputs.len(), 3);

        // Verify output structure (values aren't processed yet, but structure should be correct)
        assert_eq!(outputs[0].id, "formatted");
        assert_eq!(outputs[0].label, "Formatted Value");
        assert_eq!(outputs[1].id, "doubled");
        assert_eq!(outputs[1].label, "Doubled Value");
        assert_eq!(outputs[2].id, "is_positive");
        assert_eq!(outputs[2].label, "Is Positive");
    }

    #[test]
    fn test_mixed_multiple_outputs_execution() {
        let brick = mixed_inputs_multiple_outputs_brick();

        let args = vec![BrickArgument {
            id: "multiplier".to_string(),
            label: "Multiplier".to_string(),
            r#type: BrickArgumentType::Number,
            enum_options: None,
            default_value: Some("3".to_string()),
        }];

        let inputs = vec![BrickInput {
            id: "data".to_string(),
            label: "data".to_string(),
            r#type: BrickHandleType::String,
            default_value: Some("test".to_string()),
        }];

        let outputs = (brick.execution)(args, inputs);
        assert_eq!(outputs.len(), 2);
        assert_eq!(outputs[0].id, "processed");
        assert_eq!(outputs[0].label, "Processed Data");
        assert_eq!(outputs[1].id, "length");
        assert_eq!(outputs[1].label, "Data Length");
    }

    #[test]
    fn test_backward_compatibility_with_multiple_outputs() {
        // Test that single output bricks still work alongside multiple output bricks
        let single_brick = test_labels_brick();
        assert_eq!(single_brick.outputs.len(), 1);
        assert_eq!(single_brick.outputs[0].id, "result");

        let multiple_brick = multiple_outputs_brick();
        assert_eq!(multiple_brick.outputs.len(), 3);

        // Both should execute without issues
        let outputs1 = (single_brick.execution)(vec![], vec![]);
        assert_eq!(outputs1.len(), 1);

        let outputs2 = (multiple_brick.execution)(vec![], vec![]);
        assert_eq!(outputs2.len(), 3);
    }

    #[test]
    fn test_custom_output_labels() {
        // Test that custom output labels work for single outputs
        let brick = test_output_label_brick();
        assert_eq!(brick.outputs.len(), 1);
        assert_eq!(brick.outputs[0].label, "Custom Output");
        assert_eq!(brick.outputs[0].id, "result");

        // Test execution works
        let outputs = (brick.execution)(vec![], vec![]);
        assert_eq!(outputs.len(), 1);

        // Test that default label still works for bricks without custom output labels
        let default_brick = arguments_only_brick();
        assert_eq!(default_brick.outputs.len(), 1);
        assert_eq!(default_brick.outputs[0].label, "Result");
        assert_eq!(default_brick.outputs[0].id, "result");
    }
}
