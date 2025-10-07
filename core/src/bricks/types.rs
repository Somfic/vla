#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
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
    #[serde(skip, default = "default_execution_fn")]
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

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickInput {
    pub id: String,
    pub label: String,
    pub r#type: ConnectionType,
    #[serde(rename = "defaultValue")]
    pub default_value: Option<String>,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickOutput {
    pub id: String,
    pub label: String,
    pub r#type: ConnectionType,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickArgument {
    pub id: String,
    pub label: String,
    pub r#type: ArgumentType,
    #[serde(rename = "enumOptions")]
    pub enum_options: Option<Vec<String>>,
    #[serde(rename = "defaultValue")]
    pub default_value: Option<String>,
}

#[derive(Debug, Clone, PartialEq, serde::Serialize, serde::Deserialize, specta::Type)]
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

#[derive(Debug, Clone, PartialEq, serde::Serialize, serde::Deserialize, specta::Type)]
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

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickExecutionInput {
    pub id: String,
    pub label: String,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickExecutionOutput {
    pub id: String,
    pub label: String,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickArgumentValue {
    pub id: String,
    pub value: String,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickInputValue {
    pub id: String,
    pub value: String,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickOutputValue {
    pub id: String,
    pub value: String,
}
