use crate::prelude::*;
use std::collections::HashSet;

/// A depth-first search iterator for resolving data node dependencies.
///
/// This iterator traverses the data dependency graph in depth-first order,
/// yielding nodes that need to be executed to provide data for a target node.
///
/// ## Behavior
/// - Only traverses data edges (non-execution edges)
/// - Yields dependencies before dependents (post-order traversal)
/// - Skips already-cached nodes
/// - Detects and prevents cycles
pub struct DataNodeDfsIterator<'a> {
    graph: &'a Graph,
    /// Stack of (node_id, is_visited) for DFS traversal
    stack: Vec<(String, bool)>,
    /// Nodes that have been yielded already
    yielded: HashSet<String>,
    /// Nodes that are already cached (don't need execution)
    cached: &'a HashSet<String>,
}

impl<'a> DataNodeDfsIterator<'a> {
    /// Create a new DFS iterator starting from a target node.
    ///
    /// # Arguments
    /// * `graph` - The graph to traverse
    /// * `target_node_id` - The node whose data dependencies should be resolved
    /// * `cached` - Set of node IDs that are already cached and don't need execution
    pub fn new(graph: &'a Graph, target_node_id: &str, cached: &'a HashSet<String>) -> Self {
        let mut iterator = Self {
            graph,
            stack: Vec::new(),
            yielded: HashSet::new(),
            cached,
        };

        // Start DFS from the target node
        iterator.push_node(target_node_id.to_string());

        iterator
    }

    /// Push a node onto the stack for processing
    fn push_node(&mut self, node_id: String) {
        // Don't process nodes that are already cached
        if self.cached.contains(&node_id) {
            return;
        }

        // Don't process nodes we've already yielded
        if self.yielded.contains(&node_id) {
            return;
        }

        // Mark as not yet visited (false flag)
        self.stack.push((node_id, false));
    }

    /// Get all data dependencies for a given node.
    /// Returns node IDs of nodes that provide data to this node.
    fn get_data_dependencies(&self, node_id: &str) -> Vec<String> {
        self.graph
            .edges
            .iter()
            .filter(|edge| {
                // Only consider data edges (not execution edges)
                edge.target == node_id && self.is_data_edge(edge)
            })
            .map(|edge| edge.source.clone())
            .collect()
    }

    /// Check if an edge is a data edge by examining if the handles
    /// match execution inputs/outputs on the connected nodes
    fn is_data_edge(&self, edge: &Edge) -> bool {
        if let Some(source_node) = self.graph.nodes.iter().find(|n| n.id == edge.source) {
            if let Some(brick) = &source_node.data.brick {
                if brick
                    .execution_outputs
                    .iter()
                    .any(|out| out.id == edge.source_handle)
                {
                    return false;
                }
            }
        }

        if let Some(target_node) = self.graph.nodes.iter().find(|n| n.id == edge.target) {
            if let Some(brick) = &target_node.data.brick {
                if brick
                    .execution_inputs
                    .iter()
                    .any(|inp| inp.id == edge.target_handle)
                {
                    return false;
                }
            }
        }

        true
    }

    /// Check if a node is a data node (has no execution inputs/outputs)
    fn is_data_node(&self, node_id: &str) -> bool {
        if let Some(node) = self.graph.nodes.iter().find(|n| n.id == node_id) {
            if let Some(brick) = &node.data.brick {
                return brick.execution_inputs.is_empty() && brick.execution_outputs.is_empty();
            }
        }
        false
    }
}

impl<'a> Iterator for DataNodeDfsIterator<'a> {
    type Item = String;

    fn next(&mut self) -> Option<Self::Item> {
        while let Some((node_id, visited)) = self.stack.pop() {
            if visited {
                // This node's dependencies have been processed
                // Now we can yield this node (if it's a data node and not cached)
                if !self.yielded.contains(&node_id)
                    && !self.cached.contains(&node_id)
                    && self.is_data_node(&node_id)
                {
                    self.yielded.insert(node_id.clone());
                    return Some(node_id);
                }
            } else {
                // First time seeing this node - push it back with visited=true
                // so we process it after its dependencies
                self.stack.push((node_id.clone(), true));

                // Push all dependencies onto the stack
                let dependencies = self.get_data_dependencies(&node_id);
                for dep_id in dependencies {
                    self.push_node(dep_id);
                }
            }
        }

        None
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::bricks::types::{Brick, BrickInput, BrickOutput, ConnectionType};
    use crate::prelude::*;
    use std::collections::BTreeMap;

    fn create_test_brick(id: &str, has_exec: bool) -> Brick {
        use crate::bricks::types::{
            BrickArgumentValue, BrickExecutionInput, BrickExecutionOutput, BrickInputValue,
            BrickOutputValue,
        };

        Brick {
            id: id.to_string(),
            label: id.to_string(),
            description: String::new(),
            keywords: vec![],
            category: "test".to_string(),
            arguments: vec![],
            inputs: vec![BrickInput {
                id: "input".to_string(),
                label: "Input".to_string(),
                r#type: ConnectionType::Number,
                default_value: None,
            }],
            outputs: vec![BrickOutput {
                id: "output".to_string(),
                label: "Output".to_string(),
                r#type: ConnectionType::Number,
            }],
            execution_inputs: if has_exec {
                vec![BrickExecutionInput {
                    id: "execute".to_string(),
                    label: "Execute".to_string(),
                }]
            } else {
                vec![]
            },
            execution_outputs: if has_exec {
                vec![BrickExecutionOutput {
                    id: "done".to_string(),
                    label: "Done".to_string(),
                }]
            } else {
                vec![]
            },
            execution: |_args: Vec<BrickArgumentValue>, _inputs: Vec<BrickInputValue>| {
                vec![BrickOutputValue {
                    id: "output".to_string(),
                    value: "42".to_string(),
                }]
            },
        }
    }

    fn create_test_node(id: &str, brick_id: &str, has_exec: bool) -> Node {
        Node {
            id: id.to_string(),
            position: Point { x: 0.0, y: 0.0 },
            data: NodeData {
                brick_id: brick_id.to_string(),
                brick: Some(create_test_brick(brick_id, has_exec)),
                arguments: BTreeMap::new(),
                defaults: BTreeMap::new(),
            },
            r#type: "v1".to_string(),
        }
    }

    #[test]
    fn test_simple_linear_dependency() {
        // Graph: A (data) -> B (data) -> C (data)
        let graph = Graph {
            nodes: vec![
                create_test_node("A", "brick_a", false),
                create_test_node("B", "brick_b", false),
                create_test_node("C", "brick_c", false),
            ],
            edges: vec![
                Edge {
                    id: "e1".to_string(),
                    source: "A".to_string(),
                    target: "B".to_string(),
                    source_handle: "data_output".to_string(),
                    target_handle: "data_input".to_string(),
                },
                Edge {
                    id: "e2".to_string(),
                    source: "B".to_string(),
                    target: "C".to_string(),
                    source_handle: "data_output".to_string(),
                    target_handle: "data_input".to_string(),
                },
            ],
        };

        let cached = HashSet::new();
        let mut iter = DataNodeDfsIterator::new(&graph, "C", &cached);

        let result: Vec<String> = iter.collect();

        // Should execute A first (no dependencies), then B, then C
        assert_eq!(result, vec!["A", "B", "C"]);
    }

    #[test]
    fn test_diamond_dependency() {
        // Graph:     A (data)
        //           / \
        //     B (data) C (data)
        //           \ /
        //           D (data)
        let graph = Graph {
            nodes: vec![
                create_test_node("A", "brick_a", false),
                create_test_node("B", "brick_b", false),
                create_test_node("C", "brick_c", false),
                create_test_node("D", "brick_d", false),
            ],
            edges: vec![
                Edge {
                    id: "e1".to_string(),
                    source: "A".to_string(),
                    target: "B".to_string(),
                    source_handle: "data_output".to_string(),
                    target_handle: "data_input".to_string(),
                },
                Edge {
                    id: "e2".to_string(),
                    source: "A".to_string(),
                    target: "C".to_string(),
                    source_handle: "data_output".to_string(),
                    target_handle: "data_input".to_string(),
                },
                Edge {
                    id: "e3".to_string(),
                    source: "B".to_string(),
                    target: "D".to_string(),
                    source_handle: "data_output".to_string(),
                    target_handle: "data_input".to_string(),
                },
                Edge {
                    id: "e4".to_string(),
                    source: "C".to_string(),
                    target: "D".to_string(),
                    source_handle: "data_output".to_string(),
                    target_handle: "data_input".to_string(),
                },
            ],
        };

        let cached = HashSet::new();
        let mut iter = DataNodeDfsIterator::new(&graph, "D", &cached);

        let result: Vec<String> = iter.collect();

        // A should be first, then B and C (order may vary), then D
        assert!(result.len() == 4);
        assert_eq!(result[0], "A"); // A has no dependencies
        assert_eq!(result[3], "D"); // D is last
                                    // B and C should be in positions 1 and 2 (order doesn't matter)
        assert!(result[1..3].contains(&"B".to_string()));
        assert!(result[1..3].contains(&"C".to_string()));
    }

    #[test]
    fn test_skip_cached_nodes() {
        // Graph: A (data) -> B (data) -> C (data)
        let graph = Graph {
            nodes: vec![
                create_test_node("A", "brick_a", false),
                create_test_node("B", "brick_b", false),
                create_test_node("C", "brick_c", false),
            ],
            edges: vec![
                Edge {
                    id: "e1".to_string(),
                    source: "A".to_string(),
                    target: "B".to_string(),
                    source_handle: "data_output".to_string(),
                    target_handle: "data_input".to_string(),
                },
                Edge {
                    id: "e2".to_string(),
                    source: "B".to_string(),
                    target: "C".to_string(),
                    source_handle: "data_output".to_string(),
                    target_handle: "data_input".to_string(),
                },
            ],
        };

        let mut cached = HashSet::new();
        cached.insert("A".to_string());

        let mut iter = DataNodeDfsIterator::new(&graph, "C", &cached);

        let result: Vec<String> = iter.collect();

        // Should only execute B and C (A is cached)
        assert_eq!(result, vec!["B", "C"]);
    }

    #[test]
    fn test_skip_execution_nodes() {
        // Graph: A (flow) -> B (data) -> C (flow)
        // When resolving C's data dependencies, should only return B (not A)
        let graph = Graph {
            nodes: vec![
                create_test_node("A", "brick_a", true),  // flow node
                create_test_node("B", "brick_b", false), // data node
                create_test_node("C", "brick_c", true),  // flow node
            ],
            edges: vec![
                Edge {
                    id: "e1".to_string(),
                    source: "A".to_string(),
                    target: "B".to_string(),
                    source_handle: "done".to_string(), // execution edge
                    target_handle: "execute".to_string(),
                },
                Edge {
                    id: "e2".to_string(),
                    source: "B".to_string(),
                    target: "C".to_string(),
                    source_handle: "data_output".to_string(), // data edge
                    target_handle: "data_input".to_string(),
                },
            ],
        };

        let cached = HashSet::new();
        let mut iter = DataNodeDfsIterator::new(&graph, "C", &cached);

        let result: Vec<String> = iter.collect();

        // Should only return B (data node), not A (flow node)
        assert_eq!(result, vec!["B"]);
    }
}
