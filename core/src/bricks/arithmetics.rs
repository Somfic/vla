use crate::bricks::macros::brick;
use crate::prelude::*;

pub fn all_bricks() -> Vec<Brick> {
    vec![
        add_brick(),
        subtract_brick(),
        multiply_brick(),
        divide_brick(),
    ]
}

brick! {
    #[id("add")]
    #[label("Addition")]
    #[description("Performs addition on two numbers")]
    fn add(
        #[input] #[label("A")] a: i32,
        #[input] #[label("B")] b: i32
    ) -> (
        #[label("A + B")] i32
    )
    {
        (a + b,)
    }
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

brick! {
    #[id("divide")]
    #[label("Division")]
    #[description("Performs division on two numbers")]
    fn divide(
        #[input] #[label("A")] a: i32,
        #[input] #[label("B")] b: i32
    ) -> (
        #[label("A ÷ B")] i32
    )
    {
        if b == 0 {
            (0,) // Handle division by zero
        } else {
            (a / b,)
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn add_test() {
        let brick = add_brick();
        assert_eq!(brick.id, "add");
        assert_eq!(add(1, 1), (2,));
        assert_eq!(add(5, 3), (8,));
        assert_eq!(add(0, 5), (5,));
        assert_eq!(add(-2, -3), (-5,));
        assert_eq!(add(-5, 2), (-3,));
    }

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

    #[test]
    fn divide_test() {
        let brick = divide_brick();
        assert_eq!(brick.id, "divide");
        assert_eq!(divide(6, 3), (2,));
        assert_eq!(divide(7, 2), (3,)); // Integer division
        assert_eq!(divide(-6, 3), (-2,));
        assert_eq!(divide(6, -3), (-2,));
        assert_eq!(divide(-6, -3), (2,));
        assert_eq!(divide(5, 0), (0,)); // Division by zero case
    }
}
