use schemars::JsonSchema;
use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, Clone, JsonSchema)]
#[serde(rename_all = "lowercase")]
pub(crate) enum Level {
    Success,
    Error,
    Warning,
    Information,
}

#[derive(Serialize, Deserialize, Clone, JsonSchema)]
pub(crate) struct Notification {
    title: String,
    body: Option<String>,
    level: Level,
    call_to_action: Option<String>,
    call_to_action_secondary: Option<String>,
}
