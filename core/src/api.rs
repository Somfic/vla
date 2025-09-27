use std::sync::{Arc, Mutex};

use tauri::{AppHandle, Runtime};

use crate::prelude::*;
use crate::{bricks, canvas};

pub struct AppState {}

#[taurpc::procedures(export_to = "../ui/lib/core.ts", event_trigger = ApiEventTrigger)]
pub trait CoreApi {
    #[taurpc(event)]
    async fn graph_updated(graph: Graph);

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
}
