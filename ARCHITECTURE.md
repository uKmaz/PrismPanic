# ARCHITECTURE.md
# System Architecture & Dependency Map — Reflection (Working Title)

---

## System Overview

```
┌─────────────────────────────────────────────────────────┐
│                        GameManager                       │
│              (owns phase, level index, run state)        │
└────────────┬────────────────────────────┬───────────────┘
             │ EventBus                   │ EventBus
             ▼                            ▼
┌─────────────────────┐      ┌────────────────────────────┐
│   RoomConfigurator  │      │        DoorManager          │
│  (tiles, mirrors,   │      │  (spawns 3 doors on clear)  │
│   pillars, spawns)  │      └────────────┬───────────────┘
└─────────────────────┘                   │ EventBus
                                          ▼
                               ┌─────────────────────┐
                               │    UpgradeApplier    │
                               │  (modifies PlayerStats│
                               │   SO at runtime)     │
                               └─────────────────────┘

┌──────────────────────────────────────────────────────────┐
│                     Player GameObject                     │
│  ┌─────────────────┐    ┌──────────────────────────────┐ │
│  │PlayerController │    │     FlashlightController     │ │
│  │(movement, input)│    │  ┌────────────────────────┐  │ │
│  └─────────────────┘    │  │      BeamCaster         │  │ │
│                         │  │  (raycast, reflect,     │  │ │
│                         │  │   populate illumination │  │ │
│                         │  │   registry)             │  │ │
│                         │  └────────────────────────┘  │ │
│                         └──────────────────────────────┘ │
└──────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────┐
│                   Angel GameObject (pooled)               │
│  ┌────────────────────────────────────────────────────┐  │
│  │                  AngelController                   │  │
│  │  Checks AngelIlluminationRegistry each frame       │  │
│  │  State: Idle → Pursuing → Stunned → Dead           │  │
│  └────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────┘
```

---

## EventBus Flow Diagram

```
BeamCaster ──────────── fires ──────────► EventBus.OnBeamHit
                                                │
                              ┌─────────────────┤
                              ▼                 ▼
                     BeamHitHandler      AngelController
                    (apply dmg/stun)    (receive stun/dmg)
                              │
                    AngelController.HP → 0
                              │
                         fires ──────────► EventBus.OnAngelKilled
                                                │
                                         GameManager
                                    (decrement angel count)
                                         activeCount == 0
                                                │
                                         fires ──► EventBus.OnAllAngelsCleared
                                                        │
                                              ┌─────────┤
                                              ▼         ▼
                                        DoorManager   (future hooks)
                                       (open 3 doors)
                                              │
                                        Player enters door
                                              │
                                         fires ──► EventBus.OnUpgradeSelected(SO)
                                                        │
                                              ┌─────────┤
                                              ▼         ▼
                                      UpgradeApplier  GameManager
                                    (modify PlayerStats) (advance level)
                                                        │
                                                   fires ──► EventBus.OnRoomReconfigure(SO)
                                                                  │
                                                          RoomConfigurator
                                                         (rebuild room layout)
                                                                  │
                                                         fires ──► EventBus.OnRoomReconfigureComplete
                                                                        │
                                                                  GameManager
                                                               (enter CombatPhase)
```

---

## ScriptableObject Dependency Map

```
LevelLayoutSO
    ├── references: TileBase[] (tilemap tiles)
    ├── contains:  MirrorPlacement[] (position, rotation)
    ├── contains:  Vector2[] pillarPositions
    ├── contains:  Vector2[] enemySpawnPoints
    ├── contains:  Vector2[] doorSpawnPoints
    └── references: EnemyWaveData[]
                        └── references: EnemyDataSO
                                            ├── moveSpeed
                                            ├── maxHP
                                            └── stunDurationModifier

UpgradeDefinitionSO
    ├── upgradeID (string)
    ├── displayName
    ├── icon (Sprite)
    ├── description
    ├── effectType (enum)
    └── effectValue (float)

PlayerStatsSO  [Runtime — NonSerialized fields, reset each run]
    ├── stunDuration
    ├── moveSpeed
    ├── flashlightConeAngle
    └── placeableMirrorCount
```

---

## Object Pool Map

| Pool | Managed By | Initial Size |
|------|-----------|--------------|
| AngelController | PoolManager | 8 |
| BeamSegment | PoolManager | 16 |
| MirrorController | PoolManager | 12 |
| PillarController | PoolManager | 8 |
| UpgradeDoor | PoolManager | 3 |

All pools pre-warm on scene load. `PoolManager` is a singleton MonoBehaviour on a persistent GameObject.

---

## Scene Hierarchy (Main.unity)

```
Main.unity
├── [PERSISTENT]
│   ├── GameManager
│   ├── PoolManager
│   ├── EventBus (static — no GameObject needed)
│   └── AudioManager (placeholder)
├── [ROOM]
│   ├── Tilemap_Floor
│   ├── Tilemap_Walls
│   ├── Mirrors_Container     ← pooled mirrors parented here
│   ├── Pillars_Container     ← pooled pillars parented here
│   └── Enemies_Container     ← pooled angels parented here
├── [PLAYER]
│   ├── PlayerController
│   └── FlashlightController
│       └── BeamCaster
├── [DOORS]
│   └── Doors_Container       ← pooled doors parented here
├── [UI]
│   ├── HUD_Canvas
│   └── GameOver_Canvas
└── [CAMERA]
    └── Main Camera
```

---

## Input Map (Unity Input System)

| Action | Binding (KB/M) | Binding (Gamepad) |
|--------|----------------|-------------------|
| Move | WASD | Left Stick |
| Aim | Mouse Position | Right Stick |
| Flashlight | Left Mouse Hold | Right Trigger Hold |
| Place Mirror | Right Mouse Click | Right Shoulder |
| Confirm Placement | Left Mouse Click | A Button |

Input asset path: `Assets/_Game/Input/ReflectionInputActions.inputactions`

---

## Rendering Pipeline Notes

- **Pipeline:** URP (Universal Render Pipeline)
- **Camera:** 2D orthographic, fixed zoom per layout (set in LevelLayoutSO or CameraController)
- **Sorting Layers (top to bottom):**
  - UI
  - Player
  - Enemy
  - Beam
  - Interactable (mirrors, pillars, doors)
  - Floor
- **Light2D:** Use URP 2D Light for flashlight cone visual — separate from physics beam logic
- **Beam rendering:** LineRenderer on "Beam" sorting layer, no URP light attached — visual only

---

## Performance Budget (Jam Target)

| System | Budget |
|--------|--------|
| Max active Angels | 8 |
| Max beam segments per frame | 3 (primary + 2 bounces) |
| Max mirrors in room | 8 |
| Max pillars in room | 6 |
| Target framerate | 60 FPS on mid-range hardware |

Raycasts per frame: max ~24 (3 beams × 8 potential hits — early-out on wall hit)
