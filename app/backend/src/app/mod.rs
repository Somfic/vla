use crate::app::settings::Settings;
use crate::shared::Shared;

pub mod bricks;
pub mod clock;
pub mod error;
pub mod settings;

#[derive(Clone)]
pub struct App {
    pub settings: Shared<Settings>,

    pub handle: Shared<Option<tauri::AppHandle>>,
    pub events: crate::_generated::Events,
}

impl Default for App {
    fn default() -> Self {
        let handle = Shared::new(None);
        Self {
            settings: Shared::new(Settings::default()),
            events: crate::_generated::Events::new(handle.clone()),
            handle,
        }
    }
}
