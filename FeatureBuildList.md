# Feature Build List

## Task Board

| ID | Task | Description | Depends On | Estimate |
|---|---|---|---|---|
| T01 | Baseline + Safety Net | Create a baseline branch, capture current behavior checklist, and define a manual regression pass for modes/sliders/colors/SVG/audio startup. | None | 0.5 day |
| T02 | Central App State Model | Add a unified `AppState` object for sliders, colors, mode, media state, audio input selection, and UI flags. | T01 | 1 day |
| T03 | Profile Data Format | Define JSON profile schema and serialization/deserialization for full runtime state. | T02 | 1 day |
| T04 | Profile Manager + Slots 1-10 | Implement profile storage paths, slot mapping logic, and profile manager APIs for save/load. | T03 | 1 day |
| T05 | Quick Save/Load Hotkeys | Add hotkeys for quick profile actions (`1-0` load and `Ctrl+1-0` save, or equivalent) with debounce. | T04 | 0.5 day |
| T06 | Timed Mouse Hide | Hide cursor after 2 seconds of inactivity and immediately restore on movement/click. | T02 | 0.5 day |
| T07 | Space-Hold Help Overlay | Show temporary button labels/overlay only while Space is held; remove on key release. | T02 | 1 day |
| T08 | Resolution-Aware Layout Engine | Make sidebar/buttons/sliders scale and reposition with viewport so the full menu stays visible on different resolutions. | T02 | 1.5 days |
| T09 | Media Import Refactor | Add a shared media loader pipeline for SVG and raster formats. | T02 | 1 day |
| T10 | Auto-Scaling on Import | Standardize imported media size ratio and aspect-fit behavior for SVG/PNG/JPEG/GIF on load. | T09, T08 | 1 day |
| T11 | Persist Media in Profiles | Include media path, position, scale, and perturbation values in saved profiles. | T04, T10 | 0.5 day |
| T12 | Audio Device Enumeration | Enumerate available audio inputs and expose selectable device list in app state/menu. | T02 | 1 day |
| T13 | System Audio (Loopback) Mode | Support selecting system audio loopback as input in addition to microphone input. | T12 | 1.5 days |
| T14 | Persist Audio Selection | Save/load audio mode and selected device as part of profile state. | T04, T13 | 0.5 day |
| T15 | Controller Navigation Layer | Add controller-based focus navigation, selection, and slider adjustments while keeping mouse support active. | T08, T02 | 2 days |
| T16 | UX Polish + Input Conflict Pass | Resolve input conflicts, improve debounce behavior, and polish visual focus/feedback cues. | T05, T07, T15 | 1 day |
| T17 | Full Regression + Performance Pass | Run full manual matrix across resolutions, visualization modes, media formats, profiles, and audio source switching. | T16 | 1 day |

## Suggested Implementation Order

1. **Foundation and Profiles**: T01 -> T02 -> T03 -> T04 -> T05
   - Establishes stable state architecture and fast workflow wins (profile slots/hotkeys).
2. **Interaction and UI Scaling**: T06 -> T07 -> T08
   - Delivers core UX quality: cleaner presentation, contextual labels, and menu visibility at any resolution.
3. **Media Pipeline Expansion**: T09 -> T10 -> T11
   - Adds multi-format import and standardized sizing while keeping profile continuity.
4. **Audio Source Flexibility**: T12 -> T13 -> T14
   - Enables explicit input selection and system loopback, then persists it through profiles.
5. **Controller + Final Stabilization**: T15 -> T16 -> T17
   - Adds alternate navigation and finishes with integration polish and regression/perf validation.

## Definition of Done

1. All requested features are implemented and integrated without breaking current visualization behavior.
2. UI remains minimal-text by default, with temporary overlay labels only on Space hold.
3. Full menu remains visible and usable across target resolutions.
4. Profiles reliably save/load colors, mode, sliders, media state, and audio source.
5. Mouse and controller inputs coexist without conflict, and quick profile switching is stable during playback.

## Current Status Snapshot (2026-02-06)

### Section 1: Foundation and Profiles (T01-T05)

- `T01` complete: feature branch and baseline regression checklist exist.
- `T02` complete for current phase: centralized runtime `AppState` added and wired to update/load flow.
- `T03` complete: JSON profile format in use.
- `T04` complete: slot-based profile save/load manager added (`1-10`).
- `T05` complete: quick hotkeys implemented (`1-0` load, `Ctrl+1-0` save).

### Section 1 Finalization Gate

Before starting Section 2 code, run and confirm:

1. Manual pass on `BaselineRegressionChecklist.md`.
2. Hotkey behavior validation in fullscreen and windowed mode.
3. Profile slot validation for:
   - empty slot load behavior,
   - overwritten slot behavior,
   - missing SVG path on load behavior.
4. Build validation (`dotnet build`) with zero errors.
5. Commit tag for Section 1 completion checkpoint.

## Build Plan: Close Section 1 and Move to Section 2

### Phase A: Section 1 Closeout (0.5 day)

1. Run the full checklist and record pass/fail notes.
2. Fix any regressions found in hotkeys/profile restore.
3. Verify profile persistence on restart with at least 3 occupied slots.
4. Finalize Section 1 with a checkpoint commit.

### Phase B: T06 Timed Mouse Hide (0.5 day)

1. Add inactivity timer state (`lastMouseMoveTime`, `mouseVisible`).
2. Detect movement delta/click and reset timer.
3. Hide cursor after 2 seconds without movement.
4. Instantly restore cursor on movement/button input.
5. Validate interaction safety for sidebar reveal, slider drag, SVG drag.

### Phase C: T07 Space-Hold Overlay Labels (1 day)

1. Add overlay visibility state (`showHelpOverlay = Space held`).
2. Define lightweight label map for key UI elements (buttons, sliders, modes).
3. Render overlay only while Space is held; no persistent labels.
4. Keep style minimal so base interface remains text-light.
5. Validate that overlay does not block clicks or drag interaction.

### Phase D: T08 Resolution-Aware Layout Engine (1.5 days)

1. Create reference-resolution layout constants (for example: 1800x1200 baseline).
2. Convert hard-coded menu/button/slider rectangles into computed layout functions.
3. Recompute layout on startup, fullscreen toggle, and resize event.
4. Ensure sidebar and all controls fit within low-height viewports.
5. Validate on multiple resolutions: `1280x720`, `1920x1080`, `2560x1440`, and `3840x2160`.

### Exit Criteria to Start Section 3

1. T06-T08 merged and stable.
2. No regressions in profile hotkeys/save-load flow.
3. Menu fully visible and usable at target resolutions.
4. Space overlay behaves as hold-to-show only.

## Progress Update (2026-02-06)

- `T06` implemented in code: cursor auto-hides after 2s inactivity and reappears immediately on movement/click/scroll.
- `T07` implemented in code: Space-hold overlay labels render only while key is held.
- `T08` implemented in code: sidebar/menu layout now recalculates from viewport height and rebuilds control geometry/textures on load, resize, and fullscreen toggle.
- Overlay polish complete: help-label shading now matches the size of each target control.
- Small-window polish complete: color grid now compresses primarily in vertical dimension while preserving horizontal alignment/spacing for better usability.
- Manual validation pass completed for Section 2 behavior.

## Section Completion (2026-02-06)

- Section 1 (`T01-T05`) complete.
- Section 2 (`T06-T08`) complete and validated.
- Next active section: Section 3 (`T09-T11`) Media Pipeline Expansion.

## Section 3 Progress (2026-02-06)

- `T09` implemented in code: shared media loader added for `.svg`, `.png`, `.jpg`, `.jpeg`, and `.gif`.
- `T10` implemented in code: imported media now applies standardized initial scale and centered placement.
- `T11` implemented in code: profile restore path now reloads persisted media through the shared loader.
- Default startup media logic now supports supported extensions (`startup.svg/png/jpg/jpeg/gif`).
- Manual validation pass completed across supported import types and profile reload scenarios.

## Section Completion (2026-02-06)

- Section 1 (`T01-T05`) complete.
- Section 2 (`T06-T08`) complete and validated.
- Section 3 (`T09-T11`) complete and validated.
- Next active section: Section 4 (`T12-T14`) Audio Source Flexibility.

## Section 4 Progress (2026-02-06)

- Added right-panel audio control scaffold using the same button style as the left menu.
- Implemented audio source switching between microphone input and system loopback (`T13`).
- Implemented microphone device cycling controls (previous/next input) with live capture restart (`T12`).
- Added profile persistence for selected audio source and selected microphone device (`T14`).
- Manual validation completed for source switching, input cycling, and profile restore behavior.

## Section Completion (2026-02-06)

- Section 1 (`T01-T05`) complete.
- Section 2 (`T06-T08`) complete and validated.
- Section 3 (`T09-T11`) complete and validated.
- Section 4 (`T12-T14`) complete and validated.
- Next active section: Section 5 (`T15-T17`) Controller + Final Stabilization.
