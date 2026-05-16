use super::error::Error;
use crate::app::App;
pub use crate::bricks::types::Brick;
use schema::vla_api;

#[vla_api(namespace = "bricks")]
pub trait BricksApi {
    /// Returns the full catalog of available bricks.
    async fn get_bricks(&self) -> Result<Vec<Brick>, Error>;
}

#[vla_api]
impl BricksApi for App {
    async fn get_bricks(&self) -> Result<Vec<Brick>, Error> {
        Ok(crate::bricks::all_bricks())
    }
}
