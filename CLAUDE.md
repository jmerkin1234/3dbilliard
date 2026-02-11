# 3D Billiards Simulation — Claude Project Context

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
- Ball: diameter 0.05715m, mass 0.17kg, friction 0.05, bounce 0.95
- Felt: friction 0.6-0.8, bounce 0
- Rails: friction 0.2, bounce 0.9
- Solver iterations: 12-16, velocity iterations: 8-12
- Fixed timestep: 0.02, bounce threshold: 0.1

## Coding Rules
- ALL physics in FixedUpdate — never Update()
- SphereCollider only for balls — never MeshCollider
- Each script stays in its designated folder
- Agents must not cross domain boundaries
- Test with primitive shapes before real assets
- No per-frame allocations
- Deterministic physics required

## Script Locations
- Assets/Scripts/Physics/ — BallPhysics.cs, BallSleepMonitor.cs
- Assets/Scripts/Spin/ — BallSpin.cs
- Assets/Scripts/Cue/ — CueAim.cs, ShotPower.cs, CueStrike.cs
- Assets/Scripts/Table/ — RailResponse.cs, PocketTrigger.cs
- Assets/Scripts/GameState/ — TurnManager.cs
- Assets/Scripts/Debug/ — Test utilities

## Current Phase
Phase 1 — Project Foundation (not started)
