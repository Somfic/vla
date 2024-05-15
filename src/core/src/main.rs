// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use std::{rc::Rc, sync::Arc};

use tauri::Manager;
use window_shadows::set_shadow;

#[cfg(target_os = "macos")]
use window_vibrancy::{apply_vibrancy, NSVisualEffectMaterial};

#[cfg(target_os = "windows")]
use window_vibrancy::apply_acrylic;

pub mod notifications;
pub mod plugins;

fn main() {
    #[cfg(debug_assertions)]
    generate_type_schemas();

    tauri::Builder::default()
        .setup(|app| {
            #[cfg(any(windows, target_os = "macos"))]
            set_shadow(app.get_window("main").unwrap(), true).unwrap();

            #[cfg(target_os = "macos")]
            apply_vibrancy(app.get_window("main").unwrap(), NSVisualEffectMaterial::HudWindow, None, None)
                .expect("Unsupported platform! Window vibrancy is only supported on macOS machines");

            #[cfg(target_os = "windows")]
            apply_acrylic(app.get_window("main").unwrap(), None)
                .expect("Unsupported platform! Window vibrancy effect is only supported on Windows machines");

            let app_handle = plugins::AppHandle::new(app);
            let mut plugin_manager = plugins::PluginManager::new(app_handle);

            plugin_manager.load_plugins()?;

            Ok(())
        })
        .invoke_handler(tauri::generate_handler![show_window, get_platform, on_nodes_changed, on_connections_changed])
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

#[tauri::command]
async fn on_connections_changed() {
    println!("Connections changed!");
}

#[tauri::command]
async fn on_nodes_changed() {
    println!("Nodes changed!");
}

#[cfg(debug_assertions)]
fn generate_type_schemas() {
    use crate::notifications::Notification;
    use schemars::schema_for;

    // Notification
    let schema = schema_for!(Notification);
    let output = serde_json::to_string_pretty(&schema).unwrap();
    std::fs::write("../notification.schema.json", output).unwrap();
}
