!/bin/bash
# Terminal Racer - Kali Linux Build & Run Script
# Make executable: chmod +x build.sh

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Banner
echo -e "${CYAN}"
cat << "EOF"
╔═══════════════════════════════════════════════╗
║     TERMINAL RACER - BUILD SYSTEM             ║
║     Optimized for Kali Linux                  ║
╚═══════════════════════════════════════════════╝
EOF
echo -e "${NC}"

# Check prerequisites
echo -e "${BLUE}[1/6]${NC} Checking prerequisites..."

if ! command -v rustc &> /dev/null; then
    echo -e "${RED}❌ Rust not found!${NC}"
    echo -e "${YELLOW}Install with: curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh${NC}"
    exit 1
fi

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK not found!${NC}"
    echo -e "${YELLOW}Install with: sudo apt-get install -y dotnet-sdk-9.0${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Rust $(rustc --version | cut -d' ' -f2)${NC}"
echo -e "${GREEN}✓ .NET $(dotnet --version)${NC}"

# Build Rust library
echo -e "\n${BLUE}[3/6]${NC} Building Rust rendering library..."

cd rust-renderer

# Check if source file exists
if [ ! -f src/lib.rs ]; then
    echo -e "${RED}❌ rust-renderer/src/lib.rs not found!${NC}"
    echo -e "${YELLOW}Please create this file with the Rust code from the artifacts.${NC}"
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

# Build C# project
echo -e "\n${BLUE}[4/6]${NC} Building C# game engine..."

cd game-engine

# Check if source file exists
if [ ! -f Program.cs ]; then
    echo -e "${RED}❌ game-engine/Program.cs not found!${NC}"
    echo -e "${YELLOW}Please create this file with the C# code from the artifacts.${NC}"
    exit 1
fi

echo -e "${YELLOW}Compiling C# project...${NC}"
dotnet build -c Release

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ C# project compiled successfully${NC}"
else
    echo -e "${RED}❌ C# compilation failed${NC}"
    exit 1
fi

cd ..

# Copy library to output directory
echo -e "\n${BLUE}[5/6]${NC} Copying Rust library to C# output..."

RUST_LIB="rust-renderer/target/release/librust_renderer.so"
CS_OUTPUT="game-engine/bin/Release/net9.0/"

if [ -f "$RUST_LIB" ]; then
    cp "$RUST_LIB" "$CS_OUTPUT"
    echo -e "${GREEN}✓ Library copied to $CS_OUTPUT${NC}"
else
    echo -e "${RED}❌ Rust library not found at $RUST_LIB${NC}"
    exit 1
fi

# Set library path
echo -e "\n${BLUE}[6/6]${NC} Setting up runtime environment..."

export LD_LIBRARY_PATH="$PWD/rust-renderer/target/release:$LD_LIBRARY_PATH"
echo -e "${GREEN}✓ LD_LIBRARY_PATH configured${NC}"

# Success message
echo -e "\n${GREEN}╔═══════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║            BUILD SUCCESSFUL! 🏎️                ║${NC}"
echo -e "${GREEN}╚═══════════════════════════════════════════════╝${NC}"

echo -e "\n${CYAN}Run the game with:${NC}"
echo -e "${YELLOW}  cd game-engine${NC}"
echo -e "${YELLOW}  LD_LIBRARY_PATH=../rust-renderer/target/release:$LD_LIBRARY_PATH dotnet run --no-build -c Release${NC}"

echo -e "\n${CYAN}Or use the quick start script:${NC}"
echo -e "${YELLOW}  ./run.sh${NC}"

# Create run script
cat > run.sh << 'RUN_EOF'
#!/bin/bash
cd game-engine
export LD_LIBRARY_PATH="../rust-renderer/target/release:$LD_LIBRARY_PATH"
dotnet run --no-build -c Release
RUN_EOF

chmod +x run.sh
echo -e "\n${GREEN}✓ Created run.sh launcher${NC}"

# Terminal optimization tips
echo -e "\n${MAGENTA}╔═══════════════════════════════════════════════╗${NC}"
echo -e "${MAGENTA}║         TERMINAL OPTIMIZATION TIPS            ║${NC}"
echo -e "${MAGENTA}╚═══════════════════════════════════════════════╝${NC}"

echo -e "\n${CYAN}For best experience:${NC}"
echo -e "  1. Use a terminal with good Unicode support (Kali's default works well)"
echo -e "  2. Maximize your terminal window (at least 100x30)"
echo -e "  3. Use a monospace font (JetBrains Mono, Fira Code, or Hack)"
echo -e "  4. Enable 256-color support: ${YELLOW}export TERM=xterm-256color${NC}"

echo -e "\n${CYAN}Troubleshooting:${NC}"
echo -e "  • If you see 'DllNotFoundException', run: ${YELLOW}export LD_LIBRARY_PATH=\$PWD/rust-renderer/target/release:\$LD_LIBRARY_PATH${NC}"
echo -e "  • If colors look wrong, try: ${YELLOW}export COLORTERM=truecolor${NC}"
echo -e "  • For audio beeps to work, ensure your terminal has sound enabled"

echo -e "\n${GREEN}Ready to race! 🏁${NC}\n"