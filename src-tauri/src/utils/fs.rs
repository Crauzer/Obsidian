use std::{
    fs::{create_dir, create_dir_all},
    io,
    path::Path,
};

use color_eyre::eyre::{self, Context};

pub fn try_create_dir(path: impl AsRef<Path>) -> eyre::Result<()> {
    if let Err(error) = create_dir_all(path.as_ref()) {
        if error.kind() != io::ErrorKind::AlreadyExists {
            return Err(error).wrap_err(format!(
                "failed to create directory (path: {})",
                path.as_ref().display()
            ));
        }
    }

    Ok(())
}
