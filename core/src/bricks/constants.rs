use crate::bricks::macros::brick;
use crate::prelude::*;

pub fn all_bricks() -> Vec<Brick> {
    vec![number_constant_brick()]
}

brick! {
    #[id("number_constant")]
    #[label("Number constant")]
    #[description("Outputs a constant number")]
    #[category("Boolean Logic")]
    fn number_constant(
        #[argument] #[label("Value")] value: f32
    ) -> (
        #[label("")] f32
    )
    {
        (value,)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn number_constant_test() {
        let brick = number_constant_brick();
        assert_eq!(brick.id, "number_constant");
        assert_eq!(number_constant(42.0), (42.0,));
    }
}
