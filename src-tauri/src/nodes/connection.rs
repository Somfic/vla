#[derive(serde::Serialize, serde::Deserialize, Debug)]
pub struct Connection {
    /// The id of the node connection
    id: String,

    /// The id of the source node instance
    source: String,

    /// The id of the source node output
    source_output: String,

    /// The id of the target node instance
    target: String,

    /// The id of the target node input
    target_input: String,
}
