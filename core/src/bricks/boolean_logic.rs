use crate::bricks::macros::brick;

brick! {
    #[id("logical_or")]
    #[label("OR")]
    #[description("Performs logical OR operation on two boolean values")]
    fn logical_or(
        #[input] #[label("A")] a: bool,
        #[input] #[label("B")] b: bool
    ) -> (
        #[label("Result")] bool
    )
    {
        (a || b,)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_logical_or_brick_basic() {
        let brick = logical_or_brick();

        assert_eq!(brick.id, "logical_or");
        assert_eq!(brick.label, "OR");
        assert_eq!(
            brick.description,
            "Performs logical OR operation on two boolean values"
        );

        // Should have 0 arguments, 2 inputs, and 1 output
        assert_eq!(brick.arguments.len(), 0);
        assert_eq!(brick.inputs.len(), 2);
        assert_eq!(brick.outputs.len(), 1);
    }

    #[test]
    fn test_logical_or_function() {
        // Test the basic logic
        assert_eq!(logical_or(true, true), (true,));
        assert_eq!(logical_or(true, false), (true,));
        assert_eq!(logical_or(false, true), (true,));
        assert_eq!(logical_or(false, false), (false,));
    }
}
