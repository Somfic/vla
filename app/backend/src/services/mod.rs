//! Backend background services: long-running tasks that push events to the
//! frontend (as opposed to request/response API commands in `crate::api`).
//!
//! All services are started once from the Tauri `setup` hook, after the
//! `AppHandle` has been stored on `App` so `emit_*` works.

pub mod clock;

use crate::app::App;

/// Starts every background service. Called once on app startup.
pub fn start(app: &App) {
    clock::start(app.clone());
}
