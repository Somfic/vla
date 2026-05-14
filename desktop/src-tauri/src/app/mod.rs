use crate::shared::Shared;
use schema::settings::Settings;

mod settings;

#[derive(Clone)]
pub struct App {
    pub settings: Shared<Settings>,
}

impl Default for App {
    fn default() -> Self {
        Self {
            settings: Shared::new(Settings {}),
        }
    }
}
