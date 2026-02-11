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
 │    ├── Physics/      BallPhysics, BallSleepMonitor
 │    ├── Spin/         BallSpin
 │    ├── Cue/          CueAim, ShotPower, CueStrike
 │    ├── Table/        RailResponse, PocketTrigger
 │    ├── GameState/    TurnManager
 │    └── Debug/        Test utilities
 ├── Prefabs/
 ├── Materials/
 ├── PhysicsMaterials/
 └── Scenes/
```

## Progress

- [ ] Phase 1 — Project Foundation
- [ ] Phase 2 — Core Ball Physics
- [ ] Phase 3 — Table Interaction
- [ ] Phase 4 — Cue & Input System
- [ ] Phase 5 — Gameplay Layer
- [ ] Phase 6 — Visual Realism
- [ ] Phase 7 — Validation Protocol
