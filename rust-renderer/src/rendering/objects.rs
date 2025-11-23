/// Objects rendering module for cars, obstacles, and powerups
/// Handles rendering of player cars, AI cars, and interactive objects

use ratatui::{
    layout::Rect,
    style::{Color, Modifier, Style},
    widgets::Paragraph,
    Frame,
};
use crate::core::types::GameState;
use crate::core::utils::{get_car_design, get_powerup_icon};

/// Render all dynamic objects (AI cars and obstacles)
pub fn render_objects(
    f: &mut Frame,
    area: Rect,
    state: &GameState,
    lane_width: u16,
    height: u16,
    curve: i16,
    player_dist: f32,
) {
    render_ai_cars(f, area, state, lane_width, height, curve, player_dist);
    render_obstacles(f, area, state, lane_width, height, curve, player_dist);
}

/// Render all AI cars on the track
fn render_ai_cars(
    f: &mut Frame,
    area: Rect,
    state: &GameState,
    lane_width: u16,
    height: u16,
    curve: i16,
    player_dist: f32,
) {
    if state.car_count == 0 || state.ai_positions.is_null() {
        return;
    }
    
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

/// Render all obstacles and powerups on the track
fn render_obstacles(
    f: &mut Frame,
    area: Rect,
    state: &GameState,
    lane_width: u16,
    height: u16,
    curve: i16,
    player_dist: f32,
) {
    if state.obstacle_count == 0 || state.obstacle_positions.is_null() {
        return;
    }
    
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

/// Render an AI car with visual effects for boss cars
fn render_car(f: &mut Frame, x: u16, y: u16, car_type: i32, is_boss: bool) {
    let design = get_car_design(car_type, is_boss);
    
    for (i, line) in design.art.iter().enumerate() {
        let modifier = if is_boss {
            Modifier::BOLD | Modifier::RAPID_BLINK
        } else {
            Modifier::BOLD
        };
        
        f.render_widget(
            Paragraph::new(*line).style(Style::default().fg(design.color).add_modifier(modifier)),
            Rect::new(x, y + i as u16, 7, 1),
        );
    }
    
    if !design.label.is_empty() {
        f.render_widget(
            Paragraph::new(design.label)
                .style(Style::default().fg(Color::Black).bg(design.color).add_modifier(Modifier::BOLD)),
            Rect::new(x + 1, y + 3, 5, 1),
        );
    }
}

/// Render the player car with powerup visual effects
pub fn render_player(
    f: &mut Frame,
    area: Rect,
    state: &GameState,
    position: i32,
    lane_width: u16,
    curve: i16,
    car_type: i32,
    is_p1: bool,
) {
    let base_x = area.x + (position as u16 * lane_width) + lane_width / 2 - 3;
    let x = (base_x as i16 + curve).max(area.x as i16) as u16;
    let y = area.bottom() - 7;
    
    let mut design = get_car_design(car_type, false);
    
    // Color modifiers for powerups
    if state.invincibility_active {
        design.color = Color::Yellow;
    } else if state.boost_active {
        design.color = Color::Magenta;
    } else if state.shield_active {
        design.color = Color::Cyan;
    } else if is_p1 {
        design.color = Color::Green;
    }
    
    // Render car
    for (i, line) in design.art.iter().enumerate() {
        f.render_widget(
            Paragraph::new(*line).style(Style::default().fg(design.color).add_modifier(Modifier::BOLD)),
            Rect::new(x, y + i as u16, 7, 1),
        );
    }
    
    // Boost effect
    if state.boost_active {
        f.render_widget(
            Paragraph::new("🔥🔥").style(Style::default().fg(Color::Red).add_modifier(Modifier::RAPID_BLINK)),
            Rect::new(x + 1, y + 4, 4, 1),
        );
    }
    
    // Shield effect
    if state.shield_active {
        f.render_widget(
            Paragraph::new(" ◯◯◯ ").style(Style::default().fg(Color::Cyan).add_modifier(Modifier::BOLD)),
            Rect::new(x.saturating_sub(1), y.saturating_sub(1), 9, 1),
        );
    }
    
    // Invincibility effect
    if state.invincibility_active {
        f.render_widget(
            Paragraph::new("✨⭐✨").style(Style::default().fg(Color::Yellow).add_modifier(Modifier::RAPID_BLINK)),
            Rect::new(x.saturating_sub(1), y.saturating_sub(1), 9, 1),
        );
    }
    
    // Magnet effect
    if state.magnet_active {
        f.render_widget(
            Paragraph::new("🧲").style(Style::default().fg(Color::Red)),
            Rect::new(x + 6, y + 1, 2, 1),
        );
    }
}

/// Render ghost car from replay mode
pub fn render_ghost(
    f: &mut Frame,
    area: Rect,
    state: &GameState,
    lane_width: u16,
    height: u16,
    curve: i16,
    player_dist: f32,
) {
    let rel_dist = state.ghost_distance - player_dist;
    if rel_dist > -10.0 && rel_dist < 50.0 {
        let screen_y = area.y + height - ((rel_dist + 10.0) * height as f32 / 60.0) as u16;
        let base_x = area.x + (state.ghost_position as u16 * lane_width) + lane_width / 2 - 3;
        let x = (base_x as i16 + curve).max(area.x as i16) as u16;
        
        if screen_y < area.bottom() - 1 {
            let design = get_car_design(state.player_car_type, false);
            for (i, line) in design.art.iter().enumerate() {
                f.render_widget(
                    Paragraph::new(*line)
                        .style(Style::default().fg(Color::Rgb(150, 150, 200)).add_modifier(Modifier::DIM)),
                    Rect::new(x, screen_y + i as u16, 7, 1),
                );
            }
        }
    }
}

/// Render a single powerup or obstacle icon
fn render_powerup(f: &mut Frame, x: u16, y: u16, ptype: i32) {
    let (icon, color) = get_powerup_icon(ptype);
    
    f.render_widget(
        Paragraph::new(icon).style(Style::default().fg(color).add_modifier(Modifier::BOLD)),
        Rect::new(x, y, 2, 1),
    );
}
