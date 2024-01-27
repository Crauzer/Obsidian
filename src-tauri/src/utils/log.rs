pub fn create_log_filename() -> String {
    format!(
        "obsidian_{}.log",
        chrono::offset::Utc::now().format("%Y-%m-%dT%H_%M_%S")
    )
}
