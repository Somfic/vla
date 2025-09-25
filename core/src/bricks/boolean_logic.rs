use crate::bricks::macros::brick;

brick! {
    #[id("logical_or")]
    #[label("OR")]
    #[description("Performs logical OR operation on two boolean values")]
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
    fn logical_not(
        #[input] #[label("A")] a: bool
    ) -> (
        #[label("!A")] bool
    )
    {
        (!a,)
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
}
