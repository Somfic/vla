use crate::{
    canvas,
    plugins::{AppHandle, Metadata, Plugin},
};
use anyhow::Result;
use extism::UserData;

pub struct TestPlugin {
    app_handle: UserData<AppHandle>,
}

impl TestPlugin {
    pub fn new(app_handle: UserData<AppHandle>) -> Self {
        TestPlugin { app_handle }
    }
}

impl Plugin for TestPlugin {
    fn metadata(&mut self) -> Result<Metadata> {
        Ok(Metadata {
            namespace: "test".to_string(),
            name: "Test Plugin".to_string(),
        })
    }

    fn canvas_on_nodes_changed(&mut self, nodes: &[canvas::models::Node]) -> Result<()> {
        println!("{:?}", nodes);
        Ok(())
    }
}
