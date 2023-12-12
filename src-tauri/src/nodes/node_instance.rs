use super::node_structure::NodeStructure;

#[derive(serde::Serialize, serde::Deserialize, Debug)]
pub struct NodeInstance {
    /// The id of the node instance
    id: String,

    /// The structure of the node instance
    structure: NodeStructure,
}
