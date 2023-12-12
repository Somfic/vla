use super::{connection::Connection, node_instance::NodeInstance};

#[derive(serde::Serialize, serde::Deserialize, Debug)]
pub struct Web {
    /// The id of the web
    id: String,

    /// The name of the web
    name: String,

    /// The node instances in the web
    instances: Vec<NodeInstance>,

    /// The connections between the node instances in the web
    connections: Vec<Connection>,
}
