// Quick test to verify execution flow brick labels
use std::path::Path;

fn main() {
    // Add core as a dependency path
    let core_path = Path::new("core");
    if core_path.exists() {
        println!("cargo:rustc-link-search=native=core/target/debug/deps");
    }
}

#[cfg(test)]
mod tests {
    use vla::bricks::control_flow::{if_else_brick, start_brick};

    #[test]
    fn test_execution_labels() {
        // Test the if_else brick with custom labels
        let if_else = if_else_brick();
        println!("If/Else Brick execution metadata:");

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

        println!("✅ All execution labels work correctly!");
    }
}