// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]
use std::sync::{Arc, Mutex};
use vla_lib::api::AppState;
use vla_lib::prelude::*;

fn main() {
    run()
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
fn run() {
    tauri::Builder::default()
        .manage(AppState {
            engine: Arc::new(Mutex::new(None)),
        })
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(taurpc::create_ipc_handler(CoreApiImpl.into_handler()))
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
