// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use tauri::{
    api::process::{Command, CommandEvent},
    Manager,
};
use window_vibrancy::{apply_acrylic, apply_mica, apply_vibrancy, NSVisualEffectMaterial};

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
        .setup(|app| {
            let window = app.get_window("main").unwrap();

            #[cfg(target_os = "macos")]
            apply_vibrancy(&window, NSVisualEffectMaterial::HudWindow, None, None)
                .expect("Unsupported platform! 'apply_vibrancy' is only supported on macOS");

            #[cfg(target_os = "windows")]
            {
                println!("Applying mica");
                //apply_mica(&window, None)
                apply_acrylic(&window, Some((30, 30, 30, 255)))
                    .expect("Unsupported platform! 'apply_mica' is only supported on Windows");
            }

            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
