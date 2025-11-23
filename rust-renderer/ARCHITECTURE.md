# Terminal Racer - Rust Renderer Architecture

## Overview
The Rust renderer is organized into a highly modular, hierarchical structure with clear separation of concerns. The codebase is divided into three primary domains: **Core**, **I/O**, and **Rendering**.

---

## Directory Structure

```
src/
├── lib.rs                          # Main library entry point & C FFI exports
├── core/                           # Core domain - Data & utilities
│   ├── mod.rs                      # Core module exports
│   ├── types.rs                    # Game state structures & type constants
│   └── utils.rs                    # Helper functions & styling utilities
├── io/                             # I/O domain - Input & terminal management
│   ├── mod.rs                      # I/O module exports
│   ├── input.rs                    # Keyboard input polling & event handling
│   └── terminal.rs                 # Terminal initialization & lifecycle
└── rendering/                      # Rendering domain - Visual output
    ├── mod.rs                      # Rendering module exports
    ├── track.rs                    # Track rendering (highway, city, mountain, desert, tunnel)
    ├── objects.rs                  # Dynamic objects (cars, obstacles, powerups)
    ├── effects.rs                  # Visual effects (weather, lane markers, slowmo)
    └── hud.rs                      # UI elements (gauges, stats, menus)
```

---

## Module Breakdown

### 1. **Core Domain** (`src/core/`)
Handles all data structures and utility functions.

#### `types.rs` - Game State Definitions
- **GameState**: Complete game state with player data, AI, obstacles, environment
- **InputState**: Dual-player input tracking
- **AudioCommand**: Sound effect commands
- **Constants**: Game modes, track types, weather, obstacle types, building types, audio types

**Key Features:**
- C-compatible `#[repr(C)]` structures for FFI
- Organized constant modules for type safety
- Comprehensive state management for multiplayer, career, and replay modes

#### `utils.rs` - Utility Functions
- **Color Functions**: `get_combo_color()`, `get_health_color()`
- **Lookup Functions**: `get_track_name()`, `get_weather_icon()`, `get_track_style()`
- **Car Designs**: `get_car_design()` with 10 car types (sports, police, racer, truck, taxi, van, muscle, convertible, limo, default)
- **Powerup Styles**: `get_powerup_icon()` with 6 powerup types
- **Building Styles**: `get_building_style()` with 3 building types
- **Road Rendering**: `get_road_char()` for weather-based road textures

**Design Pattern:** Pure functions with no side effects, enabling easy testing and reusability.

---

### 2. **I/O Domain** (`src/io/`)
Manages all input and terminal operations.

#### `terminal.rs` - Terminal Lifecycle
- **init()**: Initialize raw mode, alternate screen, and terminal
- **cleanup()**: Restore terminal to normal state
- **get_terminal()**: Safely access the global terminal instance

**Key Features:**
- Global static terminal management
- Safe mutable access through getter function
- Proper error handling for terminal operations

#### `input.rs` - Input Handling
- **poll_input()**: Non-blocking keyboard event polling
- **handle_key_event()**: Key mapping for dual-player controls

**Supported Controls:**
- **Player 1**: Arrow keys or WASD for movement, Space for boost
- **Player 2**: IJKL for movement, U for boost
- **System**: Q/Esc for quit, P for pause, M for menu

**Design Pattern:** Event-driven input with 16ms polling interval (60 FPS).

---

### 3. **Rendering Domain** (`src/rendering/`)
All visual output and UI rendering.

#### `track.rs` - Track Rendering Engine
**Main Function:** `render_track()` - Routes to track-specific renderers

**Track Types:**
1. **Highway** - Basic road with lane markers
2. **City** - Urban environment with buildings
3. **Mountain** - Elevated terrain with peaks
4. **Desert** - Sparse landscape with cacti
5. **Tunnel** - Dark environment with ceiling lights

**Sub-Functions:**
- `render_road_base()` - Animated road texture
- `render_lane_markers()` - Curved lane dividers
- `render_buildings()` - City buildings with windows
- `render_mountain_bg()` - Mountain peaks
- `render_desert_bg()` - Desert vegetation
- `render_tunnel_walls()` - Tunnel walls with dynamic lighting

#### `objects.rs` - Dynamic Object Rendering
**Main Function:** `render_objects()` - Renders AI cars and obstacles

**Sub-Functions:**
- `render_ai_cars()` - Enemy vehicles with boss effects
- `render_obstacles()` - Powerups and hazards
- `render_car()` - Individual car rendering with styling
- `render_player()` - Player car with powerup effects (boost, shield, invincibility, magnet)
- `render_ghost()` - Replay mode ghost car (semi-transparent)
- `render_powerup()` - Obstacle/powerup icons

**Features:**
- Perspective-based positioning
- Boss car visual effects (blinking, bold)
- Powerup visual feedback (colored auras, effects)

#### `effects.rs` - Visual Effects
**Main Functions:**
- `render_lane_markers()` - Animated lane dividers with curve offset
- `render_weather_overlay()` - Weather effects (rain, fog)
- `render_rain()` - Animated rain drops
- `render_slowmo_effect()` - Motion blur lines

**Design Pattern:** Modular effect system for easy addition of new visual effects.

#### `hud.rs` - User Interface
**Layout Functions:**
- `render_singleplayer_hud()` - Single-player layout
- `render_splitscreen_hud()` - Split-screen multiplayer
- `render_career_hud()` - Career mode with progression
- `render_replay_hud()` - Replay mode with playback controls

**Gauge Functions:**
- `render_enhanced_hud()` - Main stats display (score, combo, health, level)
- `render_player_hud()` - Per-player stats (score, health, speed)
- `render_speed_gauge()` - Dynamic speed indicator
- `render_powerup_gauge()` - Powerup duration bars

**Info Functions:**
- `render_career_info()` - Career progress and objectives
- `render_replay_info()` - Replay time and ghost info
- `render_replay_controls()` - Playback control instructions
- `render_controls()` - Game control instructions
- `render_menu()` - Menu rendering with selection

---

## Data Flow

### Rendering Pipeline
```
ratatui_render() [FFI Entry]
    ↓
match game_mode:
    ├─ SINGLE_PLAYER → render_singleplayer()
    ├─ SPLIT_SCREEN → render_splitscreen()
    ├─ CAREER → render_career_mode()
    └─ REPLAY → render_replay_mode()
        ↓
    Layout (vertical/horizontal split)
        ↓
    render_enhanced_hud() [core::utils + rendering::hud]
        ↓
    render_track() [rendering::track]
        ├─ render_road_base() [core::utils]
        ├─ render_buildings() [rendering::track]
        ├─ render_lane_markers() [rendering::effects]
        ├─ render_objects() [rendering::objects]
        │   ├─ render_ai_cars()
        │   └─ render_obstacles()
        ├─ render_player() [rendering::objects]
        ├─ render_ghost() [rendering::objects]
        └─ render_weather_overlay() [rendering::effects]
```

### Input Pipeline
```
ratatui_poll_input() [FFI Entry]
    ↓
io::input::poll_input()
    ↓
event::poll() [crossterm]
    ↓
handle_key_event()
    ↓
Update InputState struct
```

### Terminal Lifecycle
```
ratatui_init() → io::terminal::init()
    ↓
enable_raw_mode()
    ↓
EnterAlternateScreen
    ↓
Create Terminal instance
    ↓
[Game Loop]
    ↓
ratatui_cleanup() → io::terminal::cleanup()
    ↓
disable_raw_mode()
    ↓
LeaveAlternateScreen
```

---

## Module Dependencies

```
lib.rs (FFI Layer)
    ├─ core::types (GameState, InputState, AudioCommand)
    ├─ core::utils (Helpers)
    ├─ io::input (Input polling)
    ├─ io::terminal (Terminal management)
    └─ rendering::* (All rendering)

rendering::track
    ├─ core::types
    ├─ core::utils
    ├─ rendering::objects
    └─ rendering::effects

rendering::objects
    ├─ core::types
    └─ core::utils

rendering::effects
    └─ core::types

rendering::hud
    ├─ core::types
    └─ core::utils

io::input
    └─ core::types

io::terminal
    (No internal dependencies)

core::utils
    └─ core::types
```

---

## Design Principles

### 1. **Separation of Concerns**
- **Core**: Data and pure functions
- **I/O**: External interaction (keyboard, terminal)
- **Rendering**: Visual output only

### 2. **Modularity**
- Each module has a single responsibility
- Functions are small and focused
- Easy to test and maintain

### 3. **Reusability**
- Utility functions are pure (no side effects)
- Rendering functions are composable
- Common patterns extracted to helpers

### 4. **Performance**
- Minimal allocations in hot paths
- Efficient unsafe pointer handling for C FFI
- Lazy evaluation where possible

### 5. **Extensibility**
- New track types: Add function to `track.rs`
- New effects: Add function to `effects.rs`
- New UI elements: Add function to `hud.rs`
- New car types: Extend `get_car_design()` in `utils.rs`

---

## C FFI Exports

All public C-compatible functions are in `lib.rs`:

```rust
#[unsafe(no_mangle)]
pub extern "C" fn ratatui_init() -> bool
pub extern "C" fn ratatui_cleanup()
pub extern "C" fn ratatui_poll_input(input: *mut InputState) -> bool
pub extern "C" fn ratatui_render(state: *const GameState) -> bool
pub extern "C" fn ratatui_render_menu(...) -> bool
```

---

## Adding New Features

### Add a New Track Type
1. Add constant to `core/types.rs`
2. Add case to `track.rs::render_track()`
3. Implement `render_[name]_track()` function

### Add a New Powerup
1. Add constant to `core/types.rs`
2. Update `core/utils.rs::get_powerup_icon()`
3. Update `rendering/objects.rs` if visual effects needed

### Add a New Visual Effect
1. Create function in `rendering/effects.rs`
2. Call from appropriate track renderer
3. Update `rendering/effects.rs::render_weather_overlay()` if global

### Add a New UI Element
1. Create function in `rendering/hud.rs`
2. Call from appropriate layout function
3. Update layout constraints if needed

---

## Testing Strategy

### Unit Tests
- `core/utils.rs`: Pure function tests
- `core/types.rs`: Constant validation

### Integration Tests
- Input → State updates
- State → Rendering output
- FFI boundary tests

### Visual Tests
- Manual terminal rendering verification
- Different game modes
- All track types
- Multiplayer layouts

---

## Performance Considerations

- **Rendering**: O(n) where n = visible objects
- **Input**: O(1) per frame
- **Memory**: Fixed allocation for game state
- **Terminal**: Double-buffered by ratatui

---

## Future Enhancements

1. **Particle Effects**: Extend `effects.rs` for explosions, dust
2. **Animation System**: Keyframe-based animations
3. **Sound Integration**: Expand `AudioCommand` handling
4. **Replay System**: Full replay data structures
5. **Network Multiplayer**: Extend input/state for network sync
6. **Advanced Lighting**: Dynamic lighting for tunnel
7. **Terrain Deformation**: Procedural track generation
