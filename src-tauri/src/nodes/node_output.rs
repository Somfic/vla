#[derive(serde::Serialize, serde::Deserialize, Debug)]
pub struct NodeOutput {
    /// The id of the node output
    id: String,

    /// The name of the node output
    name: String,

    /// The type of the node output
    #[serde(rename = "type")]
    type_: String,

    /// The value of the node output
    /// This value is JSON encoded
    value: String,
}
