use anyhow::{Context, Result};
use extism::{convert::Json, host_fn, Manifest, Plugin, PluginBuilder, UserData, Wasm, PTR};

#[derive(Debug)]
struct AppHandle {
    logs: Vec<String>,
}

impl AppHandle {
    pub fn new() -> Self {
        AppHandle { logs: vec![] }
    }

    pub fn log(&mut self, text: impl Into<String>) {
        self.logs.push(text.into());
    }
}

struct PluginManager {
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

    pub fn load_plugins_from_files(
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
        PluginBuilder::new(manifest)
            .with_wasi(true)
            .with_function("log", [PTR], [PTR], self.plugin_data.clone(), log)
            .build()
            .context("Could not build plugin")
    }
}

host_fn!(log(user_data: AppHandle; text: String) -> Result<()> {
    println!("{}", text);

    let app_handle = user_data.get()?;
    let mut app_handle = app_handle.lock().unwrap();

    app_handle.log(text);

    Ok(())
});

#[test]
pub fn test_load_plugin_from_path_loads_plugin() -> Result<()> {
    let path = "src/plugins/test.wasm";
    let app_handle = AppHandle::new();
    let plugin_manager = PluginManager::new(app_handle);
    let mut plugin = plugin_manager.load_plugin_from_file(path)?;

    plugin.call::<(), ()>("on_start", ())?;

    assert_eq!(
        plugin_manager.plugin_data.get()?.lock().unwrap().logs,
        &["Hello from C#"]
    );

    Ok(())
}

#[test]
pub fn test_load_plugin_from_path_errors_on_invalid_path() {
    let path = "avocado";
    let app_handle = AppHandle::new();
    let plugin_manager = PluginManager::new(app_handle);
    let plugins = plugin_manager.load_plugin_from_file(path);

    assert_eq!(plugins.is_err(), true);
}
