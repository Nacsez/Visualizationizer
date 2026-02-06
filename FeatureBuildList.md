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