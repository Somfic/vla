/// Brick macro supporting separate attribute syntax.
/// Each function parameter MUST have one of: #[argument], #[input], or #[output]
///
/// Usage:
/// ```rust,ignore
//  brick! {
//         #[id("basic_multiple")]
//         #[label("Basic Multiple")]
//         fn basic_multiple(
//             #[input] #[label("X")] x: i32,
//             #[input] #[label("Y")] y: i32
//         ) -> (
//             #[label("Addition")] i32,
//             #[label("Subtraction")] i32
//         ) {
//             (x + y, x - y)
//         }
//     }
/// ```
macro_rules! brick {
    // Pattern that explicitly requires each parameter to have input or argument
    (
        #[id($id:expr)]
        $(#[label($label:expr)])?
        $(#[description($description:expr)])?
        fn $fn_name:ident(
            $(brick!(@param $(#[label($param_label:expr)])? #[input] $param_name:ident: $param_type:ident)),*
        ) -> $return_type:ident
        $body:block
    ) => {
        brick!(@generate_brick $id, $($label)?, $($description)?, $fn_name,
               [$(brick!(@param_input $(#[label($param_label)])? $param_name, $param_type)),*],
               $return_type, $body)
    };

    (
        #[id($id:expr)]
        $(#[label($label:expr)])?
        $(#[description($description:expr)])?
        fn $fn_name:ident(
            $(brick!(@param $(#[label($param_label:expr)])? #[output] $param_name:ident: $param_type:ident)),*
        ) -> $return_type:ident
        $body:block
    ) => {
        brick!(@generate_brick $id, $($label)?, $($description)?, $fn_name,
               [$(brick!(@param_output $(#[label($param_label)])? $param_name, $param_type)),*],
               $return_type, $body)
    };

    (
        #[id($id:expr)]
        $(#[label($label:expr)])?
        $(#[description($description:expr)])?
        fn $fn_name:ident(
            $(brick!(@param $(#[label($param_label:expr)])? #[argument] $param_name:ident: $param_type:ident)),*
        ) -> $return_type:ident
        $body:block
    ) => {
        brick!(@generate_brick $id, $($label)?, $($description)?, $fn_name,
               [$(brick!(@param_argument $(#[label($param_label)])? $param_name, $param_type)),*],
               $return_type, $body)
    };

    // Mixed parameter types with single tuple return (1-tuple)
    (
        #[id($id:expr)]
        $(#[label($label:expr)])?
        $(#[description($description:expr)])?
        $(#[keywords($keywords:expr)])?
        #[category($category:expr)]
        fn $fn_name:ident(
            $($(#[$param_attr:ident$(($($param_attr_content:tt)*))? ])+ $param_name:ident: $param_type:ident $(= $default:expr)?),*
        ) -> (
            $(#[$output_attr:ident$(($($output_attr_content:tt)*))? ])+ $output_type:ident
        )
        $body:block
    ) => {
        // Validate each parameter has at least one required attribute
        $(
            brick!(@ensure_valid_attrs [$(#[$param_attr$(($($param_attr_content)*))? ])+] -> $param_name);
        )*

        paste::paste! {
            // Define the actual function
            fn $fn_name($($param_name: $param_type),*) -> ($output_type,) $body

            // Define the execution wrapper
            #[allow(unused_variables)]
            fn [<$fn_name _execution>](
                args: Vec<crate::bricks::types::BrickArgument>,
                inputs: Vec<crate::bricks::types::BrickInput>
            ) -> Vec<crate::bricks::types::BrickOutput> {
                // Extract parameters based on their attributes
                $(
                    let $param_name = brick!(@get_param_value_with_attrs
                        [$(#[$param_attr$(($($param_attr_content)*))? ])+],
                        $param_type,
                        &args,
                        &inputs,
                        stringify!($param_name),
                        brick!(@get_custom_default_or_type_default $param_type, $($default)?)
                    );
                )*

                // Call the function
                let result = $fn_name($($param_name),*);

                // Return outputs from single tuple element
                let mut outputs = Vec::new();

                brick!(@add_tuple_outputs outputs, result, [([$(#[$output_attr$(($($output_attr_content)*))? ])+], $output_type)]);

                outputs
            }

            // Generate the brick structure function
            pub fn [<$fn_name _brick>]() -> crate::bricks::types::Brick {
                let mut arguments = Vec::new();
                let mut inputs = Vec::new();
                let mut outputs = Vec::new();

                // Process each parameter based on its attributes
                $(
                    brick!(@process_param_with_attrs
                        arguments,
                        inputs,
                        outputs,
                        [$(#[$param_attr$(($($param_attr_content)*))? ])+],
                        $param_name,
                        $param_type,
                        brick!(@get_custom_default_or_type_default $param_type, $($default)?)
                    );
                )*

                // Add output from single tuple return type
                brick!(@add_tuple_outputs outputs, dummy_result, [([$(#[$output_attr$(($($output_attr_content)*))? ])+], $output_type)]);

                crate::bricks::types::Brick {
                    id: $id.to_string(),
                    label: brick!(@get_label_or_default $($label)?),
                    description: brick!(@get_description_or_default $($description)?),
                    keywords: brick!(@get_keywords_or_default $($keywords)?),
                    category: brick!(@get_category_or_default $category),
                    arguments,
                    inputs,
                    outputs,
                    execution: [<$fn_name _execution>],
                }
            }
        }
    };

    // Mixed parameter types with tuple return (multiple outputs)
    (
        #[id($id:expr)]
        $(#[label($label:expr)])?
        $(#[description($description:expr)])?
        $(#[keywords($keywords:expr)])?
        #[category($category:expr)]
        fn $fn_name:ident(
            $($(#[$param_attr:ident$(($($param_attr_content:tt)*))? ])+ $param_name:ident: $param_type:ident $(= $default:expr)?),*
        ) -> (
            $($(#[$output_attr:ident$(($($output_attr_content:tt)*))? ])+ $output_type:ident),+
        )
        $body:block
    ) => {
        // Validate each parameter has at least one required attribute
        $(
            brick!(@ensure_valid_attrs [$(#[$param_attr$(($($param_attr_content)*))? ])+] -> $param_name);
        )*

        paste::paste! {
            // Define the actual function
            fn $fn_name($($param_name: $param_type),*) -> ($($output_type),+) $body

            // Define the execution wrapper
            #[allow(unused_variables)]
            fn [<$fn_name _execution>](
                args: Vec<crate::bricks::types::BrickArgument>,
                inputs: Vec<crate::bricks::types::BrickInput>
            ) -> Vec<crate::bricks::types::BrickOutput> {
                // Extract parameters based on their attributes
                $(
                    let $param_name = brick!(@get_param_value_with_attrs
                        [$(#[$param_attr$(($($param_attr_content)*))? ])+],
                        $param_type,
                        &args,
                        &inputs,
                        stringify!($param_name),
                        brick!(@get_custom_default_or_type_default $param_type, $($default)?)
                    );
                )*

                // Call the function
                let result = $fn_name($($param_name),*);

                // Return outputs from tuple elements
                let mut outputs = Vec::new();

                brick!(@add_tuple_outputs outputs, result, [$(([$(#[$output_attr$(($($output_attr_content)*))? ])+], $output_type)),+]);

                outputs
            }

            // Generate the brick structure function
            pub fn [<$fn_name _brick>]() -> crate::bricks::types::Brick {
                let mut arguments = Vec::new();
                let mut inputs = Vec::new();
                let mut outputs = Vec::new();

                // Process each parameter based on its attributes
                $(
                    brick!(@process_param_with_attrs
                        arguments,
                        inputs,
                        outputs,
                        [$(#[$param_attr$(($($param_attr_content)*))? ])+],
                        $param_name,
                        $param_type,
                        brick!(@get_custom_default_or_type_default $param_type, $($default)?)
                    );
                )*

                // Add outputs from tuple return type
                brick!(@add_tuple_outputs outputs, dummy_result, [$(([$(#[$output_attr$(($($output_attr_content)*))? ])+], $output_type)),+]);

                crate::bricks::types::Brick {
                    id: $id.to_string(),
                    label: brick!(@get_label_or_default $($label)?),
                    description: brick!(@get_description_or_default $($description)?),
                    keywords: brick!(@get_keywords_or_default $($keywords)?),
                    category: brick!(@get_category_or_default $category),
                    arguments,
                    inputs,
                    outputs,
                    execution: [<$fn_name _execution>],
                }
            }
        }
    };

    // Mixed parameter types with single return (single output)
    (
        #[id($id:expr)]
        $(#[label($label:expr)])?
        $(#[description($description:expr)])?
        $(#[keywords($keywords:expr)])?
        #[category($category:expr)]
        fn $fn_name:ident(
            $($(#[$param_attr:ident$(($($param_attr_content:tt)*))? ])+ $param_name:ident: $param_type:ident $(= $default:expr)?),*
        ) -> $return_type:ident
        $body:block
    ) => {
        // Validate each parameter has at least one required attribute
        $(
            brick!(@ensure_valid_attrs [$(#[$param_attr$(($($param_attr_content)*))? ])+] -> $param_name);
        )*

        paste::paste! {
            // Define the actual function
            fn $fn_name($($param_name: $param_type),*) -> $return_type $body

            // Define the execution wrapper
            #[allow(unused_variables)]
            fn [<$fn_name _execution>](
                args: Vec<crate::bricks::types::BrickArgument>,
                inputs: Vec<crate::bricks::types::BrickInput>
            ) -> Vec<crate::bricks::types::BrickOutput> {
                // Extract parameters based on their attributes
                $(
                    let $param_name = brick!(@get_param_value_with_attrs
                        [$(#[$param_attr$(($($param_attr_content)*))? ])+],
                        $param_type,
                        &args,
                        &inputs,
                        stringify!($param_name),
                        brick!(@get_custom_default_or_type_default $param_type, $($default)?)
                    );
                )*

                // Call the function
                let result = $fn_name($($param_name),*);

                // Return outputs (including function return value and any output parameters)
                let mut outputs = Vec::new();

                // Add the function return value
                outputs.push(crate::bricks::types::BrickOutput {
                    id: "result".to_string(),
                    label: "Result".to_string(),
                    r#type: brick!(@get_return_type $return_type),
                });

                // Outputs only come from function return value, not from parameters

                outputs
            }

            // Generate the brick structure function
            pub fn [<$fn_name _brick>]() -> crate::bricks::types::Brick {
                let mut arguments = Vec::new();
                let mut inputs = Vec::new();
                let mut outputs = Vec::new();

                // Process each parameter based on its attributes
                $(
                    brick!(@process_param_with_attrs
                        arguments,
                        inputs,
                        outputs,
                        [$(#[$param_attr$(($($param_attr_content)*))? ])+],
                        $param_name,
                        $param_type,
                        brick!(@get_custom_default_or_type_default $param_type, $($default)?)
                    );
                )*

                // Always add the function return value as an output
                outputs.push(crate::bricks::types::BrickOutput {
                    id: "result".to_string(),
                    label: "Result".to_string(),
                    r#type: brick!(@get_return_type $return_type),
                });

                crate::bricks::types::Brick {
                    id: $id.to_string(),
                    label: brick!(@get_label_or_default $($label)?),
                    description: brick!(@get_description_or_default $($description)?),
                    keywords: brick!(@get_keywords_or_default $($keywords)?),
                    category: brick!(@get_category_or_default $category),
                    arguments,
                    inputs,
                    outputs,
                    execution: [<$fn_name _execution>],
                }
            }
        }
    };

    // Helper: Get label or default
    (@get_label_or_default $label:expr) => { $label.to_string() };
    (@get_label_or_default) => { "Default Label".to_string() };

    // Helper: Get description or default
    (@get_description_or_default $desc:expr) => { $desc.to_string() };
    (@get_description_or_default) => { "Default Description".to_string() };

    // Helper: Get keywords or default
    (@get_keywords_or_default $keywords:expr) => {
        $keywords.iter().map(|s| s.to_string()).collect::<Vec<String>>()
    };
    (@get_keywords_or_default) => { Vec::<String>::new() };

    // Helper: Get category (required, no default)
    (@get_category_or_default $category:expr) => { $category.to_string() };

    // Helper: Get custom default or type default
    (@get_custom_default_or_type_default $param_type:ident, $default:expr) => {
        brick!(@convert_default_to_string $default)
    };
    (@get_custom_default_or_type_default $param_type:ident,) => {
        brick!(@get_default_value $param_type)
    };

    // Helper: Convert default value to string
    (@convert_default_to_string $default:expr) => {
        stringify!($default).to_string()
    };

    // Helper: Get default value as string
    (@get_default_value String) => { "".to_string() };
    (@get_default_value i32) => { "0".to_string() };
    (@get_default_value f32) => { "0".to_string() };
    (@get_default_value bool) => { "false".to_string() };

    // Helper: Get default typed value
    (@get_default_typed_value String) => { String::new() };
    (@get_default_typed_value i32) => { 0i32 };
    (@get_default_typed_value f32) => { 0f32 };
    (@get_default_typed_value bool) => { false };

    // Helper: Get return type enum
    (@get_return_type String) => { crate::bricks::types::Type::String };
    (@get_return_type i32) => { crate::bricks::types::Type::Number };
    (@get_return_type f32) => { crate::bricks::types::Type::Number };
    (@get_return_type bool) => { crate::bricks::types::Type::Boolean };

    // Helper: Check if attribute list contains a specific attribute
    (@has_attr argument, [#[argument] $($rest:tt)*]) => { true };
    (@has_attr argument, [#[argument($($content:tt)*)] $($rest:tt)*]) => { true };
    (@has_attr argument, [#[$other:ident$($other_content:tt)*] $($rest:tt)*]) => { brick!(@has_attr argument, [$($rest)*]) };
    (@has_attr argument, []) => { false };

    (@has_attr input, [#[input] $($rest:tt)*]) => { true };
    (@has_attr input, [#[input($($content:tt)*)] $($rest:tt)*]) => { true };
    (@has_attr input, [#[$other:ident$($other_content:tt)*] $($rest:tt)*]) => { brick!(@has_attr input, [$($rest)*]) };
    (@has_attr input, []) => { false };



    // Helper: Extract label from attribute list
    (@get_attr_label [#[label($label:expr)] $($rest:tt)*]) => { $label.to_string() };
    (@get_attr_label [#[$other:ident$($other_content:tt)*] $($rest:tt)*]) => { brick!(@get_attr_label [$($rest)*]) };
    (@get_attr_label []) => { "".to_string() };

    // Helper: Get parameter value based on attributes
    (@get_param_value_with_attrs $attrs:tt, $param_type:ident, $args:expr, $inputs:expr, $param_name:expr, $custom_default:expr) => {
        if brick!(@has_attr argument, $attrs) {
            // Look in arguments
            let value = $args.iter().find(|arg| arg.id == $param_name)
                .and_then(|arg| arg.default_value.clone())
                .unwrap_or_else(|| $custom_default);
            brick!(@parse_to_type $param_type, value)
        } else if brick!(@has_attr input, $attrs) {
            // Look in inputs
            let value = $inputs.iter().find(|input| input.id == $param_name)
                .and_then(|input| input.default_value.clone())
                .unwrap_or_else(|| $custom_default);
            brick!(@parse_to_type $param_type, value)
        } else {
            // For outputs, use default value (outputs are set by function execution)
            brick!(@get_default_typed_value $param_type)
        }
    };

    // Helper: Process parameter with attributes for brick creation
    (@process_param_with_attrs $arg_vec:ident, $input_vec:ident, $output_vec:ident, $attrs:tt, $param_name:ident, $param_type:ident, $default_value:expr) => {
        let label = {
            let attr_label = brick!(@get_attr_label $attrs);
            if attr_label.is_empty() {
                stringify!($param_name).to_string()
            } else {
                attr_label
            }
        };

        if brick!(@has_attr argument, $attrs) {
            $arg_vec.push(crate::bricks::types::BrickArgument {
                id: stringify!($param_name).to_string(),
                label,
                r#type: brick!(@get_argument_type $param_type),
                enum_options: None,
                default_value: Some($default_value),
            });
        } else if brick!(@has_attr input, $attrs) {
            $input_vec.push(crate::bricks::types::BrickInput {
                id: stringify!($param_name).to_string(),
                label,
                r#type: brick!(@get_return_type $param_type),
                default_value: Some($default_value),
            });
        }
    };

    // Helper: Add tuple outputs to outputs vector - handle 1-tuple
    (@add_tuple_outputs $output_vec:ident, $result:ident, [($attrs0:tt, $type0:ident)]) => {
        // Single output
        let id0 = {
            let attr_id = brick!(@get_attr_id $attrs0);
            if attr_id.is_empty() {
                "output_0".to_string()
            } else {
                attr_id
            }
        };
        let label0 = {
            let attr_label = brick!(@get_attr_label $attrs0);
            if attr_label.is_empty() {
                "Output".to_string()
            } else {
                attr_label
            }
        };
        $output_vec.push(crate::bricks::types::BrickOutput {
            id: id0,
            label: label0,
            r#type: brick!(@get_return_type $type0),
        });
    };

    // Helper: Add tuple outputs to outputs vector - handle 2-tuple
    (@add_tuple_outputs $output_vec:ident, $result:ident, [($attrs0:tt, $type0:ident), ($attrs1:tt, $type1:ident)]) => {
        // First output
        let id0 = {
            let attr_id = brick!(@get_attr_id $attrs0);
            if attr_id.is_empty() {
                "output_0".to_string()
            } else {
                attr_id
            }
        };
        let label0 = {
            let attr_label = brick!(@get_attr_label $attrs0);
            if attr_label.is_empty() {
                "Output 1".to_string()
            } else {
                attr_label
            }
        };
        $output_vec.push(crate::bricks::types::BrickOutput {
            id: id0,
            label: label0,
            r#type: brick!(@get_return_type $type0),
        });

        // Second output
        let id1 = {
            let attr_id = brick!(@get_attr_id $attrs1);
            if attr_id.is_empty() {
                "output_1".to_string()
            } else {
                attr_id
            }
        };
        let label1 = {
            let attr_label = brick!(@get_attr_label $attrs1);
            if attr_label.is_empty() {
                "Output 2".to_string()
            } else {
                attr_label
            }
        };
        $output_vec.push(crate::bricks::types::BrickOutput {
            id: id1,
            label: label1,
            r#type: brick!(@get_return_type $type1),
        });
    };



    // Helper: Extract id from attribute list
    (@get_attr_id [#[id($id:expr)] $($rest:tt)*]) => { $id.to_string() };
    (@get_attr_id [#[$other:ident$($other_content:tt)*] $($rest:tt)*]) => { brick!(@get_attr_id [$($rest)*]) };
    (@get_attr_id []) => { "".to_string() };

    // Helper: Extract description from attribute list
    (@get_attr_description [#[description($desc:expr)] $($rest:tt)*]) => { $desc.to_string() };
    (@get_attr_description [#[$other:ident$($other_content:tt)*] $($rest:tt)*]) => { brick!(@get_attr_description [$($rest)*]) };
    (@get_attr_description []) => { "".to_string() };

    // Helper: Parse value from string
    (@parse_to_type String, $value:expr) => { $value };
    (@parse_to_type i32, $value:expr) => { $value.parse::<i32>().unwrap_or(0) };
    (@parse_to_type f32, $value:expr) => { $value.parse::<f32>().unwrap_or(0.0) };
    (@parse_to_type bool, $value:expr) => { $value.parse::<bool>().unwrap_or(false) };

    // Helper: Get argument type enum
    (@get_argument_type String) => { crate::bricks::types::Type::String };
    (@get_argument_type i32) => { crate::bricks::types::Type::Number };
    (@get_argument_type f32) => { crate::bricks::types::Type::Number };
    (@get_argument_type bool) => { crate::bricks::types::Type::Boolean };

    // Helper: Ensure parameter has valid attributes
    (@ensure_valid_attrs [$($attrs:tt)*] -> $param_name:ident) => {
        brick!(@has_any_required_attr [$($attrs)*] $param_name);
    };

    // Helper: Check if any required attribute exists
    (@has_any_required_attr [#[input] $($rest:tt)*] $param_name:ident) => {
        // Found input - valid
    };
    (@has_any_required_attr [#[argument] $($rest:tt)*] $param_name:ident) => {
        // Found argument - valid
    };
    (@has_any_required_attr [#[input($($content:tt)*)] $($rest:tt)*] $param_name:ident) => {
        // Found input with content - valid
    };
    (@has_any_required_attr [#[argument($($content:tt)*)] $($rest:tt)*] $param_name:ident) => {
        // Found argument with content - valid
    };
    (@has_any_required_attr [#[$other:ident] $($rest:tt)*] $param_name:ident) => {
        // Skip this attribute and check the rest
        brick!(@has_any_required_attr [$($rest)*] $param_name);
    };
    (@has_any_required_attr [#[$other:ident($($content:tt)*)] $($rest:tt)*] $param_name:ident) => {
        // Skip this attribute and check the rest
        brick!(@has_any_required_attr [$($rest)*] $param_name);
    };
    (@has_any_required_attr [] $param_name:ident) => {
        // No required attributes found - compile error
        compile_error!(concat!("Parameter '", stringify!($param_name), "' must have one of: #[input] or #[argument]. Outputs come from the function return value."));
    };
}

pub(crate) use brick;
