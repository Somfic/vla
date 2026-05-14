use std::sync::Arc;

use jsonrpsee::server::ServerBuilder;
use tokio::sync::Notify;

use crate::app::App;

pub async fn serve(shutdown: Arc<Notify>) -> Result<(), Box<dyn std::error::Error + Send + Sync>> {
    let state = App::default();
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
