use std::sync::{Arc, Mutex};

use tauri::{AppHandle, Manager, Runtime};

use crate::prelude::*;
use crate::{bricks, canvas, engine::Engine};

/// Shared state for the execution engine
pub struct AppState {
    pub engine: Arc<Mutex<Option<Engine>>>,
}

/// Result of executing a single step in the engine
#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct ExecutionStep {
    pub node_id: String,
    pub success: bool,
    pub error: Option<String>,
}

/// Result of executing the entire graph
#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct ExecutionResult {
    pub steps: Vec<ExecutionStep>,
    pub total_steps: u32,
    pub success: bool,
    pub error: Option<String>,
}

/// Current state of execution
#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct ExecutionState {
    pub is_initialized: bool,
    pub is_complete: bool,
    pub steps_executed: u32,
}

#[taurpc::procedures(export_to = "../frontend/src/lib/core.ts", event_trigger = ApiEventTrigger)]
pub trait CoreApi {
    #[taurpc(event)]
    async fn graph_updated(graph: Graph);

    async fn save_graph<R: Runtime>(
        app_handle: AppHandle<R>,
        graph: Graph,
        filename: String,
    ) -> Result<String, String>;
    async fn load_graph<R: Runtime>(
        app_handle: AppHandle<R>,
        filename: String,
    ) -> Result<Graph, String>;
    async fn get_brick<R: Runtime>(app_handle: AppHandle<R>, brick_id: String) -> Option<Brick>;
    async fn get_bricks<R: Runtime>(app_handle: AppHandle<R>) -> Vec<Brick>;
    async fn insert_node<R: Runtime>(
        app_handle: AppHandle<R>,
        graph_path: String,
        brick_id: String,
        position: Point,
    ) -> Result<Graph, String>;

    async fn execute_graph<R: Runtime>(
        app_handle: AppHandle<R>,
        graph: Graph,
    ) -> Result<ExecutionResult, String>;

    async fn start_execution<R: Runtime>(
        app_handle: AppHandle<R>,
        graph: Graph,
    ) -> Result<ExecutionState, String>;

    async fn step_execution<R: Runtime>(app_handle: AppHandle<R>) -> Result<ExecutionStep, String>;

    async fn get_execution_state<R: Runtime>(
        app_handle: AppHandle<R>,
    ) -> Result<ExecutionState, String>;

    async fn reset_execution<R: Runtime>(app_handle: AppHandle<R>) -> Result<(), String>;
}

#[derive(Clone)]
pub struct CoreApiImpl;

#[taurpc::resolvers]
impl CoreApi for CoreApiImpl {
    async fn save_graph<R: Runtime>(
        self,
        app_handle: AppHandle<R>,
        graph: Graph,
        graph_path: String,
    ) -> Result<String, String> {
        canvas::save_graph(app_handle, &graph, &graph_path, false).await
    }

    async fn load_graph<R: Runtime>(
        self,
        app_handle: AppHandle<R>,
        graph_path: String,
    ) -> Result<Graph, String> {
        canvas::load_graph(app_handle, &graph_path).await
    }

    async fn get_brick<R: Runtime>(
        self,
        app_handle: AppHandle<R>,
        brick_id: String,
    ) -> Option<bricks::types::Brick> {
        canvas::get_brick(&brick_id)
    }

    async fn get_bricks<R: Runtime>(self, app_handle: AppHandle<R>) -> Vec<bricks::types::Brick> {
        bricks::all_bricks()
    }

    async fn insert_node<R: Runtime>(
        self,
        app_handle: AppHandle<R>,
        graph_path: String,
        brick_id: String,
        position: Point,
    ) -> Result<Graph, String> {
        canvas::insert_node(app_handle, &graph_path, &brick_id, position).await
    }

    async fn execute_graph<R: Runtime>(
        self,
        app_handle: AppHandle<R>,
        mut graph: Graph,
    ) -> Result<ExecutionResult, String> {
        // Re-attach brick definitions (execution functions are skipped during serialization)
        for node in &mut graph.nodes {
            node.data.brick = canvas::get_brick(&node.data.brick_id);
        }

        // Create engine with the graph
        let mut engine = Engine::with_debug(graph, true);
        engine.start();

        let mut steps = Vec::new();
        let mut total_steps: u32 = 0;
        let mut execution_error = None;

        // Execute all steps
        for result in engine {
            total_steps = total_steps.saturating_add(1);
            match result {
                Ok(node_id) => {
                    steps.push(ExecutionStep {
                        node_id,
                        success: true,
                        error: None,
                    });
                }
                Err(e) => {
                    // Create an error step
                    steps.push(ExecutionStep {
                        node_id: String::from("unknown"),
                        success: false,
                        error: Some(e.clone()),
                    });
                    execution_error = Some(e);
                    break;
                }
            }
        }

        Ok(ExecutionResult {
            steps,
            total_steps,
            success: execution_error.is_none(),
            error: execution_error,
        })
    }

    async fn start_execution<R: Runtime>(
        self,
        app_handle: AppHandle<R>,
        mut graph: Graph,
    ) -> Result<ExecutionState, String> {
        let state = app_handle.state::<AppState>();

        // Re-attach brick definitions (execution functions are skipped during serialization)
        for node in &mut graph.nodes {
            node.data.brick = canvas::get_brick(&node.data.brick_id);
        }

        // Create new engine and start it
        let mut engine = Engine::new(graph);
        engine.start();

        // Store in state
        *state.engine.lock().unwrap() = Some(engine);

        Ok(ExecutionState {
            is_initialized: true,
            is_complete: false,
            steps_executed: 0,
        })
    }

    async fn step_execution<R: Runtime>(
        self,
        app_handle: AppHandle<R>,
    ) -> Result<ExecutionStep, String> {
        let state = app_handle.state::<AppState>();
        let mut engine_guard = state.engine.lock().unwrap();

        if let Some(engine) = engine_guard.as_mut() {
            // Execute one step
            match engine.next() {
                Some(Ok(node_id)) => Ok(ExecutionStep {
                    node_id,
                    success: true,
                    error: None,
                }),
                Some(Err(e)) => Ok(ExecutionStep {
                    node_id: String::from("unknown"),
                    success: false,
                    error: Some(e),
                }),
                None => {
                    // Execution complete
                    *engine_guard = None;
                    Err("Execution complete".to_string())
                }
            }
        } else {
            Err("No execution in progress. Call start_execution first.".to_string())
        }
    }

    async fn get_execution_state<R: Runtime>(
        self,
        app_handle: AppHandle<R>,
    ) -> Result<ExecutionState, String> {
        let state = app_handle.state::<AppState>();
        let engine_guard = state.engine.lock().unwrap();

        Ok(ExecutionState {
            is_initialized: engine_guard.is_some(),
            is_complete: engine_guard.is_none(),
            steps_executed: 0, // We could track this if needed
        })
    }

    async fn reset_execution<R: Runtime>(self, app_handle: AppHandle<R>) -> Result<(), String> {
        let state = app_handle.state::<AppState>();
        *state.engine.lock().unwrap() = None;
        Ok(())
    }
}
