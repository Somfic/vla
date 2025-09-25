use crate::prelude::*;
use crate::{bricks, canvas};

#[taurpc::procedures(export_to = "../ui/lib/core.ts")]
pub trait Api {
    async fn save_graph(graph: Graph, filename: String) -> Result<String, String>;
    async fn load_graph(filename: String) -> Result<Graph, String>;
    async fn get_brick(brick_id: String) -> Option<Brick>;
    async fn get_bricks() -> Vec<Brick>;
    async fn insert_node(
        graph_path: String,
        brick_id: String,
        position: Point,
    ) -> Result<Graph, String>;
}

#[derive(Clone)]
pub struct ApiImpl;

#[taurpc::resolvers]
impl Api for ApiImpl {
    async fn save_graph(self, graph: Graph, graph_path: String) -> Result<String, String> {
        canvas::save_graph(&graph, &graph_path).await
    }

    async fn load_graph(self, graph_path: String) -> Result<Graph, String> {
        canvas::load_graph(&graph_path).await
    }

    async fn get_brick(self, brick_id: String) -> Option<bricks::types::Brick> {
        canvas::get_brick(&brick_id)
    }

    async fn get_bricks(self) -> Vec<bricks::types::Brick> {
        bricks::all_bricks()
    }

    async fn insert_node(
        self,
        graph_path: String,
        brick_id: String,
        position: Point,
    ) -> Result<Graph, String> {
        canvas::insert_node(&graph_path, &brick_id, position).await
    }
}
