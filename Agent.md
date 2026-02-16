# 3D Billiards Simulation — Claude Project Context

## BEFORE YOU DO ANYTHING
1. Read THIS file completely
2. Read `docs/PHASE_TRACKER.md` to know current state
3. Read the relevant phase MD from the list below BEFORE writing any code
4. Check what phase we're on, what's done, what's next
5. Never assume — always verify current project state first

## Project Type
Unity 6 (6000.3.6f1) billiards simulation using HDRP.

## Architecture
Two parallel pipelines:
- Pipeline 1 (User): Blender asset creation — models, UVs, PBR materials, export
- Pipeline 2 (Claude): Unity physics, gameplay, validation

## Pipeline 2 Agent Structure
- **Planner** — Orchestrates 7 phases, 14 milestones, gates agent execution
- **Agent 1** (Ball Physics) — BallPhysics.cs, BallSpin.cs, BallSleepMonitor.cs
- **Agent 2** (Cue & Input) — CueAim.cs, ShotPower.cs, CueStrike.cs
- **Agent 3** (Table Interaction) — RailResponse.cs, PocketTrigger.cs
- **Agent 4** (Game State) — TurnManager.cs, Rule Engine

## Execution Order
Phase 1 (foundation) → Phase 2 (Agent 1) → Phase 3+4 (Agents 2+3 parallel) → Phase 5 (Agent 4) → Phase 6 (asset swap) → Phase 7 (validation)

## Key Physics Values
- Ball: diameter 0.05715m, mass 0.17kg, friction 0.03, bounce 0.92
- Felt: friction 0.05, bounce 0
- Rails: friction 0.06, bounce 0.86
- Solver iterations: 12-16, velocity iterations: 8-12
- Fixed timestep: 0.02, bounce threshold: 0.1

## Mandatory Workflow Rules
- ALWAYS read the relevant phase MD from `/home/justin/Desktop/project md's/` BEFORE starting any phase
- DOCUMENT EVERYTHING — update PHASE_TRACKER.md, Agent.md, and README.md after every milestone
- Commit after each phase/milestone completion
- Never skip validation steps defined in the phase doc

## Phase Documents
- `/home/justin/Desktop/project md's/phase 1 Project_Foundation.md`
- `/home/justin/Desktop/project md's/Phase 2 — Core Physics system.md`
- `/home/justin/Desktop/project md's/Phase 3 — Table Interaction.md`
- `/home/justin/Desktop/project md's/Phase 4 — Cue & Input System.md`
- `/home/justin/Desktop/project md's/pahase 5_Gameplay_Layer.md`
- `/home/justin/Desktop/project md's/phase 06_Asset_Integration.md`
- `/home/justin/Desktop/project md's/phase 07_Validation_Protocol.md`

## Coding Rules
- ALL physics in FixedUpdate — never Update()
- SphereCollider only for balls — never MeshCollider
- Each script stays in its designated folder
- Agents must not cross domain boundaries
- Test with primitive shapes before real assets
- No per-frame allocations
- Deterministic physics required

## Script Locations
- Assets/Scripts/Physics/ — BallPhysics.cs, BallSleepMonitor.cs, CueBallCollision.cs
- Assets/Scripts/Spin/ — BallSpin.cs
- Assets/Scripts/Cue/ — CueAim.cs, ShotPower.cs, CueStrike.cs
- Assets/Scripts/Table/ — RailResponse.cs, PocketTrigger.cs
- Assets/Scripts/GameState/ — TurnManager.cs, RuleEngine.cs
- Assets/Scripts/Debug/ — Test utilities
- Assets/Scripts/UI/ — GameUI.cs

## Documentation
- docs/PHASE_TRACKER.md — Detailed milestone checklist with dates and values
- README.md — Project overview and roadmap
- Agent.md — This file, agent context

## Current Phase
Phase 1 — Project Foundation (COMPLETE 2026-02-11)
Phase 2 — Core Ball Physics (COMPLETE 2026-02-11, REFINED 2026-02-15)
Phase 3 — Table Interaction (COMPLETE 2026-02-11, REFINED 2026-02-15)
Phase 4 — Cue & Input System (COMPLETE 2026-02-11, REFINED 2026-02-15)
Phase 5 — Gameplay Layer (COMPLETE 2026-02-11, REFINED 2026-02-15)
Phase 6 — Asset Integration (COMPLETE 2026-02-15 - Fresh Scene)
Phase 7 — Validation Protocol (IN PROGRESS 2026-02-16)

## Scripts Created
| Script | Location | Status |
|--------|----------|--------|
| BallPhysics.cs | Assets/Scripts/Physics/ | Phase 2, refined |
| BallSpin.cs | Assets/Scripts/Spin/ | Phase 2, friction consolidated |
| BallSleepMonitor.cs | Assets/Scripts/Physics/ | Phase 2, compiled |
| CueBallCollision.cs | Assets/Scripts/Physics/ | Phase 5, rule detection |
| DebugBallLauncher.cs | Assets/Scripts/Debug/ | Phase 2, compiled (test utility) |
| RailResponse.cs | Assets/Scripts/Table/ | Phase 3, refined |
| PocketTrigger.cs | Assets/Scripts/Table/ | Phase 3, race condition fix |
| CueAim.cs | Assets/Scripts/Cue/ | Phase 4, perf optimization |
| ShotPower.cs | Assets/Scripts/Cue/ | Phase 4, compiled |
| CueStrike.cs | Assets/Scripts/Cue/ | Phase 4, compiled |
| TurnManager.cs | Assets/Scripts/GameState/ | Phase 5, integrated |
| RuleEngine.cs | Assets/Scripts/GameState/ | Phase 5, integrated |
| GameUI.cs | Assets/Scripts/UI/ | Phase 4, compiled |
| AssignPoolTableMaterials.cs | Assets/Scripts/Debug/ | Phase 6, material assignment tool |
| WirePoolGameReferences.cs | Assets/Scripts/Debug/ | Phase 6, reference wiring tool |

## Scenes
- **PoolGame.unity** (PRODUCTION) — Fresh scene with full PoolTableFixed physics configuration
- **BilliardTestScene.unity** (DEPRECATED) — Original test scene, use PoolGame instead

## Change Log
- 2026-02-11: Phase 1 complete. Folders created, physics configured (solver 14, velocity 10, bounce threshold 0.1, sleep 0.001), physics materials created (Ball/Felt/Rails)
- 2026-02-11: Phase 2 complete. BallPhysics.cs, BallSpin.cs, BallSleepMonitor.cs created and compiled. Test scene built with DebugBallLauncher. Implementation complete, Play Mode validation deferred to Phase 7.
- 2026-02-11: Phase 3 complete. RailResponse.cs, PocketTrigger.cs created. Reflection physics with restitution (0.9), energy loss (0.95), spin inversion. Pocket detection with trigger colliders, ball disabling, event firing.
- 2026-02-11: Phase 4 complete. CueAim.cs (mouse rotation with smooth damping), ShotPower.cs (hold-to-charge 0.5-8N with exponential curve), CueStrike.cs (impulse + spin injection via contact offset). Full shot pipeline functional. Fixed Debug namespace collision (replaced with UnityEngine.Debug). Zero compilation errors verified via Unity MCP.
- 2026-02-11: Phase 5 complete. TurnManager.cs (turn state machine: Aiming→BallsMoving→TurnEnding, player switching, foul handling, input control), RuleEngine.cs (shot tracking, legal contact, pocket validation, scratch detection). Zero compilation errors. Event-driven architecture integrates with BallSleepMonitor and PocketTrigger.
- 2026-02-11: TurnManager refactored for single-player. Removed 2-player logic. Added GameMode enum (Training, VsComputer). Training mode = unlimited shots, VsComputer = player vs AI with turn switching on fouls. AI logic placeholder for future implementation.
- 2026-02-15: Backend script refinement and logic audit.
  - Consolidated ball friction and spin-to-velocity transfer in `BallSpin.cs` using force-based physics.
  - Fixed `PocketTrigger.cs` race condition for simultaneous pocketing using coroutines.
  - Integrated `RuleEngine` with `TurnManager` and added `CueBallCollision.cs` for foul detection.
  - Optimized `CueAim.cs` by caching renderers and only updating visibility on state change.
  - Refined `RailResponse.cs` to rely on Unity physics for base reflection, preventing double-bouncing.
- 2026-02-15: Phase 6 complete. PoolTableFixed.fbx imported with full physics configuration.
  - Model import: PoolTableFixed.fbx (15MB, 34 child objects) with corrected mesh origins from Blender.
  - Physics components: 16 balls (Rigidbody, SphereCollider, BallPhysics, BallSpin), felt (BoxCollider), 6 rails (BoxCollider + RailResponse), 6 pockets (SphereCollider trigger + PocketTrigger), cue stick (CueAim, ShotPower, CueStrike).
  - Physics materials: Ball.physicMaterial assigned to all 16 ball colliders, Felt.physicMaterial to felt, Rails.physicMaterial to all 6 rails.
  - Component references: Wired CueAim.cueBall/aimLineRenderer, CueStrike.cueBall, TurnManager.cueAim/shotPower via Unity MCP batch operations.
  - Ball positioning: All balls at Y=0.8 (clears felt surface), proper triangle rack with 57.15mm spacing, cueball at X=-0.62028, cue stick behind at X=-1.0.
  - Tools created: AssignPoolTableMaterials.cs (auto-assign HDRP materials), VerifyPoolTableSetup.cs (validate physics setup).
  - Scene ready: BilliardGameScene fully configured and tested, all components hooked up, ready for Play Mode validation.
- 2026-02-15: Phase 6 complete - Fresh PoolGame.unity scene created.
  - **Issue**: BilliardGameScene had position corruption and ball bouncing issues.
  - **Solution**: Created fresh PoolGame.unity scene, preserved original FBX import positions.
  - **Physics configuration via Unity MCP**: 10 batch operations, ~140 commands total:
    - Batch 1-7: 110 components (cueball 7, balls 1-15 × 5, felt 1, rails 6 × 2, pockets 6 × 2, cuestick 3)
    - Batch 8-9: 23 physics material assignments (16 balls + 1 felt + 6 rails)
    - Batch 10: 5 component references wired via instanceID format
  - **Original positions preserved**: Cueball X=-0.561705, Cuestick X=-0.620280, Balls Y=0.79485, Felt Y=0.752951
  - **Zero bouncing**: 5mm clearance between ball bottoms and felt surface confirmed
  - **Tools**: WirePoolGameReferences.cs created as backup reference wiring utility
  - **Scene saved**: PoolGame.unity ready for Phase 7 Play Mode validation
  - **Deprecated**: BilliardGameScene.unity (use PoolGame.unity going forward)
- 2026-02-15: **CRITICAL FIX - Ball bouncing issue ACTUALLY resolved (third attempt)**
  - **First Attempt FAILED**: Set balls to Y=0.781526 - balls STILL bounced
  - **Root Cause Discovery**: Was using felt Transform Y (0.752951) instead of felt COLLIDER top surface
  - **Collider Analysis**:
    - Felt Transform Y: 0.752951
    - Felt BoxCollider center offset Y: 0.0021005
    - Felt BoxCollider size Y: 0.032456
    - **Felt collider TOP surface**: 0.752951 + 0.0021005 + (0.032456/2) = 0.771279
    - Ball radius: 0.028575m
    - Ball SphereCollider contactOffset: 0.01m
  - **WRONG Formula** (attempt 1): felt transform Y + radius = 0.752951 + 0.028575 = 0.781526 → FAILED
  - **WRONG Formula** (attempt 2): felt collider top + radius = 0.771279 + 0.028575 = 0.799854 → FAILED (ignored contactOffset)
  - **WRONG Formula** (attempt 3): 0.771279 + 0.028575 + 0.01 = 0.809854 + bounceCombine=1 → STILL FAILED (old material instances)
  - **FINAL SOLUTION (2026-02-15 evening)**:
    - Changed Ball.physicMaterial bounceCombine from 3 (Maximum) to 1 (Minimum)
    - Saved and reloaded scene to force Unity to recreate physics material instances from updated asset
    - Raised all 16 balls to Y=0.811854 (added 2mm clearance to prevent floating-point precision issues)
    - Formula: 0.771279 (felt top) + 0.028575 (radius) + 0.01 (contactOffset) + 0.002 (clearance) = 0.811854
  - **Cuestick**: Rotation set to (0,0,0) - default orientation aims correctly at rack
  - **Why it finally worked**:
    - bounceCombine=1 (Minimum) means ball (0.95) + felt (0) = 0 bounce (no bouncing)
    - Scene reload applied updated physics material to runtime instances
    - 2mm clearance prevents penetration from floating-point errors
  - **Lessons**:
    - Always use COLLIDER bounds, not Transform position, for physics calculations
    - Account for SphereCollider contactOffset when positioning
    - Modifying physics material assets requires scene reload to update runtime instances
    - Add small clearance (1-2mm) to prevent floating-point precision issues
- 2026-02-16: **Phase 7 validation fix — cue stick Play Mode alignment**
  - **Issue**: Cue stick was correctly placed in edit mode but offset/rotated incorrectly on Play Mode start.
  - **Root Cause**: `CueAim` assumed the cue mesh forward axis was +Z (`transform.forward`), but imported cue mesh points along local +X.
  - **Code Fix** (`Assets/Scripts/Cue/CueAim.cs`):
    - Added `cueForwardAxis` enum (Right/Forward/Left/Back) and axis-aware rotation alignment.
    - Added world-space ball radius support for cue distance math.
    - Added startup pivot calibration support (`cuePivotToTipOffset`, `autoCalibratePivotOffset`).
  - **Scene Update** (`Assets/Scenes/PoolGame.unity`):
    - Saved `CueAim.cueForwardAxis = Right` and `CueAim.autoCalibratePivotOffset = true`.
  - **Validation (Unity MCP)**:
    - Cue ball in Play Mode: `(-0.561705, 0.811854, 0.0)`
    - Cue stick in Play Mode: `(-0.620280, 0.811854, ~0.0)`, rotation `(0,0,0)`
    - Console warnings/errors: `0`
- 2026-02-16: **Phase 7 validation update — aim UI + shot UI integrated**
  - **Issue**: `GameUI.cs` existed but was not attached in `PoolGame.unity`, so no runtime UI for aiming/power/shoot.
  - **Scene Fix** (`Assets/Scenes/PoolGame.unity`):
    - Added `Billiards.UI.GameUI` to `GameManager`.
  - **Code Fix** (`Assets/Scripts/UI/GameUI.cs`):
    - Added `SyncUIWithCueState()` and invoked it at startup.
    - Aim slider now initializes from current `CueAim.AimAngle` to prevent first input from snapping aim.
    - Added warning logs if `CueAim` or `ShotPower` references are missing at runtime.
  - **Validation (Unity MCP)**:
    - Play Mode objects found: `GameCanvas`, `AimSlider`, `PowerSlider`
    - Console warnings/errors: `0`
- 2026-02-16: **Phase 7 UI layout refinement — increased vertical gap**
  - **Issue**: Aim and shot control clusters needed separation and were not behaving consistently.
  - **Fix** (`Assets/Scripts/UI/GameUI.cs`):
    - Kept sliders vertical and moved clusters to opposite sides.
      - Left side: power (release to shoot)
      - Right side: aim + lock
    - Added explicit side anchors (`shotClusterAnchorX/Y`, `aimClusterAnchorX/Y`).
    - Removed explicit shoot button; releasing the power slider now triggers shot at selected power percentage.
    - Added `EndDrag` trigger path in `GameUI` so release-to-shoot still fires when pointer leaves the slider before release.
    - Added `ShotPower.TryShoot()` and updated `GameUI.OnPowerReleased(...)` to switch to balls-moving only when a shot actually emits (avoids no-shot lock when released at near-zero power).
    - Improved EventSystem setup to prefer Input System UI module with fallback to `StandaloneInputModule`.
    - Added shared mid-screen Y-band constraint so both sliders and `LockButton` remain visible.
    - Pinned aim cluster Y to shot cluster Y so both vertical sliders align at the same height.
  - **Validation (Unity MCP)**:
    - `AimSlider` and `PowerSlider` present in Play Mode.
    - Runtime EventSystem uses `InputSystemUIInputModule`.
    - Runtime positions verified: `PowerSlider` left and `AimSlider` right at same Y; `LockButton` visible below aim.
    - Console warnings/errors: `0`.
- 2026-02-16: **Phase 7 Unity MCP re-validation (runtime pass)**
  - **Scene/Mode check**:
    - Active scene confirmed: `Assets/Scenes/PoolGame.unity`
    - Play mode enter/exit succeeded via `manage_editor` (`play`/`stop`)
  - **Console check**:
    - `read_console` errors/warnings in play mode: `0`
  - **Runtime UI object checks**:
    - `GameCanvas=1`, `AimSlider=1`, `PowerSlider=1`, `LockButton=1`, `ShootButton=0`, `EventSystem=1`
  - **Runtime RectTransform checks**:
    - `AimSlider`: `anchorMin=(0.86, 0.52)`, `anchoredPosition=(0, -20)`
    - `PowerSlider`: `anchorMin=(0.14, 0.52)`, `anchoredPosition=(0, -20)`
    - `LockButton`: `anchorMin=(0.86, 0.52)`, `anchoredPosition=(0, -220)`
    - Confirmed aim/power sliders are on opposite X sides and share Y anchor/offset.
  - **Input module check**:
    - EventSystem component list includes `UnityEngine.InputSystem.UI.InputSystemUIInputModule`
- 2026-02-16: **Cue aiming UX update — visible cueball path + wheel fine control**
  - **User requirement**:
    - Show cueball path line clearly from the cueball.
    - Enable fine aiming control with mouse wheel scrolling.
  - **Code fix** (`Assets/Scripts/Cue/CueAim.cs`):
    - Added mouse wheel fine aim controls:
      - `enableMouseWheelFineControl`
      - `mouseWheelDegreesPerStep`
      - `invertMouseWheel`
    - Added input-system-safe scroll read path (`UnityEngine.InputSystem.Mouse` when available).
    - Aim line now starts at cueball surface (`startLineAtBallSurface`) and uses slight height offset (`aimLineHeightOffset`) for visibility on felt.
    - Added `OnAimAngleChanged` event for UI synchronization.
  - **UI sync** (`Assets/Scripts/UI/GameUI.cs`):
    - Subscribed to `CueAim.OnAimAngleChanged`.
    - Aim slider/value now update when angle changes from wheel input (without slider drag).
  - **Validation (Unity MCP)**:
    - Play mode enter/exit: success.
    - Console warnings/errors: `0`.
    - Runtime object checks: `cueball=1`, `cuestick=1`.
    - Runtime component checks: `cueball` has `LineRenderer`, `CueAim.enableMouseWheelFineControl=true`.
- 2026-02-16: **UI drag + aim-line visibility runtime fix**
  - **User-reported issue**:
    - Could not click/drag aim or power UI.
    - Aim line not visible in game.
  - **Runtime findings (Unity MCP)**:
    - `AimSlider.interactable=false`, `PowerSlider.interactable=true` on one play pass (phase mismatch).
    - Cueball `LineRenderer` existed but `enabled=false`.
    - EventSystem had only one effective input path enabled.
  - **Code fixes**:
    - `Assets/Scripts/UI/GameUI.cs`
      - `EnsureEventSystem()` now ensures both `StandaloneInputModule` and `InputSystemUIInputModule` exist and enables the active backend.
      - `Start()` now force-unlocks cue aim and sets `UIPhase.Aiming` at startup.
    - `Assets/Scripts/Cue/CueAim.cs`
      - `SetupAimLine()` and `UpdateAimLine()` now force-enable `aimLineRenderer`.
  - **Validation after fix (Unity MCP)**:
    - `AimSlider.interactable=true`, `PowerSlider.interactable=false` at play start (expected flow).
    - Cueball line renderer `enabled=true`.
    - EventSystem includes both modules:
      - `UnityEngine.EventSystems.StandaloneInputModule`
      - `UnityEngine.InputSystem.UI.InputSystemUIInputModule`
    - Console warnings/errors: `0`.
- 2026-02-16: **UI status text position adjustment**
  - **User request**: Move the top-center status text closer to the table.
  - **Code fix** (`Assets/Scripts/UI/GameUI.cs`):
    - Added `statusTextOffsetY` (serialized layout setting).
    - Updated status text anchored position to use `statusTextOffsetY` with new default `-115` (was `-20`).
- 2026-02-16: **Red-dot drag fix (Aim/Power sliders)**
  - **Issue**: Could not hold and drag the red dot handles reliably.
  - **Root cause**: `EventTrigger` components on handle/release paths could intercept pointer/drag event routing.
  - **Code fix** (`Assets/Scripts/UI/GameUI.cs`):
    - Replaced power release trigger with `SliderReleaseRelay` (`IPointerUpHandler` + `IEndDragHandler` only).
    - Replaced handle hover trigger with `SliderHandleHoverFeedback` (`IPointerEnterHandler` + `IPointerExitHandler` only).
    - Enabled `PowerSlider` interaction in `UIPhase.Aiming` so both sliders can be dragged at startup.
  - **Validation (Unity MCP)**:
    - `AimSlider.interactable=true`
    - `PowerSlider.interactable=true`
    - Console warnings/errors: `0`
- 2026-02-16: **Physics realism tuning pass**
  - **User concern**: Ball motion/interaction felt non-realistic.
  - **Code updates**:
    - `Assets/Scripts/Cue/CueStrike.cs`
      - Added `powerToImpulseScale` to convert gameplay power to physical impulse (N*s) before cue-ball strike.
      - Reduced default spin injection magnitude from the old scene value (`spinMultiplier` from `50` down to realistic range).
      - Spin scaling now uses physical impulse magnitude.
    - `Assets/Scripts/Table/RailResponse.cs`
      - Removed spin clear/reinject pattern on rail hit.
      - Rail response now applies mild tangential velocity retention + physically plausible spin correction using rigidbody angular velocity.
    - `Assets/Scripts/Spin/BallSpin.cs`
      - Converted realism constants to serialized tuning fields.
      - Added `SyncSpinStateFromRigidbody()` and kept tracked spin synchronized with actual rigidbody angular velocity.
    - `Assets/Scripts/Physics/BallPhysics.cs`
      - Rolling resistance now operates on horizontal velocity only.
      - Rolling resistance and velocity thresholds made serialized/tunable.
  - **Intent**:
    - Avoid unrealistically high cue-ball launch speeds.
    - Improve cushion/english behavior.
    - Reduce non-physical spin discontinuities during rail collisions.
- 2026-02-16: **Physics calibration update (pass 2)**
  - **User concern**: Feel still not realistic enough after initial pass.
  - **Final tuning values applied**:
    - `Assets/Scripts/Cue/CueStrike.cs`: `spinMultiplier=18`, `powerToImpulseScale=0.13`.
    - `Assets/Scripts/Spin/BallSpin.cs`: `slidingFrictionCoefficient=0.12`, `spinDecayRate=0.08`, `sideSpinExtraDecayRate=0.16`, thresholds tightened.
    - `Assets/Scripts/Physics/BallPhysics.cs`: `rollingResistanceCoefficient=0.0075`, `minVelocityThreshold=0.0008`.
    - `Assets/Scripts/Table/RailResponse.cs`: stronger cushion realism damping (`TangentialVelocityRetention=0.97`, `AxialSpinRetention=0.94`, `SideSpinInversionRetention=0.62`).
    - `Assets/PhysicsMaterials 1/*.physicMaterial`: calibrated to ball/felt/rail values (`0.03/0.92`, `0.05/0`, `0.06/0.86`) and rails set to `bounceCombine=Multiply`.
    - `Assets/Scenes/PoolGame.unity`: updated `CueStrike` serialized values and inline ball/felt material instances to match calibration.
  - **Intended effect**: Reduce superball-like rebounds and over-spin while keeping shots responsive and controllable.
- 2026-02-16: **Cue/aim visibility behavior fix**
  - **User request**: Cue stick and aim line should not stay visible after shooting.
  - **Code fix** (`Assets/Scripts/Cue/CueAim.cs`):
    - Added `hideCueAndAimWhileBallsMoving` (default `true`).
    - On `ShotPower.OnShotReleased`, hide cue renderers + aim line.
    - On `BallSleepMonitor.OnAllBallsStopped`, restore cue renderers + aim line.
    - Removed forced aim-line re-enable while shot-hide state is active.
  - **Scene update** (`Assets/Scenes/PoolGame.unity`):
    - Saved `hideCueAndAimWhileBallsMoving = true` on cue `CueAim` component.
- 2026-02-16: **Fine aim mouse-wheel input fix**
  - **Issue**: Mouse-wheel fine aim control appeared non-responsive.
  - **Code fix** (`Assets/Scripts/Cue/CueAim.cs`):
    - Updated wheel delta reading to support both Input System and Legacy Input Manager backends.
    - Added normalization for ±120-style wheel events.
    - Uses the larger non-zero delta if both backends are enabled.
  - **Intent**: Make wheel-based fine aiming consistent regardless of project input backend settings.
- 2026-02-16: **BallsMoving stuck-state fix**
  - **Issue**: After break shots, game could remain in `BallsMoving` and hide cue/aim indefinitely.
  - **Code fix**:
    - `Assets/Scripts/Physics/BallSleepMonitor.cs`
      - Switched rest detection to horizontal velocity + angular thresholds.
      - Treats `Rigidbody.IsSleeping()` / `isKinematic` as stopped.
      - Hardened stale-entry cleanup.
    - `Assets/Scripts/GameState/TurnManager.cs`
      - Added `ballsMovingTimeoutSeconds` watchdog (default `20s`) and recovery path.
      - If timeout expires and monitor reports no moving balls, force-completes turn end.
  - **Intent**: Prevent turn/UI lockups after high-energy rack breaks.
- 2026-02-16: **Cue/aim hide reliability patch**
  - **Issue**: Cue stick and aim line did not always hide after shot.
  - **Code fix**:
    - `Assets/Scripts/Cue/CueAim.cs`: added `SetShotVisualsHidden(bool)` API.
    - `Assets/Scripts/UI/GameUI.cs`: now explicitly hides on `OnShotFired` and restores on `OnBallsStopped`.
  - **Intent**: Enforce cue/aim visibility from the same UI state machine that controls shot phases.
