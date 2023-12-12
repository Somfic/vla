#[derive(serde::Serialize, serde::Deserialize, Debug)]
pub struct NodeInput {
    /// The id of the node input
    id: String,

    /// The name of the node input
    name: String,

    /// The type of the node input
    #[serde(rename = "type")]
    type_: String,

    /// The default value of the node input
    /// This value is JSON encoded
    default: String,
}
