use crate::bricks::macros::brick;
use crate::prelude::*;

pub fn all_bricks() -> Vec<Brick> {
    vec![
        add_brick(),
        subtract_brick(),
        multiply_brick(),
        divide_brick(),
        exponent_brick(),
        modulo_brick(),
    ]
}

brick! {
    #[id("add")]
    #[label("Addition")]
    #[description("Performs addition on two numbers")]
    #[category("Arithmetic")]
    fn add(
        #[input] #[label("A")] a: f32 = 1,
        #[input] #[label("B")] b: f32 = 1
    ) -> (
        #[label("A + B")] f32
    )
    {
        (a + b,)
    }
}

brick! {
    #[id("subtract")]
    #[label("Subtraction")]
    #[description("Performs subtraction on two numbers")]
    #[category("Arithmetic")]
    fn subtract(
        #[input] #[label("A")] a: f32,
        #[input] #[label("B")] b: f32
    ) -> (
        #[label("A - B")] f32
    )
    {
        (a - b,)
    }
}

brick! {
    #[id("multiply")]
    #[label("Multiplication")]
    #[description("Performs multiplication on two numbers")]
    #[category("Arithmetic")]
    fn multiply(
        #[input] #[label("A")] a: f32,
        #[input] #[label("B")] b: f32
    ) -> (
        #[label("A × B")] f32
    )
    {
        (a * b,)
    }
}

brick! {
    #[id("divide")]
    #[label("Division")]
    #[description("Performs division on two numbers")]
    #[category("Arithmetic")]
    fn divide(
        #[input] #[label("A")] a: f32,
        #[input] #[label("B")] b: f32
    ) -> (
        #[label("A ÷ B")] f32
    )
    {
        if b.abs() < f32::EPSILON {
            if a == f32::EPSILON {
                (f32::NAN,)
            } else if a > 0.0 {
                (f32::INFINITY,)
            } else {
                (f32::NEG_INFINITY,)
            }
        } else {
            (a / b,)
        }
    }
}

brick! {
    #[id("modulo")]
    #[label("Modulo")]
    #[description("Performs modulo operation on two numbers")]
    #[category("Arithmetic")]
    fn modulo(
        #[input] #[label("A")] a: f32,
        #[input] #[label("B")] b: f32
    ) -> (
        #[label("A % B")] f32
    )
    {
        (a % b,)
    }
}

brick! {
    #[id("exponent")]
    #[label("Exponent")]
    #[description("Performs exponentiation on two numbers")]
    #[category("Arithmetic")]
    fn exponent(
        #[input] #[label("A")] a: f32,
        #[input] #[label("B")] b: f32
    ) -> (
        #[label("A<sup>B</sup>")] f32
    )
    {
        (a.powf(b),)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn add_test() {
        let brick = add_brick();
        assert_eq!(brick.id, "add");
        assert_eq!(add(1.0, 1.0), (2.0,));
        assert_eq!(add(5.0, 3.0), (8.0,));
        assert_eq!(add(0.0, 5.0), (5.0,));
        assert_eq!(add(-2.0, -3.0), (-5.0,));
        assert_eq!(add(-5.0, 2.0), (-3.0,));
    }

    #[test]
    fn subtract_test() {
        let brick = subtract_brick();
        assert_eq!(brick.id, "subtract");
        assert_eq!(subtract(1.0, 1.0), (0.0,));
        assert_eq!(subtract(5.0, 3.0), (2.0,));
        assert_eq!(subtract(0.0, 5.0), (-5.0,));
        assert_eq!(subtract(-2.0, -3.0), (1.0,));
        assert_eq!(subtract(-5.0, 2.0), (-7.0,));
    }

    #[test]
    fn multiply_test() {
        let brick = multiply_brick();
        assert_eq!(brick.id, "multiply");
        assert_eq!(multiply(2.0, 3.0), (6.0,));
        assert_eq!(multiply(-2.0, 3.0), (-6.0,));
        assert_eq!(multiply(2.0, -3.0), (-6.0,));
        assert_eq!(multiply(-2.0, -3.0), (6.0,));
        assert_eq!(multiply(0.0, 5.0), (0.0,));
    }

    #[test]
    fn divide_test() {
        let brick = divide_brick();
        assert_eq!(brick.id, "divide");
        assert_eq!(divide(6.0, 3.0), (2.0,));
        assert_eq!(divide(7.0, 2.0), (3.5,)); // Float division
        assert_eq!(divide(-6.0, 3.0), (-2.0,));
        assert_eq!(divide(6.0, -3.0), (-2.0,));
        assert_eq!(divide(-6.0, -3.0), (2.0,));
        assert_eq!(divide(5.0, 0.0), (f32::INFINITY,)); // Division by zero case
    }

    #[test]
    fn exponent_test() {
        let brick = exponent_brick();
        assert_eq!(brick.id, "exponent");
        assert_eq!(exponent(2.0, 3.0), (8.0,));
        assert_eq!(exponent(5.0, 0.0), (1.0,)); // Any number to the power of 0 is 1
        assert_eq!(exponent(3.0, 1.0), (3.0,)); // Any number to the power of 1 is itself
        assert_eq!(exponent(2.0, -2.0), (0.25,)); // Negative exponent case
    }

    #[test]
    fn modulo_test() {
        let brick = modulo_brick();
        assert_eq!(brick.id, "modulo");
        assert_eq!(modulo(5.0, 3.0), (2.0,));
        assert_eq!(modulo(10.0, 4.0), (2.0,));
        assert_eq!(modulo(7.0, 7.0), (0.0,)); // Any number modulo itself is 0
        assert_eq!(modulo(5.0, 1.0), (0.0,)); // Any number modulo 1 is 0
        assert_eq!(modulo(-5.0, 3.0), (-2.0,)); // Negative dividend case
        assert_eq!(modulo(5.0, -3.0), (2.0,)); // Negative divisor case
    }
}
