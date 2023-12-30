use color_eyre::eyre;
use serde::{ser::SerializeStruct, Serialize};

const UNKNOWN_ERROR: &str = "Unknown error";

#[derive(Debug)]
pub struct ApiError {
    title: Option<String>,
    message: String,
    extensions: Option<Vec<ApiErrorExtension>>,
}

pub struct ApiErrorBuilder {
    title: Option<String>,
    message: String,
    extensions: Option<Vec<ApiErrorExtension>>,
}

#[derive(Debug, Serialize)]
pub enum ApiErrorExtension {}

impl ApiError {
    pub fn from_message(message: impl AsRef<str>) -> Self {
        Self {
            title: None,
            message: String::from(message.as_ref()),
            extensions: None,
        }
    }
}

impl ApiErrorBuilder {
    pub fn new() -> Self {
        Self {
            title: None,
            message: String::from(UNKNOWN_ERROR),
            extensions: None,
        }
    }

    pub fn build(self) -> ApiError {
        ApiError {
            title: self.title,
            message: self.message,
            extensions: self.extensions,
        }
    }

    pub fn message(mut self, message: impl AsRef<str>) -> Self {
        self.message = String::from(message.as_ref());
        self
    }
    pub fn extend(mut self, extension: ApiErrorExtension) -> Self {
        if let Some(extensions) = &mut self.extensions {
            extensions.push(extension);
        } else {
            self.extensions = Some(vec![extension]);
        }

        self
    }
}

impl From<eyre::Report> for ApiError {
    fn from(value: eyre::Report) -> Self {
        Self {
            title: None,
            message: format!("{:#}", value),
            extensions: None,
        }
    }
}

impl serde::Serialize for ApiError {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where
        S: serde::Serializer,
    {
        let mut state = serializer.serialize_struct("ApiError", 2)?;
        state.serialize_field("message", &self.message)?;
        state.serialize_field("extensions", &self.extensions)?;

        state.end()
    }
}
