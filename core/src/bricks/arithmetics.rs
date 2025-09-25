use crate::bricks::macros::brick;
use crate::prelude::*;

pub fn all_bricks() -> Vec<Brick> {
    vec![subtract_brick(), multiply_brick()]
}

brick! {
    #[id("subtract")]
    #[label("Subtraction")]
    #[description("Performs subtraction on two numbers")]
    fn subtract(
        #[input] #[label("A")] a: i32,
        #[input] #[label("B")] b: i32
    ) -> (
        #[label("A - B")] i32
    )
    {
        (a - b,)
    }
}

brick! {
    #[id("multiply")]
    #[label("Multiplication")]
    #[description("Performs multiplication on two numbers")]
    fn multiply(
        #[input] #[label("A")] a: i32,
        #[input] #[label("B")] b: i32
    ) -> (
        #[label("A × B")] i32
    )
    {
        (a * b,)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn subtract_test() {
        let brick = subtract_brick();
        assert_eq!(brick.id, "subtract");
        assert_eq!(subtract(1, 1), (0,));
        assert_eq!(subtract(5, 3), (2,));
        assert_eq!(subtract(0, 5), (-5,));
        assert_eq!(subtract(-2, -3), (1,));
        assert_eq!(subtract(-5, 2), (-7,));
    }

    #[test]
    fn multiply_test() {
        let brick = multiply_brick();
        assert_eq!(brick.id, "multiply");
        assert_eq!(multiply(2, 3), (6,));
        assert_eq!(multiply(-2, 3), (-6,));
        assert_eq!(multiply(2, -3), (-6,));
        assert_eq!(multiply(-2, -3), (6,));
        assert_eq!(multiply(0, 5), (0,));
    }
}
