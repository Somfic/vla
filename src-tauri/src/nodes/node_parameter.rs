#[derive(serde::Serialize, serde::Deserialize, Debug)]
pub struct NodeParameter {
    /// The id of the node parameter
    id: String,

    /// The name of the node parameter
    name: String,

    /// The type of the node parameter
    #[serde(rename = "type")]
    type_: String,

    /// The default value of the node parameter
    /// This value is JSON encoded
    default: String,
}
