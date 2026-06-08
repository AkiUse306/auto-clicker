# Auto Clicker

A cross-platform auto clicker with a real C#/.NET desktop GUI, plus the existing Electron scaffold for comparison.

## Included


## Quick start

### .NET GUI (C# / .NET)
1. Run `dotnet build apps/desktop-dotnet/AutoClickerGui.csproj`
2. Run `dotnet publish apps/desktop-dotnet/AutoClickerGui.csproj -c Release -r linux-x64 --self-contained false`

## Desktop .NET app (Avalonia)

The repo includes a native C#/.NET desktop GUI at `apps/desktop-dotnet` implemented with Avalonia.

Build and publish (example):

```bash
# build electron app
pnpm build

# publish .NET app (produces artifacts under publish/)
scripts/publish-dotnet.sh

# create DMG (on macOS, after publishing)
# npm i -g create-dmg
# create-dmg --overwrite publish/osx-x64/AutoClickerGui.app dist/AutoClickerGui.dmg
```

Electron packaging is configured via `electron-builder` to produce `.exe` (NSIS) and `.dmg` targets using:

```bash
pnpm dist:win  # builds Windows installer (.exe)
pnpm dist:mac  # builds macOS .dmg
```

Notes:
- The Avalonia desktop app currently implements Windows and Linux (X11) click simulation. macOS requires additional implementation and permissions for synthetic events.
- See `apps/desktop-dotnet/MainWindow.axaml.cs` and `apps/desktop-dotnet/NativeMouse.cs` for implementation details.

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

