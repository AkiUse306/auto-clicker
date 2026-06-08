#!/usr/bin/env bash
set -euo pipefail

# Comprehensive build script for the auto-clicker project
# Builds all TypeScript packages, .NET apps, and Electron installer

ROOT=$(dirname "$0")
cd "$ROOT"

echo "=========================================="
echo "Auto Clicker - Full Build"
echo "=========================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_step() {
  echo -e "${BLUE}▶ $1${NC}"
}

log_done() {
  echo -e "${GREEN}✓ $1${NC}"
  echo ""
}

# Step 1: Install dependencies
log_step "Installing dependencies..."
pnpm install
log_done "Dependencies installed"

# Step 2: Build TypeScript packages
log_step "Building TypeScript packages..."
pnpm -r build
log_done "TypeScript packages built"

# Step 3: Build .NET CLI
log_step "Building .NET CLI (apps/cli-dotnet)..."
cd apps/cli-dotnet
dotnet build -c Release
cd ../..
log_done ".NET CLI built"

# Step 4: Build .NET GUI
log_step "Building .NET GUI (apps/desktop-dotnet)..."
cd apps/desktop-dotnet
dotnet build -c Release
cd ../..
log_done ".NET GUI built"

# Step 5: Build Electron app
log_step "Building Electron app..."
pnpm build
log_done "Electron app built"

echo ""
echo "=========================================="
echo "Build Summary"
echo "=========================================="
echo -e "${GREEN}All builds completed successfully!${NC}"
echo ""
echo "Next steps:"
echo "  • Run .NET GUI:    dotnet run -p apps/desktop-dotnet/AutoClickerGui.csproj"
echo "  • Run Electron:    pnpm dev"
echo "  • Build installers:"
echo "      - Windows:    pnpm dist:win"
echo "      - macOS:      pnpm dist:mac"
echo ""
