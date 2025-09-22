use std::collections::BTreeMap;
use std::fs;
use std::path::Path;

use crate::bricks::types::Brick;

pub mod bricks;

#[taurpc::procedures(export_to = "../ui/lib/core.ts")]
pub trait Api {
    async fn hello_world(name: String) -> String;
    async fn save_graph(graph: Graph, filename: String) -> Result<String, String>;
    async fn load_graph(filename: String) -> Result<Graph, String>;
    async fn get_brick(brick_id: String) -> Option<Brick>;
    async fn get_bricks() -> Vec<Brick>;
}

#[derive(Clone)]
pub struct ApiImpl;

#[taurpc::resolvers]
impl Api for ApiImpl {
    async fn hello_world(self, name: String) -> String {
        format!("Hello, {}! You've been greeted from Rust!", name)
    }

    async fn save_graph(self, graph: Graph, filename: String) -> Result<String, String> {
        // Create a copy of the graph with brick fields cleared for file storage
        let mut graph_for_file = graph.clone();
        for node in &mut graph_for_file.nodes {
            node.data.brick = None;
        }

        match serde_json::to_string_pretty(&graph_for_file) {
            Ok(json) => match fs::write(&filename, json) {
                Ok(_) => Ok(format!("Graph saved to {}", filename)),
                Err(e) => Err(format!("Failed to write file: {}", e)),
            },
            Err(e) => Err(format!("Failed to serialize graph: {}", e)),
        }
    }

    async fn load_graph(self, filename: String) -> Result<Graph, String> {
        if !Path::new(&filename).exists() {
            return Err(format!("File {} does not exist", filename));
        }

        match fs::read_to_string(&filename) {
            Ok(content) => match serde_json::from_str::<Graph>(&content) {
                Ok(mut graph) => {
                    // Populate brick data for each node
                    for node in &mut graph.nodes {
                        node.data.brick = self.clone().get_brick(node.data.brick_id.clone()).await;
                    }
                    Ok(graph)
                }
                Err(e) => Err(format!("Failed to parse graph: {}", e)),
            },
            Err(e) => Err(format!("Failed to read file: {}", e)),
        }
    }

    async fn get_brick(self, brick_id: String) -> Option<bricks::types::Brick> {
        let bricks = self.get_bricks().await;
        bricks.into_iter().find(|b| b.id == brick_id)
    }

    async fn get_bricks(self) -> Vec<bricks::types::Brick> {
        let bricks = bricks::get_all_bricks();
        println!("{:#?}", bricks);
        bricks
    }
}

#[taurpc::ipc_type]
pub struct Graph {
    pub nodes: Vec<Node>,
    pub edges: Vec<Edge>,
}

#[taurpc::ipc_type]
pub struct Node {
    pub id: String,
    pub position: Point,
    pub data: NodeData,
    pub r#type: String,
}

#[taurpc::ipc_type]
pub struct Point {
    pub x: f64,
    pub y: f64,
}

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct NodeData {
    pub brick_id: String,
    pub brick: Option<Brick>,
    pub arguments: BTreeMap<String, String>,
}

#[taurpc::ipc_type]
pub struct Edge {
    pub id: String,
    pub source: String,
    pub target: String,
}
