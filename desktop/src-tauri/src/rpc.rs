use std::sync::{Arc, Mutex};

use jsonrpsee::core::{async_trait, RpcResult};
use jsonrpsee::server::ServerBuilder;
use schema::settings::{Settings, SettingsApiServer};
use tokio::sync::Notify;

#[derive(Clone)]
pub struct State {
    pub settings: Arc<Mutex<Settings>>,
}

impl Default for State {
    fn default() -> Self {
        Self {
            settings: Arc::new(Mutex::new(Settings {
                launch_at_login: false,
                server_port: 9876,
                theme: "system".into(),
            })),
        }
    }
}

#[async_trait]
impl SettingsApiServer for State {
    async fn get(&self) -> RpcResult<Settings> {
        Ok(self.settings.lock().unwrap().clone())
    }

    async fn update(&self, settings: Settings) -> RpcResult<()> {
        if settings.server_port == 0 {
            return Err(schema::error::Error::InvalidValue {
                field: "server_port".into(),
            }
            .into());
        }
        *self.settings.lock().unwrap() = settings;
        Ok(())
    }
}

pub async fn serve(shutdown: Arc<Notify>) -> Result<(), Box<dyn std::error::Error + Send + Sync>> {
    let state = State::default();
    let server = ServerBuilder::default().build("127.0.0.1:9876").await?;
    let addr = server.local_addr()?;
    let handle = server.start(schema::into_module(state)?);
    println!("rpc listening on ws://{addr}");

    let waiter = handle.clone();
    tokio::select! {
        _ = shutdown.notified() => {
            println!("rpc shutdown signal received");
        }
        _ = waiter.stopped() => {
            println!("rpc server stopped unexpectedly");
        }
    }

    handle.stop().ok();
    handle.stopped().await;
    println!("rpc stopped cleanly");
    Ok(())
}
