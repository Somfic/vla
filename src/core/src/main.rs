// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]
// Do not allow unwrap
#![deny(clippy::unwrap_used)]

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
    generate_type_schemas().expect("Could not generate type schemas");

    tauri::Builder::default()
        .setup(|app| {
            let window = app.get_window("main").expect("Could not get main window");

            #[cfg(any(windows, target_os = "macos"))]
            let _ = set_shadow(&window, true);

            #[cfg(target_os = "macos")]
            let _ = apply_vibrancy(&window, NSVisualEffectMaterial::HudWindow, None, None);

            #[cfg(target_os = "windows")]
            let _ = apply_acrylic(&window, None);

            let app_handle = plugins::AppHandle::new(app);
            let mut plugin_manager = plugins::PluginManager::new(app_handle);

            plugin_manager.load_plugins()?;

            Ok(())
        })
        .invoke_handler(tauri::generate_handler![
            show_window,
            get_platform,
            on_nodes_changed,
            on_connections_changed
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}

#[tauri::command]
async fn show_window(window: tauri::Window) {
    let window = window
        .get_window("main")
        .expect("Could not get main window");

    window.show().expect("Could not show window");
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
fn generate_type_schemas() -> anyhow::Result<()> {
    use crate::notifications::Notification;
    use schemars::schema_for;

    // Notification
    let schema = schema_for!(Notification);
    let output = serde_json::to_string_pretty(&schema)?;
    std::fs::write("../notification.schema.json", output)?;

    Ok(())
}
