use tauri::{AppHandle, Runtime};

use crate::prelude::*;
use crate::{bricks, canvas, engine::Engine};

/// Result of executing the entire graph
#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct ExecutionResult {
    pub total_nodes: u32,
    pub success: bool,
    pub error: Option<String>,
}

#[taurpc::procedures(export_to = "../frontend/src/lib/core.ts", event_trigger = ApiEventTrigger)]
pub trait CoreApi {
    #[taurpc(event)]
    async fn graph_updated(graph: Graph);

    #[taurpc(event)]
    async fn node_execution_updated(update: crate::engine::ExecutionStateUpdate);

    async fn save_graph<R: Runtime>(
        app_handle: AppHandle<R>,
        graph: Graph,
        filename: String,
    ) -> Result<String, String>;
    async fn load_graph<R: Runtime>(
        app_handle: AppHandle<R>,
        filename: String,
    ) -> Result<Graph, String>;
    async fn get_brick<R: Runtime>(app_handle: AppHandle<R>, brick_id: String) -> Option<Brick>;
    async fn get_bricks<R: Runtime>(app_handle: AppHandle<R>) -> Vec<Brick>;
    async fn insert_node<R: Runtime>(
        app_handle: AppHandle<R>,
        graph_path: String,
        brick_id: String,
        position: Point,
    ) -> Result<Graph, String>;

    async fn execute_graph<R: Runtime>(
        app_handle: AppHandle<R>,
        graph: Graph,
        mode: crate::engine::ExecutionMode,
    ) -> Result<ExecutionResult, String>;
}

#[derive(Clone)]
pub struct CoreApiImpl;

#[taurpc::resolvers]
impl CoreApi for CoreApiImpl {
    async fn save_graph<R: Runtime>(
        self,
        app_handle: AppHandle<R>,
        graph: Graph,
        graph_path: String,
    ) -> Result<String, String> {
        canvas::save_graph(app_handle, &graph, &graph_path, false).await
    }

    async fn load_graph<R: Runtime>(
        self,
        app_handle: AppHandle<R>,
        graph_path: String,
    ) -> Result<Graph, String> {
        canvas::load_graph(app_handle, &graph_path).await
    }

    async fn get_brick<R: Runtime>(
        self,
        app_handle: AppHandle<R>,
        brick_id: String,
    ) -> Option<bricks::types::Brick> {
        canvas::get_brick(&brick_id)
    }

    async fn get_bricks<R: Runtime>(self, app_handle: AppHandle<R>) -> Vec<bricks::types::Brick> {
        bricks::all_bricks()
    }

    async fn insert_node<R: Runtime>(
        self,
        app_handle: AppHandle<R>,
        graph_path: String,
        brick_id: String,
        position: Point,
    ) -> Result<Graph, String> {
        canvas::insert_node(app_handle, &graph_path, &brick_id, position).await
    }

    async fn execute_graph<R: Runtime>(
        self,
        app_handle: AppHandle<R>,
        mut graph: Graph,
        mode: crate::engine::ExecutionMode,
    ) -> Result<ExecutionResult, String> {
        // Re-attach brick definitions (execution functions are skipped during serialization)
        for node in &mut graph.nodes {
            node.data.brick = canvas::get_brick(&node.data.brick_id);
        }

        let total_nodes = graph.nodes.len() as u32;

        // Create engine with the graph and app handle
        let mut engine = Engine::with_app_handle(graph, app_handle);
        engine.set_execution_mode(mode);
        engine.start();

        let mut execution_error = None;

        // Execute all steps (events are automatically broadcast via engine)
        for result in engine {
            if let Err(e) = result {
                execution_error = Some(e);
                break;
            }
        }

        Ok(ExecutionResult {
            total_nodes,
            success: execution_error.is_none(),
            error: execution_error,
        })
    }
}
