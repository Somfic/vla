/// Usage:
/// ```
/// brick_fn! {
///     fn hello_world(name: String = "World") -> String {
///         id: "hello_world",
///         label: "Hello World",
///         description: "Greets someone",
///         body: {
///             format!("Hello, {}!", name)
///         }
///     }
/// }
/// ```
macro_rules! brick_fn {
    (
        fn $fn_name:ident(
            $($param_name:ident: $param_type:ident $(= $default:expr)?),*
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
            fn [<$fn_name _execution>](
                args: Vec<crate::bricks::types::BrickArgument>,
                _inputs: Vec<crate::bricks::types::BrickInput>
            ) -> Vec<crate::bricks::types::BrickOutput> {
                // Extract arguments
                let mut arg_iter = args.iter();
                $(
                    let $param_name = arg_iter.next()
                        .and_then(|arg| arg.default_value.clone())
                        .unwrap_or_else(|| brick_fn!(@default_value $param_type $(, $default)?));
                    let $param_name = brick_fn!(@parse_value $param_type, $param_name);
                )*

                let _result = $fn_name($($param_name),*);

                vec![crate::bricks::types::BrickOutput {
                    id: "result".to_string(),
                    label: "Result".to_string(),
                    r#type: brick_fn!(@return_type $return_type),
                }]
            }

            // Define the brick getter function
            pub fn [<get_ $fn_name _brick>]() -> crate::bricks::types::Brick {
                crate::bricks::types::Brick {
                    id: $id.to_string(),
                    label: $label.to_string(),
                    description: $description.to_string(),
                    arguments: vec![
                        $(
                            crate::bricks::types::BrickArgument {
                                id: stringify!($param_name).to_string(),
                                label: stringify!($param_name).to_string(),
                                r#type: brick_fn!(@arg_type $param_type),
                                enum_options: None,
                                default_value: Some(brick_fn!(@default_value $param_type $(, $default)?)),
                            }
                        ),*
                    ],
                    inputs: vec![],
                    outputs: vec![
                        crate::bricks::types::BrickOutput {
                            id: "result".to_string(),
                            label: "Result".to_string(),
                            r#type: brick_fn!(@return_type $return_type),
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
}

pub(crate) use brick_fn;
