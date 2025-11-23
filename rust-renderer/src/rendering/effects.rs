/// Visual effects module for weather, lane markers, and special effects
/// Handles rain, fog, slowmo effects, and lane rendering

use ratatui::{
    layout::Rect,
    style::{Color, Modifier, Style},
    widgets::Paragraph,
    Frame,
};
use crate::core::types::GameState;

/// Render lane dividers with animation
pub fn render_lane_markers(
    f: &mut Frame,
    area: Rect,
    state: &GameState,
    lane_width: u16,
    height: u16,
    curve: i16,
) {
    let offset = (state.player_distance as u16 * 2) % 6;
    
    for y in (0..height).step_by(3) {
        let actual_y = (y + offset) % height;
        
        for lane in 1..3 {
            let base_x = area.x + lane * lane_width;
            let curved_x = (base_x as i16 + curve)
                .max(area.x as i16)
                .min((area.x + area.width) as i16) as u16;
            
            if actual_y + area.y < area.bottom() && curved_x < area.x + area.width {
                f.render_widget(
                    Paragraph::new("┃").style(Style::default().fg(Color::White)),
                    Rect::new(curved_x, area.y + actual_y, 1, 2),
                );
            }
        }
    }
}

/// Render weather effects (rain, fog) and special effects (slowmo)
pub fn render_weather_overlay(f: &mut Frame, area: Rect, state: &GameState) {
    match state.weather {
        1 => render_rain(f, area, state),
        2 => {
            // Fog is mainly handled by background color
        },
        _ => {},
    }
    
    if state.slowmo_active {
        render_slowmo_effect(f, area);
    }
}

/// Render rain effect with animated drops
fn render_rain(f: &mut Frame, area: Rect, state: &GameState) {
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
}

/// Render slowmo motion blur effect
fn render_slowmo_effect(f: &mut Frame, area: Rect) {
    for y in (0..area.height).step_by(4) {
        f.render_widget(
            Paragraph::new("━").style(Style::default().fg(Color::Rgb(80, 80, 150))),
            Rect::new(area.x + 2, area.y + y, 3, 1),
        );
    }
}
