use std::sync::Arc;

use crate::notifications::{Notification, NotificationHandle};
use anyhow::{Context, Result};
use extism::{convert::Json, host_fn, Manifest, Plugin, PluginBuilder, UserData, Wasm, PTR};
use std::sync::Mutex;
use tauri::Manager;

#[derive(Debug)]
pub struct AppHandle {
    notifications: NotificationHandle,
    logs: Vec<String>,
}

impl AppHandle {
    pub fn new(app: &tauri::App) -> Self {
        AppHandle {
            logs: vec![],
            notifications: NotificationHandle::new(app.get_window("main").unwrap()),
        }
    }

    pub fn log(&mut self, text: impl Into<String>) {
        self.logs.push(text.into());
    }
}

pub struct PluginManager {
    plugins: Vec<Plugin>,
    plugin_data: UserData<AppHandle>,
}

impl PluginManager {
    pub fn new(app_handle: AppHandle) -> Self {
        PluginManager {
            plugins: vec![],
            plugin_data: UserData::new(app_handle),
        }
    }

    pub fn load_plugins(&mut self) -> Result<()> {
        let plugin_directory = std::path::Path::new("plugins");

        // Create the directory if it doesn't exist
        if !plugin_directory.exists() {
            std::fs::create_dir(plugin_directory).context("Could not create plugin directory")?;
        }

        let applicable_files = std::fs::read_dir(plugin_directory)
            .context("Could not read plugin directory")?
            .filter_map(|entry| entry.ok())
            .map(|entry| entry.path())
            .filter(|path| path.extension().map_or(false, |ext| ext == "vla"))
            .collect::<Vec<_>>();

        self.plugins = self
            .load_plugins_from_files(applicable_files)
            .into_iter()
            .filter(|result| result.is_ok())
            .map(|result| result.unwrap())
            .collect();

        Ok(())
    }

    fn load_plugins_from_files(
        &self,
        paths: Vec<impl AsRef<std::path::Path>>,
    ) -> Vec<Result<Plugin>> {
        paths
            .into_iter()
            .map(|path| self.load_plugin_from_file(path))
            .collect()
    }

    fn load_plugin_from_file(&self, path: impl AsRef<std::path::Path>) -> Result<Plugin> {
        let wasm = Wasm::file(path);
        self.load_plugin_from_wasm(wasm)
    }

    fn load_plugin_from_wasm(&self, wasm: Wasm) -> Result<Plugin> {
        let manifest = Manifest::new([wasm]);
        let result = PluginBuilder::new(manifest)
            .with_wasi(true)
            .with_function("notify", [PTR], [PTR], self.plugin_data.clone(), notify)
            .build()
            .context("Could not build plugin");

        if result.is_err() {
            println!("Could not load plugin: {:?}", result.as_ref().unwrap_err());
        }

        result
    }
}

host_fn!(notify(user_data: AppHandle; notification: Json<Notification>) -> Result<()> {
    let app_handle = user_data.get()?;
    let app_handle = app_handle.lock().unwrap();

    app_handle.notifications.notify(notification.0)?;

    Ok(())
});

// #[test]
// pub fn test_load_plugin_from_path_loads_simple_plugin() -> Result<()> {
//     let path = "src/plugins/count_vowels.wasm";
//     let app_handle = AppHandle::new();
//     let plugin_manager = PluginManager::new(app_handle);

//     plugin_manager.load_plugin_from_file(path)?;

//     Ok(())
// }

// #[test]
// pub fn test_load_plugin_from_path_loads_complex_plugin() -> Result<()> {
//     let path = "src/plugins/test.wasm";
//     let app_handle = AppHandle::new();
//     let plugin_manager = PluginManager::new(app_handle);

//     let mut plugin = plugin_manager.load_plugin_from_file(path)?;

//     plugin.call::<(), ()>("on_start", ())?;

//     assert_eq!(
//         plugin_manager.plugin_data.get()?.lock().unwrap().logs,
//         &["Hello from C#"]
//     );

//     Ok(())
// }

// #[test]
// pub fn test_load_plugin_from_path_errors_on_invalid_path() {
//     let path = "avocado";
//     let app_handle = AppHandle::new();
//     let plugin_manager = PluginManager::new(app_handle);

//     let plugins = plugin_manager.load_plugin_from_file(path);

//     assert_eq!(plugins.is_err(), true);
// }
