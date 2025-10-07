pub mod api;
pub mod bricks;
pub mod canvas;
pub mod engine;
pub mod prelude;

#[macro_export]
macro_rules! trigger {
    ($output_id:expr) => {
        $crate::engine::trigger::add_trigger($output_id);
    };
}

#[macro_export]
macro_rules! set_current_node_id {
    ($node_id:expr) => {
        $crate::engine::trigger::set_current_node_id($node_id);
    };
}
