# Terminal Racer Rust Renderer - Module System Guide

## 🎯 Overview

The Terminal Racer Rust Renderer is a **highly modular, professionally organized** rendering engine for a terminal-based racing game. The codebase has been refactored from a monolithic 1,113-line file into a **hierarchical module system** with clear separation of concerns.

---

## 📁 Directory Structure

```
rust-renderer/
├── src/
│   ├── lib.rs                    # C FFI entry point (195 lines)
│   │
│   ├── core/                     # Core domain (303 lines)
│   │   ├── mod.rs               # Module exports
│   │   ├── types.rs             # Game state structures & constants
│   │   └── utils.rs             # Utility functions & styling
│   │
│   ├── io/                       # I/O domain (117 lines)
│   │   ├── mod.rs               # Module exports
│   │   ├── input.rs             # Keyboard input handling
│   │   └── terminal.rs          # Terminal lifecycle management
│   │
│   └── rendering/               # Rendering domain (910 lines)
│       ├── mod.rs               # Module exports
│       ├── track.rs             # Track rendering (5 types)
│       ├── objects.rs           # Dynamic objects (cars, obstacles)
│       ├── effects.rs           # Visual effects (weather, etc.)
│       └── hud.rs               # UI elements (gauges, menus)
│
├── ARCHITECTURE.md              # Detailed architecture guide
├── MODULE_TREE.txt              # Visual module hierarchy
├── MODULARIZATION_SUMMARY.md    # Refactoring summary
└── README_MODULES.md            # This file
```

---

## 🏗️ Architecture Domains

### 1. **Core Domain** - Data & Utilities
**Location**: `src/core/`

Handles all data structures, type definitions, and pure utility functions.

#### `types.rs` - Game State Definitions
```rust
// Main structures
pub struct GameState { ... }      // Complete game state
pub struct InputState { ... }     // Player input tracking
pub struct AudioCommand { ... }   // Sound effect commands

// Constants (organized in submodules)
pub mod game_modes { ... }        // 4 game modes
pub mod track_types { ... }       // 5 track types
pub mod weather { ... }           // 4 weather types
pub mod obstacle_types { ... }    // 6 obstacle types
pub mod building_types { ... }    // 3 building types
pub mod audio_types { ... }       // 5 audio types
```

#### `utils.rs` - Utility Functions
```rust
// Color functions
pub fn get_combo_color(combo: i32) -> Color
pub fn get_health_color(health: i32) -> Color

// Lookup functions
pub fn get_track_name(track_type: i32) -> &'static str
pub fn get_weather_icon(weather: i32) -> &'static str
pub fn get_track_style(...) -> Style

// Design functions
pub fn get_car_design(car_type, is_boss) -> CarDesign
pub fn get_powerup_icon(ptype) -> (icon, color)
pub fn get_building_style(btype) -> (chars, color)
pub fn get_road_char(weather) -> &'static str
```

---

### 2. **I/O Domain** - Input & Terminal
**Location**: `src/io/`

Manages all external interactions: keyboard input and terminal lifecycle.

#### `terminal.rs` - Terminal Management
```rust
pub fn init() -> bool              // Initialize terminal
pub fn cleanup()                   // Restore terminal
pub fn get_terminal() -> Option<&'static mut Terminal>  // Safe access
```

**Lifecycle:**
1. `init()` → Enable raw mode → Enter alternate screen → Create terminal
2. Game loop runs
3. `cleanup()` → Disable raw mode → Leave alternate screen

#### `input.rs` - Keyboard Input
```rust
pub fn poll_input(input: *mut InputState) -> bool

// Supported controls:
// Player 1: Arrow keys or WASD (movement) + Space (boost)
// Player 2: IJKL (movement) + U (boost)
// System: Q/Esc (quit), P (pause), M (menu)
```

**Polling:** 16ms interval (60 FPS target)

---

### 3. **Rendering Domain** - Visual Output
**Location**: `src/rendering/`

All visual rendering and UI elements, organized by rendering concern.

#### `track.rs` - Track Rendering (280 lines)
**5 Track Types:**
1. **Highway** - Basic road with lane markers
2. **City** - Urban environment with buildings
3. **Mountain** - Elevated terrain with peaks
4. **Desert** - Sparse landscape with cacti
5. **Tunnel** - Dark environment with ceiling lights

**Main Function:**
```rust
pub fn render_track(f, area, state, player_pos, player_dist, is_primary)
```

**Shared Components:**
- Road base (animated texture)
- Lane markers (curved dividers)
- Objects (AI cars, obstacles)
- Player car (with powerup effects)
- Ghost car (replay mode)
- Weather overlay (rain, fog)

#### `objects.rs` - Dynamic Objects (200 lines)
**Renders:**
- AI cars (10 types, with boss effects)
- Obstacles (6 types: cone, oil, boost, star, magnet, clock)
- Player car (with 4 powerup effects)
- Ghost car (semi-transparent replay car)

**Powerup Effects:**
- 🔥 **Boost**: Red flames, blinking
- 🛡️ **Shield**: Cyan circles
- ✨ **Invincibility**: Yellow stars, blinking
- 🧲 **Magnet**: Red magnet icon

#### `effects.rs` - Visual Effects (80 lines)
**Effects:**
- Lane markers (animated curved dividers)
- Rain (animated drops)
- Fog (background color adjustment)
- Slowmo (motion blur lines)

**Design:** Modular system for easy addition of new effects

#### `hud.rs` - User Interface (350 lines)
**4 Layout Types:**
1. **Single-player**: HUD + Game area + Controls
2. **Split-screen**: P1 and P2 side-by-side
3. **Career**: HUD + Game area + Career info
4. **Replay**: Controls + Game area + Replay info

**UI Elements:**
- Speed gauge (dynamic color based on speed)
- Powerup gauges (⚡BOOST, 🛡SHIELD, ⭐STAR, 🧲MAG)
- Score display (⭐ with 8-digit formatting)
- Combo counter (🔥 with color coding)
- Health bar (❤ with color coding)
- Level display (LV.X)
- Career progress (percentage)
- Replay info (time, ghost car)
- Menu system (interactive selection)

---

## 🔄 Data Flow

### Rendering Pipeline
```
ratatui_render() [FFI]
    ↓
match game_mode:
    ├─ SINGLE_PLAYER → render_singleplayer()
    ├─ SPLIT_SCREEN → render_splitscreen()
    ├─ CAREER → render_career_mode()
    └─ REPLAY → render_replay_mode()
        ↓
    Layout (vertical/horizontal split)
        ↓
    render_enhanced_hud()
        ↓
    render_track()
        ├─ render_road_base()
        ├─ render_buildings() [if city]
        ├─ render_lane_markers()
        ├─ render_objects()
        ├─ render_player()
        ├─ render_ghost() [if replay]
        └─ render_weather_overlay()
```

### Input Pipeline
```
ratatui_poll_input() [FFI]
    ↓
io::input::poll_input()
    ↓
event::poll(16ms)
    ↓
handle_key_event()
    ↓
Update InputState
```

---

## 📊 Statistics

### Code Distribution
| Module | Lines | Functions | Purpose |
|--------|-------|-----------|---------|
| core/types.rs | 120 | 0 | Data structures |
| core/utils.rs | 180 | 8 | Utilities |
| io/input.rs | 70 | 2 | Input handling |
| io/terminal.rs | 40 | 3 | Terminal mgmt |
| rendering/track.rs | 280 | 12 | Track rendering |
| rendering/objects.rs | 200 | 6 | Object rendering |
| rendering/effects.rs | 80 | 3 | Visual effects |
| rendering/hud.rs | 350 | 15 | UI rendering |
| lib.rs | 195 | 5 | FFI entry |
| **Total** | **1,515** | **54** | **Complete** |

### Feature Coverage
- **Game Modes**: 4 (single, split, career, replay)
- **Track Types**: 5 (highway, city, mountain, desert, tunnel)
- **Car Types**: 10 (sports, police, racer, truck, taxi, van, muscle, convertible, limo, default)
- **Powerup Types**: 6 (cone, oil, boost, star, magnet, clock)
- **Building Types**: 3 (glass, concrete, brick)
- **Weather Types**: 4 (clear, rain, fog, night)
- **Visual Effects**: 4+ (rain, lane markers, slowmo, lighting)

---

## 🚀 Quick Start

### Adding a New Track Type
1. Add constant to `src/core/types.rs`:
   ```rust
   pub const VOLCANO: i32 = 5;
   ```

2. Add case to `src/rendering/track.rs`:
   ```rust
   5 => render_volcano_track(f, inner, state, player_pos, player_dist),
   ```

3. Implement renderer:
   ```rust
   fn render_volcano_track(f, inner, state, player_pos, player_dist) {
       // Your rendering code
   }
   ```

### Adding a New Powerup
1. Add constant to `src/core/types.rs`:
   ```rust
   pub const NITRO: i32 = 6;
   ```

2. Update `src/core/utils.rs`:
   ```rust
   pub fn get_powerup_icon(ptype: i32) -> (&'static str, Color) {
       match ptype {
           // ...
           6 => ("💨", Color::Cyan),  // Nitro
           // ...
       }
   }
   ```

### Adding a New Visual Effect
1. Create function in `src/rendering/effects.rs`:
   ```rust
   fn render_dust_effect(f, area, state) {
       // Your effect code
   }
   ```

2. Call from appropriate renderer:
   ```rust
   render_dust_effect(f, area, state);
   ```

---

## 📚 Documentation Files

### ARCHITECTURE.md
Comprehensive guide covering:
- Module breakdown with detailed descriptions
- Data flow diagrams
- Module dependencies
- Design principles
- C FFI exports
- Feature addition guide
- Testing strategy
- Performance considerations
- Future enhancements

### MODULE_TREE.txt
Visual representation showing:
- Complete module hierarchy
- Function organization
- Data structures
- Constants
- Dependencies
- Statistics

### MODULARIZATION_SUMMARY.md
Refactoring overview including:
- Before/after comparison
- Key improvements
- Code organization benefits
- Dependency management
- Feature addition examples
- Migration checklist

---

## 🔗 Module Dependencies

```
lib.rs (top-level)
  ├─ core::types (no dependencies)
  ├─ core::utils (depends on core::types)
  ├─ io::terminal (no dependencies)
  ├─ io::input (depends on core::types)
  └─ rendering::* (depend on core::types and core::utils)

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

**Key Property**: Acyclic dependency graph (no circular dependencies)

---

## 🎮 Game Features

### Game Modes
- **Single-player**: One player racing
- **Split-screen**: Two players side-by-side
- **Career**: Progressive level-based gameplay
- **Replay**: Watch recorded races with ghost car

### Track Types
- **Highway**: Straight road with lane markers
- **City**: Urban environment with buildings
- **Mountain**: Elevated terrain with peaks
- **Desert**: Sparse landscape with vegetation
- **Tunnel**: Dark environment with dynamic lighting

### Powerups
- **Cone** (🚧): Obstacle to avoid
- **Oil** (💧): Slippery surface
- **Boost** (⚡): Speed increase
- **Star** (⭐): Invincibility
- **Magnet** (🧲): Attract powerups
- **Clock** (🕐): Slow motion

### Visual Effects
- **Weather**: Rain, fog, night
- **Lane markers**: Animated curved dividers
- **Slowmo**: Motion blur effect
- **Tunnel lighting**: Dynamic brightness
- **Powerup effects**: Visual feedback for active powerups

---

## 🔧 Development Workflow

### Modifying Existing Functionality
1. Locate the relevant module (see directory structure)
2. Find the function to modify
3. Make changes
4. Test the specific module
5. Run full test suite

### Adding New Features
1. Determine which domain it belongs to
2. Add to appropriate module
3. Update related modules if needed
4. Add documentation
5. Test thoroughly

### Debugging
1. Check module organization (is code in right place?)
2. Verify imports and module paths
3. Check function signatures
4. Review data flow
5. Use logging/debugging tools

---

## 📝 Coding Standards

### Naming Conventions
- **Functions**: `snake_case` (e.g., `render_track`)
- **Modules**: `snake_case` (e.g., `core`, `rendering`)
- **Constants**: `SCREAMING_SNAKE_CASE` (e.g., `HIGHWAY`)
- **Types**: `PascalCase` (e.g., `GameState`)

### Documentation
- Module-level documentation comments
- Function documentation with purpose
- Inline comments for complex logic
- Clear parameter descriptions

### Code Organization
- One concern per module
- Pure functions where possible
- Clear dependency direction
- Minimal global state

---

## 🚦 Performance

### Compilation
- **Modular structure**: Enables incremental compilation
- **Faster rebuilds**: Only changed modules recompile
- **Better caching**: Dependencies are clearer

### Runtime
- **No performance degradation**: All inlining still occurs
- **Same binary size**: Compiler optimizations unchanged
- **Same execution speed**: Pure organizational improvement

---

## 🔮 Future Enhancements

### Short Term
- Particle effects system
- Animation framework
- Expanded replay system
- Sound effect integration

### Medium Term
- Network multiplayer
- Advanced lighting
- Procedural generation
- Custom car painting

### Long Term
- Physics engine
- AI improvements
- Leaderboards
- Cross-platform support

---

## 📖 How to Use This Guide

1. **First time?** Start with this README
2. **Need architecture details?** Read ARCHITECTURE.md
3. **Want visual overview?** Check MODULE_TREE.txt
4. **Curious about refactoring?** See MODULARIZATION_SUMMARY.md
5. **Adding features?** Use Quick Start section above

---

## ✅ Checklist for New Developers

- [ ] Read this README
- [ ] Review ARCHITECTURE.md
- [ ] Study MODULE_TREE.txt
- [ ] Examine src/lib.rs (FFI entry point)
- [ ] Explore src/core/ (data structures)
- [ ] Review src/io/ (input/terminal)
- [ ] Study src/rendering/ (visual output)
- [ ] Run the project locally
- [ ] Make a small modification
- [ ] Run tests
- [ ] Add a new feature

---

## 📞 Support

For questions about:
- **Architecture**: See ARCHITECTURE.md
- **Module organization**: See MODULE_TREE.txt
- **Refactoring details**: See MODULARIZATION_SUMMARY.md
- **Code locations**: Use Quick Reference below

---

## 🎯 Quick Reference

### File Locations
| What | Where |
|------|-------|
| Game state structures | `src/core/types.rs` |
| Utility functions | `src/core/utils.rs` |
| Input handling | `src/io/input.rs` |
| Terminal management | `src/io/terminal.rs` |
| Track rendering | `src/rendering/track.rs` |
| Object rendering | `src/rendering/objects.rs` |
| Visual effects | `src/rendering/effects.rs` |
| UI elements | `src/rendering/hud.rs` |
| FFI entry points | `src/lib.rs` |

### Common Tasks
| Task | File |
|------|------|
| Add new track type | `src/rendering/track.rs` |
| Add new powerup | `src/core/utils.rs` |
| Add new effect | `src/rendering/effects.rs` |
| Add new UI element | `src/rendering/hud.rs` |
| Change input mapping | `src/io/input.rs` |
| Add new car type | `src/core/utils.rs` |

---

**Status**: ✅ Complete and Ready for Development

**Last Updated**: November 23, 2025

**Total Lines of Code**: 1,515 (organized, modular, well-documented)
