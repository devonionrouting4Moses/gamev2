/// Utility functions for colors, styling, and game data lookups
/// Provides helper functions for rendering and game logic

use ratatui::style::{Color, Style};
use super::types::{track_types, weather};

/// Get color based on combo multiplier
pub fn get_combo_color(combo: i32) -> Color {
    match combo {
        0..=2 => Color::White,
        3..=5 => Color::Cyan,
        6..=10 => Color::Yellow,
        11..=20 => Color::Magenta,
        _ => Color::Red,
    }
}

/// Get color based on health percentage
pub fn get_health_color(health: i32) -> Color {
    if health > 66 {
        Color::Green
    } else if health > 33 {
        Color::Yellow
    } else {
        Color::Red
    }
}

/// Get track name from track type
pub fn get_track_name(track_type: i32) -> &'static str {
    match track_type {
        1 => "CITY STREETS",
        2 => "MOUNTAIN PASS",
        3 => "DESERT HIGHWAY",
        4 => "UNDERGROUND TUNNEL",
        _ => "HIGHWAY RUSH",
    }
}

/// Get weather icon and label
pub fn get_weather_icon(weather_type: i32) -> &'static str {
    match weather_type {
        1 => "🌧RAIN",
        2 => "🌫FOG",
        3 => "🌙NIGHT",
        _ => "☀CLEAR",
    }
}

/// Get track background style based on track type and weather
pub fn get_track_style(track_type: i32, weather_type: i32, darkness: f32) -> Style {
    let base_color = match track_type {
        1 => Color::Rgb(30, 30, 40),   // City
        2 => Color::Rgb(25, 35, 25),   // Mountain
        3 => Color::Rgb(50, 40, 20),   // Desert
        4 => {
            // Tunnel - brightness based on darkness
            let brightness = ((1.0 - darkness) * 30.0) as u8;
            Color::Rgb(brightness, brightness, brightness)
        },
        _ => Color::Black,              // Highway
    };
    
    let weather_adjusted = match weather_type {
        1 => Color::Rgb(20, 30, 50),   // Rain
        2 => Color::Rgb(40, 40, 40),   // Fog
        3 => Color::Rgb(10, 10, 30),   // Night
        _ => base_color,
    };
    
    Style::default().bg(weather_adjusted)
}

/// Car design data structure
pub struct CarDesign {
    pub art: [&'static str; 4],
    pub color: Color,
    pub label: &'static str,
}

/// Get car design based on car type and boss status
pub fn get_car_design(car_type: i32, is_boss: bool) -> CarDesign {
    if is_boss {
        return CarDesign {
            art: [
                " ▄███▄ ",
                "███████",
                "▐██▌██▌",
                " BOSS! ",
            ],
            color: Color::Red,
            label: "BOSS!",
        };
    }
    
    match car_type {
        0 => CarDesign {
            // Sports car
            art: [
                "  ▄█▄  ",
                " █████ ",
                " ▐█▌█▌ ",
                "  YOU  ",
            ],
            color: Color::Green,
            label: "YOU",
        },
        1 => CarDesign {
            // Police
            art: [
                "  ▄█▄  ",
                " █🚨█ ",
                " ▐█▌█▌ ",
                "  🚔  ",
            ],
            color: Color::Blue,
            label: "POL",
        },
        2 => CarDesign {
            // Racer
            art: [
                "  ▀█▀  ",
                " █████ ",
                " ▐██▌▌ ",
                "  🏁  ",
            ],
            color: Color::Magenta,
            label: "RCR",
        },
        3 => CarDesign {
            // Truck
            art: [
                " ▄███▄ ",
                "███████",
                "▐██▌██▌",
                " TRUCK ",
            ],
            color: Color::Yellow,
            label: "TRK",
        },
        4 => CarDesign {
            // Taxi
            art: [
                "  ▄█▄  ",
                " █▓▓█ ",
                " ▐█▌█▌ ",
                " TAXI ",
            ],
            color: Color::Yellow,
            label: "TXI",
        },
        5 => CarDesign {
            // Van
            art: [
                " ▄███▄ ",
                "███▓███",
                "▐█▌▌█▌ ",
                "  VAN  ",
            ],
            color: Color::Rgb(150, 150, 150),
            label: "VAN",
        },
        6 => CarDesign {
            // Muscle car
            art: [
                "  ▄█▄  ",
                " ▓███▓ ",
                " ▐██▌▌ ",
                " MSCL ",
            ],
            color: Color::Red,
            label: "MSC",
        },
        7 => CarDesign {
            // Convertible
            art: [
                "  ─█─  ",
                " █▒▒█ ",
                " ▐█▌█▌ ",
                " CONV ",
            ],
            color: Color::Cyan,
            label: "CNV",
        },
        8 => CarDesign {
            // Limo
            art: [
                "▄█████▄",
                "███████",
                "▐█▌▌▌█▌",
                " LIMO! ",
            ],
            color: Color::Black,
            label: "LMO",
        },
        _ => CarDesign {
            // Default
            art: [
                "  ▄█▄  ",
                " █████ ",
                " ▐█▌█▌ ",
                "  CAR  ",
            ],
            color: Color::Gray,
            label: "CAR",
        },
    }
}

/// Get powerup icon and color
pub fn get_powerup_icon(ptype: i32) -> (&'static str, Color) {
    match ptype {
        0 => ("🚧", Color::Yellow),
        1 => ("💧", Color::Blue),
        2 => ("⚡", Color::Magenta),
        3 => ("⭐", Color::Yellow),
        4 => ("🧲", Color::Red),
        5 => ("🕐", Color::Cyan),
        _ => ("⚠", Color::Red),
    }
}

/// Get building character set and color
pub fn get_building_style(btype: i32) -> (&'static str, Color) {
    match btype {
        1 => ("▓▓▓▓▓▓", Color::Rgb(100, 100, 150)),  // Glass building
        2 => ("██████", Color::Rgb(80, 80, 80)),     // Concrete
        3 => ("▒▒▒▒▒▒", Color::Rgb(120, 90, 70)),    // Brick
        _ => ("▓▓▓▓▓▓", Color::Rgb(60, 60, 90)),     // Default
    }
}

/// Get road character based on weather
pub fn get_road_char(weather_type: i32) -> &'static str {
    match weather_type {
        1 => "▒",  // Rain
        2 => "░",  // Fog
        _ => "▓",  // Clear
    }
}
