#[taurpc::procedures(export_to = "../src/lib/core_gen.ts")]
pub trait Api {
    async fn hello_world(name: String) -> String;
}

#[derive(Clone)]
pub struct ApiImpl;

#[taurpc::resolvers]
impl Api for ApiImpl {
    async fn hello_world(self, name: String) -> String {
        format!("Hello, {}! You've been greeted from Rust!", name)
    }
}
