pub mod actions;
pub mod error;
pub mod fs;
pub mod game_explorer;
pub mod hashtable;
pub mod settings;
pub mod wad;

pub type ApiResult<T> = Result<T, error::ApiError>;

/// Generates the Tauri invoke handler with all API commands.
/// Commands are organized by module - add new commands to the appropriate section.
#[macro_export]
macro_rules! generate_command_handler {
    () => {
        tauri::generate_handler![
            // actions
            $crate::api::actions::get_action_progress,
            // fs
            $crate::api::fs::get_app_directory,
            $crate::api::fs::open_path,
            $crate::api::fs::pick_directory,
            $crate::api::fs::pick_file,
            // game_explorer
            $crate::api::game_explorer::get_game_explorer_status,
            $crate::api::game_explorer::mount_game_explorer,
            $crate::api::game_explorer::get_game_explorer_items,
            $crate::api::game_explorer::get_game_explorer_path_components,
            // hashtable
            $crate::api::hashtable::get_wad_hashtable_status,
            $crate::api::hashtable::load_wad_hashtables,
            // settings
            $crate::api::settings::get_settings,
            $crate::api::settings::update_settings,
            // wad
            $crate::api::wad::extract_mounted_wad,
            $crate::api::wad::extract_wad_items,
            $crate::api::wad::get_chunk_preview_types,
            $crate::api::wad::get_image_bytes,
            $crate::api::wad::get_mounted_wad_directory_path_components,
            $crate::api::wad::get_mounted_wads,
            $crate::api::wad::get_wad_parent_items,
            $crate::api::wad::mount_wads,
            $crate::api::wad::move_mounted_wad,
            $crate::api::wad::search_wad,
            $crate::api::wad::unmount_wad,
            $crate::api::wad::update_mounted_wad_item_selection,
        ]
    };
}