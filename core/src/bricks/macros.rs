/// Usage:
/// ```rust,ignore
/// brick! {
///     fn hello_world(#[argument(label = "Name")] name: String = "World", #[input(label = "Data Input")] data: String) -> String {
///         id: "hello_world",
///         label: "Hello World",
///         description: "Greets someone",
///         body: {
///             format!("Hello, {}! Data: {}", name, data)
///         }
///     }
/// }
/// ```
macro_rules! brick {
    (
        fn $fn_name:ident(
            $($(#[$param_attr:ident $(($($attr_content:tt)*))?])? $param_name:ident: $param_type:ident $(= $default:expr)?),*
        ) -> $return_type:ident {
            id: $id:expr,
            label: $label:expr,
            description: $description:expr,
            body: $body:block
        }
    ) => {
        paste::paste! {
            // Define the actual function
            fn $fn_name($($param_name: $param_type),*) -> $return_type $body

            // Define the execution wrapper
            #[allow(unused_variables)]
            fn [<$fn_name _execution>](
                args: Vec<crate::bricks::types::BrickArgument>,
                inputs: Vec<crate::bricks::types::BrickInput>
            ) -> Vec<crate::bricks::types::BrickOutput> {
                // Extract arguments and inputs
                $(
                    let $param_name = brick!(@extract_value
                        $(#[$param_attr $(($($attr_content)*))?])?,
                        $param_type,
                        &args,
                        &inputs,
                        stringify!($param_name)
                        $(, $default)?
                    );
                )*

                let _result = $fn_name($($param_name),*);

                vec![crate::bricks::types::BrickOutput {
                    id: "result".to_string(),
                    label: "Result".to_string(),
                    r#type: brick!(@return_type $return_type),
                }]
            }

            // Define the brick getter function
            #[allow(unused_mut)]
            pub fn [<$fn_name _brick>]() -> crate::bricks::types::Brick {
                let mut arguments = Vec::new();
                let mut inputs = Vec::new();

                $(
                    brick!(@push_argument
                        arguments,
                        $(#[$param_attr $(($($attr_content)*))?])?,
                        $param_name,
                        $param_type
                        $(, $default)?
                    );
                    brick!(@push_input
                        inputs,
                        $(#[$param_attr $(($($attr_content)*))?])?,
                        $param_name,
                        $param_type
                        $(, $default)?
                    );
                )*

                crate::bricks::types::Brick {
                    id: $id.to_string(),
                    label: $label.to_string(),
                    description: $description.to_string(),
                    arguments,
                    inputs,
                    outputs: vec![
                        crate::bricks::types::BrickOutput {
                            id: "result".to_string(),
                            label: "Result".to_string(),
                            r#type: brick!(@return_type $return_type),
                        }
                    ],
                    execution: [<$fn_name _execution>],
                }
            }
        }
    };

    // Helper to get default values
    (@default_value String) => { "".to_string() };
    (@default_value String, $default:expr) => { $default.to_string() };
    (@default_value i32) => { "0".to_string() };
    (@default_value i32, $default:expr) => { $default.to_string() };
    (@default_value bool) => { "false".to_string() };
    (@default_value bool, $default:expr) => { $default.to_string() };

    // Helper to parse string values to proper types
    (@parse_value String, $value:expr) => { $value };
    (@parse_value i32, $value:expr) => { $value.parse::<i32>().unwrap_or(0) };
    (@parse_value bool, $value:expr) => { $value.parse::<bool>().unwrap_or(false) };

    // Convert type identifiers to BrickArgumentType
    (@arg_type String) => { crate::bricks::types::BrickArgumentType::String };
    (@arg_type i32) => { crate::bricks::types::BrickArgumentType::Number };
    (@arg_type bool) => { crate::bricks::types::BrickArgumentType::Boolean };

    // Convert type identifiers to BrickHandleType for return values
    (@return_type String) => { crate::bricks::types::BrickHandleType::String };
    (@return_type i32) => { crate::bricks::types::BrickHandleType::Number };
    (@return_type bool) => { crate::bricks::types::BrickHandleType::Boolean };

    // Extract label from attribute content
    (@extract_label label = $label:expr) => { $label.to_string() };
    (@extract_label $param_name:ident) => { stringify!($param_name).to_string() };

    // Extract default value handling
    (@maybe_default_value $param_type:ident) => { None };
    (@maybe_default_value $param_type:ident, $default:expr) => { Some(brick!(@default_value $param_type, $default)) };

    // Extract value from arguments or inputs based on annotation
    (@extract_value #[argument($($attr_content:tt)*)], $param_type:ident, $args:expr, $inputs:expr, $param_name:expr) => {
        {
            let value = $args.iter().find(|arg| arg.id == $param_name)
                .and_then(|arg| arg.default_value.clone())
                .unwrap_or_else(|| brick!(@default_value $param_type));
            brick!(@parse_value $param_type, value)
        }
    };
    (@extract_value #[argument($($attr_content:tt)*)], $param_type:ident, $args:expr, $inputs:expr, $param_name:expr, $default:expr) => {
        {
            let value = $args.iter().find(|arg| arg.id == $param_name)
                .and_then(|arg| arg.default_value.clone())
                .unwrap_or_else(|| brick!(@default_value $param_type, $default));
            brick!(@parse_value $param_type, value)
        }
    };
    (@extract_value #[argument], $param_type:ident, $args:expr, $inputs:expr, $param_name:expr) => {
        {
            let value = $args.iter().find(|arg| arg.id == $param_name)
                .and_then(|arg| arg.default_value.clone())
                .unwrap_or_else(|| brick!(@default_value $param_type));
            brick!(@parse_value $param_type, value)
        }
    };
    (@extract_value #[argument], $param_type:ident, $args:expr, $inputs:expr, $param_name:expr, $default:expr) => {
        {
            let value = $args.iter().find(|arg| arg.id == $param_name)
                .and_then(|arg| arg.default_value.clone())
                .unwrap_or_else(|| brick!(@default_value $param_type, $default));
            brick!(@parse_value $param_type, value)
        }
    };
    (@extract_value #[input($($attr_content:tt)*)], $param_type:ident, $args:expr, $inputs:expr, $param_name:expr) => {
        {
            let value = $inputs.iter().find(|input| input.id == $param_name)
                .and_then(|input| input.default_value.clone())
                .unwrap_or_else(|| brick!(@default_value $param_type));
            brick!(@parse_value $param_type, value)
        }
    };
    (@extract_value #[input($($attr_content:tt)*)], $param_type:ident, $args:expr, $inputs:expr, $param_name:expr, $default:expr) => {
        {
            let value = $inputs.iter().find(|input| input.id == $param_name)
                .and_then(|input| input.default_value.clone())
                .unwrap_or_else(|| brick!(@default_value $param_type, $default));
            brick!(@parse_value $param_type, value)
        }
    };
    (@extract_value #[input], $param_type:ident, $args:expr, $inputs:expr, $param_name:expr) => {
        {
            let value = $inputs.iter().find(|input| input.id == $param_name)
                .and_then(|input| input.default_value.clone())
                .unwrap_or_else(|| brick!(@default_value $param_type));
            brick!(@parse_value $param_type, value)
        }
    };
    (@extract_value #[input], $param_type:ident, $args:expr, $inputs:expr, $param_name:expr, $default:expr) => {
        {
            let value = $inputs.iter().find(|input| input.id == $param_name)
                .and_then(|input| input.default_value.clone())
                .unwrap_or_else(|| brick!(@default_value $param_type, $default));
            brick!(@parse_value $param_type, value)
        }
    };
    // Default to argument if no annotation is provided
    (@extract_value , $param_type:ident, $args:expr, $inputs:expr, $param_name:expr) => {
        {
            let value = $args.iter().find(|arg| arg.id == $param_name)
                .and_then(|arg| arg.default_value.clone())
                .unwrap_or_else(|| brick!(@default_value $param_type));
            brick!(@parse_value $param_type, value)
        }
    };
    (@extract_value , $param_type:ident, $args:expr, $inputs:expr, $param_name:expr, $default:expr) => {
        {
            let value = $args.iter().find(|arg| arg.id == $param_name)
                .and_then(|arg| arg.default_value.clone())
                .unwrap_or_else(|| brick!(@default_value $param_type, $default));
            brick!(@parse_value $param_type, value)
        }
    };

    // Generate argument definitions
    (@push_argument $vec:ident, #[argument($($attr_content:tt)*)], $param_name:ident, $param_type:ident) => {
        $vec.push(crate::bricks::types::BrickArgument {
            id: stringify!($param_name).to_string(),
            label: brick!(@extract_label $($attr_content)*),
            r#type: brick!(@arg_type $param_type),
            enum_options: None,
            default_value: brick!(@maybe_default_value $param_type),
        });
    };
    (@push_argument $vec:ident, #[argument($($attr_content:tt)*)], $param_name:ident, $param_type:ident, $default:expr) => {
        $vec.push(crate::bricks::types::BrickArgument {
            id: stringify!($param_name).to_string(),
            label: brick!(@extract_label $($attr_content)*),
            r#type: brick!(@arg_type $param_type),
            enum_options: None,
            default_value: brick!(@maybe_default_value $param_type, $default),
        });
    };
    (@push_argument $vec:ident, #[argument], $param_name:ident, $param_type:ident) => {
        $vec.push(crate::bricks::types::BrickArgument {
            id: stringify!($param_name).to_string(),
            label: brick!(@extract_label $param_name),
            r#type: brick!(@arg_type $param_type),
            enum_options: None,
            default_value: brick!(@maybe_default_value $param_type),
        });
    };
    (@push_argument $vec:ident, #[argument], $param_name:ident, $param_type:ident, $default:expr) => {
        $vec.push(crate::bricks::types::BrickArgument {
            id: stringify!($param_name).to_string(),
            label: brick!(@extract_label $param_name),
            r#type: brick!(@arg_type $param_type),
            enum_options: None,
            default_value: brick!(@maybe_default_value $param_type, $default),
        });
    };
    (@push_argument $vec:ident, #[input($($attr_content:tt)*)], $param_name:ident, $param_type:ident) => {};
    (@push_argument $vec:ident, #[input($($attr_content:tt)*)], $param_name:ident, $param_type:ident, $default:expr) => {};
    (@push_argument $vec:ident, #[input], $param_name:ident, $param_type:ident) => {};
    (@push_argument $vec:ident, #[input], $param_name:ident, $param_type:ident, $default:expr) => {};
    // Default to argument if no annotation
    (@push_argument $vec:ident, , $param_name:ident, $param_type:ident) => {
        $vec.push(crate::bricks::types::BrickArgument {
            id: stringify!($param_name).to_string(),
            label: brick!(@extract_label $param_name),
            r#type: brick!(@arg_type $param_type),
            enum_options: None,
            default_value: brick!(@maybe_default_value $param_type),
        });
    };
    (@push_argument $vec:ident, , $param_name:ident, $param_type:ident, $default:expr) => {
        $vec.push(crate::bricks::types::BrickArgument {
            id: stringify!($param_name).to_string(),
            label: brick!(@extract_label $param_name),
            r#type: brick!(@arg_type $param_type),
            enum_options: None,
            default_value: brick!(@maybe_default_value $param_type, $default),
        });
    };

    // Generate input definitions
    (@push_input $vec:ident, #[input($($attr_content:tt)*)], $param_name:ident, $param_type:ident) => {
        $vec.push(crate::bricks::types::BrickInput {
            id: stringify!($param_name).to_string(),
            label: brick!(@extract_label $($attr_content)*),
            r#type: brick!(@input_type $param_type),
            default_value: brick!(@maybe_default_value $param_type),
        });
    };
    (@push_input $vec:ident, #[input($($attr_content:tt)*)], $param_name:ident, $param_type:ident, $default:expr) => {
        $vec.push(crate::bricks::types::BrickInput {
            id: stringify!($param_name).to_string(),
            label: brick!(@extract_label $($attr_content)*),
            r#type: brick!(@input_type $param_type),
            default_value: brick!(@maybe_default_value $param_type, $default),
        });
    };
    (@push_input $vec:ident, #[input], $param_name:ident, $param_type:ident) => {
        $vec.push(crate::bricks::types::BrickInput {
            id: stringify!($param_name).to_string(),
            label: brick!(@extract_label $param_name),
            r#type: brick!(@input_type $param_type),
            default_value: brick!(@maybe_default_value $param_type),
        });
    };
    (@push_input $vec:ident, #[input], $param_name:ident, $param_type:ident, $default:expr) => {
        $vec.push(crate::bricks::types::BrickInput {
            id: stringify!($param_name).to_string(),
            label: brick!(@extract_label $param_name),
            r#type: brick!(@input_type $param_type),
            default_value: brick!(@maybe_default_value $param_type, $default),
        });
    };
    (@push_input $vec:ident, #[argument($($attr_content:tt)*)], $param_name:ident, $param_type:ident) => {};
    (@push_input $vec:ident, #[argument($($attr_content:tt)*)], $param_name:ident, $param_type:ident, $default:expr) => {};
    (@push_input $vec:ident, #[argument], $param_name:ident, $param_type:ident) => {};
    (@push_input $vec:ident, #[argument], $param_name:ident, $param_type:ident, $default:expr) => {};
    (@push_input $vec:ident, , $param_name:ident, $param_type:ident) => {};
    (@push_input $vec:ident, , $param_name:ident, $param_type:ident, $default:expr) => {};

    // Convert type identifiers to BrickHandleType for inputs
    (@input_type String) => { crate::bricks::types::BrickHandleType::String };
    (@input_type i32) => { crate::bricks::types::BrickHandleType::Number };
    (@input_type bool) => { crate::bricks::types::BrickHandleType::Boolean };
}

pub(crate) use brick;
