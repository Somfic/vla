#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct Brick {
    pub id: String,
    pub label: String,
    pub description: String,
    pub arguments: Vec<BrickArgument>,
    pub inputs: Vec<BrickInput>,
    pub outputs: Vec<BrickOutput>,
    #[serde(skip, default = "default_execution_fn")]
    #[specta(skip)]
    pub execution: fn(Vec<BrickArgument>, Vec<BrickInput>) -> Vec<BrickOutput>,
}

fn default_execution(_args: Vec<BrickArgument>, _inputs: Vec<BrickInput>) -> Vec<BrickOutput> {
    vec![]
}

fn default_execution_fn() -> fn(Vec<BrickArgument>, Vec<BrickInput>) -> Vec<BrickOutput> {
    default_execution
}

impl Default for Brick {
    fn default() -> Self {
        fn default_execution(
            _args: Vec<BrickArgument>,
            _inputs: Vec<BrickInput>,
        ) -> Vec<BrickOutput> {
            vec![]
        }

        Brick {
            id: "".to_string(),
            label: "".to_string(),
            description: "".to_string(),
            arguments: vec![],
            inputs: vec![],
            outputs: vec![],
            execution: default_execution,
        }
    }
}

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickInput {
    pub id: String,
    pub label: String,
    pub r#type: BrickHandleType,
    pub default_value: Option<String>,
}

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickOutput {
    pub id: String,
    pub label: String,
    pub r#type: BrickHandleType,
}

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub struct BrickArgument {
    pub id: String,
    pub label: String,
    pub r#type: BrickArgumentType,
    pub enum_options: Option<Vec<String>>,
    pub default_value: Option<String>,
}

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub enum BrickArgumentType {
    String,
    Number,
    Boolean,
    Enum,
}

#[derive(Clone, serde::Serialize, serde::Deserialize, specta::Type)]
pub enum BrickHandleType {
    String,
    Number,
    Boolean,
    Enum,
}
