use async_trait::async_trait;

use crate::api::error::Error;
use crate::api::settings::{Settings, SettingsApi};
use crate::app::App;

#[async_trait]
impl SettingsApi for App {
    async fn get(&self) -> Result<Settings, Error> {
        Ok(self.settings.get())
    }

    async fn update(&self, settings: Settings) -> Result<(), Error> {
        self.settings.set(settings);
        Ok(())
    }
}
