use crate::bricks::macros::brick;
use crate::prelude::*;
use crate::trigger;

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_execution_labels() {
        // Test the if_else brick with custom labels
        let if_else = if_else_brick();

        // Check execution inputs
        assert_eq!(if_else.execution_inputs.len(), 1);
        assert_eq!(if_else.execution_inputs[0].id, "execute");
        assert_eq!(if_else.execution_inputs[0].label, "Execute");

        // Check execution outputs
        assert_eq!(if_else.execution_outputs.len(), 2);
        assert_eq!(if_else.execution_outputs[0].id, "true_branch");
        assert_eq!(if_else.execution_outputs[0].label, "True");
        assert_eq!(if_else.execution_outputs[1].id, "false_branch");
        assert_eq!(if_else.execution_outputs[1].label, "False");

        // Test the start brick
        let start = start_brick();

        // Should have no execution inputs
        assert_eq!(start.execution_inputs.len(), 0);

        // Should have one execution output with custom label
        assert_eq!(start.execution_outputs.len(), 1);
        assert_eq!(start.execution_outputs[0].id, "begin");
        assert_eq!(start.execution_outputs[0].label, "Begin Execution");
    }
}

pub fn all_bricks() -> Vec<Brick> {
    vec![
        if_else_brick(),
        start_brick(),
    ]
}

// Test brick with execution flow
brick! {
    #[id("if_else")]
    #[label("If/Else")]
    #[description("Conditional execution - runs true branch if condition is true, false branch otherwise")]
    #[category("Control Flow")]
    #[execution_input("execute", "Execute")]
    #[execution_output("true_branch", "True")]
    #[execution_output("false_branch", "False")]
    fn if_else(
        #[input] #[label("Condition")] condition: bool
    ) -> (
        #[label("Condition Value")] bool
    ) {
        if condition {
            trigger!("true_branch");
        } else {
            trigger!("false_branch");
        }

        (condition,)
    }
}

// Start node example
brick! {
    #[id("start")]
    #[label("Start")]
    #[description("Starts execution flow")]
    #[category("Control Flow")]
    #[execution_output("begin", "Begin Execution")]
    fn start() -> (
        #[label("Started")] bool
    ) {
        trigger!("begin");
        (true,)
    }
}