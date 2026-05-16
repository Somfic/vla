mod _generated;
mod app;
mod bricks;
mod engine;
mod prelude;
mod shared;

#[macro_export]
macro_rules! trigger {
    ($output_id:expr) => {
        $crate::engine::trigger::add_trigger($output_id);
    };
}

#[macro_export]
macro_rules! set_current_node_id {
    ($node_id:expr) => {
        $crate::engine::trigger::set_current_node_id($node_id);
    };
}

use app::App;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .manage(App::default())
        .setup(|app| {
            use tauri::Manager;
            let handle = app.handle().clone();
            let app_state = app.state::<App>().inner().clone();
            app_state.handle.set(Some(handle));
            _generated::start_services(&app_state);
            Ok(())
        })
        .invoke_handler(_generated::invoke_handler())
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
