use std::collections::HashMap;

use crate::api::actions::ActionProgress;
use parking_lot::RwLock;
use uuid::Uuid;

#[derive(Clone, Debug)]
pub struct Action {
    id: Uuid,
    progress: ActionProgress,
}

impl Action {
    pub fn progress(&self) -> &ActionProgress {
        &self.progress
    }
}

pub struct ActionsState(pub RwLock<HashMap<Uuid, Action>>);
