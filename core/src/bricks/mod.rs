pub mod macros;
pub mod types;

use crate::bricks::macros::brick;
use types::Brick;

brick! {
    #[id("math")]
    #[label("Math Operations")]
    #[description("Performs addition and multiplication on two numbers")]
    fn math(
        #[input] #[label("First")] first: i32,
        #[input] #[label("Second")] second: i32
    ) -> (
        #[label("Addition")] i32,
        #[label("Multiplication")] i32
    ) {
        (first + second, first * second)
    }
}

pub fn get_all_bricks() -> Vec<Brick> {
    vec![math_brick()]
}
