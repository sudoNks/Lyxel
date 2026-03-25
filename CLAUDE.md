# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MobiladorStex is a Windows desktop application (WinForms, .NET 8.0) that serves as a GUI frontend for controlling Android devices via ADB and streaming their screens via Scrcpy. The UI text is in Spanish.

## Build Commands

```bash
# Build debug
dotnet build MobiladorStex.sln

# Build release
dotnet build MobiladorStex.csproj -c Release

# Publish self-contained
dotnet publish MobiladorStex.csproj -c Release -o bin/Release/net8.0-windows10.0.17763.0/win-x64/publish
```

No test project exists. There is no linter configured beyond standard C# build warnings.

## Version Management

Version is defined in three places inside `MobiladorStex.csproj` — update all three together:
```xml
<Version>1.2.0</Version>
<AssemblyVersion>1.2.0</AssemblyVersion>
<FileVersion>1.2.0</FileVersion>
```

## Architecture

The application uses a **partial class + manager pattern**:

### Form1 (Main Window)
`Form1.cs` is the root partial class (~1849 lines) that owns global UI state: active theme (Blue/Green × Dark/Light), device connection state, and all settings. It is split across four additional partials:
- `Form1.Conexion.cs` — ADB/device connection panel logic
- `Form1.Video.cs` — codec, FPS, bitrate, encoder settings panel
- `Form1.Pantalla.cs` — resolution, aspect ratio, crop, fullscreen panel
- `Form1.Extras.cs` — stay-awake, screen-off, screensaver controls

### Manager Classes
| Class | Responsibility |
|---|---|
| `ADBManager.cs` | Wraps `adb.exe` — device tracking, WiFi pairing, resolution/DPI changes |
| `ScrcpyManager.cs` | Builds `scrcpy.exe` arguments from a config object; manages its process lifecycle |
| `PerfilManager.cs` | Reads/writes named profiles from `perfiles.ini` using ini-parser |

### Supporting Components
- `FloatingWindow.cs` — draggable toolbar shown while Scrcpy is running
- `DialogoAvanzado.cs` — confirmation dialog with per-feature checkboxes for advanced options
- `ToastNotification.cs` — in-app success/warning/error/info toasts
- `StexNumericUpDown.cs` — custom numeric spinner that respects the active theme

### Data Flow
```
UI (Form1 partials) → Manager classes → adb.exe / scrcpy.exe → Android device
```

### Configuration Files
- `perfiles.ini` — embedded resource; 7 predefined device profiles (Gaming USB, Gaming WiFi, Productividad, etc.) loaded by `PerfilManager`
- `config.ini` — generated at runtime in the app directory; stores theme, last selected profile, and detected encoders

### Key Dependencies
- `Guna.UI2.WinForms` (v2.0.4.7) — all UI controls; required for theming to work correctly
- `ini-parser` (v2.5.2) — profile and config persistence
- `bin/adb/` and `bin/scrcpy/` — bundled external binaries copied to output on build
