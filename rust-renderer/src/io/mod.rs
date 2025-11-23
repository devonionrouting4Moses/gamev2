/// I/O module - Input handling and terminal management
/// Manages keyboard input, terminal initialization, and raw mode

pub mod input;
pub mod terminal;

pub use input::poll_input;
pub use terminal::{init, cleanup, get_terminal};
