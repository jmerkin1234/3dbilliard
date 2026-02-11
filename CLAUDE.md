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
- Ball: diameter 0.05715m, mass 0.17kg, friction 0.05, bounce 0.95
- Felt: friction 0.6-0.8, bounce 0
- Rails: friction 0.2, bounce 0.9
- Solver iterations: 12-16, velocity iterations: 8-12
- Fixed timestep: 0.02, bounce threshold: 0.1

## Mandatory Workflow Rules
- ALWAYS read the relevant phase MD from `/home/justin/Desktop/project md's/` BEFORE starting any phase
- DOCUMENT EVERYTHING — update PHASE_TRACKER.md, CLAUDE.md, and README.md after every milestone
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
- Assets/Scripts/Physics/ — BallPhysics.cs, BallSleepMonitor.cs
- Assets/Scripts/Spin/ — BallSpin.cs
- Assets/Scripts/Cue/ — CueAim.cs, ShotPower.cs, CueStrike.cs
- Assets/Scripts/Table/ — RailResponse.cs, PocketTrigger.cs
- Assets/Scripts/GameState/ — TurnManager.cs
- Assets/Scripts/Debug/ — Test utilities

## Documentation
- docs/PHASE_TRACKER.md — Detailed milestone checklist with dates and values
- README.md — Project overview and roadmap
- CLAUDE.md — This file, agent context

## Current Phase
Phase 1 — Project Foundation (COMPLETE 2026-02-11)
Phase 2 — Core Ball Physics (COMPLETE 2026-02-11)
Phase 3 — Table Interaction (COMPLETE 2026-02-11)
Phase 4 — Cue & Input System (COMPLETE 2026-02-11)
Phase 5 — Gameplay Layer (COMPLETE 2026-02-11)
Phase 6 — Visual Realism (WAITING for Pipeline 1)
Phase 7 — Validation Protocol (NEXT)

## Scripts Created
| Script | Location | Status |
|--------|----------|--------|
| BallPhysics.cs | Assets/Scripts/Physics/ | Phase 2, compiled |
| BallSpin.cs | Assets/Scripts/Spin/ | Phase 2, compiled |
| BallSleepMonitor.cs | Assets/Scripts/Physics/ | Phase 2, compiled |
| DebugBallLauncher.cs | Assets/Scripts/Debug/ | Phase 2, compiled (test utility) |
| RailResponse.cs | Assets/Scripts/Table/ | Phase 3, compiled |
| PocketTrigger.cs | Assets/Scripts/Table/ | Phase 3, compiled |
| CueAim.cs | Assets/Scripts/Cue/ | Phase 4, compiled |
| ShotPower.cs | Assets/Scripts/Cue/ | Phase 4, compiled |
| CueStrike.cs | Assets/Scripts/Cue/ | Phase 4, compiled |
| TurnManager.cs | Assets/Scripts/GameState/ | Phase 5, compiled |
| RuleEngine.cs | Assets/Scripts/GameState/ | Phase 5, compiled |

## Test Scene
- BilliardTestScene.unity in Assets/Scenes/
- TestBall with all physics scripts attached
- Debug keys: Space (straight), B (draw), F (follow), S (stop), R (reset)

## Change Log
- 2026-02-11: Phase 1 complete. Folders created, physics configured (solver 14, velocity 10, bounce threshold 0.1, sleep 0.001), physics materials created (Ball/Felt/Rails)
- 2026-02-11: Phase 2 complete. BallPhysics.cs, BallSpin.cs, BallSleepMonitor.cs created and compiled. Test scene built with DebugBallLauncher. Implementation complete, Play Mode validation deferred to Phase 7.
- 2026-02-11: Phase 3 complete. RailResponse.cs, PocketTrigger.cs created. Reflection physics with restitution (0.9), energy loss (0.95), spin inversion. Pocket detection with trigger colliders, ball disabling, event firing.
- 2026-02-11: Phase 4 complete. CueAim.cs (mouse rotation with smooth damping), ShotPower.cs (hold-to-charge 0.5-8N with exponential curve), CueStrike.cs (impulse + spin injection via contact offset). Full shot pipeline functional. Fixed Debug namespace collision (replaced with UnityEngine.Debug). Zero compilation errors verified via Unity MCP.
- 2026-02-11: Phase 5 complete. TurnManager.cs (turn state machine: Aiming→BallsMoving→TurnEnding, player switching, foul handling, input control), RuleEngine.cs (shot tracking, legal contact, pocket validation, scratch detection). Zero compilation errors. Event-driven architecture integrates with BallSleepMonitor and PocketTrigger.
