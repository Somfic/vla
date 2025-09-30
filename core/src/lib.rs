pub mod api;
pub mod bricks;
pub mod canvas;
pub mod prelude;

/// Macro to trigger execution outputs from within brick functions
///
/// Usage: `trigger!("output_id");`
///
/// This macro marks an execution output for triggering during brick execution.
/// The ExecutionEngine will collect these triggers after the brick function
/// completes and send execution signals to connected downstream nodes.
///
/// # Examples
///
/// ```rust
/// use vla::trigger;
///
/// // In an if/else brick
/// if condition {
///     trigger!("true_branch");
/// } else {
///     trigger!("false_branch");
/// }
/// ```
#[macro_export]
macro_rules! trigger {
    ($output_id:expr) => {
        $crate::bricks::execution::add_trigger($output_id);
    };
}
