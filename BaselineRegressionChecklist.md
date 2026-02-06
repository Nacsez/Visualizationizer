# Baseline Regression Checklist

Use this checklist for full-session validation before release or merge.

## Startup and Exit

- App launches without exceptions.
- Default visualization renders and reacts to audio.
- `Esc` exits app cleanly.

## Left Panel Interaction

- Left panel opens when mouse touches left edge.
- Left panel close button hides panel.
- Load media button opens file dialog.
- `Shift + Load` sets startup media successfully.
- Sliders respond and update visuals immediately:
  - amplitude
  - cutoff
  - FFT bins
  - media size
  - media perturbation
- Color toggles enable/disable output colors.
- Mode buttons switch all six modes.

## Right Panel Interaction

- Right panel opens when mouse touches right edge.
- Right panel close button hides panel.
- Mic/System mode buttons switch source without crash.
- Prev/Next input buttons cycle microphone devices.
- Explicit input-device grid buttons select specific input devices.

## Media Import and Display

- SVG import works and renders correctly.
- PNG import works and renders correctly.
- JPG/JPEG import works and renders correctly.
- GIF import works and renders correctly.
- Imported media is centered and standardized on initial load.
- Media dragging works after import.
- `Del` removes loaded media.
- Startup media auto-loads on relaunch.

## Profiles and Hotkeys

- `Ctrl+1..0` saves profiles to quick slots.
- `1..0` loads profiles from quick slots.
- Loading profile restores:
  - sliders
  - colors
  - mode
  - FFT length
  - media path and position
  - audio source and selected input device
- Empty-slot load is safe (no crash).
- Overwriting existing slot works.
- Missing media path on load is handled safely.

## Audio Source and Device

- Microphone source capture works.
- System loopback capture works.
- Switching Mic/System repeatedly remains stable.
- Selecting different microphone devices updates capture source.
- Device selection persists through profile load.

## Controller and Input Handoff

- Controller mode activates only after controller input.
- Focus highlight appears on navigable controls.
- D-pad / left stick navigation moves focus predictably.
- `A` activates focused controls.
- Focused sliders adjust with controller left/right input.
- `Left Bumper` toggles left panel open/close.
- `Right Bumper` toggles right panel open/close.
- Controller `Start` holds overlay while pressed and hides on release.
- Mouse input returns control to mouse/keyboard mode.
- Keyboard input returns control to mouse/keyboard mode.
- Mouse and controller can alternate without stuck focus state.

## Overlay and Cursor UX

- `Space` hold shows overlay only while held.
- Controller `Start` hold shows same overlay behavior.
- Overlay labels align with target controls and remain readable.
- Controller focus highlight is visible on white/gray/black controls.
- Mouse cursor auto-hides after ~2 seconds inactivity.
- Cursor reappears immediately on movement/click/scroll.

## Layout and Resolution Matrix (T17)

- `1280x720`: full menu visibility, no control overlap.
- `1920x1080`: full menu visibility, no control overlap.
- `2560x1440`: full menu visibility, no control overlap.
- `3840x2160`: full menu visibility, no control overlap.
- Windowed resize stress: repeated resize up/down remains stable.
- Fullscreen toggle stress: repeated `F11` toggles remain stable.

## Visualization Modes Matrix (T17)

- Standard mode stable and reactive.
- MirroredMiddle mode stable and reactive.
- MirroredCorners mode stable and reactive.
- Radial mode stable and reactive.
- CenterColumn mode stable and reactive.
- Puddle mode stable and reactive.

## Performance and Stability (T17)

- No crashes during 15+ minute continuous run.
- No obvious memory runaway during mode switching + media switching.
- Input remains responsive during rapid panel open/close and profile switching.
