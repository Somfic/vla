use super::macros::brick_fn;
use super::types::*;

// Define bricks using the new improved macro syntax
brick_fn! {
    fn hello_world(name: String = "World") -> String {
        id: "hello_world",
        label: "Hello World",
        description: "A simple test brick that outputs a greeting message",
        body: {
            format!("Hello, {}!", name)
        }
    }
}

brick_fn! {
    fn add_numbers(first: i32 = 0, second: i32 = 0) -> i32 {
        id: "add_numbers",
        label: "Add Numbers",
        description: "Adds two numbers together",
        body: {
            first + second
        }
    }
}

brick_fn! {
    fn is_positive(number: i32 = 0) -> bool {
        id: "is_positive",
        label: "Is Positive",
        description: "Checks if a number is positive",
        body: {
            number > 0
        }
    }
}

pub fn get_test_bricks() -> Vec<Brick> {
    vec![
        get_hello_world_brick(),
        get_add_numbers_brick(),
        get_is_positive_brick(),
    ]
}
