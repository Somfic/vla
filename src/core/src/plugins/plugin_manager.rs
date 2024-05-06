use anyhow::{Context, Result};
use extism::{convert::Json, host_fn, Manifest, Plugin, PluginBuilder, UserData, Wasm, PTR};

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

    pub fn load_plugins_from_urls(&self, urls: Vec<impl Into<String>>) -> Vec<Plugin> {
        urls.into_iter()
            .map(|url| self.load_plugin_from_url(url))
            .filter(|plugin| plugin.is_ok())
            .map(|plugin| plugin.unwrap())
            .collect()
    }

    fn load_plugin_from_url(&self, url: impl Into<String>) -> Result<Plugin> {
        let wasm = Wasm::url(url);
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
pub fn test_load_plugin_from_urls_loads_plugin() {
    let url = "https://github.com/extism/plugins/releases/latest/download/count_vowels.wasm";
    let app_handle = AppHandle::new();
    let plugin_manager = PluginManager::new(app_handle);
    let plugins = plugin_manager.load_plugins_from_urls(vec![url]);

    assert_eq!(plugins.len(), 1);
}

#[test]
pub fn test_load_plugin_from_urls_skips_invalid_url() {
    let url = "avocado";
    let app_handle = AppHandle::new();
    let plugin_manager = PluginManager::new(app_handle);
    let plugins = plugin_manager.load_plugins_from_urls(vec![url]);

    assert_eq!(plugins.len(), 0);
}
