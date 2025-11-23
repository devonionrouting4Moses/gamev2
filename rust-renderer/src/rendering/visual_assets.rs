/// Visual Assets Module - Enhanced graphics and styling
/// Provides detailed car designs, powerup visuals, and environmental assets
/// Inspired by modern mobile racing games with colorful, detailed graphics

use ratatui::style::Color;

/// Enhanced car designs with detailed ASCII art
pub struct DetailedCarDesign {
    pub top: &'static str,
    pub middle: &'static str,
    pub bottom: &'static str,
    pub color: Color,
    pub label: &'static str,
}

/// Get detailed car design with 3-line ASCII art
pub fn get_detailed_car(car_type: i32, is_boss: bool) -> DetailedCarDesign {
    if is_boss {
        return DetailedCarDesign {
            top:    "╔═══╗",
            middle: "║ B ║",
            bottom: "╚═══╝",
            color: Color::Red,
            label: "BOSS",
        };
    }

    match car_type {
        0 => DetailedCarDesign {
            // Blue sports car
            top:    "┌─┐",
            middle: "│●│",
            bottom: "└─┘",
            color: Color::Blue,
            label: "P1",
        },
        1 => DetailedCarDesign {
            // Red police car
            top:    "┌─┐",
            middle: "│🚨│",
            bottom: "└─┘",
            color: Color::Red,
            label: "POL",
        },
        2 => DetailedCarDesign {
            // Yellow racer
            top:    "┌─┐",
            middle: "│⚡│",
            bottom: "└─┘",
            color: Color::Yellow,
            label: "RCR",
        },
        3 => DetailedCarDesign {
            // Green truck
            top:    "┌───┐",
            middle: "│ G │",
            bottom: "└───┘",
            color: Color::Green,
            label: "TRK",
        },
        4 => DetailedCarDesign {
            // Orange taxi
            top:    "┌─┐",
            middle: "│T│",
            bottom: "└─┘",
            color: Color::Rgb(255, 165, 0),
            label: "TXI",
        },
        5 => DetailedCarDesign {
            // Gray van
            top:    "┌───┐",
            middle: "│ V │",
            bottom: "└───┘",
            color: Color::Gray,
            label: "VAN",
        },
        6 => DetailedCarDesign {
            // Magenta muscle car
            top:    "┌─┐",
            middle: "│M│",
            bottom: "└─┘",
            color: Color::Magenta,
            label: "MSC",
        },
        7 => DetailedCarDesign {
            // Cyan convertible
            top:    "┌─┐",
            middle: "│C│",
            bottom: "└─┘",
            color: Color::Cyan,
            label: "CNV",
        },
        8 => DetailedCarDesign {
            // White limo
            top:    "┌─────┐",
            middle: "│ LIM │",
            bottom: "└─────┘",
            color: Color::White,
            label: "LMO",
        },
        _ => DetailedCarDesign {
            // Default car
            top:    "┌─┐",
            middle: "│?│",
            bottom: "└─┘",
            color: Color::Gray,
            label: "CAR",
        },
    }
}

/// Powerup visual representations
pub struct PowerupVisual {
    pub icon: &'static str,
    pub color: Color,
    pub name: &'static str,
    pub effect: &'static str,
}

pub fn get_powerup_visual(ptype: i32) -> PowerupVisual {
    match ptype {
        0 => PowerupVisual {
            icon: "🚧",
            color: Color::Yellow,
            name: "CONE",
            effect: "OBSTACLE",
        },
        1 => PowerupVisual {
            icon: "💧",
            color: Color::Blue,
            name: "OIL",
            effect: "SLIPPERY",
        },
        2 => PowerupVisual {
            icon: "⚡",
            color: Color::Magenta,
            name: "BOOST",
            effect: "SPEED+",
        },
        3 => PowerupVisual {
            icon: "⭐",
            color: Color::Yellow,
            name: "STAR",
            effect: "INVINCIBLE",
        },
        4 => PowerupVisual {
            icon: "🧲",
            color: Color::Red,
            name: "MAGNET",
            effect: "ATTRACT",
        },
        5 => PowerupVisual {
            icon: "🕐",
            color: Color::Cyan,
            name: "CLOCK",
            effect: "SLOWMO",
        },
        _ => PowerupVisual {
            icon: "⚠",
            color: Color::Red,
            name: "UNKNOWN",
            effect: "UNKNOWN",
        },
    }
}

/// Environment visual elements
pub struct EnvironmentAsset {
    pub symbol: &'static str,
    pub color: Color,
    pub name: &'static str,
}

pub fn get_tree() -> EnvironmentAsset {
    EnvironmentAsset {
        symbol: "🌲",
        color: Color::Green,
        name: "TREE",
    }
}

pub fn get_building(building_type: i32) -> EnvironmentAsset {
    match building_type {
        1 => EnvironmentAsset {
            symbol: "🏢",
            color: Color::Rgb(100, 100, 150),
            name: "GLASS_BUILDING",
        },
        2 => EnvironmentAsset {
            symbol: "🏭",
            color: Color::Rgb(80, 80, 80),
            name: "CONCRETE_BUILDING",
        },
        3 => EnvironmentAsset {
            symbol: "🏠",
            color: Color::Rgb(120, 90, 70),
            name: "BRICK_BUILDING",
        },
        _ => EnvironmentAsset {
            symbol: "🏢",
            color: Color::Gray,
            name: "BUILDING",
        },
    }
}

pub fn get_cactus() -> EnvironmentAsset {
    EnvironmentAsset {
        symbol: "🌵",
        color: Color::Green,
        name: "CACTUS",
    }
}

pub fn get_mountain() -> EnvironmentAsset {
    EnvironmentAsset {
        symbol: "⛰",
        color: Color::Rgb(100, 100, 100),
        name: "MOUNTAIN",
    }
}

/// Road markings and lane elements
pub struct RoadMarking {
    pub solid: &'static str,
    pub dashed: &'static str,
    pub color: Color,
}

pub fn get_road_marking(weather: i32) -> RoadMarking {
    match weather {
        1 => RoadMarking {
            // Rain - darker road
            solid: "▓",
            dashed: "┆",
            color: Color::Rgb(60, 60, 80),
        },
        2 => RoadMarking {
            // Fog - lighter road
            solid: "░",
            dashed: "┆",
            color: Color::Rgb(100, 100, 100),
        },
        3 => RoadMarking {
            // Night - very dark
            solid: "█",
            dashed: "┆",
            color: Color::Rgb(20, 20, 30),
        },
        _ => RoadMarking {
            // Clear - normal road
            solid: "▓",
            dashed: "┆",
            color: Color::Rgb(80, 80, 80),
        },
    }
}

/// HUD element styles
pub struct HUDStyle {
    pub border_top: &'static str,
    pub border_mid: &'static str,
    pub border_bot: &'static str,
    pub color: Color,
}

pub fn get_hud_style(element_type: i32) -> HUDStyle {
    match element_type {
        0 => HUDStyle {
            // Score display
            border_top: "╔════════╗",
            border_mid: "║ SCORE  ║",
            border_bot: "╚════════╝",
            color: Color::Yellow,
        },
        1 => HUDStyle {
            // Health display
            border_top: "╔════════╗",
            border_mid: "║ HEALTH ║",
            border_bot: "╚════════╝",
            color: Color::Red,
        },
        2 => HUDStyle {
            // Speed display
            border_top: "╔════════╗",
            border_mid: "║ SPEED  ║",
            border_bot: "╚════════╝",
            color: Color::Cyan,
        },
        3 => HUDStyle {
            // Powerup display
            border_top: "╔════════╗",
            border_mid: "║ POWER  ║",
            border_bot: "╚════════╝",
            color: Color::Magenta,
        },
        _ => HUDStyle {
            border_top: "╔════════╗",
            border_mid: "║ INFO   ║",
            border_bot: "╚════════╝",
            color: Color::White,
        },
    }
}

/// Lane configuration for different track types
pub struct LaneConfig {
    pub lane_count: usize,
    pub lane_width: u16,
    pub lane_color: Color,
    pub marker_style: &'static str,
}

pub fn get_lane_config(track_type: i32) -> LaneConfig {
    match track_type {
        1 => LaneConfig {
            // City - 4 lanes
            lane_count: 4,
            lane_width: 6,
            lane_color: Color::Rgb(100, 100, 100),
            marker_style: "║",
        },
        2 => LaneConfig {
            // Mountain - 3 lanes
            lane_count: 3,
            lane_width: 8,
            lane_color: Color::Rgb(80, 80, 80),
            marker_style: "┃",
        },
        3 => LaneConfig {
            // Desert - 2 lanes
            lane_count: 2,
            lane_width: 12,
            lane_color: Color::Rgb(120, 100, 60),
            marker_style: "┆",
        },
        4 => LaneConfig {
            // Tunnel - 3 lanes
            lane_count: 3,
            lane_width: 8,
            lane_color: Color::Rgb(40, 40, 40),
            marker_style: "┃",
        },
        _ => LaneConfig {
            // Highway - 3 lanes
            lane_count: 3,
            lane_width: 8,
            lane_color: Color::Rgb(80, 80, 80),
            marker_style: "┆",
        },
    }
}

/// Particle effect definitions
pub struct ParticleEffect {
    pub symbol: &'static str,
    pub color: Color,
    pub lifetime: u8,
}

pub fn get_boost_particle() -> ParticleEffect {
    ParticleEffect {
        symbol: "✦",
        color: Color::Magenta,
        lifetime: 10,
    }
}

pub fn get_crash_particle() -> ParticleEffect {
    ParticleEffect {
        symbol: "✕",
        color: Color::Red,
        lifetime: 15,
    }
}

pub fn get_dust_particle() -> ParticleEffect {
    ParticleEffect {
        symbol: "·",
        color: Color::Gray,
        lifetime: 8,
    }
}

/// Color palette for consistent theming
pub struct ColorPalette {
    pub primary: Color,
    pub secondary: Color,
    pub accent: Color,
    pub background: Color,
    pub text: Color,
}

pub fn get_track_palette(track_type: i32) -> ColorPalette {
    match track_type {
        1 => ColorPalette {
            // City palette
            primary: Color::Rgb(100, 100, 150),
            secondary: Color::Rgb(150, 150, 200),
            accent: Color::Yellow,
            background: Color::Rgb(30, 30, 40),
            text: Color::White,
        },
        2 => ColorPalette {
            // Mountain palette
            primary: Color::Rgb(100, 150, 100),
            secondary: Color::Rgb(150, 200, 150),
            accent: Color::White,
            background: Color::Rgb(25, 35, 25),
            text: Color::White,
        },
        3 => ColorPalette {
            // Desert palette
            primary: Color::Rgb(200, 150, 100),
            secondary: Color::Rgb(220, 180, 120),
            accent: Color::Yellow,
            background: Color::Rgb(50, 40, 20),
            text: Color::White,
        },
        4 => ColorPalette {
            // Tunnel palette
            primary: Color::Rgb(60, 60, 60),
            secondary: Color::Rgb(100, 100, 100),
            accent: Color::Cyan,
            background: Color::Rgb(20, 20, 20),
            text: Color::White,
        },
        _ => ColorPalette {
            // Highway palette
            primary: Color::Rgb(80, 80, 80),
            secondary: Color::Rgb(120, 120, 120),
            accent: Color::Cyan,
            background: Color::Black,
            text: Color::White,
        },
    }
}

/// Animation frame definitions
pub struct AnimationFrame {
    pub frame: &'static str,
    pub duration: u8,
}

pub fn get_wheel_animation(frame: u8) -> &'static str {
    match frame % 4 {
        0 => "◐",
        1 => "◓",
        2 => "◑",
        3 => "◒",
        _ => "◐",
    }
}

pub fn get_boost_animation(frame: u8) -> &'static str {
    match frame % 3 {
        0 => "🔥",
        1 => "💥",
        2 => "⚡",
        _ => "🔥",
    }
}

pub fn get_shield_animation(frame: u8) -> &'static str {
    match frame % 2 {
        0 => "◯",
        1 => "◉",
        _ => "◯",
    }
}
