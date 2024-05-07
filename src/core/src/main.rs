// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use tauri::Manager;

#[cfg(target_os = "macos")]
use window_vibrancy::{apply_vibrancy, NSVisualEffectMaterial};

#[cfg(target_os = "windows")]
use window_vibrancy::apply_acrylic;

pub mod plugins;

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
        .invoke_handler(tauri::generate_handler![get_all_plugins])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}

#[tauri::command]
fn get_all_plugins() -> Vec<String> {
    let plugin_code = r#"
    class TestExtension {
    metadata() {
        return {
            name: "test_extension",
            description: "A test extension",
            version: "1.0.0",
        };
    }
    async on_start(handle) {
        handle.log("test_extension");
    }
    async on_stop(_handle) { }
}
class IrrelevantExport {
}

export { IrrelevantExport, TestExtension };

    "#;

    vec![plugin_code.to_string()]
}
