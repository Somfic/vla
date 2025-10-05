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
