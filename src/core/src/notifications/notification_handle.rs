use crate::notifications::notification::Notification;
use anyhow::{Context, Result};

#[derive(Debug)]
pub struct NotificationHandle {
    window_handle: tauri::Window,
}

impl NotificationHandle {
    pub fn new(window: &tauri::Window) -> Self {
        NotificationHandle {
            window_handle: window.clone(),
        }
    }

    pub fn notify(&self, notification: Notification) -> Result<()> {
        self.window_handle
            .emit("notification", notification)
            .context("Could not emit notification")
    }
}
