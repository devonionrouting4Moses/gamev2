// ULTIMATE Terminal Racer - Enhanced Rust Rendering Engine
// With: Multiplayer, Advanced Graphics, Track Variety, Replay Support
// Cargo.toml dependencies:
// [dependencies]
// ratatui = "0.25"
// crossterm = "0.27"
// 
// [lib]
// crate-type = ["cdylib"]

use ratatui::{
    backend::CrosstermBackend,
    layout::{Constraint, Direction, Layout, Rect, Alignment},
    style::{Color, Modifier, Style},
    widgets::{Block, Borders, Gauge, Paragraph, List, ListItem},
    Terminal,
};
use crossterm::{
    event::{self, Event, KeyCode},
    execute,
    terminal::{disable_raw_mode, enable_raw_mode, EnterAlternateScreen, LeaveAlternateScreen},
};
use std::io;

// Enhanced game state with ALL new features
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

#[repr(C)]
pub struct AudioCommand {
    pub play_sound: bool,
    pub sound_type: i32,  // 0=engine, 1=boost, 2=crash, 3=powerup, 4=music
    pub volume: f32,
}

static mut TERMINAL: Option<Terminal<CrosstermBackend<io::Stdout>>> = None;

#[unsafe(no_mangle)]
pub extern "C" fn ratatui_init() -> bool {
    match enable_raw_mode() {
        Ok(_) => {},
        Err(_) => return false,
    }
    
    match execute!(io::stdout(), EnterAlternateScreen) {
        Ok(_) => {},
        Err(_) => return false,
    }
    
    let backend = CrosstermBackend::new(io::stdout());
    let terminal = Terminal::new(backend);
    
    match terminal {
        Ok(t) => {
            unsafe { TERMINAL = Some(t); }
            true
        },
        Err(_) => false,
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn ratatui_cleanup() {
    unsafe { TERMINAL = None; }
    let _ = disable_raw_mode();
    let _ = execute!(io::stdout(), LeaveAlternateScreen);
}

#[unsafe(no_mangle)]
pub extern "C" fn ratatui_poll_input(input: *mut InputState) -> bool {
    if input.is_null() { return false; }
    
    let input_state = unsafe { &mut *input };
    
    // Reset all inputs
    *input_state = InputState {
        p1_left: false, p1_right: false, p1_accel: false, p1_brake: false, p1_boost: false,
        p2_left: false, p2_right: false, p2_accel: false, p2_brake: false, p2_boost: false,
        quit: false, pause: false, menu: false,
    };
    
    if event::poll(std::time::Duration::from_millis(16)).unwrap_or(false) {
        if let Ok(Event::Key(key)) = event::read() {
            match key.code {
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
    }
    
    true
}

#[unsafe(no_mangle)]
pub extern "C" fn ratatui_render(state: *const GameState) -> bool {
    if state.is_null() { return false; }
    
    let game_state = unsafe { &*state };
    
    let terminal = unsafe {
        match std::ptr::addr_of_mut!(TERMINAL).as_mut() {
            Some(Some(t)) => t,
            _ => return false,
        }
    };
    
    let result = terminal.draw(|f| {
        let size = f.area();
        
        // Different layouts based on game mode
        if game_state.game_mode == 1 && game_state.player2_active {
            // Split-screen multiplayer
            render_splitscreen(f, size, game_state);
        } else if game_state.game_mode == 2 {
            // Career mode with extra UI
            render_career_mode(f, size, game_state);
        } else if game_state.game_mode == 3 {
            // Replay mode
            render_replay_mode(f, size, game_state);
        } else {
            // Single player
            render_singleplayer(f, size, game_state);
        }
    });
    
    result.is_ok()
}

fn render_singleplayer(f: &mut ratatui::Frame, area: Rect, state: &GameState) {
    let chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([
            Constraint::Length(6),   // Enhanced HUD
            Constraint::Min(0),      // Game area
            Constraint::Length(2),   // Controls
        ])
        .split(area);
    
    render_enhanced_hud(f, chunks[0], state, true);
    render_track(f, chunks[1], state, state.player_position, state.player_distance, true);
    render_controls(f, chunks[2], state.game_mode);
}

fn render_splitscreen(f: &mut ratatui::Frame, area: Rect, state: &GameState) {
    let h_chunks = Layout::default()
        .direction(Direction::Horizontal)
        .constraints([Constraint::Percentage(50), Constraint::Percentage(50)])
        .split(area);
    
    // Player 1 side
    let p1_chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([Constraint::Length(4), Constraint::Min(0), Constraint::Length(2)])
        .split(h_chunks[0]);
    
    render_player_hud(f, p1_chunks[0], state, 1);
    render_track(f, p1_chunks[1], state, state.player_position, state.player_distance, true);
    
    // Player 2 side
    let p2_chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([Constraint::Length(4), Constraint::Min(0), Constraint::Length(2)])
        .split(h_chunks[1]);
    
    render_player_hud(f, p2_chunks[0], state, 2);
    render_track(f, p2_chunks[1], state, state.player2_position, state.player2_distance, false);
    
    render_controls(f, p1_chunks[2], 1);
}

fn render_career_mode(f: &mut ratatui::Frame, area: Rect, state: &GameState) {
    let chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([
            Constraint::Length(7),   // Career HUD
            Constraint::Min(0),      // Game area
            Constraint::Length(3),   // Career info
        ])
        .split(area);
    
    render_career_hud(f, chunks[0], state);
    render_track(f, chunks[1], state, state.player_position, state.player_distance, true);
    render_career_info(f, chunks[2], state);
}

fn render_replay_mode(f: &mut ratatui::Frame, area: Rect, state: &GameState) {
    let chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([
            Constraint::Length(3),   // Replay controls
            Constraint::Min(0),      // Game area
            Constraint::Length(2),   // Replay info
        ])
        .split(area);
    
    render_replay_controls(f, chunks[0]);
    render_track(f, chunks[1], state, state.player_position, state.player_distance, true);
    render_replay_info(f, chunks[2], state);
}

fn render_enhanced_hud(f: &mut ratatui::Frame, area: Rect, state: &GameState, _show_all: bool) {
    let rows = Layout::default()
        .direction(Direction::Vertical)
        .constraints([Constraint::Length(2), Constraint::Length(4)])
        .split(area);
    
    // Top stats
    let top = Layout::default()
        .direction(Direction::Horizontal)
        .constraints([
            Constraint::Percentage(25),
            Constraint::Percentage(25),
            Constraint::Percentage(25),
            Constraint::Percentage(25),
        ])
        .split(rows[0]);
    
    let score_text = Paragraph::new(format!("⭐ {:08}", state.player_score))
        .style(Style::default().fg(Color::Yellow).add_modifier(Modifier::BOLD));
    f.render_widget(score_text, top[0]);
    
    let combo_color = get_combo_color(state.combo);
    let combo_text = Paragraph::new(format!("🔥x{}", state.combo))
        .style(Style::default().fg(combo_color).add_modifier(Modifier::BOLD));
    f.render_widget(combo_text, top[1]);
    
    let health_percent = (state.player_health as f32 / 100.0 * 100.0) as u16;
    let health_color = get_health_color(state.player_health);
    let health_bar = Gauge::default()
        .gauge_style(Style::default().fg(health_color))
        .percent(health_percent)
        .label(format!("❤ {}/100", state.player_health));
    f.render_widget(health_bar, top[2]);
    
    let level_text = Paragraph::new(format!("LV.{}", state.level))
        .style(Style::default().fg(Color::Cyan));
    f.render_widget(level_text, top[3]);
    
    // Bottom gauges
    let bottom = Layout::default()
        .direction(Direction::Horizontal)
        .constraints([
            Constraint::Percentage(20),
            Constraint::Percentage(20),
            Constraint::Percentage(20),
            Constraint::Percentage(20),
            Constraint::Percentage(20),
        ])
        .split(rows[1]);
    
    render_speed_gauge(f, bottom[0], state.player_speed, state.boost_active);
    render_powerup_gauge(f, bottom[1], "⚡BOOST", state.boost_remaining, state.boost_active, Color::Magenta);
    render_powerup_gauge(f, bottom[2], "🛡SHIELD", state.shield_remaining, state.shield_active, Color::Cyan);
    render_powerup_gauge(f, bottom[3], "⭐STAR", state.invincibility_remaining, state.invincibility_active, Color::Yellow);
    render_powerup_gauge(f, bottom[4], "🧲MAG", state.magnet_remaining, state.magnet_active, Color::Green);
}

fn render_player_hud(f: &mut ratatui::Frame, area: Rect, state: &GameState, player: i32) {
    let (score, health, speed) = if player == 1 {
        (state.player_score, state.player_health, state.player_speed)
    } else {
        (state.player2_score, state.player2_health, state.player2_speed)
    };
    
    let chunks = Layout::default()
        .direction(Direction::Horizontal)
        .constraints([
            Constraint::Percentage(33),
            Constraint::Percentage(33),
            Constraint::Percentage(34),
        ])
        .split(area);
    
    let title = format!("P{}", player);
    let score_text = Paragraph::new(format!("{}\n{:06}", title, score))
        .block(Block::default().borders(Borders::ALL))
        .style(Style::default().fg(Color::Yellow));
    f.render_widget(score_text, chunks[0]);
    
    let health_percent = (health as f32 / 100.0 * 100.0) as u16;
    let health_bar = Gauge::default()
        .block(Block::default().borders(Borders::ALL).title("HP"))
        .gauge_style(Style::default().fg(get_health_color(health)))
        .percent(health_percent);
    f.render_widget(health_bar, chunks[1]);
    
    let speed_text = Paragraph::new(format!("{:.0}\nkm/h", speed))
        .block(Block::default().borders(Borders::ALL).title("SPD"))
        .style(Style::default().fg(Color::Cyan));
    f.render_widget(speed_text, chunks[2]);
}

fn render_career_hud(f: &mut ratatui::Frame, area: Rect, state: &GameState) {
    render_enhanced_hud(f, area, state, true);
}

fn render_track(f: &mut ratatui::Frame, area: Rect, state: &GameState, player_pos: i32, player_dist: f32, _is_primary: bool) {
    let track_name = get_track_name(state.track_type);
    let weather_icon = get_weather_icon(state.weather);
    
    let title = if state.replay_mode {
        format!("═══ {} ═══ {} ═══ [REPLAY] ═══", track_name, weather_icon)
    } else {
        format!("═══ {} ═══ {} ═══", track_name, weather_icon)
    };
    
    let block = Block::default()
        .borders(Borders::ALL)
        .title(title)
        .style(get_track_style(state.track_type, state.weather, state.tunnel_darkness));
    
    let inner = block.inner(area);
    f.render_widget(block, area);
    
    // Track-specific rendering
    match state.track_type {
        1 => render_city_track(f, inner, state, player_pos, player_dist),
        2 => render_mountain_track(f, inner, state, player_pos, player_dist),
        3 => render_desert_track(f, inner, state, player_pos, player_dist),
        4 => render_tunnel_track(f, inner, state, player_pos, player_dist),
        _ => render_highway_track(f, inner, state, player_pos, player_dist),
    }
}

fn render_highway_track(f: &mut ratatui::Frame, area: Rect, state: &GameState, player_pos: i32, player_dist: f32) {
    let lane_width = area.width / 3;
    let road_height = area.height;
    let curve = (state.curve_offset * 3.0) as i16;
    
    render_road_base(f, area, state);
    render_lane_markers(f, area, state, lane_width, road_height, curve);
    render_objects(f, area, state, lane_width, road_height, curve, player_dist);
    render_player(f, area, state, player_pos, lane_width, curve, state.player_car_type, true);
    
    if state.replay_mode && state.ghost_distance > 0.0 {
        render_ghost(f, area, state, lane_width, road_height, curve, player_dist);
    }
    
    render_weather_overlay(f, area, state);
}

fn render_city_track(f: &mut ratatui::Frame, area: Rect, state: &GameState, player_pos: i32, player_dist: f32) {
    let lane_width = area.width / 3;
    let road_height = area.height;
    let curve = (state.curve_offset * 2.0) as i16;
    
    render_road_base(f, area, state);
    render_buildings(f, area, state, player_dist);
    render_lane_markers(f, area, state, lane_width, road_height, curve);
    render_objects(f, area, state, lane_width, road_height, curve, player_dist);
    render_player(f, area, state, player_pos, lane_width, curve, state.player_car_type, true);
    render_weather_overlay(f, area, state);
}

fn render_mountain_track(f: &mut ratatui::Frame, area: Rect, state: &GameState, player_pos: i32, player_dist: f32) {
    let lane_width = area.width / 3;
    let road_height = area.height;
    let curve = (state.curve_offset * 4.0) as i16;
    let elevation = (state.elevation * 5.0) as i16;
    
    render_mountain_bg(f, area, elevation);
    render_road_base(f, area, state);
    render_lane_markers(f, area, state, lane_width, road_height, curve);
    render_objects(f, area, state, lane_width, road_height, curve, player_dist);
    render_player(f, area, state, player_pos, lane_width, curve, state.player_car_type, true);
    render_weather_overlay(f, area, state);
}

fn render_desert_track(f: &mut ratatui::Frame, area: Rect, state: &GameState, player_pos: i32, player_dist: f32) {
    let lane_width = area.width / 3;
    let road_height = area.height;
    let curve = (state.curve_offset * 2.5) as i16;
    
    render_desert_bg(f, area, player_dist);
    render_road_base(f, area, state);
    render_lane_markers(f, area, state, lane_width, road_height, curve);
    render_objects(f, area, state, lane_width, road_height, curve, player_dist);
    render_player(f, area, state, player_pos, lane_width, curve, state.player_car_type, true);
    render_weather_overlay(f, area, state);
}

fn render_tunnel_track(f: &mut ratatui::Frame, area: Rect, state: &GameState, player_pos: i32, player_dist: f32) {
    let lane_width = area.width / 3;
    let road_height = area.height;
    let curve = (state.curve_offset * 1.5) as i16;
    
    render_tunnel_walls(f, area, state, player_dist);
    render_road_base(f, area, state);
    render_lane_markers(f, area, state, lane_width, road_height, curve);
    render_objects(f, area, state, lane_width, road_height, curve, player_dist);
    render_player(f, area, state, player_pos, lane_width, curve, state.player_car_type, true);
}

fn render_road_base(f: &mut ratatui::Frame, area: Rect, state: &GameState) {
    let road_char = match state.weather {
        1 => "▒",
        2 => "░",
        _ => "▓",
    };
    
    let offset = (state.player_distance as u16) % 2;
    for y in (0..area.height).step_by(2) {
        let actual_y = (y + offset) % area.height;
        if actual_y + area.y < area.bottom() {
            let road_line = road_char.repeat(area.width as usize);
            f.render_widget(
                Paragraph::new(road_line).style(Style::default().fg(Color::DarkGray)),
                Rect::new(area.x, area.y + actual_y, area.width, 1),
            );
        }
    }
}

fn render_objects(f: &mut ratatui::Frame, area: Rect, state: &GameState, lane_width: u16, height: u16, curve: i16, player_dist: f32) {
    // Render AI cars
    if state.car_count > 0 && !state.ai_positions.is_null() {
        let positions = unsafe { std::slice::from_raw_parts(state.ai_positions, state.car_count as usize) };
        let distances = unsafe { std::slice::from_raw_parts(state.ai_distances, state.car_count as usize) };
        let types = unsafe { std::slice::from_raw_parts(state.ai_types, state.car_count as usize) };
        let is_boss = unsafe { std::slice::from_raw_parts(state.ai_is_boss, state.car_count as usize) };
        
        for i in 0..state.car_count as usize {
            let rel_dist = distances[i] - player_dist;
            if rel_dist > -10.0 && rel_dist < 50.0 {
                let screen_y = area.y + height - ((rel_dist + 10.0) * height as f32 / 60.0) as u16;
                let base_x = area.x + (positions[i] as u16 * lane_width) + lane_width / 2 - 3;
                let x = (base_x as i16 + curve).max(area.x as i16) as u16;
                
                if screen_y < area.bottom() - 1 {
                    render_car(f, x, screen_y, types[i], is_boss[i]);
                }
            }
        }
    }
    
    // Render obstacles/powerups
    if state.obstacle_count > 0 && !state.obstacle_positions.is_null() {
        let positions = unsafe { std::slice::from_raw_parts(state.obstacle_positions, state.obstacle_count as usize) };
        let distances = unsafe { std::slice::from_raw_parts(state.obstacle_distances, state.obstacle_count as usize) };
        let types = unsafe { std::slice::from_raw_parts(state.obstacle_types, state.obstacle_count as usize) };
        
        for i in 0..state.obstacle_count as usize {
            let rel_dist = distances[i] - player_dist;
            if rel_dist > -5.0 && rel_dist < 60.0 {
                let screen_y = area.y + height - ((rel_dist + 5.0) * height as f32 / 65.0) as u16;
                let base_x = area.x + (positions[i] as u16 * lane_width) + lane_width / 2 - 1;
                let x = (base_x as i16 + curve).max(area.x as i16) as u16;
                
                if screen_y < area.bottom() - 1 {
                    render_powerup(f, x, screen_y, types[i]);
                }
            }
        }
    }
}

fn render_car(f: &mut ratatui::Frame, x: u16, y: u16, car_type: i32, is_boss: bool) {
    let (car_art, color, label) = get_car_design(car_type, is_boss);
    
    for (i, line) in car_art.iter().enumerate() {
        f.render_widget(
            Paragraph::new(*line).style(Style::default().fg(color).add_modifier(if is_boss { Modifier::BOLD | Modifier::RAPID_BLINK } else { Modifier::BOLD })),
            Rect::new(x, y + i as u16, 7, 1),
        );
    }
    
    if !label.is_empty() {
        f.render_widget(
            Paragraph::new(label).style(Style::default().fg(Color::Black).bg(color).add_modifier(Modifier::BOLD)),
            Rect::new(x + 1, y + 3, 5, 1),
        );
    }
}

fn render_player(f: &mut ratatui::Frame, area: Rect, state: &GameState, position: i32, lane_width: u16, curve: i16, car_type: i32, is_p1: bool) {
    let base_x = area.x + (position as u16 * lane_width) + lane_width / 2 - 3;
    let x = (base_x as i16 + curve).max(area.x as i16) as u16;
    let y = area.bottom() - 7;
    
    let (car_art, mut color, _) = get_car_design(car_type, false);
    
    // Color modifiers for powerups
    if state.invincibility_active {
        color = Color::Yellow;
    } else if state.boost_active {
        color = Color::Magenta;
    } else if state.shield_active {
        color = Color::Cyan;
    } else if is_p1 {
        color = Color::Green;
    }
    
    // Render car
    for (i, line) in car_art.iter().enumerate() {
        f.render_widget(
            Paragraph::new(*line).style(Style::default().fg(color).add_modifier(Modifier::BOLD)),
            Rect::new(x, y + i as u16, 7, 1),
        );
    }
    
    // Effects
    if state.boost_active {
        f.render_widget(
            Paragraph::new("🔥🔥").style(Style::default().fg(Color::Red).add_modifier(Modifier::RAPID_BLINK)),
            Rect::new(x + 1, y + 4, 4, 1),
        );
    }
    
    if state.shield_active {
        f.render_widget(
            Paragraph::new(" ◯◯◯ ").style(Style::default().fg(Color::Cyan).add_modifier(Modifier::BOLD)),
            Rect::new(x.saturating_sub(1), y.saturating_sub(1), 9, 1),
        );
    }
    
    if state.invincibility_active {
        f.render_widget(
            Paragraph::new("✨⭐✨").style(Style::default().fg(Color::Yellow).add_modifier(Modifier::RAPID_BLINK)),
            Rect::new(x.saturating_sub(1), y.saturating_sub(1), 9, 1),
        );
    }
    
    if state.magnet_active {
        f.render_widget(
            Paragraph::new("🧲").style(Style::default().fg(Color::Red)),
            Rect::new(x + 6, y + 1, 2, 1),
        );
    }
}

fn render_ghost(f: &mut ratatui::Frame, area: Rect, state: &GameState, lane_width: u16, height: u16, curve: i16, player_dist: f32) {
    let rel_dist = state.ghost_distance - player_dist;
    if rel_dist > -10.0 && rel_dist < 50.0 {
        let screen_y = area.y + height - ((rel_dist + 10.0) * height as f32 / 60.0) as u16;
        let base_x = area.x + (state.ghost_position as u16 * lane_width) + lane_width / 2 - 3;
        let x = (base_x as i16 + curve).max(area.x as i16) as u16;
        
        if screen_y < area.bottom() - 1 {
            let (ghost_art, _, _) = get_car_design(state.player_car_type, false);
            for (i, line) in ghost_art.iter().enumerate() {
                f.render_widget(
                    Paragraph::new(*line).style(Style::default().fg(Color::Rgb(150, 150, 200)).add_modifier(Modifier::DIM)),
                    Rect::new(x, screen_y + i as u16, 7, 1),
                );
            }
        }
    }
}

fn render_powerup(f: &mut ratatui::Frame, x: u16, y: u16, ptype: i32) {
    let (icon, color) = match ptype {
        0 => ("🚧", Color::Yellow),
        1 => ("💧", Color::Blue),
        2 => ("⚡", Color::Magenta),
        3 => ("⭐", Color::Yellow),
        4 => ("🧲", Color::Red),
        5 => ("🕐", Color::Cyan),
        _ => ("⚠", Color::Red),
    };
    
    f.render_widget(
        Paragraph::new(icon).style(Style::default().fg(color).add_modifier(Modifier::BOLD)),
        Rect::new(x, y, 2, 1),
    );
}

fn render_weather_overlay(f: &mut ratatui::Frame, area: Rect, state: &GameState) {
    match state.weather {
        1 => { // Rain
            let spacing = 7;
            let offset = (state.player_distance as u16) % spacing;
            for x in (0..area.width).step_by(spacing as usize) {
                for y in (0..area.height).step_by(3) {
                    let drop_y = (y + offset) % area.height;
                    if drop_y + area.y < area.bottom() {
                        f.render_widget(
                            Paragraph::new("·").style(Style::default().fg(Color::Rgb(100, 150, 200))),
                            Rect::new(area.x + x, area.y + drop_y, 1, 1),
                        );
                    }
                }
            }
        },
        2 => { // Fog - additional overlay
            // Fog is mainly handled by background color
        },
        _ => {},
    }
    
    if state.slowmo_active {
        // Slowmo visual effect - motion blur lines
        for y in (0..area.height).step_by(4) {
            f.render_widget(
                Paragraph::new("━").style(Style::default().fg(Color::Rgb(80, 80, 150))),
                Rect::new(area.x + 2, area.y + y, 3, 1),
            );
        }
    }
}

fn render_speed_gauge(f: &mut ratatui::Frame, area: Rect, speed: f32, boosting: bool) {
    let max_speed = if boosting { 250.0 } else { 200.0 };
    let percent = (speed / max_speed * 100.0).min(100.0) as u16;
    let color = if boosting { Color::Magenta } else if speed > 180.0 { Color::Red } else if speed > 120.0 { Color::Yellow } else { Color::Cyan };
    
    let gauge = Gauge::default()
        .block(Block::default().borders(Borders::ALL).title("🏎SPD"))
        .gauge_style(Style::default().fg(color))
        .percent(percent)
        .label(format!("{:.0}", speed));
    f.render_widget(gauge, area);
}

fn render_powerup_gauge(f: &mut ratatui::Frame, area: Rect, title: &str, remaining: f32, active: bool, color: Color) {
    let percent = remaining.max(0.0).min(100.0) as u16;
    let style = if active {
        Style::default().fg(color).add_modifier(Modifier::RAPID_BLINK)
    } else {
        Style::default().fg(if percent > 0u16 { color } else { Color::DarkGray })
    };
    
    let gauge = Gauge::default()
        .block(Block::default().borders(Borders::ALL).title(title))
        .gauge_style(style)
        .percent(percent);
    f.render_widget(gauge, area);
}

fn render_career_info(f: &mut ratatui::Frame, area: Rect, state: &GameState) {
    let chunks = Layout::default()
        .direction(Direction::Horizontal)
        .constraints([Constraint::Percentage(50), Constraint::Percentage(50)])
        .split(area);
    
    let progress_text = format!("Progress: {:.0}%", state.career_progress);
    let progress = Paragraph::new(progress_text)
        .block(Block::default().borders(Borders::ALL).title("Career"))
        .style(Style::default().fg(Color::Yellow));
    f.render_widget(progress, chunks[0]);
    
    let objective = match state.level {
        1..=3 => "Complete race",
        4..=6 => "Beat AI racers",
        7..=9 => "Defeat boss",
        _ => "Ultimate challenge",
    };
    
    let obj_text = Paragraph::new(objective)
        .block(Block::default().borders(Borders::ALL).title("Objective"))
        .style(Style::default().fg(Color::Cyan));
    f.render_widget(obj_text, chunks[1]);
}

fn render_replay_controls(f: &mut ratatui::Frame, area: Rect) {
    let text = Paragraph::new("⏮ [←] Rewind | [SPACE] Pause | [→] Fast Forward ⏭")
        .block(Block::default().borders(Borders::ALL).title("REPLAY MODE"))
        .style(Style::default().fg(Color::Magenta).add_modifier(Modifier::BOLD))
        .alignment(Alignment::Center);
    f.render_widget(text, area);
}

fn render_replay_info(f: &mut ratatui::Frame, area: Rect, state: &GameState) {
    let text = format!("Time: {:.2}s | Best: Ghost Car", state.lap_time);
    let info = Paragraph::new(text)
        .block(Block::default().borders(Borders::ALL))
        .style(Style::default().fg(Color::Cyan));
    f.render_widget(info, area);
}

fn render_controls(f: &mut ratatui::Frame, area: Rect, mode: i32) {
    let text = match mode {
        1 => "P1: WASD+SPACE | P2: IJKL+U | Q=Quit",
        2 => "← → Move | ↑ Accel | ↓ Brake | SPACE Boost | M Menu | Q Quit",
        _ => "← → Move | ↑ Accel | ↓ Brake | SPACE Boost | P Pause | Q Quit",
    };
    
    let controls = Paragraph::new(text)
        .block(Block::default().borders(Borders::ALL))
        .style(Style::default().fg(Color::DarkGray));
    f.render_widget(controls, area);
}

// Helper functions

fn get_car_design(car_type: i32, is_boss: bool) -> ([&'static str; 4], Color, &'static str) {
    if is_boss {
        return ([
            " ▄███▄ ",
            "███████",
            "▐██▌██▌",
            " BOSS! ",
        ], Color::Red, "BOSS!");
    }
    
    match car_type {
        0 => ([ // Sports car
            "  ▄█▄  ",
            " █████ ",
            " ▐█▌█▌ ",
            "  YOU  ",
        ], Color::Green, "YOU"),
        1 => ([ // Police
            "  ▄█▄  ",
            " █🚨█ ",
            " ▐█▌█▌ ",
            "  🚔  ",
        ], Color::Blue, "POL"),
        2 => ([ // Racer
            "  ▀█▀  ",
            " █████ ",
            " ▐██▌▌ ",
            "  🏁  ",
        ], Color::Magenta, "RCR"),
        3 => ([ // Truck
            " ▄███▄ ",
            "███████",
            "▐██▌██▌",
            " TRUCK ",
        ], Color::Yellow, "TRK"),
        4 => ([ // Taxi
            "  ▄█▄  ",
            " █▓▓█ ",
            " ▐█▌█▌ ",
            " TAXI ",
        ], Color::Yellow, "TXI"),
        5 => ([ // Van
            " ▄███▄ ",
            "███▓███",
            "▐█▌▌█▌ ",
            "  VAN  ",
        ], Color::Rgb(150, 150, 150), "VAN"),
        6 => ([ // Muscle car
            "  ▄█▄  ",
            " ▓███▓ ",
            " ▐██▌▌ ",
            " MSCL ",
        ], Color::Red, "MSC"),
        7 => ([ // Convertible
            "  ─█─  ",
            " █▒▒█ ",
            " ▐█▌█▌ ",
            " CONV ",
        ], Color::Cyan, "CNV"),
        8 => ([ // Limo
            "▄█████▄",
            "███████",
            "▐█▌▌▌█▌",
            " LIMO! ",
        ], Color::Black, "LMO"),
        _ => ([ // Default
            "  ▄█▄  ",
            " █████ ",
            " ▐█▌█▌ ",
            "  CAR  ",
        ], Color::Gray, "CAR"),
    }
}

fn get_combo_color(combo: i32) -> Color {
    match combo {
        0..=2 => Color::White,
        3..=5 => Color::Cyan,
        6..=10 => Color::Yellow,
        11..=20 => Color::Magenta,
        _ => Color::Red,
    }
}

fn get_health_color(health: i32) -> Color {
    if health > 66 { Color::Green }
    else if health > 33 { Color::Yellow }
    else { Color::Red }
}

fn get_track_name(track_type: i32) -> &'static str {
    match track_type {
        1 => "CITY STREETS",
        2 => "MOUNTAIN PASS",
        3 => "DESERT HIGHWAY",
        4 => "UNDERGROUND TUNNEL",
        _ => "HIGHWAY RUSH",
    }
}

fn get_weather_icon(weather: i32) -> &'static str {
    match weather {
        1 => "🌧RAIN",
        2 => "🌫FOG",
        3 => "🌙NIGHT",
        _ => "☀CLEAR",
    }
}

fn get_track_style(track_type: i32, weather: i32, darkness: f32) -> Style {
    let base_color = match track_type {
        1 => Color::Rgb(30, 30, 40),   // City
        2 => Color::Rgb(25, 35, 25),   // Mountain
        3 => Color::Rgb(50, 40, 20),   // Desert
        4 => {                          // Tunnel
            let brightness = ((1.0 - darkness) * 30.0) as u8;
            Color::Rgb(brightness, brightness, brightness)
        },
        _ => Color::Black,              // Highway
    };
    
    let weather_adjusted = match weather {
        1 => Color::Rgb(20, 30, 50),   // Rain
        2 => Color::Rgb(40, 40, 40),   // Fog
        3 => Color::Rgb(10, 10, 30),   // Night
        _ => base_color,
    };
    
    Style::default().bg(weather_adjusted)
}

// Menu rendering function
#[unsafe(no_mangle)]
pub extern "C" fn ratatui_render_menu(
    title: *const std::os::raw::c_char,
    options: *const *const std::os::raw::c_char,
    option_count: i32,
    selected: i32,
) -> bool {
    let terminal = unsafe {
        match std::ptr::addr_of_mut!(TERMINAL).as_mut() {
            Some(Some(t)) => t,
            _ => return false,
        }
    };
    
    let result = terminal.draw(|f| {
        let size = f.area();
        
        let chunks = Layout::default()
            .direction(Direction::Vertical)
            .constraints([
                Constraint::Length(5),
                Constraint::Min(0),
                Constraint::Length(3),
            ])
            .split(size);
        
        // Title
        let title_str = unsafe { std::ffi::CStr::from_ptr(title).to_str().unwrap_or("MENU") };
        let title_widget = Paragraph::new(title_str)
            .block(Block::default().borders(Borders::ALL))
            .style(Style::default().fg(Color::Cyan).add_modifier(Modifier::BOLD))
            .alignment(Alignment::Center);
        f.render_widget(title_widget, chunks[0]);
        
        // Menu options
        let mut items = Vec::new();
        for i in 0..option_count {
            let option_ptr = unsafe { *options.offset(i as isize) };
            let option_str = unsafe { std::ffi::CStr::from_ptr(option_ptr).to_str().unwrap_or("") };
            
            let style = if i == selected {
                Style::default().fg(Color::Yellow).bg(Color::Blue).add_modifier(Modifier::BOLD)
            } else {
                Style::default().fg(Color::White)
            };
            
            let prefix = if i == selected { "▶ " } else { "  " };
            items.push(ListItem::new(format!("{}{}", prefix, option_str)).style(style));
        }
        
        let list = List::new(items)
            .block(Block::default().borders(Borders::ALL).title("Select Option"));
        f.render_widget(list, chunks[1]);
        
        // Controls
        let controls = Paragraph::new("↑↓ Navigate | ENTER Select | Q Quit")
            .block(Block::default().borders(Borders::ALL))
            .style(Style::default().fg(Color::DarkGray));
        f.render_widget(controls, chunks[2]);
    });
    
    result.is_ok()
}

fn render_lane_markers(f: &mut ratatui::Frame, area: Rect, state: &GameState, lane_width: u16, height: u16, curve: i16) {
    let offset = (state.player_distance as u16 * 2) % 6;
    
    for y in (0..height).step_by(3) {
        let actual_y = (y + offset) % height;
        
        for lane in 1..3 {
            let base_x = area.x + lane * lane_width;
            let curved_x = (base_x as i16 + curve).max(area.x as i16).min((area.x + area.width) as i16) as u16;
            
            if actual_y + area.y < area.bottom() && curved_x < area.x + area.width {
                f.render_widget(
                    Paragraph::new("┃").style(Style::default().fg(Color::White)),
                    Rect::new(curved_x, area.y + actual_y, 1, 2),
                );
            }
        }
    }
}

fn render_buildings(f: &mut ratatui::Frame, area: Rect, state: &GameState, player_dist: f32) {
    if state.building_count == 0 || state.building_positions.is_null() { return; }
    
    let positions = unsafe { std::slice::from_raw_parts(state.building_positions, state.building_count as usize) };
    let distances = unsafe { std::slice::from_raw_parts(state.building_distances, state.building_count as usize) };
    let heights = unsafe { std::slice::from_raw_parts(state.building_heights, state.building_count as usize) };
    let types = unsafe { std::slice::from_raw_parts(state.building_types, state.building_count as usize) };
    
    for i in 0..state.building_count as usize {
        let rel_dist = distances[i] - player_dist;
        if rel_dist > -20.0 && rel_dist < 60.0 {
            let screen_y = area.y + area.height - ((rel_dist + 20.0) * area.height as f32 / 80.0) as u16;
            let building_height = heights[i].min(15) as u16;
            
            let x = if positions[i] < 0 { 
                area.x 
            } else { 
                area.x + area.width - 8 
            };
            
            if screen_y < area.bottom() {
                render_building(f, x, screen_y, building_height, types[i]);
            }
        }
    }
}

fn render_building(f: &mut ratatui::Frame, x: u16, y: u16, height: u16, btype: i32) {
    let (char_set, color) = match btype {
        1 => ("▓▓▓▓▓▓", Color::Rgb(100, 100, 150)),  // Glass building
        2 => ("██████", Color::Rgb(80, 80, 80)),     // Concrete
        3 => ("▒▒▒▒▒▒", Color::Rgb(120, 90, 70)),    // Brick
        _ => ("▓▓▓▓▓▓", Color::Rgb(60, 60, 90)),     // Default
    };
    
    for i in 0..height.min(10) {
        if y >= i {
            f.render_widget(
                Paragraph::new(char_set).style(Style::default().fg(color)),
                Rect::new(x, y - i, 6, 1),
            );
            
            // Windows
            if i % 2 == 0 && btype == 1 {
                f.render_widget(
                    Paragraph::new("▫▫").style(Style::default().fg(Color::Yellow)),
                    Rect::new(x + 2, y - i, 2, 1),
                );
            }
        }
    }
}

fn render_mountain_bg(f: &mut ratatui::Frame, area: Rect, elevation: i16) {
    for x in (0..area.width).step_by(5) {
        let peak_y = (area.height / 3) + (elevation.abs() % 10) as u16;
        f.render_widget(
            Paragraph::new("▲").style(Style::default().fg(Color::Rgb(100, 100, 100))),
            Rect::new(area.x + x, area.y + peak_y, 1, 1),
        );
    }
}

fn render_desert_bg(f: &mut ratatui::Frame, area: Rect, distance: f32) {
    let cactus_spacing = 15;
    for x in (0..area.width).step_by(cactus_spacing) {
        let offset = (distance as u16 + x) % (cactus_spacing as u16);
        if offset < 3 {
            f.render_widget(
                Paragraph::new("🌵").style(Style::default().fg(Color::Green)),
                Rect::new(area.x + x, area.bottom() - 8, 2, 1),
            );
        }
    }
}

fn render_tunnel_walls(f: &mut ratatui::Frame, area: Rect, state: &GameState, distance: f32) {
    let light_spacing = 10;
    let light_offset = (distance as u16) % light_spacing;
    
    for y in 0..area.height {
        // Left wall
        f.render_widget(
            Paragraph::new("▌").style(Style::default().fg(Color::Rgb(40, 40, 40))),
            Rect::new(area.x, area.y + y, 1, 1),
        );
        
        // Right wall  
        f.render_widget(
            Paragraph::new("▐").style(Style::default().fg(Color::Rgb(40, 40, 40))),
            Rect::new(area.x + area.width - 1, area.y + y, 1, 1),
        );
        
        // Ceiling lights
        if y % light_spacing == light_offset {
            let brightness = 1.0 - state.tunnel_darkness;
            let light_color = Color::Rgb(
                (255.0 * brightness) as u8,
                (255.0 * brightness) as u8,
                (200.0 * brightness) as u8,
            );
            f.render_widget(
                Paragraph::new("•").style(Style::default().fg(light_color)),
                Rect::new(area.x + area.width / 2, area.y + y, 1, 1),
            );
        }
    }
}