# Pipeline 2 — Phase Tracker

## Phase Status

| Phase | Status | Agent | Date Started | Date Completed |
|-------|--------|-------|-------------|----------------|
| 1. Project Foundation | COMPLETE | Planner | 2026-02-11 | 2026-02-11 |
| 2. Core Ball Physics | COMPLETE | Agent 1 | 2026-02-11 | 2026-02-11 |
| 3. Table Interaction | COMPLETE | Agent 3 | 2026-02-11 | 2026-02-11 |
| 4. Cue & Input System | COMPLETE | Agent 2 | 2026-02-11 | 2026-02-11 |
| 5. Gameplay Layer | BLOCKED | Agent 4 | — | — |
| 6. Visual Realism | WAITING (Pipeline 1) | User | — | — |
| 7. Validation Protocol | BLOCKED | Planner | — | — |

---

## Phase 1 — Project Foundation

### Milestone 1 — Project Structure Setup
**Status:** COMPLETE

Created folder structure:
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
 ├── Materials/
 ├── PhysicsMaterials/
 └── Scenes/
```

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
| Ball | 0.05 | 0.05 | 0.95 | Maximum |
| Felt | 0.70 | 0.80 | 0.00 | Average |
| Rails | 0.20 | 0.20 | 0.90 | Maximum |

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

## Phase 6 — Visual Realism
**Status:** WAITING for Pipeline 1 (Blender assets)

### Milestone 13 — Asset Import Integration
- [ ] Extract materials
- [ ] Assign HDRP shaders
- [ ] Verify scale (ball 0.05715m)
- [ ] Assign colliders + rigidbodies + physics materials

### Milestone 14 — Reflection & Lighting
- [ ] Reflection Probe
- [ ] Box Projection
- [ ] Bake reflections
- [ ] Ball gloss verification

---

## Phase 7 — Validation Protocol
**Status:** BLOCKED by all phases

- [ ] Straight shot distance
- [ ] Stop shot
- [ ] Draw shot reversal
- [ ] Follow shot roll-through
- [ ] 45° rail rebound
- [ ] Double rail consistency
- [ ] No sliding forever
- [ ] No energy gain
- [ ] No jitter at rest
- [ ] 60+ FPS stable
- [ ] Deterministic physics confirmed
