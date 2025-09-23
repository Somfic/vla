pub mod macros;
pub mod types;

#[cfg(test)]
mod tests;

use crate::bricks::macros::brick;
use types::Brick;

brick! {
    #[id("addition")]
    #[label("Addition")]
    #[description("Performs addition on two numbers")]
    fn addition(
        #[input] #[label("A")] a: i32,
        #[input] #[label("B")] b: i32
    ) -> (
        #[label("Sum")] i32
    )
    {
        (a + b,)
    }
}

brick! {
    #[id("euclidean_division")]
    #[label("Euclidean division")]
    #[description("Performs a division and returns the quotient and remainder")]
    fn euclidean_division(
        #[input] #[label("Dividend")] a: i32,
        #[input] #[label("Divisor")] b: i32
    ) -> (
        #[label("Quotient")] i32,
        #[label("Remainder")] i32
    )
    {
        (a / b, a % b)
    }
}

pub fn get_all_bricks() -> Vec<Brick> {
    vec![addition_brick(), euclidean_division_brick()]
}
