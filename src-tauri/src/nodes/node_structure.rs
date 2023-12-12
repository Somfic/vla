use super::{node_input::NodeInput, node_output::NodeOutput, node_parameter::NodeParameter};

#[derive(serde::Serialize, serde::Deserialize, Debug)]
pub struct NodeStructure {
    /// The name of the node structure
    name: String,

    /// The category of the node structure
    category: String,

    /// The inputs of the node structure
    inputs: Vec<NodeInput>,

    /// The outputs of the node structure
    outputs: Vec<NodeOutput>,

    /// The parameters of the node structure
    parameters: Vec<NodeParameter>,
}
