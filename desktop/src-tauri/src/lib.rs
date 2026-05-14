mod rpc;

use std::sync::{Arc, Mutex};
use tokio::sync::Notify;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    let shutdown = Arc::new(Notify::new());
    let drained = Arc::new(Notify::new());

    let app = tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .setup({
            let shutdown = shutdown.clone();
            let drained = drained.clone();
            move |_app| {
                tauri::async_runtime::spawn(async move {
                    if let Err(err) = rpc::serve(shutdown).await {
                        eprintln!("rpc server error: {err}");
                    }
                    drained.notify_one();
                });
                Ok(())
            }
        })
        .build(tauri::generate_context!())
        .expect("error while building tauri application");

    let triggered = Arc::new(Mutex::new(false));
    app.run(move |app_handle, event| {
        if let tauri::RunEvent::ExitRequested { api, .. } = event {
            let mut already = triggered.lock().unwrap();
            if *already {
                return;
            }
            *already = true;
            shutdown.notify_one();
            api.prevent_exit();
            let app_handle = app_handle.clone();
            let drained = drained.clone();
            tauri::async_runtime::spawn(async move {
                drained.notified().await;
                app_handle.exit(0);
            });
        }
    });
}
