use schema::{vla_events, vla_type};

/// Payload for the per-second clock tick.
#[vla_type]
pub struct ClockTick {
    /// Milliseconds since the Unix epoch (use `new Date(unixMs)` on the frontend).
    pub unix_ms: u64,
    /// Wall-clock time of day in UTC, `HH:MM:SS`.
    pub utc: String,
}

/// Clock events pushed from the backend.
///
/// Bodyless manifest: never implemented, erased at compile time. The codegen
/// reads it to generate `App::emit_tick` (Rust) and `api.clockEvents.onTick`
/// (TS). The tick itself is driven by a background thread spawned in
/// `lib.rs`'s Tauri `setup` hook.
#[vla_events(namespace = "clock")]
pub trait ClockEvents {
    /// Emitted once per second with the current time.
    fn tick(tick: ClockTick);
}
