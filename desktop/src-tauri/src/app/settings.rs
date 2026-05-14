use jsonrpsee::core::{async_trait, RpcResult};
use schema::settings::{Settings, SettingsApiServer};

use crate::app::App;

#[async_trait]
impl SettingsApiServer for App {
    async fn get(&self) -> RpcResult<Settings> {
        Ok(self.settings.get())
    }

    async fn update(&self, settings: Settings) -> RpcResult<()> {
        self.settings.set(settings);
        Ok(())
    }
}
