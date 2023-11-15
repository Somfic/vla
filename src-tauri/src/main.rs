// Prevents additional console window on Windows in release, DO NOT REMOVE!!
//#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use tauri::api::process::{Command, CommandEvent};

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
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
