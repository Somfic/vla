//! Brick catalog. The ported `brick!` macro generates a typed fn + a runtime
//! adapter + a `Brick` descriptor per brick; these allows cover that
//! expansion's benign patterns so the strict workspace lint bar stays green.
#![allow(unused_mut, unused_variables, clippy::vec_init_then_push)]

pub mod arithmetics;
pub mod boolean_logic;
pub mod constants;
pub mod control_flow;
pub mod debug;
pub mod events;
pub mod macros;
pub mod types;

use crate::prelude::*;

pub fn all_bricks() -> Vec<Brick> {
    let mut bricks = vec![];
    bricks.extend(arithmetics::all_bricks());
    bricks.extend(boolean_logic::all_bricks());
    bricks.extend(constants::all_bricks());
    bricks.extend(control_flow::all_bricks());
    bricks.extend(debug::all_bricks());
    bricks.extend(events::all_bricks());
    bricks
}
