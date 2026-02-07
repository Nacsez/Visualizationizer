# Visualizationizer 1.1

Visualizationizer is a Windows desktop audio-visualizer built with C# and MonoGame.  
Version 1.1 expands flexibility for media import, audio input control, profiles, and controller navigation while keeping the minimal-text UI design.

The original 1.0 README is archived in `README.1.0.md`.

## 1.1 Highlights

- Timed mouse auto-hide after ~2 seconds of inactivity.
- Hold-to-show help overlay:
  - `Space` (keyboard)
  - `Start/Pause` (controller)
- Resolution-aware menu scaling for better behavior across window sizes.
- Expanded media import support:
  - `.svg`, `.png`, `.jpg`, `.jpeg`, `.gif`
- Standardized initial media import scale and centered placement.
- Right-side control panel for audio/profile workflows.
- Audio source selection:
  - microphone input
  - system loopback input
- Explicit microphone device selection and device cycling controls.
- Profile quick slots (`1..0`) with save/load hotkeys.
- Right-panel quick-slot grid with mouse interaction:
  - click slot to load
  - `Ctrl + click` slot to save
- Named slot-package save/load:
  - save all 10 slots into a named profile package
  - load a named package back into slots
- Controller-aware menu navigation with visible focus and bumper panel toggles.

## Core Features

- Real-time audio-reactive visualization.
- Six visualization modes:
  - Standard
  - MirroredMiddle
  - MirroredCorners
  - Radial
  - CenterColumn
  - Puddle
- Live slider and color controls.
- Import and drag reactive media textures.
- Startup media support (`SVGs/startup.*`).

## Controls

### Global

- `Esc`: Exit
- `F11`: Toggle fullscreen
- `Del`: Remove currently loaded media
- `Space` (hold): Show overlay labels
- `1..0`: Load profile slot 1..10 (`0` is slot 10)
- `Ctrl+1..0`: Save profile slot 1..10 (`0` is slot 10)

### Left Panel

- Mouse to left edge: open left panel
- Top-left: close panel
- Top-right: load media
- `Shift + Load`: set startup media
- Sliders:
  - amplitude
  - cutoff
  - FFT bins
  - media size
  - media reactivity
- Color grid toggle buttons
- Mode buttons (6 modes)

### Right Panel

- Mouse to right edge: open right panel
- Top-left: close panel
- Audio source buttons:
  - mic input mode
  - system loopback mode
- Mic device controls:
  - previous / next device
  - explicit device grid buttons
- Quick-slot grid (5 x 2):
  - click = load slot
  - `Ctrl + click` = save slot
- Slot package buttons:
  - Save Slot Set (named package)
  - Load Slot Set (choose package from list)

### Controller

- Controller mode activates when controller input is detected.
- `Left Bumper`: toggle left panel
- `Right Bumper`: toggle right panel
- D-pad / left stick: move focus
- `A`: activate focused control
- Focused slider: left/right to adjust
- `Start` (hold): show overlay labels
- Mouse/keyboard activity switches control back to mouse/keyboard mode.

## Profiles and Data Storage

Profile data is stored under:

- `%LocalAppData%\Visualizationizer\Profiles`

Quick slots:

- `slot01.json` ... `slot10.json`

Named slot-package profiles:

- `%LocalAppData%\Visualizationizer\Profiles\Bundles\*.json`

Notes:

- Profiles save visualization state, colors, media state, mode, and sliders.
- Audio source/device selection is intentionally not part of profile save states in 1.1.

## Media Import

Supported formats:

- `.svg`
- `.png`
- `.jpg`
- `.jpeg`
- `.gif`

On import, media is standardized to the initial import scale and centered, with ongoing drag/scale/reactivity controls available in-app.

## Development

Build:

```powershell
dotnet build Visualizationizer1.0.sln
```

Build release artifacts (Windows + optional Linux probe):

```powershell
.\scripts\build-artifacts.ps1 -ProbeLinux
```

Run:

```powershell
dotnet run --project Visualizationizer1.0.csproj
```

CI workflow:

- `.github/workflows/build-artifacts.yml` publishes a Windows x64 artifact.
- It also runs a Linux native publish probe and uploads the probe log.

Linux note:

- Current codebase is Windows-targeted (`net6.0-windows`, Windows Forms, NAudio/CoreAudio).
- Native Linux publish currently fails with runtime-pack mismatch (`NETSDK1082`) and requires refactor before Linux shipping builds are possible.

## Contributing

Contributions are welcome via feature branches and pull requests.  
If you propose a change, include a short behavior/regression test pass summary with the PR.

## License

Distributed under the MIT License. See `LICENSE`.
