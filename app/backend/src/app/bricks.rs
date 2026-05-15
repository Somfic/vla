use async_trait::async_trait;

use crate::api::bricks::BricksApi;
use crate::api::error::Error;
use crate::app::App;
use crate::bricks::types::Brick;

#[async_trait]
impl BricksApi for App {
    async fn get_bricks(&self) -> Result<Vec<Brick>, Error> {
        Ok(crate::bricks::all_bricks())
    }

    async fn stream_bricks(&self, on_brick: tauri::ipc::Channel<Brick>) -> Result<(), Error> {
        for brick in crate::bricks::all_bricks() {
            on_brick.send(brick).map_err(|e| Error::Internal {
                message: e.to_string(),
            })?;
        }
        Ok(())
    }
}
