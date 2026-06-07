# Auto Clicker

A cross-platform auto clicker with a real C#/.NET desktop GUI, plus the existing Electron scaffold for comparison.

## Included

- Electron desktop shell and preload bridge
- React + Tailwind renderer for a modern dashboard UI
- Auto-updater integration via electron-updater
- Release workflow for Windows and macOS packaging
- Monorepo-style folder layout for desktop, website, core, automation, and shared packages

## Quick start

### .NET GUI (C# / .NET)
1. Run `dotnet build apps/desktop-dotnet/AutoClickerGui.csproj`
2. Run `dotnet publish apps/desktop-dotnet/AutoClickerGui.csproj -c Release -r linux-x64 --self-contained false`

### Electron fallback
1. Install dependencies with `pnpm install`
2. Start the dev app with `pnpm dev`
3. Build the app with `pnpm build`
4. Create distributables with `pnpm dist:win` or `pnpm dist:mac`

## Repository structure

- apps/desktop — Electron desktop experience
- apps/website — website and download landing page
- packages/core — core click logic and settings
- packages/automation — macro recorder and automation hooks
- packages/shared — shared utilities and interfaces
- .github/workflows/release.yml — release pipeline

