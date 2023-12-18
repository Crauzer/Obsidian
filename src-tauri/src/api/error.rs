use std::io;

use color_eyre::eyre;
use serde::{ser::SerializeStruct, Serialize};

const UNKNOWN_ERROR: &str = "Unknown error";

#[derive(Debug)]
pub struct ApiError {
    message: String,
    extensions: Vec<ApiErrorExtension>,
}

pub struct ApiErrorBuilder {
    message: String,
    extensions: Vec<ApiErrorExtension>,
}

#[derive(Debug, Serialize)]
pub enum ApiErrorExtension {}

impl ApiError {
    pub fn from_message(message: impl AsRef<str>) -> Self {
        Self {
            message: String::from(message.as_ref()),
            extensions: Vec::default(),
        }
    }
}

impl ApiErrorBuilder {
    pub fn new() -> Self {
        Self {
            message: String::from(UNKNOWN_ERROR),
            extensions: Vec::default(),
        }
    }

    pub fn build(self) -> ApiError {
        ApiError {
            message: self.message,
            extensions: self.extensions,
        }
    }

    pub fn message(mut self, message: impl AsRef<str>) -> Self {
        self.message = String::from(message.as_ref());
        self
    }
    pub fn extension(mut self, extension: ApiErrorExtension) -> Self {
        self.extensions.push(extension);
        self
    }
}

impl From<eyre::Report> for ApiError {
    fn from(value: eyre::Report) -> Self {
        Self {
            message: format!("{:#}", value),
            extensions: Vec::default(),
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
