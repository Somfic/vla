// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use tauri::{
    api::process::{Command, CommandEvent},
    Manager, Window,
};
use window_vibrancy::{apply_acrylic, apply_mica, apply_vibrancy, NSVisualEffectMaterial};

#[tauri::command]
async fn open_splashscreen(window: Window) {
    window
        .get_window("main")
        .expect("no window labeled 'main' found")
        .hide()
        .unwrap();
    window
        .get_window("splashscreen")
        .expect("no window labeled 'splashscreen' found")
        .show()
        .unwrap();
}

#[tauri::command]
async fn close_splashscreen(window: Window) {
    window
        .get_window("splashscreen")
        .expect("no window labeled 'splashscreen' found")
        .close()
        .unwrap();
    window
        .get_window("main")
        .expect("no window labeled 'main' found")
        .show()
        .unwrap();
}

fn main() {
    let (mut rx, _) = Command::new_sidecar("csharp")
        .expect("failed to create `csharp` binary command")
        .spawn()
        .expect("Failed to spawn sidecar for `csharp` binary");

    tauri::async_runtime::spawn(async move {
        while let Some(event) = rx.recv().await {
            match event {
                CommandEvent::Stdout(line) => {
                    print!("CSHARP: {}", line);
                }
                CommandEvent::Stderr(line) => {
                    print!("CSHARP: {}", line);
                }
                _ => {}
            }
        }
    });

    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![open_splashscreen])
        .invoke_handler(tauri::generate_handler![close_splashscreen])
        .setup(|app| {
            let window_labels = vec!["splashscreen", "main"];
            let windows = window_labels
                .iter()
                .map(|label| app.get_window(label).unwrap())
                .collect::<Vec<_>>();

            for window in windows {
                #[cfg(target_os = "macos")]
                apply_vibrancy(&window, NSVisualEffectMaterial::HudWindow, None, None)
                    .expect("Unsupported platform! 'apply_vibrancy' is only supported on macOS");

                #[cfg(target_os = "windows")]
                {
                    println!("Applying mica");
                    //apply_mica(&window, None)
                    apply_acrylic(&window, None)
                        .expect("Unsupported platform! 'apply_mica' is only supported on Windows");
                }
            }

            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
