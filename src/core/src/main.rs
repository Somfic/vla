// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use tauri::Manager;
use window_shadows::set_shadow;

#[cfg(target_os = "macos")]
use window_vibrancy::{apply_vibrancy, NSVisualEffectMaterial};

#[cfg(target_os = "windows")]
use window_vibrancy::apply_acrylic;

pub mod notifications;
pub mod plugins;

fn main() {
    // If running in debug mode, we want to enable logging
    #[cfg(debug_assertions)]
    generate_type_schemas();

    tauri::Builder::default()
        .setup(|app| {
            let main_window = app.get_window("main").unwrap();

            #[cfg(any(windows, target_os = "macos"))]
            set_shadow(&main_window, true).unwrap();

            #[cfg(target_os = "macos")]
            apply_vibrancy(main_window, NSVisualEffectMaterial::HudWindow, None, None)
                .expect("Unsupported platform! Window vibrancy is only supported on macOS machines");

            #[cfg(target_os = "windows")]
            apply_acrylic(main_window, None)
                .expect("Unsupported platform! Window vibrancy effect is only supported on Windows machines");

            Ok(())
        })
        .invoke_handler(tauri::generate_handler![show_window, get_platform])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}

#[tauri::command]
async fn show_window(window: tauri::Window) {
    window.get_window("main").unwrap().show().unwrap();
}

#[tauri::command]
async fn get_platform() -> String {
    #[cfg(target_os = "macos")]
    return "macos".to_string();

    #[cfg(target_os = "windows")]
    return "windows".to_string();

    #[cfg(target_os = "linux")]
    return "linux".to_string();

    #[cfg(not(any(target_os = "macos", target_os = "windows", target_os = "linux")))]
    return "unknown".to_string();
}

#[cfg(debug_assertions)]
fn generate_type_schemas() {
    use crate::notifications::notification::Notification;
    use schemars::schema_for;

    // Notification
    let schema = schema_for!(Notification);
    let output = serde_json::to_string_pretty(&schema).unwrap();
    std::fs::write("../notification.schema.json", output).unwrap();
}
