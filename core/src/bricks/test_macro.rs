#[cfg(test)]
mod tests {
    use super::super::macros::brick;

    brick! {
        #[id("test_brick")]
        #[label("Test Brick")]
        #[description("A simple test brick")]
        fn test_brick(#[argument] value: i32 = 42) -> String {
            format!("Value: {}", value)
        }
    }

    #[test]
    fn test_basic_functionality() {
        let brick = test_brick_brick();
        assert_eq!(brick.id, "test_brick");
        assert_eq!(brick.label, "Test Brick");
        assert_eq!(brick.description, "A simple test brick");
        assert_eq!(brick.arguments.len(), 1);
        assert_eq!(brick.inputs.len(), 0);
        assert_eq!(brick.outputs.len(), 1);
    }
}
