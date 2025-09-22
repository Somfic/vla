use super::macros::brick_fn;
use super::types::*;

// Mathematical operations
brick_fn! {
    fn multiply(a: i32 = 1, b: i32 = 1) -> i32 {
        id: "multiply",
        label: "Multiply",
        description: "Multiplies two numbers",
        body: {
            a * b
        }
    }
}

brick_fn! {
    fn divide(dividend: i32 = 10, divisor: i32 = 2) -> i32 {
        id: "divide",
        label: "Divide",
        description: "Divides dividend by divisor",
        body: {
            if divisor != 0 { dividend / divisor } else { 0 }
        }
    }
}

// String operations
brick_fn! {
    fn uppercase(text: String = "hello") -> String {
        id: "uppercase",
        label: "Uppercase",
        description: "Converts text to uppercase",
        body: {
            text.to_uppercase()
        }
    }
}

brick_fn! {
    fn concat_strings(first: String = "Hello", second: String = "World") -> String {
        id: "concat_strings",
        label: "Concatenate Strings",
        description: "Concatenates two strings with a space",
        body: {
            format!("{} {}", first, second)
        }
    }
}

// Boolean operations
brick_fn! {
    fn is_even(number: i32 = 0) -> bool {
        id: "is_even",
        label: "Is Even",
        description: "Checks if a number is even",
        body: {
            number % 2 == 0
        }
    }
}

pub fn get_example_bricks() -> Vec<Brick> {
    vec![
        get_multiply_brick(),
        get_divide_brick(),
        get_uppercase_brick(),
        get_concat_strings_brick(),
        get_is_even_brick(),
    ]
}
