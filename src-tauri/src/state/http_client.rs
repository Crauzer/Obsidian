use parking_lot::{Mutex, RwLock};
use tauri::api::http::{Client, ClientBuilder};

pub struct HttpClientProvider {
    client: tauri::api::http::Client,
}

impl HttpClientProvider {
    pub fn new() -> Self {
        Self {
            client: ClientBuilder::new().max_redirections(3).build().unwrap(),
        }
    }

    pub fn client(&self) -> &Client {
        &self.client
    }
}

pub struct HttpClientProviderState(pub RwLock<HttpClientProvider>);
