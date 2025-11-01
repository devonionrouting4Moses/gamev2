@echo off
REM Terminal Racer - Windows Build & Run Script
REM Make sure you have Rust and .NET 9.0 SDK installed
REM Run as Administrator if you encounter permission issues

setlocal enabledelayedexpansion

REM Colors for output (Windows 10+)
for /F %%A in ('echo prompt $H ^| cmd') do set "BS=%%A"

echo.
echo ╔═══════════════════════════════════════════════╗
echo ║     TERMINAL RACER - BUILD SYSTEM             ║
echo ║     Windows 11 Edition                        ║
echo ╚═══════════════════════════════════════════════╝
echo.

REM Check prerequisites
echo [1/6] Checking prerequisites...
echo.

where rustc >nul 2>nul
if errorlevel 1 (
    echo ❌ Rust not found!
    echo Install from: https://rustup.rs/
    echo.
    pause
    exit /b 1
)

where dotnet >nul 2>nul
if errorlevel 1 (
    echo ❌ .NET SDK not found!
    echo Install from: https://dotnet.microsoft.com/download/dotnet/9.0
    echo.
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('rustc --version') do set RUST_VERSION=%%i
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i

echo ✓ %RUST_VERSION%
echo ✓ .NET %DOTNET_VERSION%
echo.

REM Build Rust library
echo [2/6] Building Rust rendering library...
echo.

cd rust-renderer

if not exist src\lib.rs (
    echo ❌ Error: src\lib.rs not found
    pause
    exit /b 1
)

echo Compiling in release mode (this may take a few minutes)...
cargo build --release

if errorlevel 1 (
    echo ❌ Rust compilation failed
    pause
    exit /b 1
)

echo ✓ Rust library compiled successfully
for /f %%A in ('dir /b target\release\rust_renderer.dll 2^>nul') do (
    for /f "tokens=5" %%B in ('dir target\release\rust_renderer.dll ^| find "rust_renderer"') do (
        echo ✓ Library size: %%B bytes
    )
)
echo.

cd ..

REM Build C# game engine
echo [3/6] Building C# game engine...
echo.

cd game-engine

echo Compiling C# project...
dotnet build -c Release

if errorlevel 1 (
    echo ❌ C# compilation failed
    pause
    exit /b 1
)

echo ✓ C# project compiled successfully
echo.

cd ..

REM Copy Rust library to C# output
echo [4/6] Copying Rust library to C# output...
echo.

set RUST_LIB=rust-renderer\target\release\rust_renderer.dll
set CS_OUTPUT=game-engine\bin\Release\net9.0\

if not exist "%RUST_LIB%" (
    echo ❌ Error: Rust library not found at %RUST_LIB%
    pause
    exit /b 1
)

copy "%RUST_LIB%" "%CS_OUTPUT%" >nul

if errorlevel 1 (
    echo ❌ Failed to copy library
    pause
    exit /b 1
)

echo ✓ Library copied to %CS_OUTPUT%
echo.

REM Create batch launcher
echo [5/6] Creating launcher script...
echo.

(
    echo @echo off
    echo cd game-engine
    echo dotnet run --no-build -c Release
    echo pause
) > run.bat

echo ✓ Created run.bat launcher
echo.

REM Setup complete
echo ╔═══════════════════════════════════════════════╗
echo ║            BUILD SUCCESSFUL! 🏎️                ║
echo ╚═══════════════════════════════════════════════╝
echo.

echo Run the game with:
echo   run.bat
echo.
echo Or manually:
echo   cd game-engine
echo   dotnet run --no-build -c Release
echo.

echo ╔═══════════════════════════════════════════════╗
echo ║         WINDOWS OPTIMIZATION TIPS             ║
echo ╚═══════════════════════════════════════════════╝
echo.
echo For best experience:
echo   1. Use Windows Terminal (Microsoft Store)
echo   2. Set terminal to fullscreen (F11)
echo   3. Use a monospace font (Cascadia Code, Consolas)
echo   4. Enable 256-color support in terminal settings
echo.

echo Troubleshooting:
echo   - If DLL not found: Run from game-engine folder
echo   - If colors look wrong: Try different terminal
echo   - For best performance: Use Windows Terminal
echo.

echo Ready to race! 🏁
echo.

pause
