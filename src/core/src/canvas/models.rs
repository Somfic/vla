use schemars::JsonSchema;
use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, Clone, JsonSchema, Debug)]
#[serde(rename_all = "lowercase")]
pub enum NodeKind {
    Input,
}

#[derive(Serialize, Deserialize, Clone, JsonSchema, Debug)]
pub struct Position {
    pub x: i32,
    pub y: i32,
}

#[derive(Serialize, Deserialize, Clone, JsonSchema, Debug)]
pub struct Node {
    pub id: String,
    #[serde(rename = "type")]
    pub kind: String,
    pub position: Position,
    pub data: NodeData,
    pub selected: bool,
}

#[derive(Serialize, Deserialize, Clone, JsonSchema, Debug)]
pub struct NodeData {
    pub label: String,
}

#[derive(Serialize, Deserialize, Clone, JsonSchema, Debug)]
pub struct Connection {
    pub id: String,
    pub source: String,
    pub target: String,
    pub selected: bool,
}
