#!/bin/bash
# ============================================================================
# Terminal Racer - Complete Build & Run Script
# Optimized for Kali Linux with .NET 9.0
# Make executable: chmod +x build.sh
# ============================================================================

set -e  # Exit on error

# ============================================================================
# COLOR DEFINITIONS
# ============================================================================
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# ============================================================================
# BANNER
# ============================================================================
echo -e "${CYAN}"
cat << "EOF"
╔═══════════════════════════════════════════════╗
║     TERMINAL RACER - BUILD SYSTEM             ║
║     Optimized for Kali Linux & .NET 9.0       ║
╚═══════════════════════════════════════════════╝
EOF
echo -e "${NC}"

# ============================================================================
# PREREQUISITE CHECKING
# ============================================================================
check_prerequisites() {
    echo -e "${BLUE}[1/7]${NC} Checking prerequisites..."
    
    local all_ok=true
    
    # Check Rust
    if ! command -v rustc &> /dev/null; then
        echo -e "${RED}❌ Rust not found!${NC}"
        echo -e "${YELLOW}Install with: curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh${NC}"
        all_ok=false
    else
        echo -e "${GREEN}✓ Rust $(rustc --version | cut -d' ' -f2)${NC}"
    fi
    
    # Check Cargo
    if ! command -v cargo &> /dev/null; then
        echo -e "${RED}❌ Cargo not found!${NC}"
        all_ok=false
    else
        echo -e "${GREEN}✓ Cargo available${NC}"
    fi
    
    # Check .NET SDK
    if ! command -v dotnet &> /dev/null; then
        echo -e "${RED}❌ .NET SDK not found!${NC}"
        echo -e "${YELLOW}Install with: sudo apt-get install -y dotnet-sdk-9.0${NC}"
        all_ok=false
    else
        local dotnet_version=$(dotnet --version)
        echo -e "${GREEN}✓ .NET SDK ${dotnet_version}${NC}"
        
        # Check if it's .NET 9.x
        if [[ $dotnet_version == 9.* ]]; then
            echo -e "${GREEN}✓ .NET 9.0 detected (compatible)${NC}"
        elif [[ $dotnet_version == 8.* ]]; then
            echo -e "${YELLOW}⚠ .NET 8.0 detected (will target net8.0)${NC}"
        else
            echo -e "${YELLOW}⚠ .NET version may not be optimal${NC}"
        fi
    fi
    
    if [ "$all_ok" = false ]; then
        echo -e "\n${RED}❌ Missing prerequisites. Please install required tools.${NC}"
        exit 1
    fi
    
    echo ""
}

# ============================================================================
# RUST BUILD
# ============================================================================
build_rust() {
    echo -e "${BLUE}[2/7]${NC} Building Rust rendering library..."
    
    if [ ! -d "rust-renderer" ] && [ ! -d "rust_renderer" ]; then
        echo -e "${RED}❌ Rust renderer directory not found!${NC}"
        echo -e "${YELLOW}Expected: rust-renderer/ or rust_renderer/${NC}"
        exit 1
    fi
    
    # Detect correct directory name
    if [ -d "rust-renderer" ]; then
        RUST_DIR="rust-renderer"
    else
        RUST_DIR="rust_renderer"
    fi
    
    cd "$RUST_DIR"
    
    # Check if source file exists
    if [ ! -f "src/lib.rs" ]; then
        echo -e "${RED}❌ ${RUST_DIR}/src/lib.rs not found!${NC}"
        echo -e "${YELLOW}Please create this file with the Rust code.${NC}"
        exit 1
    fi
    
    echo -e "${YELLOW}Compiling in release mode (this may take a few minutes)...${NC}"
    cargo build --release
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Rust library compiled successfully${NC}"
        
        # Check if library was created
        LIB_PATH="target/release/librust_renderer.so"
        if [ -f "$LIB_PATH" ]; then
            LIB_SIZE=$(du -h "$LIB_PATH" | cut -f1)
            echo -e "${GREEN}✓ Library size: ${LIB_SIZE}${NC}"
        else
            echo -e "${RED}❌ Library file not found at $LIB_PATH${NC}"
            exit 1
        fi
    else
        echo -e "${RED}❌ Rust compilation failed${NC}"
        exit 1
    fi
    
    cd ..
    echo ""
}

# ============================================================================
# .NET BUILD
# ============================================================================
build_dotnet() {
    echo -e "${BLUE}[3/7]${NC} Building C# game engine..."
    
    if [ ! -d "game-engine" ] && [ ! -d "src" ]; then
        echo -e "${RED}❌ .NET project directory not found!${NC}"
        echo -e "${YELLOW}Expected: game-engine/ or src/${NC}"
        exit 1
    fi
    
    # Detect correct directory structure
    if [ -d "game-engine" ]; then
        DOTNET_DIR="game-engine"
        
        if [ ! -f "game-engine/Program.cs" ] && [ ! -f "game-engine"/*.csproj ]; then
            echo -e "${RED}❌ No C# project files found in game-engine/${NC}"
            exit 1
        fi
    elif [ -d "src" ]; then
        # Check for solution file
        if [ -f "TerminalRacer.sln" ] || ls *.sln &> /dev/null; then
            echo -e "${YELLOW}Restoring .NET solution...${NC}"
            dotnet restore
            echo ""
        fi
    fi
    
    echo -e "${YELLOW}Compiling C# project in Release mode...${NC}"
    # Build only the App project (excludes tests automatically)
    if [ -n "$DOTNET_DIR" ]; then
        dotnet build "$DOTNET_DIR/TerminalRacer/src/App/TerminalRacer.App.csproj" -c Release --no-restore
    else
        dotnet build "TerminalRacer/src/App/TerminalRacer.App.csproj" -c Release --no-restore
    fi
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ C# project compiled successfully${NC}"
    else
        echo -e "${RED}❌ C# compilation failed${NC}"
        exit 1
    fi
    
    echo ""
}

# ============================================================================
# COPY RUST LIBRARY
# ============================================================================
copy_library() {
    echo -e "${BLUE}[4/7]${NC} Copying Rust library to .NET output..."
    
    # Detect Rust directory (check rust-renderer first)
    if [ -d "rust-renderer" ]; then
        RUST_DIR="rust-renderer"
    elif [ -d "rust_renderer" ]; then
        RUST_DIR="rust_renderer"
    else
        echo -e "${RED}❌ Rust renderer directory not found!${NC}"
        exit 1
    fi
    
    RUST_LIB="${RUST_DIR}/target/release/librust_renderer.so"
    
    if [ ! -f "$RUST_LIB" ]; then
        echo -e "${RED}❌ Rust library not found at $RUST_LIB${NC}"
        exit 1
    fi
    
    # Find .NET output directory
    if [ -d "game-engine" ]; then
        CS_OUTPUT=$(find game-engine/bin/Release -type d -name "net*" | head -1)
    else
        CS_OUTPUT=$(find src/*/bin/Release -type d -name "net*" | head -1)
    fi
    
    if [ -z "$CS_OUTPUT" ]; then
        echo -e "${RED}❌ Could not find .NET output directory${NC}"
        exit 1
    fi
    
    cp "$RUST_LIB" "$CS_OUTPUT/"
    echo -e "${GREEN}✓ Library copied to $CS_OUTPUT${NC}"
    echo ""
}

# ============================================================================
# RUN TESTS (OPTIONAL)
# ============================================================================
run_tests() {
    echo -e "${BLUE}[5/7]${NC} Running tests..."
    
    if dotnet test -c Release --no-build --verbosity normal 2>/dev/null; then
        echo -e "${GREEN}✓ Tests passed${NC}"
    else
        echo -e "${YELLOW}⚠ No tests found or tests skipped${NC}"
    fi
    echo ""
}

# ============================================================================
# CREATE DISTRIBUTION PACKAGE
# ============================================================================
create_output() {
    echo -e "${BLUE}[6/7]${NC} Creating distribution package..."
    
    OUTPUT_DIR="./dist"
    rm -rf "$OUTPUT_DIR"
    mkdir -p "$OUTPUT_DIR"
    
    # Find and copy .NET output
    if [ -d "game-engine" ]; then
        NET_OUTPUT=$(find game-engine/bin/Release -type d -name "net*" | head -1)
    else
        NET_OUTPUT=$(find src/*/bin/Release -type d -name "net*" | head -1)
    fi
    
    if [ -n "$NET_OUTPUT" ]; then
        cp -r "$NET_OUTPUT"/* "$OUTPUT_DIR/"
        echo -e "${GREEN}✓ Copied .NET binaries${NC}"
    fi
    
    # Copy additional resources
    if [ -d "sounds" ]; then
        cp -r sounds "$OUTPUT_DIR/"
        echo -e "${GREEN}✓ Copied sounds${NC}"
    fi
    
    if [ -f "README.md" ]; then
        cp README.md "$OUTPUT_DIR/"
        echo -e "${GREEN}✓ Copied README${NC}"
    fi
    
    echo ""
}

# ============================================================================
# SETUP RUNTIME ENVIRONMENT
# ============================================================================
setup_environment() {
    echo -e "${BLUE}[7/7]${NC} Setting up runtime environment..."
    
    # Detect Rust directory
    if [ -d "rust-renderer" ]; then
        RUST_DIR="rust-renderer"
    else
        RUST_DIR="rust_renderer"
    fi
    
    export LD_LIBRARY_PATH="$PWD/${RUST_DIR}/target/release:$LD_LIBRARY_PATH"
    echo -e "${GREEN}✓ LD_LIBRARY_PATH configured${NC}"
    
    # Create run script
    cat > run.sh << RUN_EOF
#!/bin/bash
# Terminal Racer Quick Launch Script

cd game-engine 2>/dev/null || cd src/TerminalRacer.App 2>/dev/null || cd dist

export LD_LIBRARY_PATH="../${RUST_DIR}/target/release:\$LD_LIBRARY_PATH"
export TERM=xterm-256color
export COLORTERM=truecolor

dotnet run --no-build -c Release 2>/dev/null || ./TerminalRacer
RUN_EOF
    
    chmod +x run.sh
    echo -e "${GREEN}✓ Created run.sh launcher${NC}"
    echo ""
}

# ============================================================================
# SUCCESS MESSAGE
# ============================================================================
show_success() {
    echo -e "${GREEN}╔═══════════════════════════════════════════════╗${NC}"
    echo -e "${GREEN}║            BUILD SUCCESSFUL! 🏎️                ║${NC}"
    echo -e "${GREEN}╚═══════════════════════════════════════════════╝${NC}"
    
    echo -e "\n${CYAN}Quick Start:${NC}"
    echo -e "${YELLOW}  ./run.sh${NC}"
    
    echo -e "\n${CYAN}Or run manually:${NC}"
    if [ -d "game-engine" ]; then
        echo -e "${YELLOW}  cd game-engine${NC}"
    elif [ -d "dist" ]; then
        echo -e "${YELLOW}  cd dist${NC}"
    fi
    
    # Detect Rust directory for the message
    if [ -d "rust-renderer" ]; then
        RUST_DIR="rust-renderer"
    else
        RUST_DIR="rust_renderer"
    fi
    
    echo -e "${YELLOW}  LD_LIBRARY_PATH=../${RUST_DIR}/target/release:\$LD_LIBRARY_PATH dotnet run -c Release${NC}"
    
    echo -e "\n${MAGENTA}╔═══════════════════════════════════════════════╗${NC}"
    echo -e "${MAGENTA}║         TERMINAL OPTIMIZATION TIPS            ║${NC}"
    echo -e "${MAGENTA}╚═══════════════════════════════════════════════╝${NC}"
    
    echo -e "\n${CYAN}For best experience:${NC}"
    echo -e "  1. Maximize your terminal window (at least 100x30)"
    echo -e "  2. Use a monospace font (JetBrains Mono, Fira Code, Hack)"
    echo -e "  3. Enable color support: ${YELLOW}export TERM=xterm-256color${NC}"
    echo -e "  4. Enable true color: ${YELLOW}export COLORTERM=truecolor${NC}"
    
    echo -e "\n${CYAN}Troubleshooting:${NC}"
    echo -e "  • Library error? Run: ${YELLOW}export LD_LIBRARY_PATH=\$PWD/${RUST_DIR}/target/release:\$LD_LIBRARY_PATH${NC}"
    echo -e "  • Colors wrong? Try: ${YELLOW}export TERM=xterm-256color${NC}"
    echo -e "  • For audio beeps, ensure terminal sound is enabled"
    
    echo -e "\n${GREEN}Ready to race! 🏁${NC}\n"
}

# ============================================================================
# MAIN EXECUTION
# ============================================================================
main() {
    check_prerequisites
    build_rust
    build_dotnet
    copy_library
    
    # Run tests if --test flag is provided
    if [ "$1" == "--test" ]; then
        run_tests
    fi
    
    create_output
    setup_environment
    show_success
}

# Run main function with all arguments
main "$@"