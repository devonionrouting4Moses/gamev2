/// Terminal management module for initialization and cleanup
/// Handles raw mode, alternate screen, and terminal setup

use ratatui::{
    backend::CrosstermBackend,
    Terminal,
};
use crossterm::{
    execute,
    terminal::{disable_raw_mode, enable_raw_mode, EnterAlternateScreen, LeaveAlternateScreen},
};
use std::io;

/// Global terminal instance
pub static mut TERMINAL: Option<Terminal<CrosstermBackend<io::Stdout>>> = None;

/// Initialize the terminal for rendering
/// Sets up raw mode and alternate screen
pub fn init() -> bool {
    match enable_raw_mode() {
        Ok(_) => {},
        Err(e) => {
            eprintln!("Failed to enable raw mode: {}", e);
            return false;
        }
    }
    
    match execute!(io::stdout(), EnterAlternateScreen) {
        Ok(_) => {},
        Err(e) => {
            eprintln!("Failed to enter alternate screen: {}", e);
            let _ = disable_raw_mode();
            return false;
        }
    }
    
    let backend = CrosstermBackend::new(io::stdout());
    let terminal = Terminal::new(backend);
    
    match terminal {
        Ok(t) => {
            unsafe { TERMINAL = Some(t); }
            true
        },
        Err(e) => {
            eprintln!("Failed to create terminal: {}", e);
            let _ = disable_raw_mode();
            let _ = execute!(io::stdout(), LeaveAlternateScreen);
            false
        }
    }
}

/// Clean up and restore terminal to normal state
pub fn cleanup() {
    unsafe { TERMINAL = None; }
    let _ = disable_raw_mode();
    let _ = execute!(io::stdout(), LeaveAlternateScreen);
}

/// Get mutable reference to the terminal
pub fn get_terminal() -> Option<&'static mut Terminal<CrosstermBackend<io::Stdout>>> {
    unsafe {
        match std::ptr::addr_of_mut!(TERMINAL).as_mut() {
            Some(Some(t)) => Some(t),
            _ => None,
        }
    }
}
