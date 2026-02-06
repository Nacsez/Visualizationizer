# Baseline Regression Checklist (Phase 1)

Use this checklist before and after each Phase 1 change set.

## Startup and Exit

- App launches without exceptions.
- Default visualization renders.
- `Esc` exits app cleanly.

## Sidebar and Controls

- Sidebar opens when mouse touches left edge.
- Sidebar close button hides sidebar.
- Slider 1 changes amplitude sensitivity.
- Slider 2 changes cutoff behavior.
- Slider 3 changes FFT bin density/behavior.
- Color toggle buttons enable/disable colors.
- Mode buttons switch all 6 modes correctly.

## SVG Workflow

- SVG load button opens file dialog.
- Loaded SVG renders on screen.
- SVG can be dragged with mouse.
- `Del` removes loaded SVG.
- Shift + SVG load updates startup SVG behavior.

## Display and Input

- `F11` toggles fullscreen/windowed mode.
- Window resizing keeps rendering stable.
- Visualizer remains responsive to audio input.

## Phase 1 Additions

- Profile save hotkeys work (`Ctrl+1` to `Ctrl+0`).
- Profile load hotkeys work (`1` to `0`).
- Saved profile restores: sliders, colors, mode, FFT length, SVG path/position.
