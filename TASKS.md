# TASKS.md
# Implementation Task Tracker — Reflection (Working Title)

Agents pick tasks from this list. Mark tasks `[IN PROGRESS: AGENT_NAME]` when starting, `[DONE]` when complete. Do not start a task marked IN PROGRESS by another agent.

---

## Phase 0 — Project Setup

- [ ] Create Unity 2022.3 LTS project with URP template
- [ ] Set up folder structure per `AGENTS.md`
- [ ] Configure Physics2D layers per `CLAUDE.md` Layer Setup table
- [ ] Create Input Actions asset with bindings per `ARCHITECTURE.md`
- [ ] Set up Sorting Layers per `ARCHITECTURE.md`
- [ ] Create `PoolManager` singleton with pre-warm stubs
- [ ] Create `EventBus.cs` with full event schema from `CLAUDE.md`
- [ ] Create `Constants.cs` with beam/physics layer masks

---

## Phase 1 — Core Systems

### GameManager
- [ ] Implement `GamePhase` enum and state machine
- [ ] Wire EventBus transitions (OnAllAngelsCleared → OnDoorsOpen, OnUpgradeSelected → OnRoomReconfigure)
- [ ] Track active angel count
- [ ] Handle player death → GameOver

### ScriptableObjects
- [ ] Create `LevelLayoutSO` schema and sample assets (Map1, Map2, Map3)
- [ ] Create `UpgradeDefinitionSO` schema and starter upgrade assets (5 upgrades)
- [ ] Create `EnemyDataSO` schema and base enemy asset
- [ ] Create `PlayerStatsSO` with runtime reset

---

## Phase 2 — Room & Player

### Room
- [ ] Implement `RoomConfigurator` — listen to OnRoomReconfigure, clear + rebuild room
- [ ] Implement tilemap wall/floor placement from LevelLayoutSO
- [ ] Implement mirror placement from LevelLayoutSO
- [ ] Implement pillar placement from LevelLayoutSO
- [ ] Implement enemy spawn from LevelLayoutSO wave data

### Player
- [ ] Implement `PlayerController` — WASD movement, read from PlayerStatsSO.moveSpeed
- [ ] Implement `FlashlightController` — hold input to activate, aim toward mouse/stick
- [ ] Implement mirror placement mode (when placeableMirrorCount > 0)

---

## Phase 3 — Beam System

- [ ] Implement `BeamCaster` — primary raycast, bounce recursion (max 2)
- [ ] Implement `MirrorReflector` — reflection normal calculation
- [ ] Implement `AngelIlluminationRegistry` — HashSet populated by BeamCaster each frame
- [ ] Implement `BeamHitHandler` — stun/damage logic from OnBeamHit event
- [ ] Implement `BeamSegment` prefab + LineRenderer pool
- [ ] Wire Script Execution Order (BeamCaster → IlluminationRegistry → AngelController)

---

## Phase 4 — Angel AI

- [ ] Implement `AngelController` state machine (Idle/Pursuing/Stunned/Dead)
- [ ] Implement movement — toward player when not illuminated
- [ ] Implement illumination check — query AngelIlluminationRegistry
- [ ] Implement stun — receive from BeamHitHandler, timer, return to Pursuing
- [ ] Implement HP system — base damage and 2x damage handling
- [ ] Implement death — VFX trigger, pool return, fire OnAngelKilled
- [ ] Implement player contact — fire OnPlayerDeath

---

## Phase 5 — Doors & Upgrades

- [ ] Implement `DoorManager` — activate 3 doors on OnAllAngelsCleared
- [ ] Implement `UpgradeDoor` — display icon/label, trigger on player entry, fire OnUpgradeSelected
- [ ] Implement `UpgradeApplier` — apply all 5 upgrade effect types to PlayerStatsSO
- [ ] Implement no-duplicate logic for upgrade door offers per run
- [ ] Implement door close/hide on selection before room reconfigure

---

## Phase 6 — UI

- [ ] Implement HUD (minimal — angel count, current level number)
- [ ] Implement GameOver screen with restart button
- [ ] Implement upgrade door visual (icon + name label on door prefab)
- [ ] Implement run-start screen (title + start button)

---

## Phase 7 — Level Content

- [ ] Author Map1 LevelLayoutSO (tutorial-difficulty, few mirrors, 2 angels)
- [ ] Author Map2 LevelLayoutSO (intermediate, more mirrors, 3–4 angels)
- [ ] Author Map3 LevelLayoutSO (harder, tight layout, 4–6 angels)
- [ ] Author Map4 LevelLayoutSO (hard, complex mirror paths)
- [ ] Define level progression sequence in LevelManager (which layout per level index)

---

## Phase 8 — Polish

- [ ] Add VFX: Angel death dissolve/shatter
- [ ] Add VFX: Door open flash
- [ ] Add VFX: Room reconfigure transition flash
- [ ] Add placeholder audio hooks (AudioManager.Play("event_name") stubs)
- [ ] Add beam glow/pulse visual
- [ ] Add camera shake on player death
- [ ] Final performance pass — verify 60fps with max angels + beams

---

## Known Out-of-Scope (Do Not Implement)
- Exit puzzle / switch mechanic
- Save/load
- Sound asset implementation (stubs only)
- Multiple scenes
- Boss enemies
- Player HP
