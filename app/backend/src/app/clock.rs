use schema::{vla_api, vla_events, vla_service, vla_type};
use std::time::{Duration, SystemTime, UNIX_EPOCH};

use crate::app::App;

#[vla_type]
pub struct ClockTick {
    pub unix_ms: u64,
    pub utc: String,
}

#[vla_service]
pub async fn start(app: &App) {
    loop {
        tokio::time::sleep(Duration::from_secs(1)).await;
        let _ = app.events.clock.emit_tick(&current());
    }
}

#[vla_api(namespace = "clock")]
pub trait ClockApi {
    async fn get_time(&self) -> Result<ClockTick, ()>;
}

#[vla_api]
impl ClockApi for App {
    async fn get_time(&self) -> Result<ClockTick, ()> {
        Ok(current())
    }
}

#[vla_events(namespace = "clock")]
pub trait ClockEvents {
    fn tick(tick: ClockTick);
}

fn current() -> ClockTick {
    let now = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .unwrap_or_default();
    let secs = now.as_secs();
    ClockTick {
        unix_ms: now.as_millis() as u64,
        utc: format!(
            "{:02}:{:02}:{:02}",
            (secs / 3600) % 24,
            (secs / 60) % 60,
            secs % 60
        ),
    }
}
