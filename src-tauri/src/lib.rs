use std::collections::{BTreeMap, HashMap, HashSet};
use std::fs;
use std::path::Path;

#[taurpc::procedures(export_to = "../src/lib/core.ts")]
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

    async fn get_brick(self, brick_id: String) -> Option<Brick> {
        let bricks = self.get_bricks().await;
        bricks.into_iter().find(|b| b.id == brick_id)
    }

    async fn get_bricks(self) -> Vec<Brick> {
        vec![Brick {
            id: "testBrick".to_string(),
            label: "Test Brick".to_string(),
            description: "A simple test brick".to_string(),
            inputs: vec![BrickHandle {
                id: "in1".to_string(),
                label: "Input".to_string(),
            }],
            outputs: vec![BrickHandle {
                id: "out1".to_string(),
                label: "Output".to_string(),
            }],
            arguments: vec![
                BrickArgument {
                    id: "arg1".to_string(),
                    label: "String".to_string(),
                    r#type: BrickArgumentType::String,
                    default_value: None,
                    enum_options: None,
                },
                BrickArgument {
                    id: "arg2".to_string(),
                    label: "Number".to_string(),
                    r#type: BrickArgumentType::Number,
                    default_value: None,
                    enum_options: None,
                },
                BrickArgument {
                    id: "arg3".to_string(),
                    label: "Boolean".to_string(),
                    r#type: BrickArgumentType::Boolean,
                    default_value: None,
                    enum_options: None,
                },
                BrickArgument {
                    id: "arg4".to_string(),
                    label: "Enum".to_string(),
                    r#type: BrickArgumentType::Enum,
                    default_value: Some("Option 1".to_string()),
                    enum_options: Some(
                        vec![
                            "Option 1".to_string(),
                            "Option 2".to_string(),
                            "Option 3".to_string(),
                        ]
                        .into_iter()
                        .collect(),
                    ),
                },
            ],
        }]
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
    pub brick_id: String,
    #[serde(default)]
    pub brick: Option<Brick>,
    pub arguments: BTreeMap<String, String>,
}

#[taurpc::ipc_type]
pub struct Edge {
    pub id: String,
    pub source: String,
    pub target: String,
}

#[taurpc::ipc_type]
pub struct Brick {
    pub id: String,
    pub label: String,
    pub description: String,
    pub inputs: Vec<BrickHandle>,
    pub outputs: Vec<BrickHandle>,
    pub arguments: Vec<BrickArgument>,
}

#[taurpc::ipc_type]
pub struct BrickHandle {
    pub id: String,
    pub label: String,
}

#[taurpc::ipc_type]
pub struct BrickArgument {
    pub id: String,
    pub label: String,
    pub r#type: BrickArgumentType,
    pub enum_options: Option<Vec<String>>,
    pub default_value: Option<String>,
}

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub enum BrickArgumentType {
    String,
    Number,
    Boolean,
    Enum,
}
