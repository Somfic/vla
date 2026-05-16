use super::error::Error;
use crate::app::App;
use schema::{vla_api, vla_events, vla_type};

#[vla_type]
pub struct Settings {
    pub theme: Theme,
}

impl Default for Settings {
    fn default() -> Self {
        Self {
            theme: Theme::System,
        }
    }
}

#[vla_type]
pub enum Theme {
    Light,
    Dark,
    System,
}

#[vla_api(namespace = "settings")]
pub trait SettingsApi {
    async fn get(&self) -> Result<Settings, Error>;
    async fn update(&self, settings: Settings) -> Result<(), Error>;
}

#[vla_events(namespace = "settings")]
pub trait SettingsEvents {
    /// Emitted after settings are persisted.
    fn changed(settings: Settings);
}

#[vla_api]
impl SettingsApi for App {
    async fn get(&self) -> Result<Settings, Error> {
        Ok(self.settings.get())
    }

    async fn update(&self, settings: Settings) -> Result<(), Error> {
        self.settings.set(settings.clone());
        self.events
            .settings
            .emit_changed(&settings)
            .map_err(|e| Error::Internal {
                message: e.to_string(),
            })?;
        Ok(())
    }
}
