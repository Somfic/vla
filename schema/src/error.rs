use jsonrpsee::types::ErrorObjectOwned;
use schema_codegen::vla_type;

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

impl Error {
    pub fn code(&self) -> i32 {
        match self {
            Self::NotPaired => -32001,
            Self::InvalidValue { .. } => -32002,
            Self::Internal { .. } => -32500,
        }
    }
}

impl From<Error> for ErrorObjectOwned {
    fn from(err: Error) -> Self {
        let code = err.code();
        let message = err.to_string();
        let data = serde_json::to_value(&err).ok();
        ErrorObjectOwned::owned(code, message, data)
    }
}
