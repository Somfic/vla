use crate::{
    canvas::handle::CanvasHandle,
    notification::{handle::NotificationHandle, models::Notification},
    Error,
};
use extism::{convert::Json, host_fn, Manifest, Plugin, PluginBuilder, UserData, Wasm, PTR};
use eyre::{Context, ContextCompat};
use std::sync::{Arc, Mutex};

#[derive(Debug)]
pub struct AppHandle {
    pub notifications: NotificationHandle,
    pub canvas: Arc<Mutex<CanvasHandle>>,
    window_handle: &'static tauri::Window,
}

impl AppHandle {
    pub fn new(window: &'static tauri::Window) -> Self {
        AppHandle {
            window_handle: window,
            notifications: NotificationHandle::new(window),
            canvas: CanvasHandle::new(window),
        }
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

    pub fn load_plugins(&mut self) -> crate::prelude::Result<&Vec<Plugin>> {
        let plugin_directory = dirs::data_local_dir()
            .context("Could not get local data directory")?
            .join("vla")
            .join("plugins");

        // Create the directory if it doesn't exist
        if !plugin_directory.exists() {
            std::fs::create_dir(&plugin_directory).context("Could not create plugin directory")?;
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
            .filter_map(|result| result.ok())
            .collect();

        Ok(&self.plugins)
    }

    fn load_plugins_from_files(
        &self,
        paths: Vec<impl AsRef<std::path::Path>>,
    ) -> Vec<crate::prelude::Result<Plugin>> {
        paths
            .into_iter()
            .map(|path| self.load_plugin_from_file(path))
            .collect()
    }

    fn load_plugin_from_file(
        &self,
        path: impl AsRef<std::path::Path>,
    ) -> crate::prelude::Result<Plugin> {
        let wasm = Wasm::file(path);
        self.load_plugin_from_wasm(wasm)
    }

    fn load_plugin_from_wasm(&self, wasm: Wasm) -> crate::prelude::Result<Plugin> {
        let manifest = Manifest::new([wasm]);
        PluginBuilder::new(manifest)
            .with_wasi(true)
            .with_function("notify", [PTR], [PTR], self.plugin_data.clone(), notify)
            .build()
            .map_err(|_| Error::Generic("Could not build plugin".to_owned()))
            .context("Could not build plugin")
    }
}

host_fn!(notify(user_data: AppHandle; notification: Json<Notification>) -> crate::prelude::Result<()> {
    let app_handle = user_data.get()?;
    let app_handle = app_handle.lock().unwrap();
    app_handle.notifications.notify(notification.0)
     .map_err(|_| Error::Generic("Could not notify".to_owned()))?;

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
