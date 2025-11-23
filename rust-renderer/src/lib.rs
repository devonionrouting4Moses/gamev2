// ULTIMATE Terminal Racer - Enhanced Rust Rendering Engine
// With: Multiplayer, Advanced Graphics, Track Variety, Replay Support
// Cargo.toml dependencies:
// [dependencies]
// ratatui = "0.25"
// crossterm = "0.27"
// 
// [lib]
// crate-type = ["cdylib"]

// Module declarations - organized into logical folders
pub mod core;      // Data structures and utilities
pub mod io;        // Input handling and terminal management
pub mod rendering; // All visual rendering and UI

// Re-export public types for C FFI
pub use core::{GameState, InputState, AudioCommand};

// Import commonly used items
use crate::core::types::game_modes;

/// Initialize the rendering engine
/// Sets up terminal, raw mode, and alternate screen
#[unsafe(no_mangle)]
pub extern "C" fn ratatui_init() -> bool {
    io::terminal::init()
}

/// Clean up and restore terminal state
#[unsafe(no_mangle)]
pub extern "C" fn ratatui_cleanup() {
    io::terminal::cleanup()
}

/// Poll for keyboard input and update input state
#[unsafe(no_mangle)]
pub extern "C" fn ratatui_poll_input(input: *mut InputState) -> bool {
    io::input::poll_input(input)
}

/// Render the game based on current game state
#[unsafe(no_mangle)]
pub extern "C" fn ratatui_render(state: *const GameState) -> bool {
    if state.is_null() {
        return false;
    }
    
    let game_state = unsafe { &*state };
    
    let terminal = match io::terminal::get_terminal() {
        Some(t) => t,
        None => return false,
    };
    
    let result = terminal.draw(|f| {
        let size = f.area();
        
        // Route to appropriate render function based on game mode
        match game_state.game_mode {
            game_modes::SPLIT_SCREEN if game_state.player2_active => {
                render_splitscreen(f, size, game_state);
            },
            game_modes::CAREER => {
                render_career_mode(f, size, game_state);
            },
            game_modes::REPLAY => {
                render_replay_mode(f, size, game_state);
            },
            _ => {
                render_singleplayer(f, size, game_state);
            },
        }
    });
    
    result.is_ok()
}

/// Render single-player game mode
fn render_singleplayer(f: &mut ratatui::Frame, area: ratatui::layout::Rect, state: &GameState) {
    use ratatui::layout::{Constraint, Direction, Layout};
    
    let chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([
            Constraint::Length(6),   // Enhanced HUD
            Constraint::Min(0),      // Game area
            Constraint::Length(2),   // Controls
        ])
        .split(area);
    
    rendering::hud::render_enhanced_hud(f, chunks[0], state);
    rendering::track::render_track(f, chunks[1], state, state.player_position, state.player_distance, true);
}

/// Render split-screen multiplayer mode
fn render_splitscreen(f: &mut ratatui::Frame, area: ratatui::layout::Rect, state: &GameState) {
    use ratatui::layout::{Constraint, Direction, Layout};
    
    let h_chunks = Layout::default()
        .direction(Direction::Horizontal)
        .constraints([Constraint::Percentage(50), Constraint::Percentage(50)])
        .split(area);
    
    // Player 1 side
    let p1_chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([Constraint::Length(4), Constraint::Min(0), Constraint::Length(2)])
        .split(h_chunks[0]);
    
    rendering::hud::render_player_hud(f, p1_chunks[0], state, 1);
    rendering::track::render_track(f, p1_chunks[1], state, state.player_position, state.player_distance, true);
    
    // Player 2 side
    let p2_chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([Constraint::Length(4), Constraint::Min(0), Constraint::Length(2)])
        .split(h_chunks[1]);
    
    rendering::hud::render_player_hud(f, p2_chunks[0], state, 2);
    rendering::track::render_track(f, p2_chunks[1], state, state.player2_position, state.player2_distance, false);
}

/// Render career mode with progression tracking
fn render_career_mode(f: &mut ratatui::Frame, area: ratatui::layout::Rect, state: &GameState) {
    use ratatui::layout::{Constraint, Direction, Layout};
    
    let chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([
            Constraint::Length(7),   // Career HUD
            Constraint::Min(0),      // Game area
            Constraint::Length(3),   // Career info
        ])
        .split(area);
    
    rendering::hud::render_enhanced_hud(f, chunks[0], state);
    rendering::track::render_track(f, chunks[1], state, state.player_position, state.player_distance, true);
    rendering::hud::render_career_info(f, chunks[2], state);
}

/// Render replay mode with playback controls
fn render_replay_mode(f: &mut ratatui::Frame, area: ratatui::layout::Rect, state: &GameState) {
    use ratatui::layout::{Constraint, Direction, Layout};
    
    let chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([
            Constraint::Length(3),   // Replay controls
            Constraint::Min(0),      // Game area
            Constraint::Length(2),   // Replay info
        ])
        .split(area);
    
    rendering::hud::render_replay_controls(f, chunks[0]);
    rendering::track::render_track(f, chunks[1], state, state.player_position, state.player_distance, true);
    rendering::hud::render_replay_info(f, chunks[2], state);
}

/// Render menu with title and options
#[unsafe(no_mangle)]
pub extern "C" fn ratatui_render_menu(
    title: *const std::os::raw::c_char,
    options: *const *const std::os::raw::c_char,
    option_count: i32,
    selected: i32,
) -> bool {
    let terminal = match io::terminal::get_terminal() {
        Some(t) => t,
        None => return false,
    };
    
    let result = terminal.draw(|f| {
        // Convert C strings to Rust strings
        let title_str = unsafe { std::ffi::CStr::from_ptr(title).to_str().unwrap_or("MENU") };
        
        let mut menu_options = Vec::new();
        for i in 0..option_count {
            let option_ptr = unsafe { *options.offset(i as isize) };
            let option_str = unsafe { std::ffi::CStr::from_ptr(option_ptr).to_str().unwrap_or("") };
            menu_options.push(option_str.to_string());
        }
        
        rendering::hud::render_menu(f, title_str, &menu_options, selected);
    });
    
    result.is_ok()
}

// All rendering functions have been moved to dedicated modules
// See: track.rs, hud.rs, objects.rs, effects.rs, utils.rs, input.rs, terminal.rs
