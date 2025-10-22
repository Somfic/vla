use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::Arc;
use std::sync::OnceLock;
use tauri::{AppHandle, Runtime};
use tokio::sync::Mutex as TokioMutex;

use crate::prelude::*;
use crate::{bricks, canvas, engine::Engine};

use crate::engine::events::ExecutionEvent;
use std::sync::mpsc;

/// Global running engine handle
static ENGINE_HANDLE: OnceLock<Arc<TokioMutex<Option<tokio::task::JoinHandle<()>>>>> =
    OnceLock::new();

/// Global stop signal for the running engine (atomic for sync access from iterator)
static STOP_ENGINE: OnceLock<Arc<AtomicBool>> = OnceLock::new();

/// Global event sender for manual triggers
static EVENT_SENDER: OnceLock<Arc<TokioMutex<Option<mpsc::Sender<ExecutionEvent>>>>> =
    OnceLock::new();

fn get_engine_handle() -> Arc<TokioMutex<Option<tokio::task::JoinHandle<()>>>> {
    Arc::clone(ENGINE_HANDLE.get_or_init(|| Arc::new(TokioMutex::new(None))))
}

fn get_stop_signal() -> Arc<AtomicBool> {
    Arc::clone(STOP_ENGINE.get_or_init(|| Arc::new(AtomicBool::new(false))))
}

fn get_event_sender() -> Arc<TokioMutex<Option<mpsc::Sender<ExecutionEvent>>>> {
    Arc::clone(EVENT_SENDER.get_or_init(|| Arc::new(TokioMutex::new(None))))
}

/// Result of executing the entire graph
#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct ExecutionResult {
    pub total_nodes: u32,
    pub success: bool,
    pub error: Option<String>,
}

#[taurpc::procedures(export_to = "../frontend/src/lib/core.ts", event_trigger = ApiEventTrigger)]
pub trait CoreApi {
    #[taurpc(event)]
    async fn graph_updated(graph: Graph);

    #[taurpc(event)]
    async fn node_execution_updated(update: crate::engine::ExecutionStateUpdate);

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
        mode: crate::engine::ExecutionMode,
    ) -> Result<ExecutionResult, String>;

    async fn trigger_manual_node<R: Runtime>(
        app_handle: AppHandle<R>,
        node_id: String,
    ) -> Result<(), String>;
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
        mode: crate::engine::ExecutionMode,
    ) -> Result<ExecutionResult, String> {
        // Signal any existing engine to stop
        let stop_signal = get_stop_signal();
        stop_signal.store(true, Ordering::SeqCst);

        // Get the engine handle
        let engine_handle = get_engine_handle();
        let mut handle_guard = engine_handle.lock().await;

        // Wait for old engine to stop
        if let Some(old_handle) = handle_guard.take() {
            // Abort the old task
            old_handle.abort();
            // Wait for it to finish
            let _ = old_handle.await;
        }

        // Reset stop signal for new engine
        stop_signal.store(false, Ordering::SeqCst);

        // Re-attach brick definitions (execution functions are skipped during serialization)
        for node in &mut graph.nodes {
            node.data.brick = canvas::get_brick(&node.data.brick_id);
        }

        let total_nodes = graph.nodes.len() as u32;

        // Clone stop signal for the background task
        let stop_signal_clone = Arc::clone(&stop_signal);

        // Create event channel for manual triggers
        let (event_tx, event_rx) = std::sync::mpsc::channel();

        // Store the sender globally so trigger_manual_node can use it
        {
            let sender_guard = get_event_sender();
            *sender_guard.lock().await = Some(event_tx.clone());
        }

        // Spawn engine in background task
        let handle = tokio::task::spawn_blocking(move || {
            let mut engine = Engine::with_app_handle(graph, app_handle);
            engine.set_execution_mode(mode);
            engine.start_with_event_channel(event_rx, event_tx);

            // Execute all steps (events are automatically broadcast via engine)
            for result in engine {
                // Check stop signal
                if stop_signal_clone.load(Ordering::SeqCst) {
                    break;
                }

                if let Err(e) = result {
                    eprintln!("Engine error: {}", e);
                    break;
                }
            }
            // Engine dropped here, which stops all listeners
        });

        // Store the new handle
        *handle_guard = Some(handle);
        drop(handle_guard);

        // Return immediately
        Ok(ExecutionResult {
            total_nodes,
            success: true,
            error: None,
        })
    }

    async fn trigger_manual_node<R: Runtime>(
        self,
        _app_handle: AppHandle<R>,
        node_id: String,
    ) -> Result<(), String> {
        // Get the event sender
        let sender_guard = get_event_sender();
        let sender_opt = sender_guard.lock().await;

        if let Some(sender) = sender_opt.as_ref() {
            // Create timestamp
            let timestamp = {
                use std::time::{SystemTime, UNIX_EPOCH};
                let duration = SystemTime::now().duration_since(UNIX_EPOCH).unwrap();
                format!("{}.{:03}", duration.as_secs(), duration.subsec_millis())
            };

            // Create manual trigger event
            let event = ExecutionEvent::ManualTrigger {
                node_id: node_id.clone(),
                timestamp: timestamp.clone(),
            };

            // Send the event
            sender
                .send(event)
                .map_err(|e| format!("Failed to send manual trigger event: {}", e))?;

            Ok(())
        } else {
            Err("Engine not running".to_string())
        }
    }
}
