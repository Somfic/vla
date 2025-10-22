use crate::api::ApiEventTrigger;
use crate::bricks;
use crate::prelude::*;
use serde_json::Value;
use std::collections::BTreeMap;
use std::fs;
use std::path::Path;
use tauri::AppHandle;
use tauri::Runtime;
use uuid::Uuid;

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct Graph {
    pub nodes: Vec<Node>,
    pub edges: Vec<Edge>,
}
impl Graph {
    fn to_json(&self) -> Result<String, String> {
        let json_value =
            serde_json::to_value(self).map_err(|e| format!("Failed to serialize graph: {}", e))?;

        let json_value = remove_json_fields_from_node(json_value, vec!["brick"]);

        serde_json::to_string_pretty(&json_value)
            .map_err(|e| format!("Failed to format JSON: {}", e))
    }

    pub fn from_json(json: String) -> Result<Self, String> {
        let mut graph: Graph =
            serde_json::from_str(&json).map_err(|e| format!("Failed to parse graph: {}", e))?;

        for node in &mut graph.nodes {
            node.data.brick = get_brick(&node.data.brick_id);
        }

        Ok(graph)
    }
}

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct Node {
    pub id: String,
    pub position: Point,
    pub data: NodeData,
    pub r#type: String,
}

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct Point {
    pub x: f64,
    pub y: f64,
}

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct NodeData {
    #[serde(rename = "brickId")]
    pub brick_id: String,
    pub brick: Option<Brick>,
    pub arguments: BTreeMap<String, String>,
    pub defaults: BTreeMap<String, String>,
}

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct Edge {
    pub id: String,
    pub source: String,
    pub target: String,
    #[serde(rename = "sourceHandle")]
    pub source_handle: String,
    #[serde(rename = "targetHandle")]
    pub target_handle: String,
}

pub async fn save_graph<R: Runtime>(
    app_handle: AppHandle<R>,
    graph: &Graph,
    graph_path: &str,
    notify_frontend: bool,
) -> Result<String, String> {
    let json = graph.to_json()?;

    fs::write(graph_path, json).map_err(|e| format!("Failed to write file: {}", e))?;

    if notify_frontend {
        ApiEventTrigger::new(app_handle)
            .graph_updated(graph.clone())
            .map_err(|e| {
                format!(
                    "Failed to notify frontend about graph update: {}",
                    e.to_string()
                )
            })?;
    }

    Ok(format!("Graph saved to {}", graph_path))
}

fn remove_json_fields_from_node(mut json_value: Value, fields: Vec<&str>) -> Value {
    if let Some(nodes) = json_value.get_mut("nodes").and_then(|n| n.as_array_mut()) {
        for node in nodes {
            if let Some(data) = node.get_mut("data").and_then(|d| d.as_object_mut()) {
                for field in &fields {
                    data.remove(*field);
                }
            }
        }
    }

    json_value
}

pub async fn load_graph<R: Runtime>(
    app_handle: AppHandle<R>,
    graph_path: &str,
) -> Result<Graph, String> {
    if !Path::new(graph_path).exists() {
        let empty_graph = Graph {
            nodes: vec![],
            edges: vec![],
        };

        save_graph(app_handle, &empty_graph, graph_path, false).await?;
        return Ok(empty_graph);
    }

    let json = fs::read_to_string(graph_path).map_err(|e| format!("Failed to read file: {}", e))?;

    Graph::from_json(json)
}

pub fn get_brick(brick_id: &str) -> Option<bricks::types::Brick> {
    let bricks = bricks::all_bricks();
    bricks.into_iter().find(|b| b.id == brick_id)
}

pub async fn insert_node<R: Runtime>(
    app_handle: AppHandle<R>,
    graph_path: &str,
    brick_id: &str,
    position: Point,
) -> Result<Graph, String> {
    let mut graph = load_graph(app_handle.clone(), graph_path).await?;
    let brick = get_brick(brick_id);

    if brick.is_none() {
        return Err(format!("Brick with id '{}' not found", brick_id));
    }

    let brick = brick.unwrap();

    let node = Node {
        id: Uuid::new_v4().to_string(),
        position,
        data: NodeData {
            brick_id: brick_id.to_string(),
            brick: Some(brick.clone()),
            arguments: BTreeMap::new(),
            defaults: brick
                .inputs
                .iter()
                .filter_map(|input| {
                    input
                        .default_value
                        .as_ref()
                        .map(|v| (input.id.clone(), v.clone()))
                })
                .collect(),
        },
        r#type: "v1".to_string(),
    };

    graph.nodes.push(node);
    save_graph(app_handle, &graph, graph_path, true).await?;
    Ok(graph)
}
