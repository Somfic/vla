pub mod api;
pub mod bricks;
pub mod canvas;
pub mod prelude;

#[macro_export]
macro_rules! trigger {
    ($output_id:expr) => {
        $crate::bricks::execution::add_trigger($output_id);
    };
}
