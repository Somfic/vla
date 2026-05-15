//! Clock service: emits a `clock_tick` event once per second with the
//! current time. Runs on a detached thread for the lifetime of the process.

use std::thread;
use std::time::{Duration, SystemTime, UNIX_EPOCH};

use crate::api::clock::ClockTick;
use crate::app::App;

/// Spawns the per-second clock ticker. `app` is a cheap clone (shared `Arc`
/// state); `emit_tick` is the codegen-generated emitter on `App`.
pub fn start(app: App) {
    thread::spawn(move || loop {
        thread::sleep(Duration::from_secs(1));
        let now = SystemTime::now()
            .duration_since(UNIX_EPOCH)
            .unwrap_or_default();
        let secs = now.as_secs();
        let _ = app.emit_tick(&ClockTick {
            unix_ms: now.as_millis() as u64,
            utc: format!(
                "{:02}:{:02}:{:02}",
                (secs / 3600) % 24,
                (secs / 60) % 60,
                secs % 60
            ),
        });
    });
}
