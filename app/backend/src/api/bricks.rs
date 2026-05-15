use super::error::Error;
use schema::vla_api;

// Re-exported so the generated Tauri command wrapper (which does
// `use crate::api::bricks::*;`) can resolve `Brick` in the return type.
// `Brick` is defined in `crate::bricks::types`; codegen still maps it to
// the `types` TS module by its definition site, not this re-export.
pub use crate::bricks::types::Brick;

#[vla_api(namespace = "bricks")]
pub trait BricksApi {
    /// Returns the full catalog of available bricks.
    async fn get_bricks(&self) -> Result<Vec<Brick>, Error>;

    // SEED: streams the catalog one brick at a time over a channel — proves the
    // per-call streaming codegen path end-to-end. Safe to keep.
    /// Streams the brick catalog one brick at a time.
    async fn stream_bricks(&self, on_brick: tauri::ipc::Channel<Brick>) -> Result<(), Error>;
}
