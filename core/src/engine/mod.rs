use std::collections::{HashMap, VecDeque};

use crate::{bricks::types::BrickOutputValue, prelude::*};
#[cfg(test)]
mod tests;
pub mod trigger;

pub struct Engine {
    graph: Graph,
    queue: VecDeque<String>,
    cache: HashMap<String, Vec<BrickOutputValue>>,
    state: HashMap<String, ExecutionState>,
}

enum ExecutionState {
    Pending,
    Running,
    Completed,
    Failed(String),
}

impl Engine {
    pub fn new(graph: Graph) -> Self {
        let mut engine = Self {
            graph,
            queue: VecDeque::new(),
            cache: HashMap::new(),
            state: HashMap::new(),
        };

        for node in &engine.graph.nodes {
            engine
                .state
                .insert(node.id.clone(), ExecutionState::Pending);
        }

        engine
    }

    pub fn start(&mut self) {
        //let start_nodes = self.find_start_nodes();
        let start_nodes = self
            .graph
            .nodes
            .iter()
            .map(|n| n.id.clone())
            .collect::<Vec<_>>();
        for node_id in start_nodes {
            self.queue.push_back(node_id);
        }
    }

    fn execute_node(&mut self, node_id: String) -> Result<(), String> {
        // trigger::clear_triggers();
        self.state.insert(node_id.clone(), ExecutionState::Running);

        let node = self
            .graph
            .nodes
            .iter()
            .find(|node| node.id.eq(&node_id))
            .ok_or_else(|| "Cannot find node")?;

        let brick = node
            .data
            .brick
            .as_ref()
            .ok_or_else(|| "Cannot find brick")?;

        let arguments = vec![]; // TODO: Build BrickArgumentValue from node arguments
        let inputs = vec![]; // TODO: Build BrickInputValue from node inputs
        let outputs = (brick.execution)(arguments.clone(), inputs.clone());
        let triggered = trigger::collect_and_clear_triggers();

        println!("Executed node: {}", node_id);
        println!("Inputs: {:?}", inputs);
        println!("Outputs: {:?}", outputs);
        println!("Triggered: {:?}", triggered);

        self.cache.insert(node_id.clone(), outputs.clone());
        self.state
            .insert(node_id.clone(), ExecutionState::Completed);

        Ok(())
    }
}

impl Iterator for Engine {
    type Item = Result<(), String>;

    fn next(&mut self) -> Option<Self::Item> {
        if let Some(node_id) = self.queue.pop_front() {
            return Some(self.execute_node(node_id));
        }
        None
    }
}
