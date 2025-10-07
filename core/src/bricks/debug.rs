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

    #[test]
    fn test_trigger_execution() {
        let brick = trigger_brick();
        let outputs = (brick.execution)(vec![], vec![]);
        println!("Trigger execution returned {} outputs", outputs.len());
        assert_eq!(outputs.len(), 1, "Should return 1 output");

        // Check if triggers were set
        let trigger_count = crate::engine::trigger::trigger_count();
        println!("Trigger count: {}", trigger_count);
        assert!(trigger_count > 0, "Should have set at least one trigger");
    }
}
