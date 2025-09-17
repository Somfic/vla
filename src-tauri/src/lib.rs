use std::fs;
use std::path::Path;

#[taurpc::procedures(export_to = "../src/lib/core.ts")]
pub trait Api {
    async fn hello_world(name: String) -> String;
    async fn save_graph(graph: Graph, filename: String) -> Result<String, String>;
    async fn load_graph(filename: String) -> Result<Graph, String>;
}

#[derive(Clone)]
pub struct ApiImpl;

#[taurpc::resolvers]
impl Api for ApiImpl {
    async fn hello_world(self, name: String) -> String {
        format!("Hello, {}! You've been greeted from Rust!", name)
    }

    async fn save_graph(self, graph: Graph, filename: String) -> Result<String, String> {
        match serde_json::to_string_pretty(&graph) {
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
                Ok(graph) => Ok(graph),
                Err(e) => Err(format!("Failed to parse graph: {}", e)),
            },
            Err(e) => Err(format!("Failed to read file: {}", e)),
        }
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

#[taurpc::ipc_type]
pub struct NodeData {
    pub label: String,
}

#[taurpc::ipc_type]
pub struct Edge {
    pub id: String,
    pub source: String,
    pub target: String,
}
