use super::models::Notification;
use anyhow::Context;
use anyhow::Result;

#[derive(Debug)]
pub struct NotificationHandle {
    window_handle: &'static tauri::Window,
}

impl NotificationHandle {
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

mod host {
    use crate::{notification::models::Notification, plugins::AppHandle};
    use anyhow::Context;
    use extism::{convert::Json, host_fn};

    host_fn!(notifications_notify(user_data: AppHandle; notification: Json<Notification>) -> Result<()> {
        let app_handle = user_data.get()?;
        let app_handle = app_handle.lock().unwrap();
        app_handle.notifications.notify(notification.0)
            .context("Could not notify")
    });
}
