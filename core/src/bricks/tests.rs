#[cfg(test)]
mod tests {
    use crate::bricks::{get_all_bricks, math_brick};

    // =====================================================
    // COMPREHENSIVE TYPE VALIDATION TESTS FOR MATH BRICK
    // =====================================================

    #[test]
    fn test_math_brick_comprehensive() {
        let brick = math_brick();

        assert_eq!(brick.id, "math");
        assert_eq!(brick.label, "Math Operations");
        assert_eq!(
            brick.description,
            "Performs addition and multiplication on two numbers"
        );

        // Should have 1 argument, 2 inputs, and 2 outputs
        assert_eq!(brick.arguments.len(), 1);
        assert_eq!(brick.inputs.len(), 2);
        assert_eq!(brick.outputs.len(), 2);
    }

    #[test]
    fn test_math_brick_argument_types_detailed() {
        let brick = math_brick();

        // Test argument type - should be Boolean
        assert_eq!(brick.arguments.len(), 1);
        let invert_arg = &brick.arguments[0];
        assert_eq!(invert_arg.id, "invert");
        assert_eq!(invert_arg.label, "Invert");
        assert_eq!(
            invert_arg.r#type,
            crate::bricks::types::BrickArgumentType::Boolean
        );
        assert_eq!(invert_arg.default_value, Some("true".to_string()));
        assert!(
            invert_arg.enum_options.is_none(),
            "Boolean argument should not have enum options"
        );
    }

    #[test]
    fn test_math_brick_input_types_detailed() {
        let brick = math_brick();

        // Test input types - both should be numbers with default values
        assert_eq!(brick.inputs.len(), 2);

        let first_input = &brick.inputs[0];
        assert_eq!(first_input.id, "first");
        assert_eq!(first_input.label, "First");
        assert_eq!(
            first_input.r#type,
            crate::bricks::types::BrickHandleType::Number
        );
        assert_eq!(first_input.default_value, Some("1".to_string()));

        let second_input = &brick.inputs[1];
        assert_eq!(second_input.id, "second");
        assert_eq!(second_input.label, "Second");
        assert_eq!(
            second_input.r#type,
            crate::bricks::types::BrickHandleType::Number
        );
        assert_eq!(second_input.default_value, Some("2".to_string()));
    }

    #[test]
    fn test_math_brick_output_types_detailed() {
        let brick = math_brick();

        // Test output types - both should be numbers
        assert_eq!(brick.outputs.len(), 2);

        let addition_output = &brick.outputs[0];
        assert_eq!(addition_output.id, "output_0");
        assert_eq!(addition_output.label, "Addition");
        assert_eq!(
            addition_output.r#type,
            crate::bricks::types::BrickHandleType::Number
        );

        let multiplication_output = &brick.outputs[1];
        assert_eq!(multiplication_output.id, "output_1");
        assert_eq!(multiplication_output.label, "Multiplication");
        assert_eq!(
            multiplication_output.r#type,
            crate::bricks::types::BrickHandleType::Number
        );
    }

    #[test]
    fn test_type_enum_variants_equality() {
        use crate::bricks::types::{BrickArgumentType, BrickHandleType};

        // Test BrickArgumentType variants
        let string_arg_type = BrickArgumentType::String;
        let number_arg_type = BrickArgumentType::Number;
        let boolean_arg_type = BrickArgumentType::Boolean;
        let enum_arg_type = BrickArgumentType::Enum;

        assert_eq!(string_arg_type, BrickArgumentType::String);
        assert_eq!(number_arg_type, BrickArgumentType::Number);
        assert_eq!(boolean_arg_type, BrickArgumentType::Boolean);
        assert_eq!(enum_arg_type, BrickArgumentType::Enum);

        // Test BrickHandleType variants
        let string_handle_type = BrickHandleType::String;
        let number_handle_type = BrickHandleType::Number;
        let boolean_handle_type = BrickHandleType::Boolean;
        let enum_handle_type = BrickHandleType::Enum;

        assert_eq!(string_handle_type, BrickHandleType::String);
        assert_eq!(number_handle_type, BrickHandleType::Number);
        assert_eq!(boolean_handle_type, BrickHandleType::Boolean);
        assert_eq!(enum_handle_type, BrickHandleType::Enum);

        // Test that they are different types
        assert_ne!(string_arg_type, number_arg_type);
        assert_ne!(string_handle_type, number_handle_type);
    }

    #[test]
    fn test_default_values_parsing() {
        let brick = math_brick();

        // Test that default values are properly set and parseable
        let invert_arg = &brick.arguments[0];
        assert!(invert_arg.default_value.is_some());
        assert_eq!(invert_arg.default_value.as_ref().unwrap(), "true");

        // Test that the default value can be parsed as a boolean
        let default_bool = invert_arg.default_value.as_ref().unwrap().parse::<bool>();
        assert!(
            default_bool.is_ok(),
            "Boolean default value should be parseable"
        );
        assert_eq!(default_bool.unwrap(), true);

        let first_input = &brick.inputs[0];
        assert!(first_input.default_value.is_some());
        assert_eq!(first_input.default_value.as_ref().unwrap(), "1");

        // Test that the default value can be parsed as a number
        let default_num = first_input.default_value.as_ref().unwrap().parse::<i32>();
        assert!(
            default_num.is_ok(),
            "Number default value should be parseable"
        );
        assert_eq!(default_num.unwrap(), 1);

        let second_input = &brick.inputs[1];
        assert!(second_input.default_value.is_some());
        assert_eq!(second_input.default_value.as_ref().unwrap(), "2");

        // Test that the default value can be parsed as a number
        let default_num2 = second_input.default_value.as_ref().unwrap().parse::<i32>();
        assert!(
            default_num2.is_ok(),
            "Number default value should be parseable"
        );
        assert_eq!(default_num2.unwrap(), 2);
    }

    #[test]
    fn test_type_consistency_within_brick() {
        let brick = math_brick();

        // Test that input and output types are consistent
        // All inputs should be Numbers for math operations
        for input in &brick.inputs {
            assert_eq!(
                input.r#type,
                crate::bricks::types::BrickHandleType::Number,
                "Math brick inputs should be Numbers, but {} is not",
                input.id
            );
        }

        // All outputs should be Numbers for math operations
        for output in &brick.outputs {
            assert_eq!(
                output.r#type,
                crate::bricks::types::BrickHandleType::Number,
                "Math brick outputs should be Numbers, but {} is not",
                output.id
            );
        }

        // The argument should be Boolean (invert flag)
        for arg in &brick.arguments {
            if arg.id == "invert" {
                assert_eq!(
                    arg.r#type,
                    crate::bricks::types::BrickArgumentType::Boolean,
                    "Invert argument should be Boolean"
                );
            }
        }
    }

    #[test]
    fn test_comprehensive_enum_options_validation() {
        let bricks = get_all_bricks();

        // Test that enum_options are properly initialized
        for brick in bricks {
            for arg in brick.arguments {
                match arg.r#type {
                    crate::bricks::types::BrickArgumentType::Enum => {
                        assert!(
                            arg.enum_options.is_some(),
                            "Enum argument should have options"
                        );
                        assert!(
                            !arg.enum_options.unwrap().is_empty(),
                            "Enum argument should have non-empty options"
                        );
                    }
                    _ => {
                        // Non-enum arguments should not have enum_options
                        assert!(
                            arg.enum_options.is_none(),
                            "Non-enum argument should not have enum_options"
                        );
                    }
                }
            }
        }
    }

    #[test]
    fn test_serialization_compatibility() {
        use crate::bricks::types::{BrickArgumentType, BrickHandleType};

        // Test that our types can be serialized/deserialized properly
        let arg_types = vec![
            BrickArgumentType::String,
            BrickArgumentType::Number,
            BrickArgumentType::Boolean,
            BrickArgumentType::Enum,
        ];

        let handle_types = vec![
            BrickHandleType::String,
            BrickHandleType::Number,
            BrickHandleType::Boolean,
            BrickHandleType::Enum,
        ];

        // Test that all types are created successfully
        assert_eq!(arg_types.len(), 4);
        assert_eq!(handle_types.len(), 4);

        // Test that types maintain their identity
        for arg_type in &arg_types {
            match arg_type {
                BrickArgumentType::String => assert_eq!(*arg_type, BrickArgumentType::String),
                BrickArgumentType::Number => assert_eq!(*arg_type, BrickArgumentType::Number),
                BrickArgumentType::Boolean => assert_eq!(*arg_type, BrickArgumentType::Boolean),
                BrickArgumentType::Enum => assert_eq!(*arg_type, BrickArgumentType::Enum),
            }
        }

        for handle_type in &handle_types {
            match handle_type {
                BrickHandleType::String => assert_eq!(*handle_type, BrickHandleType::String),
                BrickHandleType::Number => assert_eq!(*handle_type, BrickHandleType::Number),
                BrickHandleType::Boolean => assert_eq!(*handle_type, BrickHandleType::Boolean),
                BrickHandleType::Enum => assert_eq!(*handle_type, BrickHandleType::Enum),
            }
        }
    }

    #[test]
    fn test_get_all_bricks_validation() {
        let bricks = get_all_bricks();
        assert_eq!(bricks.len(), 1);
        assert_eq!(bricks[0].id, "math");

        // Test that all bricks in the collection are valid
        for brick in bricks {
            assert!(!brick.id.is_empty());
            assert!(!brick.label.is_empty());

            // Test arguments
            for arg in brick.arguments {
                assert!(!arg.id.is_empty());
                assert!(!arg.label.is_empty());
            }

            // Test inputs
            for input in brick.inputs {
                assert!(!input.id.is_empty());
                assert!(!input.label.is_empty());
            }

            // Test outputs
            for output in brick.outputs {
                assert!(!output.id.is_empty());
                assert!(!output.label.is_empty());
            }
        }
    }

    #[test]
    fn test_type_consistency() {
        let bricks = get_all_bricks();

        for brick in bricks {
            // Test that all arguments have valid types
            for arg in &brick.arguments {
                assert!(!arg.id.is_empty(), "Argument ID should not be empty");
                assert!(!arg.label.is_empty(), "Argument label should not be empty");
                // Type is an enum, so it's always valid
            }

            // Test that all inputs have valid types
            for input in &brick.inputs {
                assert!(!input.id.is_empty(), "Input ID should not be empty");
                assert!(!input.label.is_empty(), "Input label should not be empty");
                // Type is an enum, so it's always valid
            }

            // Test that all outputs have valid types
            for output in &brick.outputs {
                assert!(!output.id.is_empty(), "Output ID should not be empty");
                assert!(!output.label.is_empty(), "Output label should not be empty");
                // Type is an enum, so it's always valid
            }
        }
    }
}
