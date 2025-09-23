pub mod macros;
pub mod types;

#[cfg(test)]
mod tests;

use crate::bricks::macros::brick;
use types::Brick;

brick! {
    #[id("math")]
    #[label("Math Operations")]
    #[description("Performs addition and multiplication on two numbers")]
    fn math(
        #[input] #[label("First")] first: i32 = 1,
        #[input] #[label("Second")] second: i32 = 2,
        #[argument] #[label("Invert")] invert: bool = true
    ) -> (
        #[label("Addition")] i32,
        #[label("Multiplication")] i32
    ) {
        (first + second  * if invert { -1 } else { 1 }, first * second  * if invert { -1 } else { 1 })
    }
}

pub fn get_all_bricks() -> Vec<Brick> {
    vec![math_brick()]
}
