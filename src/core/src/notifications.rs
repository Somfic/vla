use anyhow::{Context, Result};
use schemars::JsonSchema;
use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, Clone, JsonSchema)]
#[serde(rename_all = "lowercase")]
pub enum Level {
    Success,
    Error,
    Warning,
    Information,
}

#[derive(Serialize, Deserialize, Clone, JsonSchema)]
pub struct Notification {
    title: String,
    body: Option<String>,
    level: Level,
    call_to_action: Option<String>,
    call_to_action_secondary: Option<String>,
}

#[derive(Debug)]
pub struct NotificationHandle {
    window_handle: tauri::Window,
}

impl NotificationHandle {
    pub fn new(window: tauri::Window) -> Self {
        NotificationHandle {
            window_handle: window,
        }
    }

    pub fn notify(&self, notification: Notification) -> Result<()> {
        self.window_handle
            .emit("notification", notification)
            .context("Could not emit notification")
    }
}
