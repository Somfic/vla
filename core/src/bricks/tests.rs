#[cfg(test)]
mod tests {
    use crate::bricks::macros::brick;
    use crate::bricks::{get_all_bricks, math_brick};

    // Test brick that demonstrates argument and input parameters - output comes from return value
    brick! {
        #[id("demo_all_types")]
        #[label("Demo All Types")]
        #[description("Demonstrates argument and input parameters - output from return value")]
        fn demo_all_types(
            #[argument] #[label("Configuration")] config: String,
            #[input] #[label("Input Data")] data: i32
        ) -> String {
            // Function logic - return value becomes the output
            format!("Config: {}, Data: {}", config, data)
        }
    }

    // Valid example with all required attributes
    brick! {
        #[id("valid_brick")]
        #[label("Valid Brick")]
        fn valid_brick(
            #[argument] #[label("Config")] config: String,
            #[input] #[label("Data")] data: i32
        ) -> String {
            format!("{}: {}", config, data)
        }
    }

    // Valid attributes test
    brick! {
        #[id("valid_test")]
        fn valid_test(
            #[input] data: i32
        ) -> String {
            format!("Data: {}", data)
        }
    }

    // Example brick that demonstrates single return value outputs
    brick! {
        #[id("math_operations")]
        #[label("Math Operations")]
        #[description("Performs math operations - the return value becomes the output")]
        fn math_operations(
            #[input] #[label("First Number")] a: i32,
            #[input] #[label("Second Number")] b: i32
        ) -> String {
            // The function return value becomes the brick's output
            format!("Sum: {}, Product: {}, A > B: {}", a + b, a * b, a > b)
        }
    }

    // Example with mixed parameters (argument + input)
    brick! {
        #[id("data_processor")]
        #[label("Data Processor")]
        #[description("Processes data with configuration - return value is the output")]
        fn data_processor(
            #[argument] #[label("Mode")] mode: String,
            #[input] #[label("Raw Data")] data: i32
        ) -> String {
            // Function processes the input data with the configuration mode
            // The return value becomes the brick's output
            match mode.as_str() {
                "double" => format!("Doubled: {}", data * 2),
                "square" => format!("Squared: {}", data * data),
                _ => format!("Processed: {}", data),
            }
        }
    }

    // Test brick with multiple outputs using different attribute combinations
    brick! {
        #[id("multiple_outputs_test")]
        #[label("Multiple Outputs Test")]
        #[description("Tests multiple outputs with different attributes")]
        fn multiple_outputs_test(
            #[input] #[label("Number A")] a: i32,
            #[input] #[label("Number B")] b: i32
        ) -> (
            #[id("sum")] #[label("Sum")] #[description("Addition result")] i32,
            #[label("Product")] i32
        ) {
            (a + b, a * b)
        }
    }

    // Test brick with just basic labels
    brick! {
        #[id("basic_multiple")]
        #[label("Basic Multiple")]
        fn basic_multiple(
            #[input] x: i32,
            #[input] y: i32
        ) -> (
            #[label("Addition")] i32,
            #[label("Subtraction")] i32
        ) {
            (x + y, x - y)
        }
    }

    // Tests from test_all_types.rs
    #[test]
    fn test_all_parameter_types() {
        let brick = demo_all_types_brick();

        assert_eq!(brick.id, "demo_all_types");
        assert_eq!(brick.label, "Demo All Types");

        // Should have 1 argument
        assert_eq!(brick.arguments.len(), 1);
        assert_eq!(brick.arguments[0].id, "config");
        assert_eq!(brick.arguments[0].label, "Configuration");

        // Should have 1 input
        assert_eq!(brick.inputs.len(), 1);
        assert_eq!(brick.inputs[0].id, "data");
        assert_eq!(brick.inputs[0].label, "Input Data");

        // Should have 1 output: only the function return value
        assert_eq!(brick.outputs.len(), 1);

        // Check the function return value output
        let result_output = brick.outputs.iter().find(|o| o.id == "result").unwrap();
        assert_eq!(result_output.label, "Result");
    }

    // Tests from validation_test.rs
    #[test]
    fn test_valid_brick_compiles() {
        let brick = valid_brick_brick();
        assert_eq!(brick.id, "valid_brick");
        assert_eq!(brick.arguments.len(), 1);
        assert_eq!(brick.inputs.len(), 1);
        assert_eq!(brick.outputs.len(), 1); // Just the function return value
    }

    // Tests from attribute_validation_test.rs
    #[test]
    fn test_valid_attributes() {
        let brick = valid_test_brick();
        assert_eq!(brick.id, "valid_test");
        assert_eq!(brick.inputs.len(), 1);
    }

    // Tests from multiple_outputs_example.rs
    #[test]
    fn test_math_operations_brick_structure() {
        let brick = math_operations_brick();

        assert_eq!(brick.id, "math_operations");
        assert_eq!(brick.label, "Math Operations");

        // Should have 0 arguments, 2 inputs, and 1 output (just the return value)
        assert_eq!(brick.arguments.len(), 0);
        assert_eq!(brick.inputs.len(), 2);
        assert_eq!(brick.outputs.len(), 1); // Only the function return value

        // Check input names
        let input_ids: Vec<&str> = brick.inputs.iter().map(|i| i.id.as_str()).collect();
        assert!(input_ids.contains(&"a"));
        assert!(input_ids.contains(&"b"));

        // Check output is the function return value
        assert_eq!(brick.outputs[0].id, "result");
        assert_eq!(brick.outputs[0].label, "Result");
    }

    #[test]
    fn test_data_processor_brick_structure() {
        let brick = data_processor_brick();

        assert_eq!(brick.id, "data_processor");

        // Should have 1 argument, 1 input, and 1 output (just the return value)
        assert_eq!(brick.arguments.len(), 1);
        assert_eq!(brick.inputs.len(), 1);
        assert_eq!(brick.outputs.len(), 1); // Only the function return value

        // Check that we have all expected parameters
        assert_eq!(brick.arguments[0].id, "mode");
        assert_eq!(brick.arguments[0].label, "Mode");

        assert_eq!(brick.inputs[0].id, "data");
        assert_eq!(brick.inputs[0].label, "Raw Data");

        // Check output is the function return value
        assert_eq!(brick.outputs[0].id, "result");
        assert_eq!(brick.outputs[0].label, "Result");
    }

    // Tests from multiple_outputs_test.rs
    #[test]
    fn test_multiple_outputs_with_attributes() {
        let brick = multiple_outputs_test_brick();

        assert_eq!(brick.id, "multiple_outputs_test");
        assert_eq!(brick.label, "Multiple Outputs Test");
        assert_eq!(
            brick.description,
            "Tests multiple outputs with different attributes"
        );

        // Should have 0 arguments, 2 inputs, and 2 outputs
        assert_eq!(brick.arguments.len(), 0);
        assert_eq!(brick.inputs.len(), 2);
        assert_eq!(brick.outputs.len(), 2);

        // Check inputs
        assert_eq!(brick.inputs[0].id, "a");
        assert_eq!(brick.inputs[0].label, "Number A");
        assert_eq!(brick.inputs[1].id, "b");
        assert_eq!(brick.inputs[1].label, "Number B");

        // Check outputs
        assert_eq!(brick.outputs[0].id, "sum");
        assert_eq!(brick.outputs[0].label, "Sum");

        assert_eq!(brick.outputs[1].id, "output_1"); // Default ID since no ID specified
        assert_eq!(brick.outputs[1].label, "Product");
    }

    #[test]
    fn test_basic_multiple_outputs() {
        let brick = basic_multiple_brick();

        assert_eq!(brick.id, "basic_multiple");
        assert_eq!(brick.label, "Basic Multiple");

        // Should have 0 arguments, 2 inputs, and 2 outputs
        assert_eq!(brick.arguments.len(), 0);
        assert_eq!(brick.inputs.len(), 2);
        assert_eq!(brick.outputs.len(), 2);

        // Check outputs with default IDs
        assert_eq!(brick.outputs[0].id, "output_0"); // Default ID
        assert_eq!(brick.outputs[0].label, "Addition");

        assert_eq!(brick.outputs[1].id, "output_1"); // Default ID
        assert_eq!(brick.outputs[1].label, "Subtraction");
    }

    // Tests from integration_test.rs
    #[test]
    fn test_main_math_brick() {
        let brick = math_brick();

        assert_eq!(brick.id, "math");
        assert_eq!(brick.label, "Math Operations");
        assert_eq!(
            brick.description,
            "Performs addition and multiplication on two numbers"
        );

        // Should have 0 arguments, 2 inputs, and 2 outputs
        assert_eq!(brick.arguments.len(), 0);
        assert_eq!(brick.inputs.len(), 2);
        assert_eq!(brick.outputs.len(), 2);

        // Check inputs
        assert_eq!(brick.inputs[0].id, "first");
        assert_eq!(brick.inputs[0].label, "First");
        assert_eq!(brick.inputs[1].id, "second");
        assert_eq!(brick.inputs[1].label, "Second");

        // Check outputs - should have custom labels
        assert_eq!(brick.outputs[0].id, "output_0"); // Default ID since no ID specified
        assert_eq!(brick.outputs[0].label, "Addition");

        assert_eq!(brick.outputs[1].id, "output_1"); // Default ID since no ID specified
        assert_eq!(brick.outputs[1].label, "Multiplication");
    }

    #[test]
    fn test_get_all_bricks() {
        let bricks = get_all_bricks();
        assert_eq!(bricks.len(), 1);
        assert_eq!(bricks[0].id, "math");
    }
}
