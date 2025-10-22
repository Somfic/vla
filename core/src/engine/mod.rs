use std::collections::{HashMap, HashSet, VecDeque};
use std::time::Instant;

use crate::{
    api::ApiEventTrigger,
    bricks::types::{BrickArgumentValue, BrickInputValue, BrickOutputValue},
    prelude::*,
};
use tauri::{AppHandle, Runtime};
pub mod data_dfs;
pub mod emission_contexts; // Public for extensibility - users can create custom contexts
pub mod events;
pub mod listeners;

use emission_contexts::EmissionContext;
#[cfg(test)]
mod flow_test;
#[cfg(test)]
mod self_emit_test;
#[cfg(test)]
mod tests;
pub mod topological;
pub mod trigger;

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub enum ExecutionMode {
    Normal,  // Run until completion
    Stepped, // Manual step-by-step
}

impl Default for ExecutionMode {
    fn default() -> Self {
        Self::Normal
    }
}

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct ExecutionStateUpdate {
    pub node_id: String,
    pub state: NodeExecutionState,
    pub execution_mode: ExecutionMode,
}

pub struct Engine<R: Runtime = tauri::Wry> {
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
    /// AppHandle for event broadcasting
    app_handle: Option<AppHandle<R>>,
    /// Execution mode (Normal or Stepped)
    execution_mode: ExecutionMode,
    /// Per-node execution states
    node_states: HashMap<String, NodeExecutionState>,
    /// Per-node execution start times
    node_start_times: HashMap<String, Instant>,
    /// Listener registry for self-emitting nodes
    listener_registry: Option<listeners::ListenerRegistry>,
}

// Test-friendly implementation for default runtime
impl Engine {
    /// Create a test engine without app handle (for testing only)
    #[cfg(test)]
    pub fn new_test(graph: Graph) -> Self {
        Self::with_debug_test(graph, false)
    }

    /// Create a test engine with debug (for testing only)
    #[cfg(test)]
    pub fn with_debug_test(graph: Graph, debug: bool) -> Self {
        // Build node index for O(1) lookups
        let node_index: HashMap<String, usize> = graph
            .nodes
            .iter()
            .enumerate()
            .map(|(idx, node)| (node.id.clone(), idx))
            .collect();

        // Initialize node states
        let node_states: HashMap<String, NodeExecutionState> = graph
            .nodes
            .iter()
            .map(|node| (node.id.clone(), NodeExecutionState::default()))
            .collect();

        Self {
            graph,
            queue: VecDeque::new(),
            cache: HashMap::new(),
            current_flow_node: None,
            pending_data_deps: VecDeque::new(),
            node_index,
            debug,
            app_handle: None,
            execution_mode: ExecutionMode::Normal,
            node_states,
            node_start_times: HashMap::new(),
            listener_registry: None,
        }
    }
}

impl<R: Runtime> Engine<R> {
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

        // Initialize node states
        let node_states: HashMap<String, NodeExecutionState> = graph
            .nodes
            .iter()
            .map(|node| (node.id.clone(), NodeExecutionState::default()))
            .collect();

        Self {
            graph,
            queue: VecDeque::new(),
            cache: HashMap::new(),
            current_flow_node: None,
            pending_data_deps: VecDeque::new(),
            node_index,
            debug,
            app_handle: None,
            execution_mode: ExecutionMode::Normal,
            node_states,
            node_start_times: HashMap::new(),
            listener_registry: None,
        }
    }

    /// Create engine with AppHandle for event broadcasting
    pub fn with_app_handle(graph: Graph, app_handle: AppHandle<R>) -> Self {
        let mut engine = Self::new(graph);
        engine.app_handle = Some(app_handle);
        engine
    }

    /// Set execution mode
    pub fn set_execution_mode(&mut self, mode: ExecutionMode) {
        self.execution_mode = mode;
    }

    /// Update node state and broadcast change
    fn update_node_state(
        &mut self,
        node_id: &str,
        phase: ExecutionPhase,
        outputs: Option<Vec<BrickOutputValue>>,
    ) {
        if let Some(node_state) = self.node_states.get_mut(node_id) {
            node_state.outputs = outputs;

            match phase {
                ExecutionPhase::Running => {
                    // Record start time when execution begins
                    self.node_start_times
                        .insert(node_id.to_string(), Instant::now());
                    node_state.phase = phase.clone();
                    node_state.elapsed_ms = 0;
                }
                ExecutionPhase::Completed | ExecutionPhase::Errored => {
                    // Calculate elapsed time when execution finishes
                    if let Some(start_time) = self.node_start_times.get(node_id) {
                        let elapsed = start_time.elapsed();
                        node_state.elapsed_ms = elapsed.as_millis() as u32;
                        self.node_start_times.remove(node_id);
                    }
                    node_state.phase = phase.clone();
                }
                _ => {
                    // For other phases (Waiting, Queued), just update phase
                    node_state.phase = phase.clone();
                    node_state.elapsed_ms = 0;
                }
            }

            let state_copy = node_state.clone();
            self.broadcast_execution_state_update(node_id, state_copy);
        }
    }

    /// Broadcast execution state update event
    fn broadcast_execution_state_update(&self, node_id: &str, state: NodeExecutionState) {
        if let Some(app_handle) = &self.app_handle {
            let update = ExecutionStateUpdate {
                node_id: node_id.to_string(),
                state,
                execution_mode: self.execution_mode.clone(),
            };

            if let Err(e) = ApiEventTrigger::new(app_handle.clone()).node_execution_updated(update)
            {
                eprintln!("Failed to broadcast node execution state update: {}", e);
            }
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

    pub fn start(&mut self) {
        // Create emission contexts for self-emitting nodes
        let (event_sender, event_receiver) = std::sync::mpsc::channel();
        self.start_with_event_channel(event_receiver, event_sender);
    }

    pub fn start_with_event_channel(
        &mut self,
        event_receiver: std::sync::mpsc::Receiver<events::ExecutionEvent>,
        event_sender: std::sync::mpsc::Sender<events::ExecutionEvent>,
    ) {
        // Clear previous execution state
        self.cache.clear();
        self.queue.clear();
        self.current_flow_node = None;
        self.pending_data_deps.clear();
        self.node_start_times.clear();

        // Stop any existing listeners
        if let Some(mut registry) = self.listener_registry.take() {
            let _ = registry.stop_all();
        }

        // Reset all node states to Waiting
        let node_ids: Vec<String> = self.graph.nodes.iter().map(|n| n.id.clone()).collect();
        for node_id in node_ids {
            self.update_node_state(&node_id, ExecutionPhase::Waiting, None);
        }

        // Create registry with the provided event receiver
        let mut registry = listeners::ListenerRegistry::new_with_receiver(event_receiver);

        // Scan for self-emitting nodes and create listeners
        for node in &self.graph.nodes {
            if let Some(brick) = &node.data.brick {
                match &brick.emission_type {
                    crate::bricks::types::BrickEmissionType::Timer {
                        default_interval_ms,
                    } => {
                        // Get interval from node arguments or use default
                        let interval_ms = node
                            .data
                            .arguments
                            .get("interval_ms")
                            .and_then(|v| {
                                // Strip quotes if present
                                let v = v.trim_matches('"');
                                v.parse::<u64>().ok()
                            })
                            .unwrap_or(*default_interval_ms as u64);

                        self.debug_log(&format!(
                            "Creating timer listener for {} ({}ms)",
                            node.id, interval_ms
                        ));

                        // Create timer context and start it
                        let mut context =
                            Box::new(emission_contexts::TimerContext::new(interval_ms));
                        if let Err(e) = context.start(node.id.clone(), event_sender.clone()) {
                            self.debug_log(&format!("Failed to start timer context: {}", e));
                        } else {
                            // Store context in registry's listeners
                            registry
                                .listeners
                                .push(context as Box<dyn emission_contexts::EmissionContext>);
                        }
                    }
                    crate::bricks::types::BrickEmissionType::ManualTrigger => {
                        self.debug_log(&format!("Creating manual trigger context for {}", node.id));

                        // Create manual trigger context and start it
                        let mut context = Box::new(emission_contexts::ManualTriggerContext::new());
                        if let Err(e) = context.start(node.id.clone(), event_sender.clone()) {
                            self.debug_log(&format!(
                                "Failed to start manual trigger context: {}",
                                e
                            ));
                        } else {
                            // Store context in registry's listeners
                            registry
                                .listeners
                                .push(context as Box<dyn emission_contexts::EmissionContext>);
                        }
                    }
                    _ => {}
                }
            }
        }

        // Store registry (which holds the receiver internally)
        self.listener_registry = Some(registry);

        // Find nodes that have execution outputs but no execution inputs (start nodes)
        // These are the entry points for execution flow
        // IMPORTANT: Exclude self-emitting nodes (ManualTrigger, Timer, etc) as they
        // should only execute when their events fire, not on engine start
        let start_nodes: Vec<String> = self
            .graph
            .nodes
            .iter()
            .filter(|node| {
                if let Some(brick) = &node.data.brick {
                    let is_start_node =
                        !brick.execution_outputs.is_empty() && brick.execution_inputs.is_empty();
                    let is_self_emitting = !matches!(
                        brick.emission_type,
                        crate::bricks::types::BrickEmissionType::FlowTriggered
                    );
                    // Only include if it's a start node AND not self-emitting
                    is_start_node && !is_self_emitting
                } else {
                    false
                }
            })
            .map(|node| node.id.clone())
            .collect();

        // If no flow start nodes found, treat all nodes with no incoming edges as start nodes
        // BUT exclude self-emitting nodes (they should only execute on events)
        if start_nodes.is_empty() {
            let nodes_with_incoming: std::collections::HashSet<String> =
                self.graph.edges.iter().map(|e| e.target.clone()).collect();

            let nodes_to_queue: Vec<String> = self
                .graph
                .nodes
                .iter()
                .filter(|node| {
                    let has_no_incoming = !nodes_with_incoming.contains(&node.id);
                    let is_not_self_emitting = if let Some(brick) = &node.data.brick {
                        matches!(
                            brick.emission_type,
                            crate::bricks::types::BrickEmissionType::FlowTriggered
                        )
                    } else {
                        true // No brick = not self-emitting
                    };
                    has_no_incoming && is_not_self_emitting
                })
                .map(|node| node.id.clone())
                .collect();

            for node_id in nodes_to_queue {
                self.update_node_state(&node_id, ExecutionPhase::Queued, None);
                self.queue.push_back(node_id);
            }
        } else {
            // Queue all start nodes for execution
            for node_id in start_nodes {
                self.update_node_state(&node_id, ExecutionPhase::Queued, None);
                self.queue.push_back(node_id);
            }
        }
    }

    /// Manually enqueue a flow node for execution
    pub fn enqueue(&mut self, node_id: String) {
        self.update_node_state(&node_id, ExecutionPhase::Queued, None);
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
        // Mark node as running
        self.update_node_state(node_id, ExecutionPhase::Running, None);

        // Artificial delay for debugging; only enabled in debug builds
        if cfg!(debug_assertions) {
            // std::thread::sleep(std::time::Duration::from_millis(100));
        }

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
        let inputs = match self.build_inputs(node, brick) {
            Ok(inputs) => inputs,
            Err(e) => {
                // Mark as errored on input failure and set error message
                self.update_node_state(node_id, ExecutionPhase::Errored, None);
                if let Some(node_state) = self.node_states.get_mut(node_id) {
                    node_state.error_message = Some(e.clone());
                }
                trigger::clear_current_node_id();
                return Err(e);
            }
        };

        // Execute the brick
        let result = std::panic::catch_unwind(std::panic::AssertUnwindSafe(|| {
            (brick.execution)(arguments, inputs)
        }));

        match result {
            Ok(outputs) => {
                // Cache outputs and mark as completed
                self.cache.insert(node_id.to_string(), outputs.clone());
                self.update_node_state(node_id, ExecutionPhase::Completed, Some(outputs));
            }
            Err(_) => {
                // Mark as errored on execution failure and set error message
                self.update_node_state(node_id, ExecutionPhase::Errored, None);
                if let Some(node_state) = self.node_states.get_mut(node_id) {
                    node_state.error_message =
                        Some(format!("Node '{}' execution panicked", node_id));
                }
                trigger::clear_current_node_id();
                return Err(format!("Node '{}' execution panicked", node_id));
            }
        }

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
    fn find_connected_input_value(
        &self,
        target_node_id: &str,
        target_input_id: &str,
    ) -> Option<String> {
        for edge in &self.graph.edges {
            if edge.target == target_node_id && edge.target_handle == target_input_id {
                if let Some(cached_outputs) = self.cache.get(&edge.source) {
                    let source_output_id = edge.source_handle.as_str();

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
            .filter(|edge| edge.source == trigger.source_node && edge.source_handle == exec_handle)
            .map(|edge| edge.target.clone())
            .collect()
    }
}

impl<R: Runtime> Iterator for Engine<R> {
    type Item = Result<String, String>;

    fn next(&mut self) -> Option<Self::Item> {
        loop {
            // State 0: Check for events from self-emitting nodes
            if let Some(registry) = &mut self.listener_registry {
                if let Some(receiver) = registry.event_receiver() {
                    if let Ok(event) = receiver.try_recv() {
                        // Set execution context for the brick
                        let ctx = trigger::ExecutionContext::from_event(&event);
                        trigger::set_execution_context(ctx);

                        // Queue the node for execution
                        let node_id = event.target_node_id().to_string();
                        self.debug_log(&format!("Event received for node: {}", node_id));
                        self.update_node_state(&node_id, ExecutionPhase::Queued, None);
                        self.queue.push_back(node_id.clone());

                        // Don't return yet, let it go through normal flow processing
                    }
                }
            }

            // State 1: Process pending data dependencies
            if let Some(data_node_id) = self.pending_data_deps.pop_front() {
                return match self.execute_data_node(&data_node_id) {
                    Ok(_) => Some(Ok(data_node_id)),
                    Err(e) => Some(Err(e)),
                };
            }

            // State 2: Execute current flow node
            if let Some(flow_node_id) = self.current_flow_node.take() {
                if let Err(e) = self.execute_node_internal(&flow_node_id) {
                    return Some(Err(e));
                }

                // Collect triggers and queue next flow nodes
                let triggers = trigger::collect_and_clear_triggers();
                for trigger in &triggers {
                    let next_nodes = self.find_triggered_nodes(trigger);
                    for node_id in &next_nodes {
                        self.update_node_state(node_id, ExecutionPhase::Queued, None);
                    }
                    self.queue.extend(next_nodes);
                }

                return Some(Ok(flow_node_id));
            }

            // State 3: Start new flow node from queue
            if let Some(next_flow_node) = self.queue.pop_front() {
                // Resolve data dependencies for this flow node
                let data_deps = self.resolve_data_dependencies(&next_flow_node);

                // Queue data dependencies and set current flow node
                for dep_node in &data_deps {
                    self.update_node_state(dep_node, ExecutionPhase::Queued, None);
                }
                self.pending_data_deps.extend(data_deps);
                self.current_flow_node = Some(next_flow_node);
                continue;
            }

            // State 4: If listeners are active, keep polling for events
            if let Some(registry) = &self.listener_registry {
                if registry.has_active_listeners() {
                    // Small sleep to avoid busy-waiting
                    std::thread::sleep(std::time::Duration::from_millis(10));
                    // Yield control periodically to allow stop signal check
                    // Return empty node ID to signal "waiting for events"
                    return Some(Ok(String::new()));
                }
            }

            // No more work to do
            return None;
        }
    }
}

impl<R: Runtime> Drop for Engine<R> {
    fn drop(&mut self) {
        // Stop all listeners when engine is dropped
        if let Some(mut registry) = self.listener_registry.take() {
            let _ = registry.stop_all();
        }
    }
}

#[derive(Default, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct NodeExecutionState {
    pub phase: ExecutionPhase,
    #[serde(rename = "errorMessage")]
    pub error_message: Option<String>,
    #[serde(rename = "elapsedMs")]
    pub elapsed_ms: u32,
    pub outputs: Option<Vec<BrickOutputValue>>,
}

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub enum ExecutionPhase {
    Waiting,
    Queued,
    Running,
    Completed,
    Errored,
}

impl Default for ExecutionPhase {
    fn default() -> Self {
        Self::Waiting
    }
}
