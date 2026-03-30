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
`Form1.cs` is the root partial class that owns global UI state: device connection state and all settings. It is split across four additional partials:
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
Two separate INI files with different scopes:
- `perfiles.ini` — embedded resource; 7 predefined device profiles (Gaming USB, Gaming WiFi, Productividad, etc.) loaded by `PerfilManager`. User-named profiles live here too; supports import/export.
- `config.ini` — generated at runtime in the app directory; stores last selected profile, detected encoder cache, WiFi session state, DPI/cursor speed applied, and a `resolucion_pendiente_reset` recovery flag.

When adding a new setting, decide: is it profile-scoped (`perfiles.ini`) or app-session-scoped (`config.ini`)? `modo_otg` and resolution state are session-scoped, not profile-scoped.

### Key Dependencies
- `Guna.UI2.WinForms` (v2.0.4.7) — all UI controls; required for theming to work correctly
- `ini-parser` (v2.5.2) — profile and config persistence
- `bin/adb/` and `bin/scrcpy/` — bundled external binaries copied to output on build

## Theme

The app uses a single fixed dark purple palette hardcoded in `Form1.ApplyTheme()`. There is no theme-switching UI or Blue/Green/Light variant.

## Form1 Initialization Order

The startup sequence is order-dependent:
1. `InitializeComponent()` — designer controls
2. `CargarConfigTema()` — load config.ini (must run before BuildUI)
3. `ApplyTheme()` + `BuildUI()` — construct controls with palette colors
4. `CargarUltimoPerfilSiExiste()` — populate fields from last profile
5. `IniciarDeteccionDispositivoAsync()` — async: disconnect residuals → wait 1.5s → detect state → set `_inicializacionCompleta = true` → start `adbManager.IniciarTrackDevices()`

Event handlers (`OnDispositivoCambio`, `OnDispositivoUsbCambio`) and UI responses are suppressed until `_inicializacionCompleta` is true.

## Device Detection: Three Parallel Mechanisms

1. **Startup** (`IniciarDeteccionDispositivoAsync`) — runs once; clears WiFi residuals, waits for ADB daemon to stabilize (1.5s delay is intentional; shortening it causes race conditions).
2. **Event-driven** (`adbManager.IniciarTrackDevices`) — ADB's built-in tracking; fires `OnDispositivoCambio`/`OnDispositivoUsbCambio`. Suppressed during WiFi setup via `_operacionWifiEnCurso` flag.
3. **WiFi polling** (`MonitorearUsbConWifiAsync`) — 2s interval; detects USB cable insertion/removal while WiFi is active. Only runs when `_wifiConectado = true`.

WiFi setup temporarily disconnects USB (daemon restart triggers a device-lost event). Wrap the entire WiFi setup in `_operacionWifiEnCurso = true` to suppress spurious toasts.

## ScrcpyManager and OTG

`ScrcpyConfig` is a full state carrier (video, audio, DPI, encoder, WiFi, OTG). `ScrcpyManager.Lanzar()` builds arguments from it with explicit OTG-incompatible option skipping (e.g., `--stay-awake` is not emitted in OTG mode).

FPS capture (`PrintFps=true`) reads scrcpy stdout on a background thread and fires `OnFpsUpdate` events. All subscribers must marshal to the UI thread (`Invoke`).

## Child Process Cleanup

`Program.cs` creates a Windows Job Object with `KILL_ON_JOB_CLOSE`. All child processes launched (adb.exe, scrcpy.exe) are assigned to it via `AsignarAlJob()`. Any new external process launch must also call `AsignarAlJob()` to ensure cleanup on crash.

## Device Resolution Recovery

Resolution changes applied via ADB are not auto-reverted on crash. `resolucion_pendiente_reset` is persisted in `config.ini` and silently reverted in `ActualizarEstadoDispositivoAsync()` on the next startup.
