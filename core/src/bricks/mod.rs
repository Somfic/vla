pub mod arithmetics;
pub mod boolean_logic;
pub mod macros;
#[cfg(test)]
mod tests;
pub mod types;

use crate::prelude::*;

pub fn all_bricks() -> Vec<Brick> {
    let mut bricks = vec![];
    bricks.extend(arithmetics::all_bricks());
    bricks.extend(boolean_logic::all_bricks());
    bricks
}
