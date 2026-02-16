# Pipeline 2 — Phase Tracker

## Phase Status

| Phase | Status | Agent | Date Started | Date Completed |
|-------|--------|-------|-------------|----------------|
| 1. Project Foundation | COMPLETE | Planner | 2026-02-11 | 2026-02-11 |
| 2. Core Ball Physics | COMPLETE | Agent 1 | 2026-02-11 | 2026-02-15 (Refined) |
| 3. Table Interaction | COMPLETE | Agent 3 | 2026-02-11 | 2026-02-15 (Refined) |
| 4. Cue & Input System | COMPLETE | Agent 2 | 2026-02-11 | 2026-02-15 (Refined) |
| 5. Gameplay Layer | COMPLETE | Agent 4 | 2026-02-11 | 2026-02-15 (Refined) |
| 6. Asset Integration | COMPLETE | User + Planner | 2026-02-15 | 2026-02-15 |
| 7. Validation Protocol | IN PROGRESS | Planner | 2026-02-15 | — |

---

## Phase 1 — Project Foundation

### Milestone 1 — Project Structure Setup
**Status:** COMPLETE

Created folder structure (updated 2026-02-15 after pre-model cleanup):
```
Assets/
 ├── Scripts/
 │    ├── Physics/
 │    ├── Spin/
 │    ├── Cue/
 │    ├── Table/
 │    ├── GameState/
 │    └── Debug/
 ├── Prefabs/
 ├── Materials/         (hand-crafted HDRP materials retained)
 ├── PhysicsMaterials 1/
 └── Scenes/            (BilliardGameScene + BilliardTestScene)
```

Note: Assets/Models/, Assets/Textures/, and Assets/2/ removed during pre-model cleanup. Models/ will be recreated on new FBX import.

Render Pipeline: **HDRP** (High Definition Render Pipeline)
Fixed Timestep: **0.02** (confirmed in TimeManager.asset)

### Milestone 2 — Global Physics Configuration
**Status:** COMPLETE

Physics Settings (DynamicsManager.asset):
| Setting | Default | Billiards Value |
|---------|---------|----------------|
| Solver Iterations | 6 | **14** |
| Velocity Iterations | 1 | **10** |
| Bounce Threshold | 2.0 | **0.1** |
| Sleep Threshold | 0.005 | **0.001** |
| Gravity | -9.81 | -9.81 (unchanged) |

Physics Materials Created:
| Material | Dynamic Friction | Static Friction | Bounciness | Bounce Combine |
|----------|-----------------|----------------|------------|---------------|
| Ball | 0.03 | 0.03 | 0.92 | Multiply |
| Felt | 0.05 | 0.05 | 0.00 | Average |
| Rails | 0.06 | 0.06 | 0.86 | Multiply |

### Definition of Done
- [x] Folder structure complete
- [x] Physics materials created (Ball, Felt, Rails)
- [x] Physics settings configured
- [x] Fixed timestep confirmed at 0.02
- [x] Project organized — no loose files

---

## Phase 2 — Core Ball Physics
**Status:** COMPLETE (Agent 1) — Started 2026-02-11, Completed 2026-02-11

### Milestone 3 — BallPhysics.cs Base
- [x] Rigidbody caching (Awake, cached rb + sphereCollider)
- [x] Rolling resistance simulation (mu_roll * m * g, opposes velocity)
- [x] Manual slowdown logic (sliding friction toward pure roll condition v = omega * r)
- [x] FixedUpdate-only discipline (no Update physics)
- [x] Test: Straight roll decays realistically (implementation complete, Play Mode validation pending)

Implementation details:
- Rolling resistance coefficient: 0.01
- Sliding friction coefficient: 0.2
- Min velocity threshold: 0.001 (auto-zero below this)
- Ball configures its own Rigidbody: mass 0.17, drag 0, angular drag 0, interpolate, continuous
- Public accessors: Velocity, AngularVelocity, Speed, IsMoving, Rb
- Public methods: ApplyImpulse(), StopMotion()

### Milestone 4 — BallSpin.cs
- [x] Store angular velocity (via Rigidbody.angularVelocity)
- [x] Topspin/backspin/sidespin tracking (TopBackSpin, SideSpin properties)
- [x] Spin decay (exponential decay, rate 0.5/s)
- [x] Spin → velocity transfer (surface velocity diff applied at SpinTransferRate 0.15)
- [x] Test: Follow/draw/stop shots (implementation complete, Play Mode validation pending)

Implementation details:
- InjectSpin(Vector3) for cue strike spin injection
- ClearSpin() to reset
- HasSpin property for quick check
- Spin transfer uses contact-point surface velocity formula

### Milestone 5 — BallSleepMonitor.cs
- [x] Velocity threshold detection (speed < 0.005, angular < 0.01)
- [x] OnBallStopped event (static Action<BallPhysics>)
- [x] OnAllBallsStopped event (static Action)
- [x] Test: No false positives (implementation complete, Play Mode validation pending)

Implementation details:
- Sleep confirmation timer: 0.15s sustained stillness required
- RefreshTrackedBalls() scans scene for all BallPhysics
- RegisterBall() / UnregisterBall() for dynamic add/remove
- allBallsWereMoving flag prevents re-firing

### Test Scene
- BilliardTestScene.unity created in Assets/Scenes/
- Contains: Camera, Directional Light, TableSurface (9ft table 2.54x1.27), TestBall
- DebugBallLauncher.cs attached for keyboard testing:
  - Space: straight shot
  - B: draw/backspin shot
  - F: follow/topspin shot
  - S: stop shot (half power, no spin)
  - R: reset position

---

### Definition of Done
- [x] Physics works without cue system
- [x] Motion is deterministic
- [ ] Play Mode validation (deferred to Phase 7)

---

## Phase 3 — Table Interaction
**Status:** COMPLETE (Agent 3) — Started 2026-02-11, Completed 2026-02-11

### Milestone 6 — RailResponse.cs
- [x] Reflection formula: R = V - 2(V·N)N (implemented with normal from collision contact)
- [x] Restitution scaling (rail restitution 0.9, applied to normal component only)
- [x] Energy loss factor (0.95 multiplier per bounce, prevents energy gain)
- [x] Spin inversion (sidespin Y-component inverts with 0.7 factor)
- [ ] Test: 45° rebound, no energy gain (Play Mode validation pending)

Implementation details:
- OnCollisionEnter triggers reflection physics
- Normal calculated from collision contact or configured railNormal
- Reflection: R = V - 2(V·N)N with separate normal/tangential components
- Restitution only affects normal component: -N * 0.9
- Energy clamped to prevent gain: if |finalV| > |initialV|, clamp to |initialV|
- Spin inversion via ClearSpin() + InjectSpin() with inverted Y-axis
- Gizmo visualization of rail normal in editor

### Milestone 7 — PocketTrigger.cs
- [x] Trigger collider detection (OnTriggerEnter with ball tag check)
- [x] Disable ball physics on pocket (Rigidbody kinematic, collider disabled, renderer disabled)
- [x] OnBallPocketed event (static Action<GameObject, PocketTrigger>)
- [ ] Test: Clean detection, no ghosts (Play Mode validation pending)

Implementation details:
- Requires trigger collider (auto-enables if not set)
- Ball tag filtering ("Ball")
- Configurable disable delay (default 0.1s for visual feedback)
- Stops motion via BallPhysics.StopMotion() + BallSpin.ClearSpin()
- Disables Rigidbody (kinematic + detectCollisions false)
- Disables collider and renderer
- Unregisters from BallSleepMonitor
- Moves ball to y=-10 after disable
- Prevents double-triggering via activeInHierarchy check
- Gizmo visualization in editor

### Definition of Done
- [x] Rail rebounds realistic (implementation complete)
- [x] Pockets function reliably (implementation complete)
- [ ] Play Mode validation (deferred to Phase 7)

---

## Phase 4 — Cue & Input System
**Status:** COMPLETE (Agent 2) — Started 2026-02-11, Completed 2026-02-11

### Milestone 8 — CueAim.cs
- [x] Mouse rotation (horizontal mouse input with smooth damping)
- [x] Normalized direction vector (AimDirection property)
- [x] Input lock for TurnManager (InputEnabled property)
- [ ] Test: Stable, no jitter (Play Mode validation pending)

Implementation details:
- Mouse X input controls rotation around cue ball (Y-axis)
- Smooth rotation via Mathf.DeltaAngle + lerp (prevents jitter)
- Auto-finds cue ball by "CueBall" tag if not assigned
- Positions cue stick behind ball at configurable distance
- Public accessors: AimDirection, AimAngle, InputEnabled
- Public methods: SetAimAngle(), ResetAim()
- Gizmo visualization for aim direction and cue position

### Milestone 9 — ShotPower.cs
- [x] Hold-to-charge (mouse button hold duration)
- [x] Clamped max impulse (min 0.5N, max 8N, configurable)
- [x] Exponential curve option (power 1.5 default, linear fallback)
- [ ] Test: Scales correctly (Play Mode validation pending)

Implementation details:
- Charge time configurable (default 2s to max power)
- OnShotReleased event fires with final impulse value
- Exponential scaling: Pow(normalized, curvePower) for faster start
- Public accessors: NormalizedPower, CurrentImpulse, IsCharging, InputEnabled
- Public method: TriggerShot(float) for AI/testing
- On-screen power meter (GUI label + bar)

### Milestone 10 — CueStrike.cs
- [x] Apply impulse to cue ball (via BallPhysics.ApplyImpulse)
- [x] Spin injection via offset (vertical offset = topspin/backspin, horizontal = sidespin)
- [x] Respect mass (0.17 kg via BallMass constant)
- [ ] Test: Center/low/high hit behavior (Play Mode validation pending)

Implementation details:
- Subscribes to ShotPower.OnShotReleased event
- Applies impulse: direction (from CueAim) * force (from ShotPower)
- Spin calculation based on contact point offset from ball center:
  - High hit (+Y offset) → topspin (rotation around right axis)
  - Low hit (-Y offset) → backspin (rotation around right axis)
  - Right hit (+X offset) → right english (rotation around up axis)
  - Left hit (-X offset) → left english (rotation around up axis)
- Max offset clamped to 70% of ball radius
- Spin scales with impulse force and offset distance
- Public methods: SetContactOffset(), ResetContactOffset()
- Gizmo visualization of contact point and offset vector

### Compilation Fix
- Fixed namespace collision: All `Debug` calls replaced with `UnityEngine.Debug` prefix
- Affected files: CueAim.cs, ShotPower.cs, CueStrike.cs, PocketTrigger.cs
- Compilation verified: **0 errors** after fix

### Definition of Done
- [x] Full shot pipeline functional (CueAim → ShotPower → CueStrike → BallPhysics/BallSpin)
- [x] Zero compilation errors (Debug namespace collision resolved)
- [ ] Play Mode validation (deferred to Phase 7)

---

## Phase 5 — Gameplay Layer
**Status:** COMPLETE (Agent 4) — Started 2026-02-11, Completed 2026-02-11

### Milestone 11 — TurnManager.cs
- [x] Single-player game modes (Training, VsComputer)
- [x] Training mode: Unlimited shots, no turn switching
- [x] VsComputer mode: Player vs AI with turn switching on fouls
- [x] Shot validation (via ValidateShot, integrates with RuleEngine)
- [x] Foul logic (scratch detection, OnFoul event)
- [x] Rack reset (ResetRack method)

Implementation details:
- GameMode enum: Training (free practice), VsComputer (player vs AI)
- TurnOwner enum: Player, AI
- Turn state machine: PlayerAiming/AIAiming → BallsMoving → TurnEnding
- Training mode: Always continues player turn regardless of fouls
- VsComputer mode: Legal shot = continue, foul = switch to AI
- Subscribes to BallSleepMonitor.OnAllBallsStopped for turn progression
- Controls input enable/disable for CueAim and ShotPower
- Events: OnPlayerTurnStart, OnAITurnStart, OnFoul
- Auto-finds CueAim and ShotPower components if not assigned
- Scratch detection via PocketTrigger.OnBallPocketed
- AI turn placeholder (StartAITurn method for future AI implementation)

### Milestone 12 — Rule Engine
- [x] Legal contact detection (RecordFirstContact for first ball hit)
- [x] Pocket tracking (tracks all pocketed balls per shot)
- [x] Scratch detection (cue ball pocketed = foul)

Implementation details:
- Shot tracking begins on shot release, ends on balls stopped
- Tracks: firstBallContacted, ballsPocketed, cueBallPocketed
- ValidateShot() returns true/false for legal/foul
- OnFoulDetected event with foul description string
- Public accessors: GetBallsPocketed(), IsScratch(), GetFirstContact()
- Integrates with TurnManager for turn flow

### Definition of Done
- [x] Turn flow stable (state machine implemented)
- [x] No stuck states (all transitions handled)
- [x] Zero compilation errors
- [ ] Play Mode validation (deferred to Phase 7)

---

## Phase 6 — Asset Integration
**Status:** COMPLETE (2026-02-15)

### Milestone 13 — Asset Import Integration
- [x] Extract materials
- [x] Assign HDRP shaders
- [x] Verify scale (ball 0.05715m)
- [x] Assign colliders + rigidbodies + physics materials

### Milestone 14 — Reflection & Lighting
- [ ] Reflection Probe
- [ ] Box Projection
- [ ] Bake reflections
- [ ] Ball gloss verification

Note: Remaining visual realism polish (probe bake/lookdev) is deferred while gameplay validation runs.

---

## Phase 7 — Validation Protocol
**Status:** IN PROGRESS

- [ ] Straight shot distance
- [ ] Stop shot
- [ ] Draw shot reversal
- [ ] Follow shot roll-through
- [ ] 45° rail rebound
- [ ] Double rail consistency
- [ ] No sliding forever
- [ ] No energy gain
- [ ] No jitter at rest
- [x] Cue stick stays aligned in Play Mode (edit/runtime transform parity)
- [x] Aim UI integrated in PoolGame and present in Play Mode
- [x] Shot UI integrated in PoolGame and present in Play Mode
- [ ] 60+ FPS stable
- [ ] Deterministic physics confirmed

---

## 2026-02-15 Refinements (Logic Audit)
- **Physics Consolidation:** Moved all ball-cloth interaction logic (sliding friction, English transfer) into `BallSpin.cs`. Used force-based calculations instead of direct velocity manipulation for better physical realism.
- **Race Condition Fix:** Updated `PocketTrigger.cs` to use independent coroutines per ball, preventing state overwrites when multiple balls are pocketed simultaneously.
- **Rule Integration:** Created `CueBallCollision.cs` and integrated `RuleEngine.cs` fully with `TurnManager.cs`. Shot validation now respects contact rules.
- **Performance:** Optimized `CueAim.cs` to cache renderers, reducing per-frame overhead.
- **Stability:** Refined `RailResponse.cs` to prevent double-bounce artifacts by delegating base reflection to Unity's physics engine.

## 2026-02-16 Validation Update (Cue Alignment)
- **Issue Reproduced:** Cue stick matched scene placement in edit mode but shifted/rotated when entering Play Mode in `PoolGame.unity`.
- **Root Cause:** `CueAim` hard-aligned `transform.forward` to shot direction, but the imported cue mesh points along local +X.
- **Fix Implemented:** Added cue mesh axis selection (`cueForwardAxis`) and alignment logic that maps configured local axis to aim direction; also made cue placement radius world-scale aware and added startup pivot calibration support.
- **Scene Defaults Saved:** `CueAim.cueForwardAxis = Right`, `CueAim.autoCalibratePivotOffset = true`.
- **Validation (Unity MCP):** In Play Mode, cue ball remained at `(-0.561705, 0.811854, 0.0)` and cue stick remained at `(-0.620280, 0.811854, ~0.0)` with `0` console warnings/errors.

## 2026-02-16 Validation Update (Aim + Shot UI)
- **Issue:** Aim/power/shoot UI controller existed (`GameUI.cs`) but was not attached in `PoolGame.unity`, so UI did not appear at runtime.
- **Scene Fix:** Added `Billiards.UI.GameUI` component to `GameManager`.
- **Code Fix:** Updated `GameUI.cs` startup flow to sync UI controls with cue state on load (`SyncUIWithCueState`) so aim slider starts at current cue angle (prevents first-input jump).
- **Layout Fix:** Kept both sliders vertical and split control clusters across sides:
  - Left cluster: power (release to shoot)
  - Right cluster: aim + lock
- **Input Fix:** EventSystem creation now uses Input System UI module when available (fallback to `StandaloneInputModule`) to ensure UI interaction works across project input backends.
- **Shot UX Fix:** Removed explicit shoot button; releasing `PowerSlider` in locked phase now fires the shot using the current normalized power.
- **Shot Trigger Reliability:** Added `EndDrag` shot trigger path so releasing off-slider still executes the selected power shot.
- **State-Flow Hardening:** Added `ShotPower.TryShoot()` and updated `GameUI.OnPowerReleased(...)` to transition to `BallsMoving` only when a shot actually emits (prevents low-power no-shot phase lock).
- **Visibility Fix:** Forced both control clusters onto a shared mid-screen Y band to keep `LockButton` and both sliders visible.
- **Alignment Fix:** Pinned aim cluster Y to shot cluster Y so `AimSlider` and `PowerSlider` stay level.
- **Validation (Unity MCP):**
  - Play Mode object checks: `GameCanvas`, `AimSlider`, `PowerSlider` found.
  - Runtime EventSystem module: `InputSystemUIInputModule`.
  - UI positions (runtime): `PowerSlider` and `AimSlider` on opposite X sides at same Y; `LockButton` visible on right below aim.
  - Console warnings/errors: `0`.

## 2026-02-16 Validation Re-Check (Unity MCP)
- **Scene + Play Mode:** Active scene confirmed as `Assets/Scenes/PoolGame.unity`; `manage_editor` play/stop both succeeded.
- **Console:** `read_console` in play mode returned `0` warnings/errors.
- **UI Presence:** `GameCanvas=1`, `AimSlider=1`, `PowerSlider=1`, `LockButton=1`, `ShootButton=0`, `EventSystem=1`.
- **UI Layout (runtime RectTransform):**
  - `AimSlider`: `anchorMin=(0.86, 0.52)`, `anchoredPosition=(0, -20)`
  - `PowerSlider`: `anchorMin=(0.14, 0.52)`, `anchoredPosition=(0, -20)`
  - `LockButton`: `anchorMin=(0.86, 0.52)`, `anchoredPosition=(0, -220)`
  - Aim/power sliders remain level on Y and split to opposite sides on X.
- **Input Module:** EventSystem contains `UnityEngine.InputSystem.UI.InputSystemUIInputModule`.

## 2026-02-16 Cue Aiming UX Update (Aim Line + Mouse Wheel)
- **User request:** Show cueball path more clearly and support fine aiming with mouse wheel scroll.
- **Code updates:**
  - `CueAim.cs`
    - Aim line now starts at cueball surface (`startLineAtBallSurface=true`) with a small vertical lift (`aimLineHeightOffset`) for better visibility above felt.
    - Added mouse wheel fine control (`enableMouseWheelFineControl`, `mouseWheelDegreesPerStep`, `invertMouseWheel`).
    - Added `OnAimAngleChanged` event for cross-input UI synchronization.
  - `GameUI.cs`
    - Subscribed to `CueAim.OnAimAngleChanged` and now mirrors angle/value text when aim changes from wheel input.
- **Validation (Unity MCP):**
  - Play mode enter/exit successful.
  - Console warnings/errors: `0`.
  - Runtime checks: `cueball` and `cuestick` found.
  - `cueball` has `LineRenderer`.
  - `CueAim.enableMouseWheelFineControl=true` in runtime component data.

## 2026-02-16 UI Interaction + Aim Line Runtime Fix
- **Issue reported:** Aim/Power sliders were not draggable in-game, and aim line was not visible.
- **Root causes identified (runtime):**
  - EventSystem was bound only to one input path at runtime, causing slider interaction failure on some input backend setups.
  - Cueball `LineRenderer` existed but was disabled in play mode.
  - UI could start in a locked phase in runtime edge cases.
- **Fixes implemented:**
  - `GameUI.EnsureEventSystem()` now ensures both `StandaloneInputModule` and `InputSystemUIInputModule` are present and enables the active backend.
  - `GameUI.Start()` now force-unlocks cue aim and starts in `Aiming` phase.
  - `CueAim.SetupAimLine()` and `CueAim.UpdateAimLine()` now force-enable the line renderer.
- **Validation (Unity MCP):**
  - `AimSlider.interactable=true` at play start.
  - `PowerSlider.interactable=false` at play start (expected until lock phase).
  - Cueball line renderer: `enabled=true`.
  - EventSystem components include both modules:
    - `UnityEngine.EventSystems.StandaloneInputModule`
    - `UnityEngine.InputSystem.UI.InputSystemUIInputModule`
  - Console warnings/errors: `0`.

## 2026-02-16 UI Layout Tweak (Status Text)
- **User request:** Move top-center status text closer to the table.
- **Code update:** Added configurable `statusTextOffsetY` in `GameUI` and changed default from `-20` to `-115`.
- **Effect:** Status text remains top-centered but renders lower on screen for better table proximity.

## 2026-02-16 Red-Dot Drag Reliability Fix
- **Issue reported:** Could not hold/drag slider red dots reliably.
- **Root cause:** `EventTrigger` hooks on slider handles and power slider could intercept pointer/drag routing.
- **Code update (`GameUI.cs`):**
  - Replaced `EventTrigger` release handling with `SliderReleaseRelay` (`IPointerUpHandler`, `IEndDragHandler` only).
  - Replaced handle hover `EventTrigger` with `SliderHandleHoverFeedback` (`IPointerEnterHandler`, `IPointerExitHandler` only).
  - Set `PowerSlider` interactable in `UIPhase.Aiming` so both sliders are draggable at startup.
- **Validation (Unity MCP):**
  - `AimSlider.interactable=true`
  - `PowerSlider.interactable=true`
  - Console warnings/errors: `0`

## 2026-02-16 Physics Realism Tuning Pass
- **User feedback:** Ball physics did not feel realistic.
- **Key fixes:**
  - `CueStrike.cs`
    - Added `powerToImpulseScale` to convert UI/gameplay power to physical impulse (N*s) before applying `ForceMode.Impulse`.
    - Reduced default spin injection and scaled spin by physical impulse instead of raw power units.
  - `RailResponse.cs`
    - Removed non-physical spin reset/reinject behavior on cushion contact.
    - Added mild tangential velocity retention and spin correction based on actual rigidbody angular velocity:
      - preserve most top/back spin
      - invert+damp sidespin on rail hit.
  - `BallSpin.cs`
    - Moved realism values to serialized tuning fields:
      - `slidingFrictionCoefficient`
      - `spinDecayRate`
      - `sideSpinExtraDecayRate`
      - threshold fields
    - Synced tracked spin state with rigidbody angular velocity after decay/injection and after rail corrections.
  - `BallPhysics.cs`
    - Rolling resistance now damps horizontal velocity only.
    - Converted rolling resistance and thresholds to serialized tuning fields.
- **Intended result:** More realistic cue-ball launch speeds, more believable rail spin behavior, and less non-physical spin resets.

## 2026-02-16 Physics Calibration Update (Materials + Runtime Values)
- **User feedback:** Motion still felt unrealistic after first pass.
- **Runtime/asset calibration applied:**
  - `CueStrike`:
    - `spinMultiplier`: `18`
    - `powerToImpulseScale`: `0.13`
  - `BallSpin` defaults:
    - `slidingFrictionCoefficient`: `0.12`
    - `spinDecayRate`: `0.08`
    - `sideSpinExtraDecayRate`: `0.16`
    - `minAngularThreshold`: `0.008`
    - `minVelocityThreshold`: `0.0008`
  - `BallPhysics` defaults:
    - `rollingResistanceCoefficient`: `0.0075`
    - `minVelocityThreshold`: `0.0008`
  - `RailResponse` constants:
    - `TangentialVelocityRetention`: `0.97`
    - `AxialSpinRetention`: `0.94`
    - `SideSpinInversionRetention`: `0.62`
  - Physics materials:
    - `Ball.physicMaterial`: friction `0.03`, bounciness `0.92`
    - `Felt.physicMaterial`: friction `0.05`
    - `Rails.physicMaterial`: friction `0.06`, bounciness `0.86`, `bounceCombine=Multiply`
    - `PoolGame.unity` inline material instances (`Ball (Instance)`, `Felt (Instance)`) updated to match.
- **Expected effect:** Less exaggerated rebounds, smoother roll-out, reduced overpowered english, and more believable cue-ball speed for typical shots.

## 2026-02-16 Cue/Aim Visibility During Shot
- **User request:** Hide cue stick and aim line after the shot is taken.
- **Code update (`CueAim.cs`):**
  - Added `hideCueAndAimWhileBallsMoving` (default `true`).
  - Subscribed to `ShotPower.OnShotReleased` to hide cue mesh renderers + aim line immediately.
  - Subscribed to `BallSleepMonitor.OnAllBallsStopped` to restore cue mesh renderers + aim line for the next shot.
  - Removed forced per-frame aim-line re-enable when visuals are intentionally hidden.
- **Scene update (`PoolGame.unity`):**
  - Saved `CueAim.hideCueAndAimWhileBallsMoving = true` on the cue stick component.

## 2026-02-16 Mouse-Wheel Fine Aim Input Fix
- **Issue reported:** Mouse-wheel fine aiming did not respond in-game.
- **Root cause:** Scroll delta scaling/backend assumptions were too strict across different Unity input handling modes.
- **Code update (`CueAim.cs`):**
  - `ReadMouseWheelDelta()` now:
    - Reads Input System wheel delta when available.
    - Normalizes large ±120-style wheel reports to notch units.
    - Falls back to Legacy Input Manager delta when enabled.
    - Uses the larger non-zero delta from available backends.
  - Lowered deadzone threshold in `HandleMouseWheelFineControl()` from `0.0001` to `0.001` paired with normalized input values.
- **Expected result:** Fine aim wheel adjustments work consistently across Input System-only, Legacy-only, and mixed backend project setups.

## 2026-02-16 Balls-Moving Stuck Recovery Fix
- **Issue reported:** After a break, game could remain in `BallsMoving` and not return control.
- **Root cause:** Stop detection could miss settled states after chaotic collisions.
- **Code update (`BallSleepMonitor.cs`):**
  - Uses horizontal speed threshold (XZ) instead of full 3D speed for rest checks.
  - Treats `Rigidbody.isKinematic` and `Rigidbody.IsSleeping()` as stopped immediately.
  - Cleans stale tracking entries more defensively.
  - Keeps transition logic for firing `OnAllBallsStopped` when movement truly ends.
- **Code update (`TurnManager.cs`):**
  - Added watchdog recovery (`ballsMovingTimeoutSeconds`, default `20s`) while in `BallsMoving`.
  - If timeout expires and no balls are reported moving, force-completes turn end flow.
- **Expected result:** Turn reliably exits `BallsMoving` after break shots, restoring UI/cue/aim for next shot.

## 2026-02-16 Cue/Aim Hide Reliability Patch
- **Issue reported:** Cue stick + aim line sometimes remained visible after shot.
- **Code update:**
  - `CueAim.cs`: added public `SetShotVisualsHidden(bool)` to centralize visibility state.
  - `GameUI.cs`: explicitly calls `cueAim.SetShotVisualsHidden(true)` on shot fired and `cueAim.SetShotVisualsHidden(false)` on balls stopped.
- **Expected result:** Cue and aim line consistently hide during `BallsMoving` and consistently return on next aiming phase.
