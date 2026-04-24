# AGENTS.md
# Coding Agent Master Instructions — Reflection (Working Title)

This file defines how all AI coding agents must behave when working on this project. Read this file fully before touching any code.

---
## Project Identity

- **Engine:** Unity 6 (URP)
- **Language:** C# (.NET Standard 2.1)
- **Genre:** Incremental Survival Action — Top-down 3D
- **Theme:** Reflection (mirrors, light, bouncing beams)
- **Target:** Game Jam build — scope is intentionally tight
- **Scene philosophy:** One physical room, reconfigured per level by loading handcrafted map layouts (object pooling)

---

## Repository Structure

```
Assets/
├── _Game/
│   ├── Scripts/
│   │   ├── Core/           # GameManager, LevelManager, SceneController
│   │   ├── Player/         # PlayerController, FlashlightController
│   │   ├── Enemies/        # AngelAI, AngelStateManager
│   │   ├── Light/          # BeamCaster, BeamSegment, MirrorReflector, BeamHitHandler
│   │   ├── Room/           # RoomConfigurator, TileLayoutLoader, PillarController
│   │   ├── Doors/          # UpgradeDoor, DoorManager
│   │   ├── Upgrades/       # UpgradeRegistry, UpgradeDefinition, UpgradeApplier
│   │   ├── UI/             # HUDController, UpgradeIconDisplay
│   │   └── Utilities/      # ObjectPool, EventBus, Constants
│   ├── ScriptableObjects/
│   │   ├── Upgrades/
│   │   ├── LevelLayouts/
│   │   └── EnemyData/
│   ├── Prefabs/
│   │   ├── Room/
│   │   ├── Enemies/
│   │   ├── Light/
│   │   └── UI/
│   ├── Scenes/
│   │   └── Main.unity      # Single scene — everything happens here
│   ├── Tilemaps/
│   └── Art/
├── Packages/
└── ProjectSettings/
```

---

## Agent Roles

### ARCH — Architect Agent
- Owns `Core/`, `Room/`, overall scene structure
- Makes structural decisions; other agents defer to ARCH on architecture conflicts
- Responsible for EventBus schema and ScriptableObject data contracts

### COMBAT — Combat Agent
- Owns `Player/`, `Enemies/`, `Light/`
- Implements beam casting, reflection logic, hit detection, Angel AI
- Must not modify Room or Door systems directly — fires events instead

### PROG — Progression Agent
- Owns `Doors/`, `Upgrades/`, `UI/`
- Implements upgrade door display, upgrade application, HUD
- Reads from UpgradeRegistry ScriptableObjects, never hardcodes upgrade values

### LEVEL — Level Design Agent
- Owns `ScriptableObjects/LevelLayouts/`, `Tilemaps/`
- Authors room layout data (tile positions, mirror placements, pillar placements, door spawn points)
- Does not write runtime C# — works in ScriptableObjects and Tilemap assets

---

## Universal Rules — All Agents Must Follow

### Code Style
- Use `PascalCase` for class names, `camelCase` for fields, `_camelCase` for private fields
- All `MonoBehaviour` classes must use `[RequireComponent]` where dependencies are known
- No magic numbers — use `Constants.cs` or ScriptableObject fields
- No `FindObjectOfType` at runtime — wire via Inspector or EventBus
- No `Update()` polling where an event or coroutine suffices
- Prefer `ScriptableObject`-driven data over hardcoded values in MonoBehaviours

### Architecture
- **EventBus pattern** for cross-system communication (see `CLAUDE.md` for event schema)
- **Object Pooling** for beams, beam segments, Angels — never `Instantiate`/`Destroy` in hot paths
- **Single scene** — no `SceneManager.LoadScene` calls mid-game; room reconfiguration is done by `RoomConfigurator`
- All upgrade values live in `UpgradeDefinition` ScriptableObjects — never in script bodies

### Unity Specifics
- Target **Unity 6** APIs only.
- Use **URP** shader graph or URP-compatible shaders only — no Built-in pipeline shaders
- Physics: **Standard 3D Physics** only — absolutely NO Physics2D components.
- Navigation: **NavMesh** — Use Unity's built-in `NavMeshAgent` for all enemy movement.
- Input: **Unity Input System** package.
- Environment: **3D Primitives/Meshes** for walls and floors.

### Prefab Rules
- Every enemy, beam segment, mirror, pillar, and door must be a prefab under `Prefabs/`
- Prefab variants are allowed for layout-specific overrides
- Never modify a prefab from within a scene — use the Prefab editor

### ScriptableObjects
- `LevelLayoutSO` defines: room size, tile layout reference, mirror positions/rotations, pillar positions, door spawn points, enemy wave data
- `UpgradeDefinitionSO` defines: upgrade ID, display name, icon, description, effect type enum, effect value
- `EnemyDataSO` defines: move speed, stun duration modifier, health

---

## What Agents Must NOT Do

- Do not add external packages without flagging it in a comment `// PACKAGE ADDED: reason`
- Do not create new scenes — everything runs in `Main.unity`
- Do not implement features outside the defined scope (see `DESIGN.md`)
- Do not use coroutines for game logic that should be event-driven
- Do not store game state in MonoBehaviours — state lives in `GameManager` or ScriptableObjects
- Do not write UI text as hardcoded strings — use a `LocalizationConstants.cs` or pass from SO

---

## Commit Message Format

```
[SYSTEM] Short description of change

SYSTEM = CORE | COMBAT | LIGHT | ENEMY | ROOM | DOOR | UPGRADE | UI | FIX | REFACTOR
```

Example: `[LIGHT] Implement 2-bounce damage multiplier on BeamHitHandler`

---

## When In Doubt

Read `CLAUDE.md` for detailed system-by-system implementation guidance.
Read `DESIGN.md` for authoritative game design decisions.
Ask before inventing — scope is tight and jam deadlines are real.
