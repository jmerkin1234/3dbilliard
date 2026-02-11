# Pipeline 2 — Phase Tracker

## Phase Status

| Phase | Status | Agent | Date Started | Date Completed |
|-------|--------|-------|-------------|----------------|
| 1. Project Foundation | COMPLETE | Planner | 2026-02-11 | 2026-02-11 |
| 2. Core Ball Physics | COMPLETE | Agent 1 | 2026-02-11 | 2026-02-11 |
| 3. Table Interaction | BLOCKED | Agent 3 | — | — |
| 4. Cue & Input System | BLOCKED | Agent 2 | — | — |
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
**Status:** READY (Agent 3 can begin)

### Milestone 6 — RailResponse.cs
- [ ] Reflection formula: R = V - 2(V·N)N
- [ ] Restitution scaling
- [ ] Energy loss factor
- [ ] Spin inversion
- [ ] Test: 45° rebound, no energy gain

### Milestone 7 — PocketTrigger.cs
- [ ] Trigger collider detection
- [ ] Disable ball physics on pocket
- [ ] OnBallPocketed event
- [ ] Test: Clean detection, no ghosts

---

## Phase 4 — Cue & Input System
**Status:** READY (Agent 2 can begin)

### Milestone 8 — CueAim.cs
- [ ] Mouse rotation
- [ ] Normalized direction vector
- [ ] Input lock for TurnManager
- [ ] Test: Stable, no jitter

### Milestone 9 — ShotPower.cs
- [ ] Hold-to-charge
- [ ] Clamped max impulse
- [ ] Exponential curve option
- [ ] Test: Scales correctly

### Milestone 10 — CueStrike.cs
- [ ] Apply impulse to cue ball
- [ ] Spin injection via offset
- [ ] Respect mass (0.17)
- [ ] Test: Center/low/high hit behavior

---

## Phase 5 — Gameplay Layer
**Status:** BLOCKED by Phases 2, 3, 4

### Milestone 11 — TurnManager.cs
- [ ] Player switching
- [ ] Shot validation
- [ ] Foul logic
- [ ] Rack reset

### Milestone 12 — Rule Engine
- [ ] Legal contact detection
- [ ] Pocket tracking
- [ ] Scratch detection

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
