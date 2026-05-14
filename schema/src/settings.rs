use jsonrpsee::core::RpcResult;
use schema_codegen::{vla_api, vla_type};

#[vla_type]
pub struct Settings {}

#[vla_api(namespace = "settings")]
trait SettingsApi {
    async fn get(&self) -> RpcResult<Settings>;
    async fn update(&self, settings: Settings) -> RpcResult<()>;
}
