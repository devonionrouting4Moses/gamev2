# 🏎️ Terminal Racer - Ultimate Edition v2

A feature-rich terminal-based racing game built with **C# (.NET 9.0)** and **Rust (Ratatui)** using FFI for seamless cross-language integration.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Rust](https://img.shields.io/badge/Rust-1.90+-orange.svg)](https://www.rust-lang.org/)

---

## 📑 Table of Contents

- [Game Features](#-game-features)
- [Architecture](#-architecture)
- [Quick Start](#-quick-start)
- [Controls](#-controls)
- [Network Multiplayer](#-network-multiplayer-setup)
- [Game Statistics](#-game-statistics)
- [Career Mode](#-career-mode-levels)
- [API Reference](#-api-reference)
- [Contributing](#-contributing)
- [Troubleshooting](#-troubleshooting)

---

## 🎮 Game Features

### Game Modes
- **Single Player** - Race against the clock with progressive difficulty
- **Split-Screen (Local)** - Compete with a friend on the same machine
- **Career Mode** - 9 challenging levels with boss battles
- **Replay Mode** - Watch and compare your best runs
- **Network Multiplayer** - Race against friends over TCP/IP (port 9999)

### Content
- **10 Car Types** - Sports, Police, Racer, Truck, Taxi, Van, Muscle, Convertible, Limousine, Boss
- **5 Track Types** - Highway, City Streets, Mountain Pass, Desert, Underground Tunnel
- **6 Powerups** - Boost, Shield, Invincibility Star, Magnet, Slow-Motion Clock, Oil Slick
- **4 Weather Conditions** - Clear, Rain, Fog, Night
- **Scoring System** - Distance-based with combo multipliers
- **High Score Leaderboard** - Track top 10 scores

### Audio & Visuals
- Terminal-based graphics with Ratatui
- Sound effects via mpg123/aplay
- Weather effects (rain, fog)
- Dynamic lighting (day/night cycle)
- Smooth 60 FPS gameplay

---

## 🛠️ Architecture

### Project Structure

```
Terminal Racer/
├── game-engine/              # C# Game Logic
│   ├── Program.cs            # Main game engine
│   └── game-engine.csproj
├── rust-renderer/            # Rust Terminal Rendering
│   ├── src/lib.rs            # Ratatui rendering engine
│   ├── Cargo.toml
│   └── target/release/       # Compiled library
├── build.sh                  # Automated build script
├── run.sh                    # Quick start launcher
└── README.md                 # This file
```

### Technology Stack

**Frontend**
- **Rendering**: Ratatui (Rust terminal UI framework)
- **Terminal Control**: Crossterm (cross-platform)
- **Graphics**: Unicode box-drawing characters

**Backend**
- **Language**: C# (.NET 9.0)
- **Physics**: Custom engine with delta-time
- **Audio**: mpg123 command-line player
- **Networking**: TCP/IP with JSON protocol

**Interop**
- **FFI**: P/Invoke for C# ↔ Rust communication
- **Memory Management**: GCHandle for pinning
- **Data Transfer**: Binary structures via marshalling

### Architecture Layers

```
┌─────────────────────────────────────────────────────────┐
│                  Application Layer                       │
│              (TerminalRacer.App)                        │
│  - Program.cs (Entry Point)                             │
│  - MenuSystem, GameOverScreen                           │
└────────────────┬────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────┐
│                 Game Logic Layer                         │
│            (TerminalRacer.GameLogic)                    │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Engine: RacingGame (Orchestrator)               │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Services:                                        │  │
│  │  - SpawnService (AI cars, obstacles, buildings)  │  │
│  │  - CollisionService (detection & handling)       │  │
│  │  - PowerupService (powerup states & combos)      │  │
│  │  - EnvironmentService (weather, curves)          │  │
│  │  - CareerService (level progression)             │  │
│  │  - ReplayService (recording & playback)          │  │
│  │  - ScoreService (leaderboard persistence)        │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Managers:                                        │  │
│  │  - AIManager (AI car behavior)                   │  │
│  │  - InputManager (player input handling)          │  │
│  │  - GameStateManager (FFI state building)         │  │
│  └──────────────────────────────────────────────────┘  │
└────────┬──────────────┬──────────────┬─────────────────┘
         │              │              │
┌────────▼────┐  ┌──────▼──────┐  ┌───▼──────────┐
│  Rendering  │  │    Audio    │  │  Networking  │
│   Layer     │  │    Layer    │  │    Layer     │
└─────────────┘  └─────────────┘  └──────────────┘
         │              │              │
         └──────────────┴──────────────┘
                        │
                ┌───────▼────────┐
                │   Core Layer   │
                │  (Domain)      │
                │  - Models      │
                │  - Enums       │
                │  - Constants   │
                └────────────────┘
```

### Design Patterns

1. **Dependency Injection** - Constructor injection with interfaces
2. **Service Layer Pattern** - Business logic in focused services
3. **Repository Pattern** - Data persistence (scores, replays)
4. **Observer Pattern** - Event-driven game loop
5. **Strategy Pattern** - Track-specific rendering strategies
6. **Factory Pattern** - Object spawning in SpawnService

---

## 🚀 Quick Start

### Prerequisites

```bash
# Install Rust
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh

# Install .NET 9.0 (Kali Linux)
sudo apt-get update
sudo apt-get install -y dotnet-sdk-9.0

# Optional: Audio support
sudo apt-get install -y mpg123
```

### Build

```bash
# Clone repository
git clone https://github.com/yourusername/terminal-racer.git
cd terminal-racer

# Make build script executable
chmod +x build.sh

# Build everything
./build.sh

# Optional: Run tests
./build.sh --test
```

### Run

```bash
# Quick start
./run.sh

# Or manually
cd game-engine
LD_LIBRARY_PATH=../rust-renderer/target/release:$LD_LIBRARY_PATH dotnet run -c Release
```

---

## 🎮 Controls

### Single Player / Career Mode

| Key | Action |
|-----|--------|
| **← / A** | Move left |
| **→ / D** | Move right |
| **↑ / W** | Accelerate |
| **↓ / S** | Brake |
| **SPACE** | Activate boost |
| **P** | Pause |
| **M** | Menu |
| **Q / ESC** | Quit |

### Split-Screen Multiplayer

**Player 1**: `WASD` + `SPACE`  
**Player 2**: `IJKL` + `U`

---

## 🌐 Network Multiplayer Setup

### Host Game (Kisumu)

```bash
./run.sh
# Select: 5 (Network Multiplayer)
# Select: h (Host)
# Share your public IP with Player 2
```

### Join Game (Nakuru)

```bash
./run.sh
# Select: 5 (Network Multiplayer)
# Select: j (Join)
# Enter host's public IP address
```

### Network Configuration

**Find Your Public IP:**
```bash
curl ifconfig.me
```

**Configure Firewall:**
```bash
sudo ufw allow 9999/tcp
sudo ufw reload
```

**Port Forwarding** (if behind router):
- Port: 9999
- Protocol: TCP
- Forward to your local machine IP

**Network Requirements:**
- Bandwidth: <1 KB/s
- Latency: <100ms recommended
- Port 9999 must be accessible

---

## 📊 Game Statistics

| Metric | Value |
|--------|-------|
| **Target FPS** | 60 |
| **Memory Usage** | 25-30 MB |
| **CPU Usage** | 5-15% (single core) |
| **Network** | <1 KB/s (multiplayer) |
| **Disk Space** | <5 MB (without replays) |
| **Min Terminal** | 100x30 characters |

---

## 🎯 Career Mode Levels

| Level | Track | Weather | Difficulty | Boss |
|-------|-------|---------|------------|------|
| 1 | Highway | Clear | Easy | - |
| 2 | City Streets | Clear | Medium | - |
| 3 | Highway | Rain | Medium | - |
| 4 | **Mountain Pass** | Clear | Hard | **✓** |
| 5 | Desert | Clear | Hard | - |
| 6 | Underground | Clear | Hard | - |
| 7 | **City Streets** | Fog | Very Hard | **✓** |
| 8 | **Mountain Pass** | Rain | Very Hard | **✓** |
| 9 | **Highway** | Night | Extreme | **✓ FINAL** |

**Boss Features:**
- 150% health
- Aggressive AI
- Special car model (Limousine)
- Must be defeated to progress

---

## 📚 API Reference

### Core Models

#### Car

```csharp
public class Car
{
    public int Lane { get; set; }           // 0-2
    public float Distance { get; set; }      // Meters traveled
    public float Speed { get; set; }         // km/h
    public bool IsPlayer { get; set; }
    public CarType Type { get; set; }
    public bool IsBoss { get; set; }
    public int Health { get; set; }          // 0-100
    
    public void Update(float deltaTime);
    public bool IsAlive { get; }
}
```

#### Obstacle

```csharp
public class Obstacle
{
    public int Lane { get; set; }
    public float Distance { get; set; }
    public ObstacleType Type { get; set; }
    public bool Collected { get; set; }
}
```

### Services

#### ISpawnService

```csharp
public interface ISpawnService
{
    void SpawnInitialContent();
    void SpawnAiCar();
    void SpawnObstacle();
    void SpawnBuilding();
    void UpdateSpawning(float playerDistance);
    void Cleanup(float playerDistance);
    void ClearAll();
    
    IReadOnlyList<Car> AiCars { get; }
    IReadOnlyList<Obstacle> Obstacles { get; }
    IReadOnlyList<Building> Buildings { get; }
}
```

#### ICollisionService

```csharp
public interface ICollisionService
{
    void CheckCollisions(
        Car playerCar, 
        ref int score,
        IReadOnlyList<Car> aiCars,
        IReadOnlyList<Obstacle> obstacles
    );
}
```

#### IPowerupService

```csharp
public interface IPowerupService
{
    void Initialize();
    void Update(float deltaTime);
    void ActivatePowerup(string name, float duration);
    void AddBoostCharge(int amount);
    bool TryActivateBoost();
    void IncrementCombo();
    void ResetCombo();
    
    bool IsBoostActive { get; }
    bool IsInvincible { get; }
    int Combo { get; }
    float BoostRemaining { get; }
    
    IReadOnlyDictionary<string, PowerupState> Powerups { get; }
}
```

### Constants

#### GameConstants

```csharp
public static class GameConstants
{
    // Physics
    public const float MaxSpeed = 200.0f;
    public const float BoostSpeed = 250.0f;
    public const float Acceleration = 60.0f;
    public const float Deceleration = 40.0f;
    public const float BrakeForce = 100.0f;
    
    // Track
    public const int NumLanes = 3;
    
    // Powerup durations (seconds)
    public const float BoostDuration = 3.0f;
    public const float ShieldDuration = 5.0f;
    public const float StarDuration = 7.0f;
    public const float MagnetDuration = 10.0f;
    public const float SlowmoDuration = 5.0f;
    
    // Collision
    public const float CarCollisionDistance = 8.0f;
    public const float ObstacleCollisionDistance = 5.0f;
    
    // Damage
    public const int CarCollisionDamage = 20;
    public const int ConeCollisionDamage = 15;
}
```

### Enums

#### GameMode

```csharp
public enum GameMode
{
    Single = 0,
    Splitscreen = 1,
    Career = 2,
    Replay = 3,
    Multiplayer = 4
}
```

#### CarType

```csharp
public enum CarType
{
    Sports = 0,      // Fast and agile
    Police = 1,      // Balanced
    Racer = 2,       // High speed
    Truck = 3,       // Heavy and slow
    Taxi = 4,        // City specialist
    Van = 5,         // Good acceleration
    Muscle = 6,      // Raw power
    Convertible = 7, // Stylish
    Limo = 8,        // Boss car
    Boss = 9         // Special boss variant
}
```

#### ObstacleType

```csharp
public enum ObstacleType
{
    Cone = 0,      // Causes damage
    Oil = 1,       // Speed reduction
    Boost = 2,     // Speed boost powerup
    Star = 3,      // Invincibility powerup
    Magnet = 4,    // Attract powerups
    Clock = 5      // Slow motion powerup
}
```

#### Weather

```csharp
public enum Weather
{
    Clear = 0,     // Normal conditions
    Rain = 1,      // Reduced visibility + slippery
    Fog = 2,       // Very low visibility
    Night = 3      // Dark with lights
}
```

---

## 🤝 Contributing

### Getting Started

1. **Fork the repository**
2. **Clone your fork**
   ```bash
   git clone https://github.com/yourusername/terminal-racer.git
   ```
3. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```
4. **Make your changes**
5. **Test thoroughly**
   ```bash
   dotnet test
   ```
6. **Submit a pull request**

### Code Style Guidelines

**C# Conventions:**
- Follow Microsoft C# Coding Conventions
- Use `var` only when type is obvious
- Always use braces for control structures
- Async methods end with `Async` suffix
- Interfaces start with `I`

**Rust Conventions:**
- Follow standard Rust formatting (`cargo fmt`)
- Use `clippy` for linting
- Document public APIs

### Testing Requirements

**All new features must include:**
- Unit tests for business logic
- Integration tests for services
- Manual testing in all game modes

**Test Naming Convention:**
```csharp
[Fact]
public void MethodName_StateUnderTest_ExpectedBehavior()
{
    // Arrange
    // Act
    // Assert
}
```

### Pull Request Process

1. Update documentation (README, API docs)
2. Add/update tests for changes
3. Ensure all tests pass
4. Update CHANGELOG.md
5. Create PR with clear description

**PR Title Format:**
```
[Type] Brief description

Types: Feature, Fix, Refactor, Docs, Test, Perf
Example: [Feature] Add weather system with rain and fog
```

### Adding New Features

#### New Powerup Type

1. Add to `ObstacleType` enum
2. Update `SpawnService.SpawnObstacle()`
3. Add handling in `CollisionService`
4. Update `PowerupService` if special behavior needed
5. Add visual representation in Rust renderer
6. Test in all game modes

#### New Track Type

1. Add to `TrackType` enum
2. Create Rust render function (e.g., `render_space_track`)
3. Update `EnvironmentService` for track-specific physics
4. Add to career levels
5. Create unique obstacles/features
6. Test performance

#### New Game Mode

1. Add to `GameMode` enum
2. Update `RacingGame.Initialize()` for mode setup
3. Create render layout in Rust
4. Add menu option
5. Update controls documentation
6. Add to build.sh test suite

---

## 🐛 Troubleshooting

### Build Issues

**Rust library not found:**
```bash
export LD_LIBRARY_PATH=$PWD/rust-renderer/target/release:$LD_LIBRARY_PATH
```

**.NET SDK version mismatch:**
```bash
# Check version
dotnet --version

# Should be 9.x or 8.x
# Update if needed
sudo apt-get update
sudo apt-get install dotnet-sdk-9.0
```

**Cargo build fails:**
```bash
# Update Rust
rustup update

# Clean and rebuild
cd rust-renderer
cargo clean
cargo build --release
```

### Runtime Issues

**Colors look wrong:**
```bash
export COLORTERM=truecolor
export TERM=xterm-256color
```

**Audio not working:**
```bash
# Install audio player
sudo apt-get install mpg123

# Test audio
mpg123 --version
```

**Input lag:**
- Close other terminal applications
- Use hardware-accelerated terminal (Alacritty, Kitty)
- Reduce terminal font size
- Build Rust in release mode

**Network connection fails:**
```bash
# Check firewall
sudo ufw status

# Allow port
sudo ufw allow 9999/tcp

# Test connectivity
nc -zv <host_ip> 9999
```

**Performance issues:**
- Build Rust in `--release` mode (not debug)
- Close unnecessary applications
- Use lightweight terminal emulator
- Reduce terminal window size
- Check CPU/RAM usage with `htop`

### Platform-Specific

**Linux/Kali:**
- Use system terminal or Alacritty
- Ensure UTF-8 locale: `locale | grep UTF-8`
- Install audio codecs: `sudo apt-get install alsa-utils`

**Windows (WSL):**
- Use Windows Terminal
- Enable ANSI color support
- Audio may not work in WSL

**macOS:**
- Use iTerm2 or default Terminal
- Install Rust via Homebrew: `brew install rust`
- Audio requires additional setup

---

## 📝 Project Roadmap

### Planned Features (v3.0)

- [ ] Online multiplayer with matchmaking
- [ ] Custom track editor
- [ ] Achievement system
- [ ] Mod support (custom cars/tracks)
- [ ] Mobile support (termux)
- [ ] VR mode (terminal-based VR, experimental)
- [ ] Tournament mode
- [ ] Garage system (car upgrades)

### Technical Improvements

- [ ] Dependency injection container
- [ ] Configuration system (appsettings.json)
- [ ] Structured logging (Serilog)
- [ ] Better error handling
- [ ] Performance profiling
- [ ] CI/CD pipeline

---

## 🙏 Acknowledgments

- **Ratatui** - Amazing Rust TUI framework
- **Crossterm** - Cross-platform terminal library
- **.NET Team** - Excellent runtime and tooling
- **Terminal racing community** - Inspiration and feedback

---

## 📞 Contact & Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/terminal-racer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/terminal-racer/discussions)
- **Email**: your.email@example.com

---

## 🎮 Ready to Race?

```bash
./run.sh
```

**Terminal Racer Ultimate Edition - The most feature-complete terminal racing game ever created!** 🏆

---

**Version**: 2.0 Ultimate Edition  
**Last Updated**: November 2025  
**Platform**: Linux (Kali optimized), Windows (WSL), macOS  
**Status**: Production Ready ✅