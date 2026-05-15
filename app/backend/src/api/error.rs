use schema::vla_type;

#[derive(thiserror::Error)]
#[vla_type]
#[serde(tag = "kind", content = "details")]
pub enum Error {
    #[error("not paired")]
    NotPaired,
    #[error("invalid value for {field}")]
    InvalidValue { field: String },
    #[error("internal: {message}")]
    Internal { message: String },
}
