/// Track rendering module for different track types
/// Handles highway, city, mountain, desert, and tunnel rendering

use ratatui::{
    layout::Rect,
    style::{Color, Modifier, Style},
    widgets::Paragraph,
    Frame,
};
use crate::core::types::{GameState, track_types};
use crate::core::utils::{get_road_char, get_track_style, get_track_name, get_weather_icon, get_building_style};
use super::objects::{render_objects, render_player, render_ghost};
use super::effects::{render_weather_overlay, render_lane_markers};

/// Render the appropriate track based on track type
pub fn render_track(
    f: &mut Frame,
    area: Rect,
    state: &GameState,
    player_pos: i32,
    player_dist: f32,
    _is_primary: bool,
) {
    let track_name = get_track_name(state.track_type);
    let weather_icon = get_weather_icon(state.weather);
    
    let title = if state.replay_mode {
        format!("═══ {} ═══ {} ═══ [REPLAY] ═══", track_name, weather_icon)
    } else {
        format!("═══ {} ═══ {} ═══", track_name, weather_icon)
    };
    
    let block = ratatui::widgets::Block::default()
        .borders(ratatui::widgets::Borders::ALL)
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

/// Render highway track with basic road and lane markers
fn render_highway_track(
    f: &mut Frame,
    area: Rect,
    state: &GameState,
    player_pos: i32,
    player_dist: f32,
) {
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

/// Render city track with buildings and urban environment
fn render_city_track(
    f: &mut Frame,
    area: Rect,
    state: &GameState,
    player_pos: i32,
    player_dist: f32,
) {
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

/// Render mountain track with elevation and peaks
fn render_mountain_track(
    f: &mut Frame,
    area: Rect,
    state: &GameState,
    player_pos: i32,
    player_dist: f32,
) {
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

/// Render desert track with sparse vegetation
fn render_desert_track(
    f: &mut Frame,
    area: Rect,
    state: &GameState,
    player_pos: i32,
    player_dist: f32,
) {
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

/// Render tunnel track with walls and lighting
fn render_tunnel_track(
    f: &mut Frame,
    area: Rect,
    state: &GameState,
    player_pos: i32,
    player_dist: f32,
) {
    let lane_width = area.width / 3;
    let road_height = area.height;
    let curve = (state.curve_offset * 1.5) as i16;
    
    render_tunnel_walls(f, area, state, player_dist);
    render_road_base(f, area, state);
    render_lane_markers(f, area, state, lane_width, road_height, curve);
    render_objects(f, area, state, lane_width, road_height, curve, player_dist);
    render_player(f, area, state, player_pos, lane_width, curve, state.player_car_type, true);
}

/// Render the road base with animated pattern
fn render_road_base(f: &mut Frame, area: Rect, state: &GameState) {
    let road_char = get_road_char(state.weather);
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

/// Render city buildings on both sides of the road
fn render_buildings(f: &mut Frame, area: Rect, state: &GameState, player_dist: f32) {
    if state.building_count == 0 || state.building_positions.is_null() {
        return;
    }
    
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

/// Render individual building with windows
fn render_building(f: &mut Frame, x: u16, y: u16, height: u16, btype: i32) {
    let (char_set, color) = get_building_style(btype);
    
    for i in 0..height.min(10) {
        if y >= i {
            f.render_widget(
                Paragraph::new(char_set).style(Style::default().fg(color)),
                Rect::new(x, y - i, 6, 1),
            );
            
            // Windows for glass buildings
            if i % 2 == 0 && btype == 1 {
                f.render_widget(
                    Paragraph::new("▫▫").style(Style::default().fg(Color::Yellow)),
                    Rect::new(x + 2, y - i, 2, 1),
                );
            }
        }
    }
}

/// Render mountain peaks in background
fn render_mountain_bg(f: &mut Frame, area: Rect, elevation: i16) {
    for x in (0..area.width).step_by(5) {
        let peak_y = (area.height / 3) + (elevation.abs() % 10) as u16;
        f.render_widget(
            Paragraph::new("▲").style(Style::default().fg(Color::Rgb(100, 100, 100))),
            Rect::new(area.x + x, area.y + peak_y, 1, 1),
        );
    }
}

/// Render desert background with cacti
fn render_desert_bg(f: &mut Frame, area: Rect, distance: f32) {
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

/// Render tunnel walls with lighting effects
fn render_tunnel_walls(f: &mut Frame, area: Rect, state: &GameState, distance: f32) {
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
