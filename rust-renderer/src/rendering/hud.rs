/// HUD (Heads-Up Display) module for UI elements
/// Handles rendering of gauges, stats, and game information

use ratatui::{
    layout::{Alignment, Constraint, Direction, Layout, Rect},
    style::{Color, Modifier, Style},
    widgets::{Block, Borders, Gauge, List, ListItem, Paragraph},
    Frame,
};
use crate::core::types::GameState;
use crate::core::utils::{get_combo_color, get_health_color};

/// Render single-player HUD with full stats
pub fn render_singleplayer_hud(f: &mut Frame, area: Rect, state: &GameState) {
    let chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([
            Constraint::Length(6),   // Enhanced HUD
            Constraint::Min(0),      // Game area
            Constraint::Length(2),   // Controls
        ])
        .split(area);
    
    render_enhanced_hud(f, chunks[0], state);
    // Track rendering handled elsewhere
    render_controls(f, chunks[2], state.game_mode);
}

/// Render split-screen multiplayer HUD
pub fn render_splitscreen_hud(f: &mut Frame, area: Rect, state: &GameState) {
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
    // Track rendering handled elsewhere
    
    // Player 2 side
    let p2_chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([Constraint::Length(4), Constraint::Min(0), Constraint::Length(2)])
        .split(h_chunks[1]);
    
    render_player_hud(f, p2_chunks[0], state, 2);
    // Track rendering handled elsewhere
    
    render_controls(f, p1_chunks[2], 1);
}

/// Render career mode HUD with progress tracking
pub fn render_career_hud(f: &mut Frame, area: Rect, state: &GameState) {
    let chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([
            Constraint::Length(7),   // Career HUD
            Constraint::Min(0),      // Game area
            Constraint::Length(3),   // Career info
        ])
        .split(area);
    
    render_enhanced_hud(f, chunks[0], state);
    // Track rendering handled elsewhere
    render_career_info(f, chunks[2], state);
}

/// Render replay mode HUD with playback controls
pub fn render_replay_hud(f: &mut Frame, area: Rect, state: &GameState) {
    let chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([
            Constraint::Length(3),   // Replay controls
            Constraint::Min(0),      // Game area
            Constraint::Length(2),   // Replay info
        ])
        .split(area);
    
    render_replay_controls(f, chunks[0]);
    // Track rendering handled elsewhere
    render_replay_info(f, chunks[2], state);
}

/// Render enhanced HUD with all stats and gauges
pub fn render_enhanced_hud(f: &mut Frame, area: Rect, state: &GameState) {
    let rows = Layout::default()
        .direction(Direction::Vertical)
        .constraints([Constraint::Length(2), Constraint::Length(4)])
        .split(area);
    
    // Top stats row
    let top = Layout::default()
        .direction(Direction::Horizontal)
        .constraints([
            Constraint::Percentage(25),
            Constraint::Percentage(25),
            Constraint::Percentage(25),
            Constraint::Percentage(25),
        ])
        .split(rows[0]);
    
    // Score
    let score_text = Paragraph::new(format!("⭐ {:08}", state.player_score))
        .style(Style::default().fg(Color::Yellow).add_modifier(Modifier::BOLD));
    f.render_widget(score_text, top[0]);
    
    // Combo
    let combo_color = get_combo_color(state.combo);
    let combo_text = Paragraph::new(format!("🔥x{}", state.combo))
        .style(Style::default().fg(combo_color).add_modifier(Modifier::BOLD));
    f.render_widget(combo_text, top[1]);
    
    // Health
    let health_percent = (state.player_health as f32 / 100.0 * 100.0) as u16;
    let health_color = get_health_color(state.player_health);
    let health_bar = Gauge::default()
        .gauge_style(Style::default().fg(health_color))
        .percent(health_percent)
        .label(format!("❤ {}/100", state.player_health));
    f.render_widget(health_bar, top[2]);
    
    // Level
    let level_text = Paragraph::new(format!("LV.{}", state.level))
        .style(Style::default().fg(Color::Cyan));
    f.render_widget(level_text, top[3]);
    
    // Bottom powerup gauges
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

/// Render individual player HUD for split-screen
pub fn render_player_hud(f: &mut Frame, area: Rect, state: &GameState, player: i32) {
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

/// Render speed gauge with dynamic coloring
fn render_speed_gauge(f: &mut Frame, area: Rect, speed: f32, boosting: bool) {
    let max_speed = if boosting { 250.0 } else { 200.0 };
    let percent = (speed / max_speed * 100.0).min(100.0) as u16;
    let color = if boosting {
        Color::Magenta
    } else if speed > 180.0 {
        Color::Red
    } else if speed > 120.0 {
        Color::Yellow
    } else {
        Color::Cyan
    };
    
    let gauge = Gauge::default()
        .block(Block::default().borders(Borders::ALL).title("🏎SPD"))
        .gauge_style(Style::default().fg(color))
        .percent(percent)
        .label(format!("{:.0}", speed));
    f.render_widget(gauge, area);
}

/// Render powerup duration gauge
fn render_powerup_gauge(
    f: &mut Frame,
    area: Rect,
    title: &str,
    remaining: f32,
    active: bool,
    color: Color,
) {
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

/// Render career mode progress information
pub fn render_career_info(f: &mut Frame, area: Rect, state: &GameState) {
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

/// Render replay mode playback controls
pub fn render_replay_controls(f: &mut Frame, area: Rect) {
    let text = Paragraph::new("⏮ [←] Rewind | [SPACE] Pause | [→] Fast Forward ⏭")
        .block(Block::default().borders(Borders::ALL).title("REPLAY MODE"))
        .style(Style::default().fg(Color::Magenta).add_modifier(Modifier::BOLD))
        .alignment(Alignment::Center);
    f.render_widget(text, area);
}

/// Render replay mode information
pub fn render_replay_info(f: &mut Frame, area: Rect, state: &GameState) {
    let text = format!("Time: {:.2}s | Best: Ghost Car", state.lap_time);
    let info = Paragraph::new(text)
        .block(Block::default().borders(Borders::ALL))
        .style(Style::default().fg(Color::Cyan));
    f.render_widget(info, area);
}

/// Render control instructions based on game mode
fn render_controls(f: &mut Frame, area: Rect, mode: i32) {
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

/// Render menu with title and options
pub fn render_menu(
    f: &mut Frame,
    title_str: &str,
    options: &[String],
    selected: i32,
) {
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
    let title_widget = Paragraph::new(title_str)
        .block(Block::default().borders(Borders::ALL))
        .style(Style::default().fg(Color::Cyan).add_modifier(Modifier::BOLD))
        .alignment(Alignment::Center);
    f.render_widget(title_widget, chunks[0]);
    
    // Menu options
    let mut items = Vec::new();
    for (i, option_str) in options.iter().enumerate() {
        let style = if i as i32 == selected {
            Style::default().fg(Color::Yellow).bg(Color::Blue).add_modifier(Modifier::BOLD)
        } else {
            Style::default().fg(Color::White)
        };
        
        let prefix = if i as i32 == selected { "▶ " } else { "  " };
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
}
