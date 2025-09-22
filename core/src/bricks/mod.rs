pub mod macros;
mod test_macro;
pub mod types;

use types::Brick;

use crate::bricks::macros::brick;

pub fn get_all_bricks() -> Vec<Brick> {
    vec![add_numbers_brick()]
}

brick! {
    fn add_numbers(
        #[input(label = "First")] first: i32,
        #[input(label = "Second")] second: i32 = 1)
        ->
        #[output(label = "Hi")] i32 {
        id: "add_numbers",
        label: "Add Numbers",
        description: "Adds two numbers together",
        body: {
            first + second
        }
    }
}
