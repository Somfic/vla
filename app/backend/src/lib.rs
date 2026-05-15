mod _generated;
mod api;
mod app;
mod shared;

use app::App;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .manage(App::default())
        .invoke_handler(_generated::invoke_handler())
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
