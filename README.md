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
| Ball friction | 0.05 |
| Ball bounciness | 0.95 |
| Felt friction | 0.6-0.8 |
| Rail friction | 0.2 |
| Rail bounce | 0.9 |
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
- [ ] Phase 7 — Validation Protocol (Next: Full Play Mode testing)

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
