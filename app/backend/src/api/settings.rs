use super::error::Error;
use schema::{vla_api, vla_type};

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
