// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use anyhow::Result;
use std::env;
use tauri::Manager;
use window_shadows::set_shadow;

#[cfg(target_os = "macos")]
use window_vibrancy::{apply_vibrancy, NSVisualEffectMaterial};

#[cfg(target_os = "windows")]
use window_vibrancy::apply_acrylic;

pub mod canvas;
pub mod commands;
pub mod notifications;
pub mod plugins;

fn main() -> Result<()> {
    #[cfg(debug_assertions)]
    if env::args().any(|arg| arg == "--generate-type-schemas") {
        generate_type_schemas();
        return Ok(());
    }

    tauri::Builder::default()
        .setup(|app| {
            let window: &'static mut tauri::Window = Box::leak(Box::new(app.get_window("main").unwrap()));

            #[cfg(any(windows, target_os = "macos"))]
            set_shadow(app.get_window("main").unwrap(), true).unwrap();

            #[cfg(target_os = "macos")]
            apply_vibrancy(app.get_window("main").unwrap(), NSVisualEffectMaterial::HudWindow, None, None)
                .expect("Unsupported platform! Window vibrancy is only supported on macOS machines");

            #[cfg(target_os = "windows")]
            apply_acrylic(app.get_window("main").unwrap(), None)
                .expect("Unsupported platform! Window vibrancy effect is only supported on Windows machines");

            let app_handle = plugins::AppHandle::new(window);
            let mut plugin_manager = plugins::PluginManager::new(app_handle);

            plugin_manager.load_plugins()?;

            Ok(())
        })
        .invoke_handler(tauri::generate_handler![crate::commands::show_window, crate::commands::get_platform])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");

    Ok(())
}

#[cfg(debug_assertions)]
fn generate_type_schemas() {
    use schemars::schema_for;

    // Notification
    let schema = schema_for!(crate::notifications::models::Notification);
    let output = serde_json::to_string_pretty(&schema).unwrap();
    std::fs::write("../notification.schema.json", output).unwrap();
    println!("Notification schema generated");

    // Node
    let schema = schema_for!(crate::canvas::models::Node);
    let output = serde_json::to_string_pretty(&schema).unwrap();
    std::fs::write("../node.schema.json", output).unwrap();
    println!("Node schema generated");

    // Connection
    let schema = schema_for!(crate::canvas::models::Connection);
    let output = serde_json::to_string_pretty(&schema).unwrap();
    std::fs::write("../connection.schema.json", output).unwrap();
    println!("Connection schema generated");
}
