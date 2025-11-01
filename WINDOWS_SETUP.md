# 🪟 Terminal Racer - Windows 11 Setup Guide

Complete guide to build and run Terminal Racer on Windows 11.

## ⚙️ Prerequisites

### 1. Install Rust
- Download from: https://rustup.rs/
- Run the installer and follow the prompts
- Choose "1) Proceed with installation"
- Restart your terminal/PowerShell after installation

Verify installation:
```powershell
rustc --version
cargo --version
```

### 2. Install .NET 9.0 SDK
- Download from: https://dotnet.microsoft.com/download/dotnet/9.0
- Choose "Windows x64" installer
- Run and complete installation
- Restart your terminal/PowerShell

Verify installation:
```powershell
dotnet --version
```

### 3. Install Windows Terminal (Recommended)
- Download from Microsoft Store: https://aka.ms/terminal
- Or: `winget install Microsoft.WindowsTerminal`
- This provides better Unicode and color support

## 🚀 Quick Start

### Option 1: Batch File (Easiest)
```batch
build.bat
```
Then run:
```batch
run.bat
```

### Option 2: PowerShell Script (Advanced)
```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

### Option 3: Manual Build
```powershell
# Build Rust library
cd rust-renderer
cargo build --release
cd ..

# Build C# game
cd game-engine
dotnet build -c Release
cd ..

# Copy library
copy rust-renderer\target\release\rust_renderer.dll game-engine\bin\Release\net9.0\

# Run game
cd game-engine
dotnet run --no-build -c Release
```

## 🎮 Running the Game

### Method 1: Batch Launcher (Recommended)
```batch
run.bat
```

### Method 2: Command Line
```powershell
cd game-engine
dotnet run --no-build -c Release
```

### Method 3: Windows Terminal
1. Open Windows Terminal
2. Navigate to project folder
3. Run: `dotnet run -c Release --project game-engine`

## 🖥️ Terminal Configuration

### Windows Terminal Settings (Recommended)

1. **Open Windows Terminal**
2. **Settings** (Ctrl+,)
3. **Profiles** → **Default** → **Appearance**

**Recommended Settings:**
- **Font**: Cascadia Code, Consolas, or Courier New
- **Font Size**: 10-12
- **Color Scheme**: One Dark Pro or Dracula
- **Transparency**: 0% (for best visibility)
- **Antialias**: ClearType

**Advanced Settings:**
```json
{
    "profiles": {
        "defaults": {
            "fontFace": "Cascadia Code",
            "fontSize": 11,
            "antialiasingMode": "cleartype",
            "useAcrylic": false
        }
    }
}
```

### Command Prompt Settings

1. **Right-click title bar** → **Properties**
2. **Font**: Consolas or Lucida Console
3. **Size**: 10-12
4. **Colors**: Enable 256-color mode

## 🎯 Troubleshooting

### Issue: "DllNotFoundException: rust_renderer"
**Solution:**
```powershell
# Run from game-engine directory
cd game-engine
dotnet run --no-build -c Release
```

### Issue: "Rust not found"
**Solution:**
```powershell
# Restart PowerShell/CMD after installing Rust
# Or add to PATH manually:
$env:Path += ";C:\Users\YourUsername\.cargo\bin"
```

### Issue: ".NET SDK not found"
**Solution:**
```powershell
# Restart PowerShell/CMD after installing .NET
# Or verify installation:
dotnet --version
```

### Issue: "Cannot find file build.bat"
**Solution:**
- Make sure you're in the Terminal_Racer root directory
- Check that build.bat exists: `dir build.bat`

### Issue: Colors look wrong
**Solution:**
- Use Windows Terminal instead of Command Prompt
- Try different color schemes
- Set: `$env:TERM = "xterm-256color"`

### Issue: Game runs slowly
**Solution:**
- Close other applications
- Use Windows Terminal (better performance)
- Increase terminal window size
- Disable transparency in terminal settings

### Issue: "PowerShell script cannot be loaded"
**Solution:**
```powershell
# Allow script execution
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope CurrentUser

# Then run:
powershell -ExecutionPolicy Bypass -File build.ps1
```

## 📊 System Requirements

**Minimum:**
- Windows 10 or later
- 2GB RAM
- 500MB disk space
- .NET 9.0 SDK
- Rust toolchain

**Recommended:**
- Windows 11
- 4GB+ RAM
- 1GB disk space
- Windows Terminal
- SSD for faster builds

## 🔧 Build Options

### PowerShell Script Options
```powershell
# Clean build (removes all previous builds)
powershell -ExecutionPolicy Bypass -File build.ps1 -Clean

# Skip Rust build (use existing library)
powershell -ExecutionPolicy Bypass -File build.ps1 -SkipRust

# Skip .NET build (use existing executable)
powershell -ExecutionPolicy Bypass -File build.ps1 -SkipDotNet

# Combine options
powershell -ExecutionPolicy Bypass -File build.ps1 -Clean -SkipRust
```

## 🎮 Game Controls

- **← / A** - Move left
- **→ / D** - Move right
- **↑ / W** - Accelerate
- **↓ / S** - Brake
- **SPACE** - Boost
- **P** - Pause
- **M** - Menu
- **Q / ESC** - Quit

## 📁 Project Structure

```
Terminal_Racer/
├── build.bat              # Windows batch build script
├── build.ps1              # Windows PowerShell build script
├── run.bat                # Game launcher (created after build)
├── build.sh               # Linux/Mac build script
├── run.sh                 # Linux/Mac launcher
├── README.md              # Main documentation
├── WINDOWS_SETUP.md       # This file
├── GAMEV2.md              # Feature documentation
├── game-engine/
│   ├── Program.cs         # C# game logic
│   └── game-engine.csproj
└── rust-renderer/
    ├── src/lib.rs         # Rust rendering
    └── Cargo.toml
```

## 🚨 Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| `DllNotFoundException` | Library not found | Run from game-engine folder |
| `FileNotFoundException` | Missing source files | Check project structure |
| `cargo: command not found` | Rust not installed | Install from rustup.rs |
| `dotnet: command not found` | .NET not installed | Install .NET 9.0 SDK |
| `Access Denied` | Permission issue | Run as Administrator |
| `Port 9999 in use` | Network multiplayer conflict | Close other instances |

## 🌐 Network Multiplayer on Windows

### Host Setup
```powershell
# Run game
run.bat

# Select: 5 (Network Multiplayer)
# Select: h (Host)
# Note your IP address
```

### Find Your IP
```powershell
# In PowerShell:
ipconfig

# Look for "IPv4 Address" under your network adapter
# Example: 192.168.1.100
```

### Client Setup
```powershell
# Run game
run.bat

# Select: 5 (Network Multiplayer)
# Select: j (Join)
# Enter host's IP address (e.g., 192.168.1.100)
```

### Firewall Configuration
```powershell
# Allow port 9999 for Terminal Racer
netsh advfirewall firewall add rule name="Terminal Racer" dir=in action=allow protocol=tcp localport=9999

# To remove the rule later:
netsh advfirewall firewall delete rule name="Terminal Racer"
```

## 📞 Support

If you encounter issues:
1. Check this guide first
2. Verify all prerequisites are installed
3. Try the manual build steps
4. Check Windows Terminal settings
5. Ensure port 9999 is available (for multiplayer)

## ✅ Verification Checklist

- [ ] Rust installed: `rustc --version`
- [ ] .NET 9.0 installed: `dotnet --version`
- [ ] Project folder accessible
- [ ] build.bat or build.ps1 runs successfully
- [ ] run.bat launches the game
- [ ] Game displays correctly in terminal
- [ ] Controls respond to input
- [ ] Colors display properly

## 🏁 Ready to Race!

Once everything is set up, simply run:
```batch
run.bat
```

**Terminal Racer Ultimate Edition - Now on Windows 11!** 🏎️

---

**Last Updated**: November 2025  
**Windows Version**: Windows 10/11  
**Status**: Fully Tested ✓
