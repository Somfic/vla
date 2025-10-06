use crate::{bricks::macros::brick, prelude::*};

brick! {
    #[id("print")]
    #[label("Print")]
    #[description("Prints the input value to the console for debugging purposes")]
    #[category("Debug")]
    #[execution_input("execute", "Execute")]
    fn print(
        #[input] #[label("Value")] value: String
    ) -> () {
        println!("Debug Print: {}", value);
    }
}

pub fn all_bricks() -> Vec<Brick> {
    vec![print_brick()]
}
