use crate::bricks::macros::brick;
use crate::prelude::*;

pub fn all_bricks() -> Vec<Brick> {
    vec![
        logical_or_brick(),
        logical_and_brick(),
        logical_not_brick(),
        logical_xor_brick(),
    ]
}

brick! {
    #[id("logical_or")]
    #[label("OR")]
    #[description("Performs logical OR operation on two boolean values")]
    #[category("Boolean Logic")]
    fn logical_or(
        #[input] #[label("A")] a: bool,
        #[input] #[label("B")] b: bool
    ) -> (
        #[label("A | B")] bool
    )
    {
        (a || b,)
    }
}

brick! {
    #[id("logical_and")]
    #[label("AND")]
    #[description("Performs logical AND operation on two boolean values")]
    #[category("Boolean Logic")]
    fn logical_and(
        #[input] #[label("A")] a: bool,
        #[input] #[label("B")] b: bool
    ) -> (
        #[label("A & B")] bool
    )
    {
        (a && b,)
    }
}

brick! {
    #[id("logical_not")]
    #[label("NOT")]
    #[description("Performs logical NOT operation on a boolean value")]
    #[category("Boolean Logic")]
    fn logical_not(
        #[input] #[label("A")] a: bool
    ) -> (
        #[label("!A")] bool
    )
    {
        (!a,)
    }
}

brick! {
    #[id("logical_xor")]
    #[label("XOR")]
    #[description("Performs logical XOR operation on two boolean values")]
    #[category("Boolean Logic")]
    fn logical_xor(
        #[input] #[label("A")] a: bool,
        #[input] #[label("B")] b: bool
    ) -> (
        #[label("A ^ B")] bool
    )
    {
        (a ^ b,)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn logical_or_test() {
        let brick = logical_or_brick();
        assert_eq!(brick.id, "logical_or");
        assert_eq!(logical_or(true, true), (true,));
        assert_eq!(logical_or(true, false), (true,));
        assert_eq!(logical_or(false, true), (true,));
        assert_eq!(logical_or(false, false), (false,));
    }

    #[test]
    fn logical_and_test() {
        let brick = logical_and_brick();
        assert_eq!(brick.id, "logical_and");
        assert_eq!(logical_and(true, true), (true,));
        assert_eq!(logical_and(true, false), (false,));
        assert_eq!(logical_and(false, true), (false,));
        assert_eq!(logical_and(false, false), (false,));
    }

    #[test]
    fn logical_not_test() {
        let brick = logical_not_brick();
        assert_eq!(brick.id, "logical_not");
        assert_eq!(logical_not(true), (false,));
        assert_eq!(logical_not(false), (true,));
    }

    #[test]
    fn logical_xor_test() {
        let brick = logical_xor_brick();
        assert_eq!(brick.id, "logical_xor");
        assert_eq!(logical_xor(true, true), (false,));
        assert_eq!(logical_xor(true, false), (true,));
        assert_eq!(logical_xor(false, true), (true,));
        assert_eq!(logical_xor(false, false), (false,));
    }
}
