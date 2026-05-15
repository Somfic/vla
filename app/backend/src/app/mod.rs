use crate::api::settings::Settings;
use crate::shared::Shared;

mod settings;

#[derive(Clone)]
pub struct App {
    pub settings: Shared<Settings>,
}

impl Default for App {
    fn default() -> Self {
        Self {
            settings: Shared::new(Settings::default()),
        }
    }
}
