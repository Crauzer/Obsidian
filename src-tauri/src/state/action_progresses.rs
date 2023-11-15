use std::collections::HashMap;

use crate::api::actions::ActionProgress;
use parking_lot::RwLock;
use uuid::Uuid;

pub struct ActionProgresses {
    progresses: HashMap<Uuid, ActionProgress>,
}

impl ActionProgresses {
    pub fn new() -> Self {
        Self {
            progresses: HashMap::new(),
        }
    }

    pub fn get(&self, id: Uuid) -> Option<&ActionProgress> {
        self.progresses.get(&id)
    }

    pub fn insert(&mut self, id: Uuid, progress: ActionProgress) {
        self.progresses.insert(id, progress);
    }

    pub fn remove(&mut self, id: Uuid) -> Option<ActionProgress> {
        self.progresses.remove(&id)
    }
}

pub struct ActionProgressesState(pub RwLock<ActionProgresses>);
