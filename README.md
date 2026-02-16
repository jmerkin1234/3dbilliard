# 3D Billiards Simulation

A physics-accurate 3D billiards game built in Unity 6 (HDRP).

## Project Structure

Two parallel pipelines:

- **Pipeline 1** (Blender) — Asset & material creation, real-world scale modeling, PBR textures, export
- **Pipeline 2** (Unity) — Physics systems, cue mechanics, gameplay logic, validation

## Pipeline 2 — Unity Milestone Roadmap

| Phase | Milestones | Scripts |
|-------|-----------|---------|
| 1. Project Foundation | 1-2 | Folder structure, physics config |
| 2. Core Ball Physics | 3-5 | BallPhysics.cs, BallSpin.cs, BallSleepMonitor.cs |
| 3. Table Interaction | 6-7 | RailResponse.cs, PocketTrigger.cs |
| 4. Cue & Input | 8-10 | CueAim.cs, ShotPower.cs, CueStrike.cs |
| 5. Gameplay Layer | 11-12 | TurnManager.cs, Rule Engine |
| 6. Visual Realism | 13-14 | Asset import, reflection & lighting |
| 7. Validation | — | Full physics & stability test suite |

## Physics Constants

| Property | Value |
|----------|-------|
| Ball diameter | 0.05715 m |
| Ball mass | 0.17 kg |
| Ball friction | 0.03 |
| Ball bounciness | 0.92 |
| Felt friction | 0.05 |
| Rail friction | 0.06 |
| Rail bounce | 0.86 |
| Fixed timestep | 0.02 s |
| Solver iterations | 12-16 |

## Tech Stack

- Unity 6000.3.6f1
- HDRP (High Definition Render Pipeline)
- Blender (asset pipeline)

## Folder Layout

```
Assets/
 ├── Scripts/
 │    ├── Physics/         BallPhysics, BallSleepMonitor
 │    ├── Spin/            BallSpin
 │    ├── Cue/             CueAim, ShotPower, CueStrike
 │    ├── Table/           RailResponse, PocketTrigger
 │    ├── GameState/       TurnManager, RuleEngine
 │    └── Debug/           Test utilities
 ├── Prefabs/
 ├── Materials/            HDRP materials (reassign textures on new model import)
 ├── PhysicsMaterials 1/   Ball, Felt, Rails physics materials
 └── Scenes/               PoolGame (main), BilliardTestScene (deprecated)
```

## Progress

- [x] Phase 1 — Project Foundation
- [x] Phase 2 — Core Ball Physics
- [x] Phase 3 — Table Interaction
- [x] Phase 4 — Cue & Input System
- [x] Phase 5 — Gameplay Layer
- [x] Phase 6 — Asset Integration ✅ COMPLETE (2026-02-15)
- [ ] Phase 7 — Validation Protocol (IN PROGRESS: cue stick alignment validated 2026-02-16)

## Phase 6: Asset Integration (2026-02-15) ✅

### Fresh Scene Created
- **PoolGame.unity**: New production scene created from scratch
- Old BilliardGameScene deprecated (position issues)
- Camera at (0, 2, -3) with proper framing
- Original FBX positions preserved from Blender export

### PoolTableFixed Model Import
- Imported PoolTableFixed.fbx (15MB) with corrected mesh origins from Blender
- 34 child objects: 15 numbered balls, cueball, cue stick, felt, 6 rails, 6 pockets, frame, lights
- Model instantiated at (0, 0, 0) with original child positions intact

### Physics Configuration Complete (110 components via Unity MCP)
- **16 Balls**: Rigidbody (mass 0.17kg), SphereCollider (radius 0.028575m), BallPhysics, BallSpin, Ball.physicMaterial
- **Cueball extras**: BallSleepMonitor, LineRenderer (aim guide), "CueBall" tag
- **Felt**: BoxCollider with Felt.physicMaterial (friction 0.7)
- **6 Rails**: BoxCollider + Rails.physicMaterial (friction 0.2, bounce 0.9) + RailResponse script
- **6 Pockets**: SphereCollider (trigger, radius 0.05m) + PocketTrigger script
- **Cue Stick**: CueAim, ShotPower, CueStrike scripts

### Component References Wired (5 references via instanceID)
- CueAim.cueBall → cueball Transform (-28648)
- CueAim.aimLineRenderer → cueball LineRenderer (56306)
- CueStrike.cueBall → cueball GameObject (56244)
- TurnManager.cueAim → cuestick CueAim (-31552)
- TurnManager.shotPower → cuestick ShotPower (-31556)

### Ball Positioning (FINAL FIX 2026-02-15)
- **Critical Issue**: Balls were bouncing on Play Mode due to physics material instances and positioning
- **Root Cause Analysis**:
  - Felt Transform Y: 0.752951
  - Felt BoxCollider center offset Y: 0.0021005
  - Felt BoxCollider size Y: 0.032456
  - **Felt collider TOP surface**: 0.752951 + 0.0021005 + (0.032456/2) = **0.771279**
  - Ball radius: 0.028575m
  - Ball SphereCollider contactOffset: 0.01m
  - **ATTEMPT 1 (WRONG)**: Y=0.781526 (used Transform Y instead of collider top) → penetration → bounced
  - **ATTEMPT 2 (WRONG)**: Y=0.799854 (collider top + radius, ignored contactOffset) → still bounced
  - **ATTEMPT 3 (WRONG)**: Y=0.809854 (added contactOffset) + bounceCombine=1 → still bounced (old material instances)
- **Final Solution (WORKING)**:
  - **Physics Material Fix**: Set Ball.physicMaterial bounceCombine=1 (Minimum) instead of 3 (Maximum)
  - **Scene Reload**: Reloaded scene to force Unity to recreate material instances from updated asset
  - **Position with Clearance**: Y=**0.811854** (2mm clearance above felt to prevent floating-point precision issues)
  - **Formula**: 0.771279 (felt top) + 0.028575 (radius) + 0.01 (contactOffset) + 0.002 (clearance) = 0.811854
  - **Cue stick**: X=-1.0, Y=0.811854, Z=0, Rotation=(0,0,0) - 44cm behind cueball
  - **Result**: Balls settle naturally onto felt with **ZERO BOUNCING** (bounceCombine Minimum = 0)

### Tools Created
- **AssignPoolTableMaterials.cs**: Auto-assigns HDRP materials to model objects (Tools → Assign Pool Table Materials)
- **WirePoolGameReferences.cs**: Manual reference wiring tool (backup utility)

### Implementation Details
- 10 Unity MCP batch operations (~140 commands total)
- All physics materials assigned (23 colliders)
- Scene saved and verified
- Zero compilation errors
- Ready for Play Mode validation

### Ready for Phase 7
- All physics components functional
- Component references validated
- Original positions preserved
- Controls: Mouse to aim, Hold Space to charge, Release to shoot

## Phase 7: Validation Update (2026-02-16)

### Cue Stick Play Mode Alignment Fix ✅
- **Issue:** Cue stick was in the correct position in `PoolGame.unity` edit mode but shifted/mis-rotated on entering Play Mode.
- **Root cause:** `CueAim` assumed cue mesh forward axis was always `transform.forward` (+Z), but the imported cue mesh is authored along local +X.
- **Fix implemented:**
  - Added configurable cue mesh forward axis in `CueAim` (`cueForwardAxis`, default `Right`).
  - Rotation now aligns the configured local cue axis to shot direction instead of hard-setting `transform.forward`.
  - Cue distance math now uses world-space sphere radius and optional startup pivot calibration (`cuePivotToTipOffset`) for stable placement.
  - Saved scene defaults on cue stick: `cueForwardAxis=Right`, `autoCalibratePivotOffset=true`.
- **Validation result (Unity MCP, Play Mode):**
  - Cue ball: `(-0.561705, 0.811854, 0.0)`
  - Cue stick: `(-0.620280, 0.811854, ~0.0)`, rotation `(0, 0, 0)`
  - Console warnings/errors: `0`

### Aim UI + Shot UI Integration ✅
- **Scene wiring:** Attached `Billiards.UI.GameUI` to `GameManager` in `PoolGame.unity`.
- **Runtime UI verified in Play Mode (Unity MCP):**
  - `GameCanvas` present
  - `AimSlider` present
  - `PowerSlider` present
  - Console warnings/errors: `0`
- **UX refinement:** `GameUI` now syncs slider state with current cue angle on startup to prevent first-input aim snapping.
- **Layout refinement:** Kept sliders vertical and moved clusters to opposite sides of screen:
  - Left side: Shot/Power (`PowerSlider`, release to shoot)
  - Right side: Aim (`AimSlider` + `LockButton`)
- **Visibility refinement:** Both clusters are constrained to a shared mid-screen height band so controls stay visible on-screen.
- **Alignment refinement:** Aim and shot sliders now use the exact same Y anchor so they line up horizontally across sides.
- **Input compatibility:** EventSystem setup now prefers Input System UI module when available, with fallback to `StandaloneInputModule`.
- **Shot flow update:** Removed explicit shoot button. In locked phase, releasing the power slider triggers `ShotPower.TryShoot()` using the current slider percentage.
- **Input reliability fix:** Added `EndDrag` handling in addition to `PointerUp` so release-to-shoot still fires if the pointer is released outside slider bounds.
- **State-flow hardening:** Added `ShotPower.TryShoot()` and only transition UI to balls-moving when a shot actually emits (prevents low-power no-shot phase lock).
- **Aim line visibility update:** Aim line now starts from cueball surface (not center) and uses a slight height offset above felt for clearer path visibility.
- **Fine aim update:** Added mouse wheel fine control in `CueAim` for small incremental angle adjustments while aiming.
- **Fine aim compatibility fix:** Mouse-wheel fine aim now normalizes scroll deltas and reads from both Input System and Legacy Input Manager (when enabled), fixing non-responsive wheel input on some setups.
- **UI sync update:** Aim slider/value now auto-sync when angle changes from non-slider input (e.g., mouse wheel).
- **UI input reliability fix:** EventSystem now keeps both input modules present and auto-selects the active backend (Input System when devices are active, otherwise Standalone) to prevent non-draggable sliders.
- **Startup phase fix:** UI now force-starts in unlocked aiming state so `AimSlider` is interactable on play start.
- **Aim line runtime fix:** `CueAim` now forces cueball `LineRenderer` enabled at setup/runtime to prevent missing in-game aim line.
- **Status text position update:** Added `statusTextOffsetY` in `GameUI` and moved top-center status text lower (`-115`) so it sits closer to the table view.
- **Red-dot drag fix:** Removed `EventTrigger`-based handle/release hooks (which could block drag routing) and replaced them with narrow pointer-interface relays; both `AimSlider` and `PowerSlider` are now interactable at play start.
- **Cue/guide visibility update:** `CueAim` now hides the cue stick mesh and aim line immediately on shot release, and restores both when all balls stop.
- **Cue/guide visibility hardening:** `GameUI` now also explicitly calls `CueAim.SetShotVisualsHidden(true/false)` on shot start/end so cue and aim line always hide during `BallsMoving` and always restore after stop.
- **Balls-moving stuck fix:** `BallSleepMonitor` now uses horizontal speed + rigidbody sleep/kinematic state for stop detection, and `TurnManager` includes a timeout watchdog fallback to recover from rare missed stop events after chaotic breaks.
- **Physics realism tuning pass:**
  - `CueStrike` now converts gameplay power into physical impulse using `powerToImpulseScale` (default `0.13`) before applying `ForceMode.Impulse` to prevent unrealistically high cue-ball speeds.
  - `CueStrike` spin injection lowered (`spinMultiplier` default `18`) and now scales from physical impulse magnitude.
  - `RailResponse` no longer clears/reinjects spin state on rail collisions; it now applies mild tangential velocity loss and physically plausible spin damping/inversion using real rigidbody angular velocity.
  - `BallSpin` now uses tunable serialized realism coefficients (sliding friction/base spin decay/extra sidespin decay) and keeps internal spin state synced to rigidbody angular velocity.
  - `BallPhysics` rolling resistance now acts on horizontal velocity only and uses tunable serialized coefficients.
- **Material calibration update:**
  - Ball material tuned to `friction=0.03`, `bounciness=0.92`.
  - Felt material reduced to `friction=0.05` to avoid double-counting cloth drag with script-based rolling/sliding friction.
  - Rail material tuned to `friction=0.06`, `bounciness=0.86`, `bounceCombine=Multiply` for less exaggerated cushion rebounds.

### Re-Validation Snapshot (Unity MCP, 2026-02-16)
- Active scene confirmed: `Assets/Scenes/PoolGame.unity`
- Play mode control: enter/exit succeeded (`manage_editor` play/stop)
- Console issues in play mode (`error` + `warning`): `0`
- Runtime UI object checks:
  - `GameCanvas=1`, `AimSlider=1`, `PowerSlider=1`, `LockButton=1`, `ShootButton=0`, `EventSystem=1`
- Runtime UI layout checks:
  - `AimSlider` anchor: `anchorMin=(0.86, 0.52)`, `anchoredPosition=(0, -20)`
  - `PowerSlider` anchor: `anchorMin=(0.14, 0.52)`, `anchoredPosition=(0, -20)`
  - `LockButton` anchor: `anchorMin=(0.86, 0.52)`, `anchoredPosition=(0, -220)`
  - `AimSlider` and `PowerSlider` share the same Y anchor/offset and are split across opposite X sides
- EventSystem module check: `UnityEngine.InputSystem.UI.InputSystemUIInputModule` present
