use super::models::{Connection, Node};
use anyhow::{Context, Result};
use std::sync::{Arc, Mutex};
use tauri::Manager;

#[derive(Debug)]
pub struct CanvasHandle {
    window_handle: &'static tauri::Window,
    nodes: Vec<Node>,
    connections: Vec<Connection>,
}

impl CanvasHandle {
    pub fn new(window: &'static tauri::Window) -> Arc<Mutex<Self>> {
        let canvas = CanvasHandle {
            window_handle: window,
            nodes: vec![Node {
                id: "1".to_string(),
                kind: "input".to_string(),
                position: super::models::Position { x: 100, y: 100 },
                data: super::models::NodeData {
                    label: "Hello from Rust".to_string(),
                },
                selected: false,
            }],
            connections: vec![],
        };

        let canvas_arc = Arc::new(Mutex::new(canvas));

        canvas_arc
    }

    pub fn set_node(&mut self, node: Node) -> Result<()> {
        if let Some(index) = self.nodes.iter().position(|n| n.id == node.id) {
            self.nodes[index] = node;
        } else {
            self.nodes.push(node);
        }

        println!("canvas_nodes_changed event emitted");
        self.window_handle
            .emit_all("canvas_nodes_changed", self.nodes.clone())
            .context("Could not emit canvas nodes changed")
    }

    pub fn set_connection(&mut self, connection: Connection) -> Result<()> {
        if let Some(index) = self.connections.iter().position(|c| c.id == connection.id) {
            self.connections[index] = connection;
        } else {
            self.connections.push(connection);
        }

        self.window_handle
            .emit_all("canvas_connections_changed", self.connections.clone())
            .context("Could not emit canvas connections changed")
    }

    pub fn set_nodes(&mut self, nodes: Vec<Node>) -> Result<()> {
        self.nodes = nodes;

        println!("canvas_nodes_changed event emitted");
        self.window_handle
            .emit_all("canvas_nodes_changed", self.nodes.clone())
            .context("Could not emit canvas nodes changed")
    }

    pub fn set_connections(&mut self, connections: Vec<Connection>) -> Result<()> {
        self.connections = connections;

        self.window_handle
            .emit_all("canvas_connections_changed", self.connections.clone())
            .context("Could not emit canvas connections changed")
    }

    pub fn get_nodes(&self) -> Vec<Node> {
        self.nodes.clone()
    }

    pub fn get_connections(&self) -> Vec<Connection> {
        self.connections.clone()
    }
}

pub mod host {
    use crate::{canvas::models::Node, notifications::models::Notification, plugins::AppHandle};
    use anyhow::{anyhow, Context};
    use extism::{convert::Json, host_fn};

    host_fn!(pub get_nodes(app_handle: AppHandle; notification: Json<Notification>) -> Result<Vec<Node>> {
        let app_handle = app_handle.get().context("Could not get app handle")?;
        let app_handle = app_handle.lock().map_err(|_| anyhow!("Could not lock app handle"))?;
        let canvas_handle = app_handle.canvas.lock().map_err(|_| anyhow!("Could not lock canvas handle"))?;

        Ok(Json(canvas_handle.get_nodes()))
    });

    host_fn!(pub set_nodes(app_handle: AppHandle; nodes: Json<Vec<Node>>) -> Result<()> {
        let app_handle = app_handle.get().context("Could not get app handle")?;
        let app_handle = app_handle.lock().map_err(|_| anyhow!("Could not lock app handle"))?;
        let mut canvas_handle = app_handle.canvas.lock().map_err(|_| anyhow!("Could not lock canvas handle"))?;

        canvas_handle.set_nodes(nodes.0)
    });
}
