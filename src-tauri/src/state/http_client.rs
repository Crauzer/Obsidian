use parking_lot::{Mutex, RwLock};
use tauri_plugin_http::reqwest::{redirect, Client, ClientBuilder};

pub struct HttpClientProvider {
    client: Client,
}

impl HttpClientProvider {
    pub fn new() -> Self {
        Self {
            client: ClientBuilder::new()
                .redirect(redirect::Policy::limited(3))
                .build()
                .unwrap(),
        }
    }

    pub fn client(&self) -> &Client {
        &self.client
    }
}

pub struct HttpClientProviderState(pub RwLock<HttpClientProvider>);
