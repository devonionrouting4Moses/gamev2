/// Core data structures for the Terminal Racer game engine
/// Defines all C-compatible game state, input, and audio structures

/// Enhanced game state with comprehensive feature support
/// Includes player state, multiplayer support, powerups, AI, obstacles, and environment
#[repr(C)]
pub struct GameState {
    // Player 1
    pub player_position: i32,
    pub player_speed: f32,
    pub player_distance: f32,
    pub player_health: i32,
    pub player_score: i32,
    pub player_car_type: i32,  // 0-9 different car designs
    
    // Player 2 (multiplayer)
    pub player2_active: bool,
    pub player2_position: i32,
    pub player2_speed: f32,
    pub player2_distance: f32,
    pub player2_health: i32,
    pub player2_score: i32,
    pub player2_car_type: i32,
    
    // Game state
    pub lap_time: f32,
    pub game_mode: i32,  // 0=single, 1=split, 2=career, 3=replay
    pub track_type: i32,  // 0=highway, 1=city, 2=mountain, 3=desert, 4=tunnel
    pub level: i32,
    pub career_progress: f32,
    
    // Powerups
    pub boost_active: bool,
    pub boost_remaining: f32,
    pub shield_active: bool,
    pub shield_remaining: f32,
    pub invincibility_active: bool,
    pub invincibility_remaining: f32,
    pub magnet_active: bool,
    pub magnet_remaining: f32,
    pub slowmo_active: bool,
    pub slowmo_remaining: f32,
    
    // AI & Objects
    pub car_count: i32,
    pub ai_positions: *const i32,
    pub ai_distances: *const f32,
    pub ai_types: *const i32,
    pub ai_is_boss: *const bool,
    
    pub obstacle_count: i32,
    pub obstacle_positions: *const i32,
    pub obstacle_distances: *const f32,
    pub obstacle_types: *const i32,  // 0=cone, 1=oil, 2=boost, 3=star, 4=magnet, 5=clock
    
    pub building_count: i32,
    pub building_positions: *const i32,  // Left=-1, Right=1
    pub building_distances: *const f32,
    pub building_heights: *const i32,
    pub building_types: *const i32,
    
    // Environment
    pub weather: i32,
    pub curve_offset: f32,
    pub elevation: f32,
    pub tunnel_darkness: f32,
    
    // Meta
    pub combo: i32,
    pub replay_mode: bool,
    pub ghost_position: i32,
    pub ghost_distance: f32,
}

/// Input state for both players and system controls
/// Supports dual-player input with separate control schemes
#[repr(C)]
pub struct InputState {
    // Player 1
    pub p1_left: bool,
    pub p1_right: bool,
    pub p1_accel: bool,
    pub p1_brake: bool,
    pub p1_boost: bool,
    
    // Player 2
    pub p2_left: bool,
    pub p2_right: bool,
    pub p2_accel: bool,
    pub p2_brake: bool,
    pub p2_boost: bool,
    
    // System
    pub quit: bool,
    pub pause: bool,
    pub menu: bool,
}

/// Audio command structure for sound effects and music
#[repr(C)]
pub struct AudioCommand {
    pub play_sound: bool,
    pub sound_type: i32,  // 0=engine, 1=boost, 2=crash, 3=powerup, 4=music
    pub volume: f32,
}

// Game mode constants
pub mod game_modes {
    pub const SINGLE_PLAYER: i32 = 0;
    pub const SPLIT_SCREEN: i32 = 1;
    pub const CAREER: i32 = 2;
    pub const REPLAY: i32 = 3;
}

// Track type constants
pub mod track_types {
    pub const HIGHWAY: i32 = 0;
    pub const CITY: i32 = 1;
    pub const MOUNTAIN: i32 = 2;
    pub const DESERT: i32 = 3;
    pub const TUNNEL: i32 = 4;
}

// Weather constants
pub mod weather {
    pub const CLEAR: i32 = 0;
    pub const RAIN: i32 = 1;
    pub const FOG: i32 = 2;
    pub const NIGHT: i32 = 3;
}

// Obstacle/Powerup type constants
pub mod obstacle_types {
    pub const CONE: i32 = 0;
    pub const OIL: i32 = 1;
    pub const BOOST: i32 = 2;
    pub const STAR: i32 = 3;
    pub const MAGNET: i32 = 4;
    pub const CLOCK: i32 = 5;
}

// Building type constants
pub mod building_types {
    pub const GLASS: i32 = 1;
    pub const CONCRETE: i32 = 2;
    pub const BRICK: i32 = 3;
}

// Audio command types
pub mod audio_types {
    pub const ENGINE: i32 = 0;
    pub const BOOST: i32 = 1;
    pub const CRASH: i32 = 2;
    pub const POWERUP: i32 = 3;
    pub const MUSIC: i32 = 4;
}
