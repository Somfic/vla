use crate::{bricks::macros::brick, prelude::*, trigger};

brick! {
    #[id("print")]
    #[label("Print")]
    #[description("Prints the input value to the console for debugging purposes")]
    #[category("Debug")]
    #[execution_input("execute", "Execute")]
    fn print(
        #[input] #[label("Value")] value: String
    ) -> () {
        println!("{}", value);
    }
}

brick! {
    #[id("trigger")]
    #[label("Trigger")]
    #[description("Manual trigger to start execution flow")]
    #[category("Debug")]
    #[execution_output("triggered", "Triggered")]
    fn trigger() -> (
        #[label("Done")] bool
    ) {
        trigger!("triggered");
        (true,)
    }
}

pub fn all_bricks() -> Vec<Brick> {
    vec![print_brick(), trigger_brick()]
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_trigger_brick_definition() {
        let brick = trigger_brick();
        println!(
            "Trigger brick: id={}, exec_outputs={}",
            brick.id,
            brick.execution_outputs.len()
        );
        assert_eq!(brick.id, "trigger");
        assert_eq!(brick.execution_outputs.len(), 1);
        assert_eq!(brick.execution_outputs[0].id, "triggered");
    }
}
