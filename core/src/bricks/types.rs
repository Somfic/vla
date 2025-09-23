#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct Brick {
    pub id: String,
    pub label: String,
    pub description: String,
    pub arguments: Vec<BrickArgument>,
    pub inputs: Vec<BrickInput>,
    pub outputs: Vec<BrickOutput>,
    #[serde(skip, default = "default_execution_fn")]
    pub execution: fn(Vec<BrickArgument>, Vec<BrickInput>) -> Vec<BrickOutput>,
}

fn default_execution(_args: Vec<BrickArgument>, _inputs: Vec<BrickInput>) -> Vec<BrickOutput> {
    vec![]
}

fn default_execution_fn() -> fn(Vec<BrickArgument>, Vec<BrickInput>) -> Vec<BrickOutput> {
    default_execution
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickInput {
    pub id: String,
    pub label: String,
    pub r#type: BrickHandleType,
    pub default_value: Option<String>,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickOutput {
    pub id: String,
    pub label: String,
    pub r#type: BrickHandleType,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickArgument {
    pub id: String,
    pub label: String,
    pub r#type: BrickArgumentType,
    pub enum_options: Option<Vec<String>>,
    pub default_value: Option<String>,
}

#[derive(Debug, Clone, PartialEq, serde::Serialize, serde::Deserialize, specta::Type)]
pub enum BrickArgumentType {
    String,
    Number,
    Boolean,
    Enum,
}

#[derive(Debug, Clone, PartialEq, serde::Serialize, serde::Deserialize, specta::Type)]
pub enum BrickHandleType {
    String,
    Number,
    Boolean,
    Enum,
}
