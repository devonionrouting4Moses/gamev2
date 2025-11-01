#!/bin/bash
cd game-engine
export LD_LIBRARY_PATH="../rust-renderer/target/release:$LD_LIBRARY_PATH"
dotnet run --no-build -c Release
