# Terminal Racer - Windows PowerShell Build Script
# Usage: powershell -ExecutionPolicy Bypass -File build.ps1
# Or right-click and select "Run with PowerShell"

param(
    [switch]$Clean = $false,
    [switch]$SkipRust = $false,
    [switch]$SkipDotNet = $false
)

# Color output
function Write-Success { Write-Host "✓ $args" -ForegroundColor Green }
function Write-Error { Write-Host "❌ $args" -ForegroundColor Red }
function Write-Info { Write-Host "ℹ $args" -ForegroundColor Cyan }
function Write-Section { Write-Host "`n╔═══════════════════════════════════════════════╗" -ForegroundColor Cyan; Write-Host "║ $args" -ForegroundColor Cyan; Write-Host "╚═══════════════════════════════════════════════╝`n" -ForegroundColor Cyan }

Clear-Host

Write-Section "TERMINAL RACER - BUILD SYSTEM (Windows 11)"

# Check prerequisites
Write-Info "[1/6] Checking prerequisites..."

$rustInstalled = $null -ne (Get-Command rustc -ErrorAction SilentlyContinue)
$dotnetInstalled = $null -ne (Get-Command dotnet -ErrorAction SilentlyContinue)

if (-not $rustInstalled) {
    Write-Error "Rust not found!"
    Write-Info "Install from: https://rustup.rs/"
    Read-Host "Press Enter to exit"
    exit 1
}

if (-not $dotnetInstalled) {
    Write-Error ".NET SDK not found!"
    Write-Info "Install from: https://dotnet.microsoft.com/download/dotnet/9.0"
    Read-Host "Press Enter to exit"
    exit 1
}

$rustVersion = rustc --version
$dotnetVersion = dotnet --version

Write-Success $rustVersion
Write-Success ".NET $dotnetVersion"

# Clean if requested
if ($Clean) {
    Write-Info "[2/6] Cleaning previous builds..."
    Remove-Item -Path "rust-renderer\target" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "game-engine\bin" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "game-engine\obj" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Success "Clean complete"
}

# Build Rust library
if (-not $SkipRust) {
    Write-Info "[3/6] Building Rust rendering library..."
    
    if (-not (Test-Path "rust-renderer\src\lib.rs")) {
        Write-Error "Error: src\lib.rs not found"
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    Write-Info "Compiling in release mode (this may take a few minutes)..."
    
    Push-Location "rust-renderer"
    cargo build --release
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Rust compilation failed"
        Pop-Location
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    Pop-Location
    
    Write-Success "Rust library compiled successfully"
    
    $libSize = (Get-Item "rust-renderer\target\release\rust_renderer.dll").Length / 1KB
    Write-Success "Library size: $([math]::Round($libSize, 1)) KB"
} else {
    Write-Info "[3/6] Skipping Rust build"
}

# Build C# game engine
if (-not $SkipDotNet) {
    Write-Info "[4/6] Building C# game engine..."
    Write-Info "Compiling C# project..."
    
    Push-Location "game-engine"
    dotnet build -c Release
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "C# compilation failed"
        Pop-Location
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    Pop-Location
    
    Write-Success "C# project compiled successfully"
} else {
    Write-Info "[4/6] Skipping .NET build"
}

# Copy Rust library
Write-Info "[5/6] Copying Rust library to C# output..."

$rustLib = "rust-renderer\target\release\rust_renderer.dll"
$csOutput = "game-engine\bin\Release\net9.0\"

if (-not (Test-Path $rustLib)) {
    Write-Error "Error: Rust library not found at $rustLib"
    Read-Host "Press Enter to exit"
    exit 1
}

Copy-Item -Path $rustLib -Destination $csOutput -Force

Write-Success "Library copied to $csOutput"

# Create launcher
Write-Info "[6/6] Creating launcher script..."

$launcherContent = @"
@echo off
cd game-engine
dotnet run --no-build -c Release
pause
"@

$launcherContent | Out-File -FilePath "run.bat" -Encoding ASCII -Force

Write-Success "Created run.bat launcher"

# Summary
Write-Section "BUILD SUCCESSFUL! 🏎️"

Write-Host "Run the game with:" -ForegroundColor Yellow
Write-Host "  run.bat" -ForegroundColor White
Write-Host ""
Write-Host "Or manually:" -ForegroundColor Yellow
Write-Host "  cd game-engine" -ForegroundColor White
Write-Host "  dotnet run --no-build -c Release" -ForegroundColor White
Write-Host ""

Write-Section "WINDOWS OPTIMIZATION TIPS"

Write-Host "For best experience:" -ForegroundColor Yellow
Write-Host "  1. Use Windows Terminal (Microsoft Store)" -ForegroundColor White
Write-Host "  2. Set terminal to fullscreen (F11)" -ForegroundColor White
Write-Host "  3. Use a monospace font (Cascadia Code, Consolas)" -ForegroundColor White
Write-Host "  4. Enable 256-color support in terminal settings" -ForegroundColor White
Write-Host ""

Write-Host "Troubleshooting:" -ForegroundColor Yellow
Write-Host "  - If DLL not found: Run from game-engine folder" -ForegroundColor White
Write-Host "  - If colors look wrong: Try different terminal" -ForegroundColor White
Write-Host "  - For best performance: Use Windows Terminal" -ForegroundColor White
Write-Host "  - If script won't run: Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope CurrentUser" -ForegroundColor White
Write-Host ""

Write-Host "Ready to race! 🏁" -ForegroundColor Green
Write-Host ""

Read-Host "Press Enter to exit"
