use std::collections::{HashMap, HashSet, VecDeque};

use crate::{
    bricks::types::{BrickArgumentValue, BrickInputValue, BrickOutputValue},
    prelude::*,
    set_current_node_id, trigger,
};
#[cfg(test)]
mod tests;
#[cfg(test)]
mod flow_test;
pub mod data_dfs;
pub mod topological;
pub mod trigger;

pub struct Engine {
    graph: Graph,
    /// Queue of flow nodes waiting to execute
    queue: VecDeque<String>,
    /// Cache of data node outputs (node_id -> outputs)
    cache: HashMap<String, Vec<BrickOutputValue>>,
    /// Current flow node being processed
    current_flow_node: Option<String>,
    /// Pending data dependencies for the current flow node
    pending_data_deps: VecDeque<String>,
    /// Fast node lookup (node_id -> node index)
    node_index: HashMap<String, usize>,
    /// Enable debug output
    debug: bool,
}

impl Engine {
    pub fn new(graph: Graph) -> Self {
        Self::with_debug(graph, false)
    }

    pub fn with_debug(graph: Graph, debug: bool) -> Self {
        // Build node index for O(1) lookups
        let node_index: HashMap<String, usize> = graph
            .nodes
            .iter()
            .enumerate()
            .map(|(idx, node)| (node.id.clone(), idx))
            .collect();

        Self {
            graph,
            queue: VecDeque::new(),
            cache: HashMap::new(),
            current_flow_node: None,
            pending_data_deps: VecDeque::new(),
            node_index,
            debug,
        }
    }

    /// Get a node by ID (O(1) lookup)
    fn get_node(&self, node_id: &str) -> Option<&Node> {
        self.node_index
            .get(node_id)
            .and_then(|&idx| self.graph.nodes.get(idx))
    }

    /// Debug logging helper
    fn debug_log(&self, msg: &str) {
        if self.debug {
            println!("{}", msg);
        }
    }

    /// Check if an edge is a data edge (not an execution edge)
    /// Data edges have handles starting with "data_" or other non-"exec_" prefixes
    fn is_data_edge(edge: &Edge) -> bool {
        !edge.source_handle.starts_with("exec_") && !edge.target_handle.starts_with("exec_")
    }

    /// Check if an edge is an execution edge
    /// Execution edges have handles starting with "exec_"
    fn is_execution_edge(edge: &Edge) -> bool {
        edge.source_handle.starts_with("exec_") || edge.target_handle.starts_with("exec_")
    }

    pub fn start(&mut self) {
        // Find nodes that have execution outputs but no execution inputs (start nodes)
        // These are the entry points for execution flow
        let start_nodes: Vec<String> = self
            .graph
            .nodes
            .iter()
            .filter(|node| {
                if let Some(brick) = &node.data.brick {
                    !brick.execution_outputs.is_empty() && brick.execution_inputs.is_empty()
                } else {
                    false
                }
            })
            .map(|node| node.id.clone())
            .collect();

        // If no flow start nodes found, treat all nodes with no incoming edges as start nodes
        if start_nodes.is_empty() {
            let nodes_with_incoming: std::collections::HashSet<String> = self
                .graph
                .edges
                .iter()
                .map(|e| e.target.clone())
                .collect();

            for node in &self.graph.nodes {
                if !nodes_with_incoming.contains(&node.id) {
                    self.queue.push_back(node.id.clone());
                }
            }
        } else {
            // Queue all start nodes for execution
            for node_id in start_nodes {
                self.queue.push_back(node_id);
            }
        }
    }

    /// Manually enqueue a flow node for execution
    pub fn enqueue(&mut self, node_id: String) {
        self.queue.push_back(node_id);
    }

    /// Get cached node IDs
    fn get_cached_node_ids(&self) -> HashSet<String> {
        self.cache.keys().cloned().collect()
    }

    /// Resolve all data dependencies for a node using DFS
    fn resolve_data_dependencies(&self, node_id: &str) -> Vec<String> {
        let cached = self.get_cached_node_ids();
        let dfs_iter = data_dfs::DataNodeDfsIterator::new(&self.graph, node_id, &cached);
        dfs_iter.collect()
    }

    /// Execute a single node (data or flow) and cache its outputs
    fn execute_node_internal(&mut self, node_id: &str) -> Result<(), String> {
        trigger::set_current_node_id(node_id);

        let node = self
            .get_node(node_id)
            .ok_or_else(|| format!("Node '{}' not found", node_id))?;

        let brick = node
            .data
            .brick
            .as_ref()
            .ok_or_else(|| format!("Node '{}' has no brick", node_id))?;

        // Build arguments from node data
        let arguments = self.build_arguments(node, brick);

        // Build inputs from connected edges and cached data
        let inputs = self.build_inputs(node, brick)?;

        // Execute the brick
        let outputs = (brick.execution)(arguments, inputs);

        // Cache outputs
        self.cache.insert(node_id.to_string(), outputs.clone());

        trigger::clear_current_node_id();

        self.debug_log(&format!("✓ Executed: {}", node_id));

        Ok(())
    }

    /// Build brick arguments from node data
    fn build_arguments(&self, node: &Node, brick: &Brick) -> Vec<BrickArgumentValue> {
        brick
            .arguments
            .iter()
            .map(|arg_def| {
                // Get value from node arguments, or use brick default
                let value = node
                    .data
                    .arguments
                    .get(&arg_def.id)
                    .or(arg_def.default_value.as_ref())
                    .cloned()
                    .unwrap_or_default();

                BrickArgumentValue {
                    id: arg_def.id.clone(),
                    value,
                }
            })
            .collect()
    }

    /// Build brick inputs from connected edges and cached data
    fn build_inputs(&self, node: &Node, brick: &Brick) -> Result<Vec<BrickInputValue>, String> {
        brick
            .inputs
            .iter()
            .map(|input_def| {
                // Find incoming data edge for this input
                let connected_value = self.find_connected_input_value(&node.id, &input_def.id);

                // Priority: connected edge > node defaults > brick defaults
                let value = connected_value
                    .or_else(|| node.data.defaults.get(&input_def.id).cloned())
                    .or_else(|| input_def.default_value.clone())
                    .ok_or_else(|| {
                        format!(
                            "No value available for input '{}' on node '{}'",
                            input_def.id, node.id
                        )
                    })?;

                Ok(BrickInputValue {
                    id: input_def.id.clone(),
                    value,
                })
            })
            .collect()
    }

    /// Find the value for an input by traversing data edges
    fn find_connected_input_value(&self, target_node_id: &str, target_input_id: &str) -> Option<String> {
        // Find data edge targeting this input
        let target_handle = format!("data_{}", target_input_id);

        for edge in &self.graph.edges {
            if edge.target == target_node_id && edge.target_handle == target_handle {
                // Found the edge, now get the source node's cached output
                if let Some(cached_outputs) = self.cache.get(&edge.source) {
                    // Extract output ID from source handle (e.g., "data_result" -> "result")
                    let source_output_id = edge
                        .source_handle
                        .strip_prefix("data_")
                        .unwrap_or(&edge.source_handle);

                    // Find the matching output
                    return cached_outputs
                        .iter()
                        .find(|output| output.id == source_output_id)
                        .map(|output| output.value.clone());
                }
            }
        }

        None
    }

    /// Execute a data node (skips if already cached)
    fn execute_data_node(&mut self, node_id: &str) -> Result<(), String> {
        if self.cache.contains_key(node_id) {
            return Ok(());
        }
        self.execute_node_internal(node_id)
    }

    /// Find nodes triggered by an execution output
    fn find_triggered_nodes(&self, trigger: &trigger::Trigger) -> Vec<String> {
        let exec_handle = trigger.to_handle();

        self.graph
            .edges
            .iter()
            .filter(|edge| {
                edge.source == trigger.source_node && edge.source_handle == exec_handle
            })
            .map(|edge| edge.target.clone())
            .collect()
    }
}

impl Iterator for Engine {
    type Item = Result<String, String>;

    fn next(&mut self) -> Option<Self::Item> {
        // Explicit state machine loop - clearer than recursive calls
        loop {
            // State 1: Process pending data dependencies
            if let Some(data_node_id) = self.pending_data_deps.pop_front() {
                return match self.execute_data_node(&data_node_id) {
                    Ok(_) => Some(Ok(data_node_id)),
                    Err(e) => Some(Err(e)),
                };
            }

            // State 2: Execute current flow node
            if let Some(flow_node_id) = self.current_flow_node.take() {
                // Execute the flow node
                if let Err(e) = self.execute_node_internal(&flow_node_id) {
                    return Some(Err(e));
                }

                // Collect triggers and queue next flow nodes
                let triggers = trigger::collect_and_clear_triggers();
                for trigger in &triggers {
                    let next_nodes = self.find_triggered_nodes(trigger);
                    self.queue.extend(next_nodes);
                }

                return Some(Ok(flow_node_id));
            }

            // State 3: Start new flow node from queue
            let next_flow_node = self.queue.pop_front()?;

            // Resolve data dependencies for this flow node
            let data_deps = self.resolve_data_dependencies(&next_flow_node);

            // Queue data dependencies and set current flow node
            self.pending_data_deps.extend(data_deps);
            self.current_flow_node = Some(next_flow_node);

            // Loop continues to process the pending state
            // (will either execute data deps or the flow node itself)
        }
    }
}
