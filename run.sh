#!/bin/bash
cd game-engine/TerminalRacer
export LD_LIBRARY_PATH="../../rust-renderer/target/release:$LD_LIBRARY_PATH"
dotnet run --project src/App/TerminalRacer.App.csproj
