use crate::error::Result;
use std::{fs::create_dir, io, path::Path};

pub fn try_create_dir(path: impl AsRef<Path>) -> Result<()> {
    if let Err(error) = create_dir(path.as_ref()) {
        if error.kind() != io::ErrorKind::AlreadyExists {
            return Err(error.into());
        }
    }

    Ok(())
}
