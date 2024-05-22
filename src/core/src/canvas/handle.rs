use super::models::{Connection, Node};
use crate::prelude::*;
use eyre::Context;
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
            nodes: vec![],
            connections: vec![],
        };

        let canvas_arc = Arc::new(Mutex::new(canvas));
        Self::setup(canvas_arc.clone());

        canvas_arc
    }

    pub fn setup(canvas: Arc<Mutex<Self>>) {
        let window_handle = {
            let handle = canvas.lock().unwrap();
            handle.window_handle.clone()
        };

        let handle_clone = Arc::clone(&canvas);
        window_handle.listen_global("canvas_nodes_changed", move |e| {
            let nodes: Vec<Node> = serde_json::from_str(e.payload().unwrap()).unwrap();
            if let Ok(mut handle) = handle_clone.lock() {
                handle.set_nodes(nodes);
            }
        });
    }

    pub fn set_node(&mut self, node: Node) -> Result<()> {
        if let Some(index) = self.nodes.iter().position(|n| n.id == node.id) {
            self.nodes[index] = node;
        } else {
            self.nodes.push(node);
        }

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
