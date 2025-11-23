/// Input handling module for keyboard and system events
/// Manages player input polling and state management

use crossterm::event::{self, Event, KeyCode};
use std::time::Duration;
use crate::core::types::InputState;

/// Poll for keyboard input and update input state
/// Handles both single and multiplayer input schemes
pub fn poll_input(input: *mut InputState) -> bool {
    if input.is_null() {
        return false;
    }
    
    let input_state = unsafe { &mut *input };
    
    // Reset all inputs
    *input_state = InputState {
        p1_left: false,
        p1_right: false,
        p1_accel: false,
        p1_brake: false,
        p1_boost: false,
        p2_left: false,
        p2_right: false,
        p2_accel: false,
        p2_brake: false,
        p2_boost: false,
        quit: false,
        pause: false,
        menu: false,
    };
    
    // Poll with 16ms timeout (60 FPS)
    if event::poll(Duration::from_millis(16)).unwrap_or(false) {
        if let Ok(Event::Key(key)) = event::read() {
            handle_key_event(key.code, input_state);
        }
    }
    
    true
}

/// Process individual key events and update input state
fn handle_key_event(code: KeyCode, input_state: &mut InputState) {
    match code {
        // Player 1 (Arrows/WASD)
        KeyCode::Left | KeyCode::Char('a') | KeyCode::Char('A') => input_state.p1_left = true,
        KeyCode::Right | KeyCode::Char('d') | KeyCode::Char('D') => input_state.p1_right = true,
        KeyCode::Up | KeyCode::Char('w') | KeyCode::Char('W') => input_state.p1_accel = true,
        KeyCode::Down | KeyCode::Char('s') | KeyCode::Char('S') => input_state.p1_brake = true,
        KeyCode::Char(' ') => input_state.p1_boost = true,
        
        // Player 2 (IJKL)
        KeyCode::Char('j') | KeyCode::Char('J') => input_state.p2_left = true,
        KeyCode::Char('l') | KeyCode::Char('L') => input_state.p2_right = true,
        KeyCode::Char('i') | KeyCode::Char('I') => input_state.p2_accel = true,
        KeyCode::Char('k') | KeyCode::Char('K') => input_state.p2_brake = true,
        KeyCode::Char('u') | KeyCode::Char('U') => input_state.p2_boost = true,
        
        // System
        KeyCode::Char('q') | KeyCode::Char('Q') | KeyCode::Esc => input_state.quit = true,
        KeyCode::Char('p') | KeyCode::Char('P') => input_state.pause = true,
        KeyCode::Char('m') | KeyCode::Char('M') => input_state.menu = true,
        _ => {},
    }
}
