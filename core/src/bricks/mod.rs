pub mod macros;
mod test_macro;
pub mod types;

use types::Brick;

use crate::bricks::macros::brick;

pub fn get_all_bricks() -> Vec<Brick> {
    vec![add_numbers_brick()]
}

/*

brick! {
    #[id("add_numbers")]
    #[label("Add Numbers")]
    #[description("Adds two numbers together")]
    fn add_numbers(
        #[input(label = "First")] first: i32,
        #[input(label = "Second")] second: i32 = 1)
        ->
        #[output(label = "Hi")] i32 {
        first + second
    }
}

*/

brick! {
    #[id("add_numbers")]
    #[label("Add Numbers")]
    #[description("Adds two numbers together")]
    fn add_numbers(
        #[input(label = "First")] first: i32,
        #[input(label = "Second")] second: i32 = 1)
        ->
        #[output(label = "Hi")] i32 {
        first + second
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_add_numbers_brick_structure() {
        let brick = add_numbers_brick();
        assert_eq!(brick.id, "add_numbers");
        assert_eq!(brick.label, "Add Numbers");
        assert_eq!(brick.description, "Adds two numbers together");
        assert_eq!(brick.arguments.len(), 0);
        assert_eq!(brick.inputs.len(), 2);
        assert_eq!(brick.outputs.len(), 1);
        
        // Check input labels
        assert_eq!(brick.inputs[0].label, "First");
        assert_eq!(brick.inputs[1].label, "Second");
        
        // Check output label
        assert_eq!(brick.outputs[0].label, "Hi");
    }
    
    #[test]
    fn test_add_numbers_function() {
        assert_eq!(add_numbers(5, 3), 8);
        assert_eq!(add_numbers(10, 1), 11);
    }
}
