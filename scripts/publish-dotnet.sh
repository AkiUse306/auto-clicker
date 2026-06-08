#!/usr/bin/env bash
set -euo pipefail

# Simple publish script for the Avalonia desktop-dotnet app.
# Produces Windows .exe (self-contained single file) and macOS bundle.

ROOT_DIR=$(dirname "$0")/..
cd "$ROOT_DIR"

echo "Publishing Windows (win-x64) single-file..."
dotnet publish apps/desktop-dotnet/AutoClickerGui.csproj -c Release -r win-x64 /p:PublishSingleFile=true /p:PublishTrimmed=false -o publish/win-x64

echo "Publishing macOS (osx-x64) self-contained..."
dotnet publish apps/desktop-dotnet/AutoClickerGui.csproj -c Release -r osx-x64 /p:PublishSingleFile=false -o publish/osx-x64

echo "Done. Output under publish/ (win-x64, osx-x64)."
echo "To create a DMG from the macOS bundle, install 'create-dmg' (npm i -g create-dmg) and run:"
echo "  create-dmg --overwrite publish/osx-x64/AutoClickerGui.app dist/AutoClickerGui.dmg"
