use schema::vla_type;

/// Defines how a brick can be triggered for execution
#[vla_type]
#[serde(tag = "type", rename_all = "camelCase")]
pub enum BrickEmissionType {
    /// Traditional flow-based execution (triggered by other nodes)
    FlowTriggered,

    /// Self-emitting: HTTP webhook listener
    #[serde(rename_all = "camelCase")]
    HttpWebhook {
        default_path: String,
        default_method: String,
    },

    /// Self-emitting: Timer/interval
    #[serde(rename_all = "camelCase")]
    Timer { default_interval_ms: u32 },

    /// Self-emitting: File system watcher
    #[serde(rename_all = "camelCase")]
    FileWatcher { default_pattern: String },

    /// Self-emitting: Manual trigger from UI
    ManualTrigger,
}

impl Default for BrickEmissionType {
    fn default() -> Self {
        Self::FlowTriggered
    }
}

#[vla_type]
pub struct Brick {
    pub id: String,
    pub label: String,
    pub description: String,
    pub keywords: Vec<String>,
    pub category: String,
    pub arguments: Vec<BrickArgument>,
    pub inputs: Vec<BrickInput>,
    pub outputs: Vec<BrickOutput>,
    pub execution_inputs: Vec<BrickExecutionInput>,
    pub execution_outputs: Vec<BrickExecutionOutput>,
    #[serde(rename = "emissionType")]
    pub emission_type: BrickEmissionType,
    // Not serialized / not in the TS schema, and not read until the
    // execution engine is ported back in.
    #[serde(skip, default = "default_execution_fn")]
    #[ts(skip)]
    #[allow(dead_code)]
    pub execution: fn(Vec<BrickArgumentValue>, Vec<BrickInputValue>) -> Vec<BrickOutputValue>,
}

fn default_execution(
    _args: Vec<BrickArgumentValue>,
    _inputs: Vec<BrickInputValue>,
) -> Vec<BrickOutputValue> {
    vec![]
}

fn default_execution_fn(
) -> fn(Vec<BrickArgumentValue>, Vec<BrickInputValue>) -> Vec<BrickOutputValue> {
    default_execution
}

#[vla_type]
pub struct BrickInput {
    pub id: String,
    pub label: String,
    pub r#type: ConnectionType,
    #[serde(rename = "defaultValue")]
    pub default_value: Option<String>,
}

#[vla_type]
pub struct BrickOutput {
    pub id: String,
    pub label: String,
    pub r#type: ConnectionType,
}

#[vla_type]
pub struct BrickArgument {
    pub id: String,
    pub label: String,
    pub r#type: ArgumentType,
    #[serde(rename = "enumOptions")]
    pub enum_options: Option<Vec<String>>,
    #[serde(rename = "defaultValue")]
    pub default_value: Option<String>,
}

#[derive(PartialEq)]
#[vla_type]
pub enum ArgumentType {
    #[serde(rename = "string")]
    String,
    #[serde(rename = "number")]
    Number,
    #[serde(rename = "boolean")]
    Boolean,
    #[serde(rename = "enum")]
    Enum,
}

#[derive(PartialEq)]
#[vla_type]
pub enum ConnectionType {
    #[serde(rename = "flow")]
    Flow,
    #[serde(rename = "string")]
    String,
    #[serde(rename = "number")]
    Number,
    #[serde(rename = "boolean")]
    Boolean,
    #[serde(rename = "enum")]
    Enum,
}

#[vla_type]
pub struct BrickExecutionInput {
    pub id: String,
    pub label: String,
}

#[vla_type]
pub struct BrickExecutionOutput {
    pub id: String,
    pub label: String,
}

#[vla_type]
pub struct BrickArgumentValue {
    pub id: String,
    pub value: String,
}

#[vla_type]
pub struct BrickInputValue {
    pub id: String,
    pub value: String,
}

#[vla_type]
pub struct BrickOutputValue {
    pub id: String,
    pub value: String,
}
