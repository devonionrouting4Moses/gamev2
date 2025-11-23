#!/bin/bash
# Terminal Racer Quick Launch Script

cd game-engine 2>/dev/null || cd src/TerminalRacer.App 2>/dev/null || cd dist

export LD_LIBRARY_PATH="../rust-renderer/target/release:$LD_LIBRARY_PATH"
export TERM=xterm-256color
export COLORTERM=truecolor

dotnet run --no-build -c Release 2>/dev/null || ./TerminalRacer
