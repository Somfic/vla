// Quick test to verify execution flow brick generation
use vla::bricks::control_flow::{if_else_brick, start_brick};

fn main() {
    // Test the if_else brick structure
    let if_else = if_else_brick();
    println!("If/Else Brick:");
    println!("  ID: {}", if_else.id);
    println!("  Execution Inputs: {:?}", if_else.execution_inputs);
    println!("  Execution Outputs: {:?}", if_else.execution_outputs);
    println!("  Data Inputs: {}", if_else.inputs.len());
    println!("  Data Outputs: {}", if_else.outputs.len());

    // Test the start brick structure
    let start = start_brick();
    println!("\nStart Brick:");
    println!("  ID: {}", start.id);
    println!("  Execution Inputs: {:?}", start.execution_inputs);
    println!("  Execution Outputs: {:?}", start.execution_outputs);
    println!("  Data Inputs: {}", start.inputs.len());
    println!("  Data Outputs: {}", start.outputs.len());
}