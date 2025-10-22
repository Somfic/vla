# Emission Types & Execution Contexts Guide

This guide explains how the VLA execution engine handles different types of node triggers (emission types) and how to implement custom emission types like speech detection, database triggers, or any other event-driven node.

## Table of Contents

1. [Core Concepts](#core-concepts)
2. [Architecture Overview](#architecture-overview)
3. [Implementing a Custom Emission Type](#implementing-a-custom-emission-type)
4. [Example: Speech Detection Node](#example-speech-detection-node)
5. [Testing Your Emission Type](#testing-your-emission-type)

---

## Core Concepts

### What is an Emission Type?

An **emission type** defines how a node can be triggered for execution:

- **FlowTriggered**: Traditional flow-based execution (triggered by other nodes via execution edges)
- **Timer**: Self-emitting node that triggers at regular intervals
- **ManualTrigger**: Self-emitting node that triggers when user clicks a button
- **HttpWebhook**: Self-emitting node that triggers on incoming HTTP requests
- **FileWatcher**: Self-emitting node that triggers on file system changes
- **Custom types**: Speech detection, database triggers, MQTT messages, etc.

### Two Key Components

#### 1. ExecutionContext (core/src/engine/trigger.rs)

**Purpose**: Provides bricks access to event-specific data during execution.

**Location**: Thread-local storage, available via `get_execution_context()`

**Example**:
```rust
pub enum ExecutionContext {
    FlowTriggered,
    TimerTick { tick_count: u64, timestamp: String },
    ManualTrigger { timestamp: String },
    HttpRequest(HttpRequestData),
    FileChanged { path: String, event_type: FileEventType },
    // Your custom type here
}
```

**Why separate contexts?** Each emission type carries different data:
- Timer: tick count + timestamp
- HTTP webhook: method, path, body, headers
- File watcher: file path + event type
- Speech detection: transcript, confidence, speaker

#### 2. EmissionContext (core/src/engine/emission_contexts.rs)

**Purpose**: Manages the lifecycle of self-emitting nodes (background threads, listeners, cleanup).

**Trait definition**:
```rust
pub trait EmissionContext: Send {
    /// Start the context (spawn threads, set up listeners, etc.)
    fn start(&mut self, node_id: String, event_sender: Sender<ExecutionEvent>)
        -> Result<(), String>;

    /// Stop the context (cleanup, join threads, etc.)
    fn stop(&mut self) -> Result<(), String>;

    /// Check if the context is active
    fn is_active(&self) -> bool;

    /// Get context type name for debugging
    fn context_type(&self) -> &'static str;
}
```

**Examples**:
- `TimerContext`: Spawns thread that sleeps and sends TimerTick events
- `ManualTriggerContext`: Stores sender, waits for external trigger call
- `HttpWebhookContext`: Spawns HTTP server listening for webhooks

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Engine Start                                             │
│    - Creates event channel (sender + receiver)              │
│    - Scans graph for self-emitting nodes                    │
│    - Creates EmissionContext for each                       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. EmissionContext.start()                                  │
│    - Spawns background thread/listener                      │
│    - Stores event_sender                                    │
│    - Begins monitoring for events                           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. Event Detected                                           │
│    - Background thread detects event (timer tick, speech,   │
│      HTTP request, etc.)                                    │
│    - Creates ExecutionEvent and sends via event_sender      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. Engine Iterator (mod.rs:536-554)                         │
│    - Polls event_receiver via try_recv()                    │
│    - Converts ExecutionEvent → ExecutionContext             │
│    - Stores in thread-local storage                         │
│    - Queues node for execution                              │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. Brick Execution                                          │
│    - Brick calls get_execution_context()                    │
│    - Extracts event-specific data                           │
│    - Produces outputs based on context                      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ 6. Engine Stop                                              │
│    - Calls EmissionContext.stop() for all contexts          │
│    - Joins background threads                               │
│    - Cleans up resources                                    │
└─────────────────────────────────────────────────────────────┘
```

---

## Implementing a Custom Emission Type

Let's walk through implementing a **Speech Detection** node that triggers when speech is detected and provides the transcript.

### Step 1: Add ExecutionEvent Variant

**File**: `core/src/engine/events.rs`

Add your event variant to the `ExecutionEvent` enum:

```rust
#[derive(Debug, Clone, Serialize, Deserialize, specta::Type)]
pub enum ExecutionEvent {
    // ... existing variants

    /// Speech detected and transcribed
    SpeechDetected {
        node_id: String,
        transcript: String,
        confidence: f32,
        speaker_id: Option<String>,
        timestamp: String,
    },
}
```

Update the `target_node_id()` method:

```rust
impl ExecutionEvent {
    pub fn target_node_id(&self) -> &str {
        match self {
            // ... existing matches
            ExecutionEvent::SpeechDetected { node_id, .. } => node_id,
        }
    }
}
```

### Step 2: Add ExecutionContext Variant

**File**: `core/src/engine/trigger.rs`

Add your context variant:

```rust
#[derive(Debug, Clone)]
pub enum ExecutionContext {
    // ... existing variants

    SpeechDetected {
        transcript: String,
        confidence: f32,
        speaker_id: Option<String>,
        timestamp: String,
    },
}
```

Update `from_event()`:

```rust
impl ExecutionContext {
    pub fn from_event(event: &ExecutionEvent) -> Self {
        match event {
            // ... existing matches

            ExecutionEvent::SpeechDetected {
                transcript,
                confidence,
                speaker_id,
                timestamp,
                ..
            } => ExecutionContext::SpeechDetected {
                transcript: transcript.clone(),
                confidence: *confidence,
                speaker_id: speaker_id.clone(),
                timestamp: timestamp.clone(),
            },
        }
    }
}
```

Add a helper method for bricks to access the data:

```rust
impl ExecutionContext {
    // ... existing helper methods

    /// Get speech detection data (if this is a speech event)
    pub fn speech_data(&self) -> Option<(&str, f32, Option<&str>, &str)> {
        match self {
            ExecutionContext::SpeechDetected {
                transcript,
                confidence,
                speaker_id,
                timestamp,
            } => Some((
                transcript.as_str(),
                *confidence,
                speaker_id.as_deref(),
                timestamp.as_str(),
            )),
            _ => None,
        }
    }
}
```

### Step 3: Add BrickEmissionType

**File**: `core/src/bricks/types.rs`

```rust
#[derive(Debug, Clone, Serialize, Deserialize, specta::Type, PartialEq)]
pub enum BrickEmissionType {
    // ... existing variants

    SpeechDetection {
        default_language: String,
        min_confidence: f32,
    },
}
```

### Step 4: Add Macro Support

**File**: `core/src/bricks/macros.rs`

Find the `@get_emission_type` macro rules and add:

```rust
(@get_emission_type SpeechDetection {
    default_language: $lang:expr,
    min_confidence: $conf:expr
}) => {
    crate::bricks::types::BrickEmissionType::SpeechDetection {
        default_language: $lang.to_string(),
        min_confidence: $conf,
    }
};
```

### Step 5: Implement EmissionContext

**File**: `core/src/engine/emission_contexts.rs`

```rust
use std::sync::{Arc, Mutex};
use std::thread;
use std::time::Duration;

/// Speech detection emission context
pub struct SpeechDetectionContext {
    language: String,
    min_confidence: f32,
    active: Arc<Mutex<bool>>,
    thread_handle: Option<thread::JoinHandle<()>>,
}

impl SpeechDetectionContext {
    pub fn new(language: String, min_confidence: f32) -> Self {
        Self {
            language,
            min_confidence,
            active: Arc::new(Mutex::new(false)),
            thread_handle: None,
        }
    }
}

impl EmissionContext for SpeechDetectionContext {
    fn start(
        &mut self,
        node_id: String,
        event_sender: Sender<ExecutionEvent>,
    ) -> Result<(), String> {
        if *self.active.lock().unwrap() {
            return Err("Speech detection context already active".to_string());
        }

        *self.active.lock().unwrap() = true;

        let active = Arc::clone(&self.active);
        let language = self.language.clone();
        let min_confidence = self.min_confidence;

        let handle = thread::spawn(move || {
            // TODO: Initialize your speech recognition library here
            // Examples: whisper.cpp, vosk, google-cloud-speech, etc.
            // let mut recognizer = SpeechRecognizer::new(language);

            while *active.lock().unwrap() {
                // TODO: Poll for speech input (blocking or with timeout)
                // This could be:
                // - Reading from microphone buffer
                // - Polling a speech API
                // - Waiting on a callback/channel

                // Example: simulate detection for testing
                thread::sleep(Duration::from_secs(5));

                // When speech is detected:
                // if let Some(result) = recognizer.recognize() {
                //     if result.confidence >= min_confidence {
                let timestamp = {
                    use std::time::{SystemTime, UNIX_EPOCH};
                    let duration = SystemTime::now()
                        .duration_since(UNIX_EPOCH)
                        .unwrap();
                    format!("{}.{:03}", duration.as_secs(), duration.subsec_millis())
                };

                let event = ExecutionEvent::SpeechDetected {
                    node_id: node_id.clone(),
                    transcript: "Hello world".to_string(), // result.transcript
                    confidence: 0.95,                       // result.confidence
                    speaker_id: None,                       // result.speaker_id
                    timestamp,
                };

                if event_sender.send(event).is_err() {
                    // Receiver dropped, stop the context
                    break;
                }
                //     }
                // }
            }

            // TODO: Cleanup speech recognition resources
        });

        self.thread_handle = Some(handle);
        Ok(())
    }

    fn stop(&mut self) -> Result<(), String> {
        if !*self.active.lock().unwrap() {
            return Ok(());
        }

        *self.active.lock().unwrap() = false;

        if let Some(handle) = self.thread_handle.take() {
            handle
                .join()
                .map_err(|_| "Failed to join speech detection thread".to_string())?;
        }

        Ok(())
    }

    fn is_active(&self) -> bool {
        *self.active.lock().unwrap()
    }

    fn context_type(&self) -> &'static str {
        "SpeechDetection"
    }
}

impl Drop for SpeechDetectionContext {
    fn drop(&mut self) {
        let _ = self.stop();
    }
}
```

### Step 6: Register Context in Engine

**File**: `core/src/engine/mod.rs`

Find the `start_with_event_channel` method (around line 259) and add your emission type to the match:

```rust
// Scan for self-emitting nodes and create listeners
for node in &self.graph.nodes {
    if let Some(brick) = &node.data.brick {
        match &brick.emission_type {
            crate::bricks::types::BrickEmissionType::Timer { default_interval_ms } => {
                // ... existing timer code
            }
            crate::bricks::types::BrickEmissionType::ManualTrigger => {
                // ... existing manual trigger code
            }
            crate::bricks::types::BrickEmissionType::SpeechDetection {
                default_language,
                min_confidence,
            } => {
                // Get language from node arguments or use default
                let language = node
                    .data
                    .arguments
                    .get("language")
                    .map(|v| v.trim_matches('"').to_string())
                    .unwrap_or_else(|| default_language.clone());

                // Get min confidence from node arguments or use default
                let min_conf = node
                    .data
                    .arguments
                    .get("min_confidence")
                    .and_then(|v| v.trim_matches('"').parse::<f32>().ok())
                    .unwrap_or(*min_confidence);

                self.debug_log(&format!(
                    "Creating speech detection context for {} (lang: {}, min_conf: {})",
                    node.id, language, min_conf
                ));

                // Create speech detection context and start it
                let mut context = Box::new(emission_contexts::SpeechDetectionContext::new(
                    language, min_conf,
                ));
                if let Err(e) = context.start(node.id.clone(), event_sender.clone()) {
                    self.debug_log(&format!("Failed to start speech detection context: {}", e));
                } else {
                    // Store context in registry's listeners
                    registry.listeners.push(context as Box<dyn emission_contexts::EmissionContext>);
                }
            }
            _ => {}
        }
    }
}
```

### Step 7: Create the Brick

**File**: `core/src/bricks/speech.rs` (new file)

```rust
use crate::prelude::*;

brick! {
    #[id("speech_detection")]
    #[label("Speech Detection")]
    #[description("Detects speech and transcribes it in real-time")]
    #[keywords("speech", "voice", "audio", "transcribe", "stt")]
    #[category("input")]

    #[emission_type(SpeechDetection {
        default_language: "en-US",
        min_confidence: 0.7
    })]

    // Arguments allow users to customize behavior
    #[argument] #[label("Language")] language: String = "en-US"
    #[argument] #[label("Min Confidence")] min_confidence: String = "0.7"

    // Execution output - fires when speech is detected
    #[execution_output] #[label("On Speech")] detected

    // Data outputs - available to connected nodes
    #[output] #[label("Transcript")] transcript: String
    #[output] #[label("Confidence")] confidence: String
    #[output] #[label("Speaker ID")] speaker: String
    #[output] #[label("Timestamp")] timestamp: String

    fn execute() -> (String, String, String, String) {
        let ctx = crate::engine::trigger::get_execution_context();

        if let Some((transcript, conf, speaker_id, ts)) = ctx.speech_data() {
            // Trigger the execution output
            trigger!(detected);

            (
                transcript.to_string(),
                conf.to_string(),
                speaker_id.unwrap_or("unknown").to_string(),
                ts.to_string(),
            )
        } else {
            // Should not happen, but provide defaults
            ("".to_string(), "0.0".to_string(), "unknown".to_string(), "".to_string())
        }
    }
}
```

### Step 8: Register the Brick

**File**: `core/src/bricks/mod.rs`

```rust
pub mod arithmetics;
pub mod boolean_logic;
pub mod constants;
pub mod control_flow;
pub mod debug;
pub mod events;
pub mod speech;  // Add this line
// ... other modules

pub fn all_bricks() -> Vec<Brick> {
    vec![
        // ... existing bricks
        speech::speech_detection(),  // Add this line
    ]
}
```

---

## Example: Speech Detection Node

Once implemented, your speech detection node will:

1. **On engine start**: Create a background thread that monitors audio input
2. **When speech detected**:
   - Transcribe the audio
   - Check if confidence meets threshold
   - Send `ExecutionEvent::SpeechDetected` through channel
3. **Engine receives event**:
   - Converts to `ExecutionContext::SpeechDetected`
   - Queues the speech_detection node for execution
4. **Brick executes**:
   - Reads transcript, confidence, speaker from context
   - Outputs the data
   - Triggers the `detected` execution output
5. **Connected nodes execute**: Use the transcript data for further processing

### Example Graph

```
[Speech Detection] --transcript--> [Sentiment Analysis]
         |
         +--detected--> [Print]
```

When you speak "Hello world":
1. Speech Detection triggers
2. Sentiment Analysis receives "Hello world"
3. Print executes and shows the result

---

## Testing Your Emission Type

### Unit Tests

Add tests to `core/src/engine/emission_contexts.rs`:

```rust
#[cfg(test)]
mod tests {
    use super::*;
    use std::sync::mpsc;
    use std::time::Duration;

    #[test]
    fn test_speech_detection_context() {
        let (sender, receiver) = mpsc::channel();
        let mut context = SpeechDetectionContext::new("en-US".to_string(), 0.7);

        context.start("test_node".to_string(), sender).unwrap();
        assert!(context.is_active());

        // Wait for simulated speech detection
        std::thread::sleep(Duration::from_secs(6));

        // Check we received event
        match receiver.try_recv() {
            Ok(ExecutionEvent::SpeechDetected { node_id, transcript, .. }) => {
                assert_eq!(node_id, "test_node");
                assert!(!transcript.is_empty());
            }
            _ => panic!("Expected SpeechDetected event"),
        }

        context.stop().unwrap();
        assert!(!context.is_active());
    }
}
```

### Integration Test

Create `core/examples/test_speech_detection.rs`:

```rust
use vla_lib::prelude::*;
use vla_lib::engine::Engine;
use vla_lib::canvas::Graph;

fn main() {
    // Load or create a graph with speech detection node
    let graph_json = r#"{
        "nodes": [
            {
                "id": "speech_node",
                "data": {
                    "brickId": "speech_detection",
                    "arguments": {
                        "language": "en-US",
                        "min_confidence": "0.7"
                    }
                },
                "position": { "x": 0, "y": 0 }
            },
            {
                "id": "print_node",
                "data": {
                    "brickId": "print"
                },
                "position": { "x": 200, "y": 0 }
            }
        ],
        "edges": [
            {
                "id": "edge1",
                "source": "speech_node",
                "target": "print_node",
                "sourceHandle": "detected",
                "targetHandle": "execute"
            },
            {
                "id": "edge2",
                "source": "speech_node",
                "target": "print_node",
                "sourceHandle": "transcript",
                "targetHandle": "value"
            }
        ]
    }"#;

    let mut graph = Graph::from_json(graph_json.to_string()).unwrap();
    let mut engine = Engine::new(graph);
    engine.start();

    println!("Speech detection active. Speak into microphone...");

    // Run engine and print results
    for result in engine {
        match result {
            Ok(node_id) => println!("Executed node: {}", node_id),
            Err(e) => eprintln!("Error: {}", e),
        }
    }
}
```

---

## Tips & Best Practices

### 1. Resource Management
- Always implement `Drop` to ensure cleanup
- Join threads in `stop()` to prevent resource leaks
- Use `Arc<Mutex<bool>>` for thread-safe active flag

### 2. Error Handling
- Return descriptive errors from `start()` and `stop()`
- Check if `event_sender.send()` fails (receiver dropped)
- Handle initialization failures gracefully

### 3. Performance
- Use appropriate sleep/poll intervals
- Avoid busy-waiting
- Consider using async/await for I/O-bound operations

### 4. Configuration
- Use brick arguments for user-configurable settings
- Provide sensible defaults in `BrickEmissionType`
- Validate argument values in `start()`

### 5. Testing
- Test context lifecycle (start/stop/is_active)
- Verify events are sent correctly
- Test with invalid configurations
- Add integration tests with real graphs

### 6. Documentation
- Document what data your context provides
- Explain when the node triggers
- Provide example use cases
- Add inline code comments for complex logic

---

## Common Emission Type Patterns

### Polling Pattern (File Watcher, API Polling)
```rust
while *active.lock().unwrap() {
    if let Some(change) = check_for_changes() {
        send_event(change);
    }
    thread::sleep(poll_interval);
}
```

### Callback Pattern (Webhooks, Events)
```rust
let server = setup_server(port);
server.on_request(move |req| {
    send_event(req);
});
while *active.lock().unwrap() {
    thread::sleep(Duration::from_millis(100));
}
```

### Blocking Pattern (Message Queues, Sockets)
```rust
while *active.lock().unwrap() {
    match receiver.recv_timeout(Duration::from_secs(1)) {
        Ok(message) => send_event(message),
        Err(RecvTimeoutError::Timeout) => continue,
        Err(_) => break,
    }
}
```

---

## FAQ

**Q: Do I need both ExecutionContext and EmissionContext?**

A: Yes. EmissionContext manages the background work (spawning threads, listening for events), while ExecutionContext provides data to the brick during execution.

**Q: Can I have multiple instances of the same emission type?**

A: Yes! Each node instance gets its own EmissionContext. For example, you can have multiple timer nodes with different intervals.

**Q: What if my emission type doesn't need a background thread?**

A: You still implement EmissionContext, but `start()` can just store the sender and set active=true, like ManualTriggerContext does.

**Q: How do I add frontend UI for my emission type?**

A: Check `frontend/src/components/canvas/Node.svelte` for examples like the manual trigger button. You can conditionally render UI based on `node.data.brick.id` or `brick.emission_type`.

**Q: Can emission types communicate with each other?**

A: Not directly. They send events through the channel independently. The engine processes all events in order through the same iterator loop.

---

## Additional Resources

- **Example Implementations**:
  - Timer: `core/src/engine/emission_contexts.rs:23-104`
  - Manual Trigger: `core/src/engine/emission_contexts.rs:112-186`
  - Timer Brick: `core/src/bricks/events.rs:39-68`

- **Related Files**:
  - `core/src/engine/events.rs` - Event definitions
  - `core/src/engine/trigger.rs` - Execution contexts
  - `core/src/engine/mod.rs` - Engine iterator and context registration
  - `core/src/bricks/macros.rs` - Brick DSL macros

- **Tests**:
  - `core/src/engine/emission_contexts.rs` - Unit tests
  - `core/examples/extensible_emission_contexts.rs` - Integration example
