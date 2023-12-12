use super::web::Web;

#[derive(serde::Serialize, serde::Deserialize, Debug)]
pub struct Workspace {
    /// The name of the workspace
    name: String,

    /// The path to the workspace
    path: String,

    /// The date and time the workspace was created
    created: chrono::DateTime<chrono::Utc>,

    /// The date and time the workspace was last modified
    last_modified: chrono::DateTime<chrono::Utc>,

    /// The accent color of the workspace
    color: String,

    /// A collection of the webs in the workspace
    webs: Vec<Web>,
}

impl Workspace {
    pub fn load(path: impl Into<String>) -> Result<Self, Box<dyn std::error::Error>> {
        let workspace_file = std::fs::read_to_string(path.into())?;
        let workspace: Self = serde_json::from_str(&workspace_file)?;
        Ok(workspace)
    }
}
