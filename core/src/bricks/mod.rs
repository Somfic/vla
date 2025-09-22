pub mod examples;
pub mod macros;
pub mod test;
pub mod types;

use types::Brick;

pub fn get_all_bricks() -> Vec<Brick> {
    let mut bricks = Vec::new();
    bricks.extend(test::get_test_bricks());
    bricks.extend(examples::get_example_bricks());
    bricks
}
