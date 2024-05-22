use eyre::Context;

use super::models::Notification;
use crate::prelude::*;

#[derive(Debug)]
pub struct NotificationHandle {
    window_handle: &'static tauri::Window,
}

impl<'a> NotificationHandle {
    pub fn new(window: &'static tauri::Window) -> Self {
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
