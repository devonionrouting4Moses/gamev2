/// Rendering module - All visual output and UI rendering
/// Organized into specialized submodules for different rendering concerns

pub mod track;
pub mod objects;
pub mod effects;
pub mod hud;
pub mod visual_assets;

pub use track::render_track;
pub use objects::{render_objects, render_player, render_ghost};
pub use effects::{render_lane_markers, render_weather_overlay};
pub use hud::{
    render_enhanced_hud, render_player_hud, render_career_info,
    render_replay_controls, render_replay_info, render_menu
};
pub use visual_assets::{
    get_detailed_car, get_powerup_visual, get_tree, get_building,
    get_cactus, get_mountain, get_road_marking, get_hud_style,
    get_lane_config, get_track_palette
};
