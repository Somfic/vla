// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use tauri::Manager;
#[cfg(target_os = "macos")]
use window_vibrancy::{apply_vibrancy, NSVisualEffectMaterial};

#[cfg(target_os = "windows")]
use window_vibrancy::apply_acrylic;

fn main() {
    tauri::Builder::default()
        .setup(|app| {
            let main_window = app.get_window("main").unwrap();

            #[cfg(target_os = "macos")]
            apply_vibrancy(main_window, NSVisualEffectMaterial::HudWindow, None, None)
                .expect("Unsupported platform! Window vibrancy is only supported on macOS machines");

            #[cfg(target_os = "windows")]
            apply_acrylic(main_window, None)
                .expect("Unsupported platform! Window vibrancy effect is only supported on Windows machines");

            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}