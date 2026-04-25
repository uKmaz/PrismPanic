# CLAUDE.md
# Claude Agent Implementation Guide — Reflection (Working Title)

This is the primary technical reference for Claude when acting as a coding agent on this project. Read fully before implementing any system. Cross-reference `AGENTS.md` for role boundaries and `DESIGN.md` for design authority.

---

## 1. EventBus Schema

All cross-system communication uses a static `EventBus`. Never call methods across system boundaries directly.

```csharp
// Core/EventBus.cs
public static class EventBus
{
    // --- Combat ---
    public static event Action<AngelController> OnAngelStunned;
    public static event Action<AngelController> OnAngelKilled;
    public static event Action OnAllAngelsCleared;

    // --- Beam ---
    public static event Action<BeamHitData> OnBeamHit;
    // BeamHitData: { AngelController target, int bounceCount, Vector2 hitPoint }

    // --- Room ---
    public static event Action<LevelLayoutSO> OnRoomReconfigure;
    public static event Action OnRoomReconfigureComplete;

    // --- Doors ---
    public static event Action OnDoorsOpen;
    public static event Action<UpgradeDefinitionSO> OnUpgradeSelected;

    // --- Game State ---
    public static event Action OnLevelStart;
    public static event Action OnLevelComplete;
    public static event Action OnPlayerDeath;
}
```

Fire events with null checks: `OnAngelKilled?.Invoke(angel);`

---

## 2. Flashlight & Beam System

### Overview
The flashlight casts a primary ray. Each mirror it hits spawns a reflected ray. The system tracks bounce count and applies damage/stun based on tier.

### BeamCaster.cs (on Player)
```csharp
// Responsibilities:
// - Cast primary ray from flashlight position in aim direction
// - On mirror hit: recurse up to MAX_BOUNCES (10)
// - On angel hit: fire EventBus.OnBeamHit with bounce count
// - Render beam using LineRenderer segments from pool

private const int MAX_BOUNCES = 10;

// Use Physics.Raycast with layerMask combining Mirrors, Angels, Walls, Pillars
// Layer priority: Wall/Pillar terminates beam. Mirror reflects. Angel triggers hit.
```

### BeamHitHandler.cs (listens to EventBus.OnBeamHit)
```csharp
// Damage/stun rules:
// bounceCount == 0 → Stun only (duration from PlayerStats.stunDuration)
// bounceCount == 1 → Deal base damage (1)
// bounceCount == 2 → Deal 2x damage (2)
// bounceCount >= 3 → Deal 3x damage (3)

// Special logic: if CurrentLevelIndex == 0 and bounceCount >= 3, instakill TutorialAngel.

// Angels have HP. Stun does not deal damage. Kill fires OnAngelKilled.
```

### MirrorReflector.cs (on Mirror prefab)
```csharp
// Mirrors are static per level (placed by RoomConfigurator from LevelLayoutSO)
// Reflect incoming ray using Vector2.Reflect(inDirection, normal)
// Mirror normal is derived from transform.right or transform.up depending on orientation
// Mirrors have a collider on "Mirror" layer only — beams detect this layer

```

### Beam Rendering
- Use a pooled `LineRenderer` per beam segment
- Segment count = bounce count + 1
- Pool lives in `ObjectPool<BeamSegment>`
- Segments are returned to pool on flashlight deactivation or each frame before recasting

---

## 3. Angel AI System

### AngelController.cs
Angels use a simple state machine: **Idle → Pursuing → Stunned → Dead**

```csharp
public enum AngelState { Idle, Pursuing, Stunned, Dead }

// Movement rule:
// - If player's flashlight beam is currently overlapping this angel's collider → STOP
// - Otherwise → move toward player at moveSpeed (from EnemyDataSO)
// - "Seen" check: Physics2D.OverlapCircle or check if angel is within beam path
//   Precise method: check if angel collider intersects any active BeamSegment line

// Stun:
// - On BeamHit with bounceCount == 0 targeting this angel → enter Stunned
// - Stunned duration = PlayerStats.stunDuration (modified by upgrades)
// - After duration → return to Pursuing

// Death:
// - HP reaches 0 → play death animation/VFX → return to pool → fire OnAngelKilled
// - GameManager listens to OnAngelKilled, tracks count → fires OnAllAngelsCleared when 0 remain
```

### "Seen" Detection
The most important rule: Angels freeze when illuminated.

```csharp
// Approach: Each frame, BeamCaster populates a HashSet<AngelController> of illuminated angels
// AngelController checks: AngelIlluminationRegistry.IsIlluminated(this)
// This avoids expensive per-angel raycasts
```

---

## 4. Room Configuration System

### RoomConfigurator.cs
Listens to EventBus.OnRoomReconfigure(LevelLayoutSO). 
Responsible strictly for loading handcrafted map data. NO procedural generation logic.
```csharp
// On reconfigure:
// 1. Clear current room (return all active environment pieces, mirrors, and pillars to their pools).
// 2. Read the provided LevelLayoutSO (e.g., Map1, Map2).
// 3. Request 3D wall and floor meshes from the PoolManager and place them at the exact coordinates defined in the layout.
// 4. Place mirrors at LevelLayoutSO.mirrorPlacements (position + rotation).
// 5. Place pillars at LevelLayoutSO.pillarPositions.
// 6. Set enemy spawn points.
// 7. Set door spawn points (3 doors, inactive until OnAllAngelsCleared).
// 8. Re-bake or update the NavMesh surface for the new layout.
// 9. Fire OnRoomReconfigureComplete.
```


### LevelLayoutSO.cs
```csharp
[CreateAssetMenu(menuName = "Reflection/LevelLayout")]
public class LevelLayoutSO : ScriptableObject
{
    public string layoutID;
    public Vector2Int roomSizeInTiles;
    public TileBase[] wallTiles;           // or reference a Tilemap asset
    public MirrorPlacement[] mirrorPlacements;
    public Vector2[] pillarPositions;
    public Vector2[] enemySpawnPoints;
    public Vector2[] doorSpawnPoints;      // always 3
    public EnemyWaveData[] waves;
}

[System.Serializable]
public struct MirrorPlacement
{
    public Vector2 position;
    public float rotationDegrees;
}
```

---

## 5. Upgrade & Door System

### Flow
1. `OnAllAngelsCleared` fires
2. `DoorManager` activates 3 doors at pre-placed spawn points
3. Each door is assigned a random `UpgradeDefinitionSO` from the eligible pool
4. Player walks into a door trigger → `OnUpgradeSelected(upgradeSO)` fires
5. `UpgradeApplier` reads `upgradeSO.effectType` and modifies `PlayerStats`
6. `LevelManager` selects next `LevelLayoutSO` and fires `OnRoomReconfigure`

### UpgradeDefinitionSO.cs
```csharp
[CreateAssetMenu(menuName = "Reflection/UpgradeDefinition")]
public class UpgradeDefinitionSO : ScriptableObject
{
    public string upgradeID;
    public string displayName;
    public Sprite icon;
    [TextArea] public string description;
    public UpgradeEffectType effectType;
    public float effectValue;
}

public enum UpgradeEffectType
{
    StunDurationBonus,
    MovementSpeedBonus,
    FlashlightConeWidth,
    ExtraMirrorPlacement,   // grants player one placeable mirror
    BeamRangeBonus,
    // extend as needed
}
```

### UpgradeApplier.cs
```csharp
// Listens to OnUpgradeSelected
// Reads PlayerStats (a persistent ScriptableObject or singleton)
// Applies effectValue to the correct stat based on effectType
// ExtraMirrorPlacement: sets PlayerStats.placeableMirrorCount += 1
//   → FlashlightController enables mirror placement input mode
```

### PlayerStats (Runtime ScriptableObject pattern)
```csharp
[CreateAssetMenu(menuName = "Reflection/PlayerStats")]
public class PlayerStatsSO : ScriptableObject
{
    [NonSerialized] public float stunDuration = 3f;
    [NonSerialized] public float moveSpeed = 5f;
    [NonSerialized] public float flashlightConeAngle = 15f;
    [NonSerialized] public int placeableMirrorCount = 0;

    public void ResetToDefaults() { /* restore base values */ }
}
// Reset on new run via ResetToDefaults() — preserves SO reference, clears runtime values
```

---

## 6. Placeable Mirror (Upgrade)

When `placeableMirrorCount > 0`, the player can place one mirror per level.

```csharp
// FlashlightController: on secondary input (right-click / gamepad shoulder)
//   → enter PlacementMode
//   → show ghost mirror at cursor position (snapped to grid or free)
//   → on confirm: call RoomConfigurator.PlaceExtraMirror(position, rotation)
//   → decrement placeableMirrorCount
//   → placed mirror is returned to pool on room reconfigure
// Placed mirrors behave identically to layout mirrors (same MirrorReflector component)
```

---

## 7. Object Pooling

```csharp
// Utilities/ObjectPool.cs — generic pool
public class ObjectPool<T> where T : MonoBehaviour
{
    // Get(), Return(T obj), PreWarm(int count)
}

// Pools to implement:
// - ObjectPool<BeamSegment>    (LineRenderer segments)
// - ObjectPool<AngelController>
// - ObjectPool<MirrorController>
// - ObjectPool<PillarController>
// - ObjectPool<UpgradeDoor>
```

All pools live on a `PoolManager` GameObject in the scene. Never call `Instantiate` for pooled types.

---

## 8. GameManager State Machine

```
Boot → RoomSetup → CombatPhase → DoorsOpen → UpgradeSelected → RoomSetup → ...
                                                              ↘ PlayerDeath → GameOver
```

```csharp
public enum GamePhase { Boot, RoomSetup, Combat, DoorsOpen, GameOver }

// GameManager owns current phase, current level index, active angel count
// Transitions are driven entirely by EventBus events — no direct method calls
```

---

## 9. Layer Setup (Physics2D)

| Layer Name    | Index | Used For                         |
|---------------|-------|----------------------------------|
| Default       | 0     | —                                |
| Player        | 6     | Player collider                  |
| Enemy         | 7     | Angel colliders                  |
| Mirror        | 8     | Mirror colliders (beam reflects) |
| Wall          | 9     | Tilemap walls (beam terminates)  |
| Pillar        | 10    | Pillar colliders                 |
| Beam          | 11    | Beam trigger colliders           |
| Door          | 12    | Upgrade door triggers            |

Beam raycast mask: `Mirror | Wall | Pillar | Enemy`
Enemy movement raycast mask: `Wall | Pillar`

---

## 10. Common Pitfalls — Avoid These

- **Do not use `GetComponent` in `Update()`** — cache in `Awake()`/`Start()`
- **Do not use `Camera.main` repeatedly** — cache it once
- **Do not use `string` for event/upgrade IDs at runtime** — use SO references or enums
- **Beam raycasts must use `Physics2D.RaycastAll`** not `Raycast` — multiple colliders can be on a line
- **Angel "seen" state must be evaluated after BeamCaster runs** — use `LateUpdate` or explicit ordering via `Script Execution Order`
- **Room reconfiguration must complete in one frame** — no coroutine spreading; use pooling so it's fast enough
- **URP LineRenderer** requires `Use World Space = true` and correct sorting layer

---

## 11. Script Execution Order

Set in `ProjectSettings > Script Execution Order`:

```
-100  BeamCaster
-50   AngelIlluminationRegistry
0     (default)
50    AngelController
100   GameManager
```

BeamCaster runs first, populates illumination registry, then Angels check registry in their update.

---

## Quick Reference Checklist

Before submitting any implementation:

- [ ] No `FindObjectOfType` at runtime
- [ ] No hardcoded numeric values (use SO or Constants)
- [ ] All cross-system calls go through EventBus
- [ ] Pooled objects are returned before room reconfigure
- [ ] New ScriptableObject types have `[CreateAssetMenu]`
- [ ] New prefabs placed in correct `Prefabs/` subfolder
- [ ] Physics layers assigned correctly
- [ ] URP-compatible shaders/materials only
