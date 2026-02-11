# Pipeline 2 — Phase Tracker

## Phase Status

| Phase | Status | Agent | Date Started | Date Completed |
|-------|--------|-------|-------------|----------------|
| 1. Project Foundation | COMPLETE | Planner | 2026-02-11 | 2026-02-11 |
| 2. Core Ball Physics | PENDING | Agent 1 | — | — |
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
**Status:** PENDING — Waiting for activation

### Milestone 3 — BallPhysics.cs Base
- [ ] Rigidbody caching
- [ ] Rolling resistance simulation
- [ ] Manual slowdown logic
- [ ] FixedUpdate-only discipline
- [ ] Test: Straight roll decays realistically

### Milestone 4 — BallSpin.cs
- [ ] Store angular velocity
- [ ] Topspin/backspin/sidespin tracking
- [ ] Spin decay
- [ ] Spin → velocity transfer
- [ ] Test: Follow/draw/stop shots

### Milestone 5 — BallSleepMonitor.cs
- [ ] Velocity threshold detection
- [ ] OnBallStopped event
- [ ] OnAllBallsStopped event
- [ ] Test: No false positives

---

## Phase 3 — Table Interaction
**Status:** BLOCKED by Phase 2

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
**Status:** BLOCKED by Phase 2

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
