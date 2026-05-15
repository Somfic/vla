use crate::api::settings::Settings;
use crate::shared::Shared;

mod bricks;
mod settings;

#[derive(Clone)]
pub struct App {
    pub settings: Shared<Settings>,
    /// Set once in the Tauri `setup` hook; `None` until then. Used by the
    /// codegen-generated `emit_*` methods to broadcast events.
    pub handle: Shared<Option<tauri::AppHandle>>,
}

impl Default for App {
    fn default() -> Self {
        Self {
            settings: Shared::new(Settings::default()),
            handle: Shared::new(None),
        }
    }
}
