# Auto Clicker

A **modern, cross-platform auto clicker** with native C#/.NET and Electron UIs, featuring advanced automation capabilities like jitter, burst mode, profiles, and native support for Windows, macOS, and Linux.

## Key Features

✨ **Advanced Clicker Capabilities:**
- 🎯 Configurable CPS (clicks per second) up to 60
- 🔘 Multiple mouse buttons (left, right, middle)
- 🎭 Click modes (single click, hold-to-click)
- 🌪️ **Jitter mode** - adds random delays between clicks for realistic behavior
- 💥 **Burst mode** - group clicks together with pauses between bursts
- 📋 **Click profiles** - save and switch between configurations
- ⌨️ Global hotkey support (coming soon)

🖥️ **Cross-Platform:**
- **Windows**: Native `user32.dll` mouse_event API
- **macOS**: CoreGraphics synthetic mouse events (OSX Platform)
- **Linux**: X11/XTest for desktop automation
- All with graceful fallback and error handling

🎨 **Multiple UIs:**
- **C#/.NET Avalonia GUI** - Rich desktop experience with real-time controls
- **Electron + React** - Modern web-based interface
- **CLI** - Scriptable command-line interface for automation

📦 **Modern Architecture:**
- Modular TypeScript packages (`@auto-clicker/core`, `@auto-clicker/shared`, `@auto-clicker/automation`)
- Native .NET CLI daemon for reliable low-level input
- Cross-process IPC via stdin/stdout
- Persistent configuration storage

## Quick Start

### Prerequisites
- **Node.js** 18+ and pnpm
- **.NET 10 SDK** (for C# apps)
- On Linux: X11 development headers (`libx11-dev`, `libxtst-dev`)

### Build Everything

```bash
# Install workspace dependencies
pnpm install

# Build all TypeScript packages
pnpm -r build

# Build .NET apps
cd apps/desktop-dotnet && dotnet build -c Release
cd ../cli-dotnet && dotnet build -c Release
```

### Run the Apps

**Avalonia GUI (C#/.NET):**
```bash
cd apps/desktop-dotnet
dotnet run -c Release
```

**Electron App:**
```bash
pnpm dev           # development with hot reload
pnpm build         # build for production
pnpm dist:win      # create Windows .exe installer
pnpm dist:mac      # create macOS .dmg
```

**CLI Daemon:**
```bash
# Run the CLI which accepts commands over stdin
cd apps/cli-dotnet
dotnet run -c Release

# Send commands (in another terminal or via IPC):
# click left
# burst left 5 50
# hold left 1000
```

## Package Structure

```
packages/
├── core/              # CPS calculation, profile types, jitter/burst helpers
├── shared/            # Platform detection, config storage, utilities  
└── automation/        # CLI spawning and IPC communication

apps/
├── desktop-dotnet/    # Avalonia GUI (Windows, macOS, Linux)
├── cli-dotnet/        # Command-line daemon with stdin/stdout API
└── desktop/           # Electron app with React UI
```

### Core Package (`@auto-clicker/core`)

```typescript
import { ClickerProfile, DEFAULT_PROFILE, validateProfile, applyJitter } from '@auto-clicker/core';

const profile: ClickerProfile = {
  id: 'gaming',
  name: 'Gaming Mode',
  cps: 15,
  button: 'left',
  mode: 'click',
  jitter: { enabled: true, minMs: 5, maxMs: 15 },
  burst: { enabled: false, clicksPerBurst: 5, pauseMs: 100 },
};

const errors = validateProfile(profile);
if (errors.length === 0) console.log('Valid profile!');
```

### Shared Package (`@auto-clicker/shared`)

```typescript
import { detectPlatform, getConfigDir, ConfigStore } from '@auto-clicker/shared';

const platform = detectPlatform(); // 'win32' | 'darwin' | 'linux'
const configDir = getConfigDir();

const store = new ConfigStore('profiles.json');
const profiles = store.load([]);
store.save(profiles);
```

### Automation Package (`@auto-clicker/automation`)

```typescript
import { startCli } from '@auto-clicker/automation';

const cli = await startCli();
cli.send('click left');
cli.send('burst left 10 50');  // 10 clicks with 50ms pause between
cli.send('hold left 2000');    // hold for 2 seconds
await cli.stop();
```

## CLI Command Reference

The CLI daemon accepts simple text commands over stdin:

```
click [button] [jitter_min] [jitter_max]
  Single click with optional random delay
  Example: click left 5 15

burst [button] count delay [jitter_min] [jitter_max]
  Multiple clicks in rapid succession
  Example: burst left 5 50 (5 clicks, 50ms between)

down [button]
  Mouse button down (without releasing)

up [button]
  Mouse button up (release)

hold [button] duration_ms
  Hold mouse button for duration
  Example: hold left 1000
```

Button values: `left`, `right`, `middle`

## Building Distributables

### Windows EXE

```bash
# Using Electron Builder (NSIS installer)
pnpm dist:win

# Using .NET Publishing
cd apps/desktop-dotnet
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true -o ../../dist/
```

### macOS DMG

```bash
# Using Electron Builder
pnpm dist:mac

# Using .NET Publishing
cd apps/desktop-dotnet
dotnet publish -c Release -r osx-x64 -o ../../dist/
```

### Linux AppImage / Snap

```bash
cd apps/desktop-dotnet
dotnet publish -c Release -r linux-x64 -o ../../dist/
```

## Development Workflow

1. **Modify packages** → Run `pnpm -r build`
2. **Test packages** → `pnpm -r test`
3. **Update CLI** → Edit `apps/cli-dotnet/Program.cs`, rebuild
4. **Update GUI** → Edit `apps/desktop-dotnet/MainWindow.axaml.cs`, rebuild or `dotnet run`
5. **Test Electron** → `pnpm dev` for hot reload

## Permissions & Security

- **Windows**: No special permissions required
- **macOS**: May need accessibility permissions; synthetic events via CGEvent
- **Linux**: Requires X11 with XTest extension; typically available in most distros
  - Install with: `sudo apt install libxtst6 libx11-6 libxext6`

## Troubleshooting

| Issue | Solution |
|-------|----------|
| macOS: "Automation not allowed" | Grant accessibility permissions in System Preferences |
| Linux: X11 connection refused | Ensure running on X11 display, not Wayland |
| CLI: "Cannot open X display" | Check `DISPLAY` environment variable is set |
| Package build fails | Run `pnpm install` and `pnpm -r build` in correct order |

## License

See `LICENSE` file for details.

## Contributing

See `CONTRIBUTING.md` for guidelines.


- apps/desktop — Electron desktop experience
- apps/website — website and download landing page
- packages/core — core click logic and settings
- packages/automation — macro recorder and automation hooks
- packages/shared — shared utilities and interfaces
- .github/workflows/release.yml — release pipeline

