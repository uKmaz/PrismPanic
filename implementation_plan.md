# PrismPanic — 44-Hour Game Jam Implementation Plan

> **Deadline:** ~44 hours from now | **Engine:** Unity 6 (URP) | **Genre:** Incremental Survival Action (Top-down 3D)

---

## ⚠️ Critical Doc Corrections

> [!WARNING]
> `CLAUDE.md` references `Physics2D`, `Vector2`, `Physics2D.Raycast`, `2D orthographic camera`, and `Light2D`.  
> `AGENTS.md` explicitly says: **"Standard 3D Physics only — absolutely NO Physics2D"** and **"NavMeshAgent for enemy movement"**.  
> **We follow AGENTS.md.** All physics = 3D. Camera = perspective (top-down angle) or ortho 3D. Raycasts = `Physics.Raycast`. Positions = `Vector3`.

---

## Time Budget (44 hours)

| Phase | Hours | Description |
|-------|-------|-------------|
| 0 — Setup | 2h | Folders, layers, input, core scripts |
| 1 — Core Systems | 4h | GameManager, SOs, EventBus, Pool |
| 2 — Room & Player | 6h | Room building, player movement, flashlight |
| 3 — Beam System | 6h | Raycasting, reflection, rendering |
| 4 — Angel AI | 5h | State machine, NavMesh, stun/death |
| 5 — Doors & Upgrades | 4h | 3 doors, upgrade apply, progression |
| 6 — UI & Menu | 4h | HUD, GameOver, Menu scene |
| 7 — Level Content | 5h | 4 handcrafted layouts |
| 8 — Polish | 8h | VFX, juice, testing, bugfixes |

---

## File Structure (Final)

```
Assets/_Game/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs
│   │   ├── LevelManager.cs
│   │   ├── EventBus.cs
│   │   ├── Constants.cs
│   │   └── SceneController.cs
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   └── FlashlightController.cs
│   ├── Enemies/
│   │   ├── AngelController.cs
│   │   └── AngelIlluminationRegistry.cs
│   ├── Light/
│   │   ├── BeamCaster.cs
│   │   ├── BeamSegment.cs
│   │   ├── BeamHitHandler.cs
│   │   └── MirrorReflector.cs
│   ├── Room/
│   │   └── RoomConfigurator.cs
│   ├── Doors/
│   │   ├── UpgradeDoor.cs
│   │   └── DoorManager.cs
│   ├── Upgrades/
│   │   ├── UpgradeApplier.cs
│   │   └── UpgradeRegistry.cs
│   ├── UI/
│   │   ├── HUDController.cs
│   │   ├── GameOverUI.cs
│   │   └── MainMenuUI.cs
│   └── Utilities/
│       ├── ObjectPool.cs
│       └── PoolManager.cs
├── ScriptableObjects/
│   ├── Upgrades/        (5 SO assets)
│   ├── LevelLayouts/    (4 SO assets)
│   ├── EnemyData/       (1+ SO assets)
│   └── PlayerStats/     (1 SO asset)
├── Prefabs/
│   ├── Room/            (Wall, Floor, Mirror, Pillar)
│   ├── Enemies/         (Angel)
│   ├── Light/           (BeamSegment)
│   ├── UI/              (DoorCanvas)
│   └── Doors/           (UpgradeDoor)
├── Input/
│   └── ReflectionInputActions.inputactions
├── Materials/           (URP materials)
├── Scenes/
│   ├── Main.unity
│   └── Menu.unity
└── Art/                 (textures, icons)
```

---

## Phase 0 — Project Setup (2h)

### Editor Steps

1. **Rename scene**: `SampleScene.unity` → `Main.unity`. Create `Menu.unity`.
2. **Build Settings**: Add both scenes. Menu = index 0, Main = index 1.
3. **Create folder structure** as shown above.
4. **Physics Layers** (Edit → Project Settings → Tags and Layers):

| Layer | Index |
|-------|-------|
| Player | 6 |
| Enemy | 7 |
| Mirror | 8 |
| Wall | 9 |
| Pillar | 10 |
| Beam | 11 |
| Door | 12 |

5. **Physics Collision Matrix** (Project Settings → Physics):
   - Disable unnecessary collisions. Key ones enabled:
   - Player ↔ Enemy (death trigger), Player ↔ Door (upgrade trigger)
   - Beam layer only used for raycast masks, not collisions

6. **Input Actions**: Create new Input Actions asset at `Assets/_Game/Input/ReflectionInputActions.inputactions` with the actions from ARCHITECTURE.md.

### Scripts to Create

#### `EventBus.cs`
```csharp
// Static class, no MonoBehaviour. All cross-system events.
// Events: OnAngelStunned, OnAngelKilled, OnAllAngelsCleared,
//         OnBeamHit, OnRoomReconfigure, OnRoomReconfigureComplete,
//         OnDoorsOpen, OnUpgradeSelected, OnLevelStart, OnLevelComplete,
//         OnPlayerDeath
// Fire with null-conditional: OnAngelKilled?.Invoke(angel);
```

#### `Constants.cs`
```csharp
// Layer masks as static readonly int fields
// LayerMask beamRaycastMask = Mirror | Wall | Pillar | Enemy
// MAX_BOUNCES = 2, BASE_STUN_DURATION = 3f, BASE_MOVE_SPEED = 5f, etc.
```

#### `ObjectPool<T>.cs`
```csharp
// Generic pool: Get(), Return(T), PreWarm(int count, T prefab, Transform parent)
// Uses Queue<T> internally. Activates/deactivates GameObjects.
```

#### `PoolManager.cs`
```csharp
// Singleton MonoBehaviour. Holds pools for Angel, BeamSegment, Mirror, Pillar, Door.
// Exposes typed Get/Return methods. PreWarms all pools in Awake().
// Inspector refs: each prefab + initial pool size.
```

---

## Phase 1 — Core Systems (4h)

### ScriptableObjects to Create

#### `LevelLayoutSO.cs`
```csharp
[CreateAssetMenu(menuName = "PrismPanic/LevelLayout")]
public class LevelLayoutSO : ScriptableObject
{
    public string layoutID;
    public Vector3[] wallPositions;     // 3D positions for wall blocks
    public Vector3[] floorPositions;    // 3D positions for floor tiles
    public MirrorPlacement[] mirrorPlacements;
    public Vector3[] pillarPositions;
    public Vector3[] enemySpawnPoints;
    public Vector3[] doorSpawnPoints;   // always 3
    public Vector3 playerSpawnPoint;
    public EnemyWaveData[] waves;
}

[System.Serializable]
public struct MirrorPlacement
{
    public Vector3 position;
    public float rotationY; // rotation around Y axis (top-down)
}

[System.Serializable]
public struct EnemyWaveData
{
    public EnemyDataSO enemyData;
    public int count;
}
```

#### `EnemyDataSO.cs`
```csharp
[CreateAssetMenu(menuName = "PrismPanic/EnemyData")]
// Fields: moveSpeed (1.5f), maxHP (2), stunDurationModifier (1f)
```

#### `PlayerStatsSO.cs`
```csharp
[CreateAssetMenu(menuName = "PrismPanic/PlayerStats")]
// [NonSerialized] runtime fields: stunDuration=3, moveSpeed=5,
// flashlightConeAngle=15, placeableMirrorCount=0, beamRange=20
// Base values stored as serialized fields for reset
// ResetToDefaults() copies base → runtime
```

#### `UpgradeDefinitionSO.cs`
```csharp
[CreateAssetMenu(menuName = "PrismPanic/UpgradeDefinition")]
// Fields: upgradeID, displayName, icon (Sprite), description, effectType (enum), effectValue
```

```csharp
public enum UpgradeEffectType
{
    StunDurationBonus, MovementSpeedBonus, FlashlightConeWidth,
    ExtraMirrorPlacement, BeamRangeBonus
}
```

### `GameManager.cs`
```csharp
// Singleton MonoBehaviour. Owns GamePhase enum, currentLevel index, activeAngelCount.
// State machine: Boot → RoomSetup → Combat → DoorsOpen → GameOver
// Subscribes to: OnAngelKilled (decrement count → if 0 fire OnAllAngelsCleared),
//   OnUpgradeSelected (advance level, fire OnRoomReconfigure with next layout),
//   OnPlayerDeath (→ GameOver phase)
// On Boot: call PlayerStatsSO.ResetToDefaults(), load level 0
```

### `LevelManager.cs`
```csharp
// Holds LevelLayoutSO[] array (ordered). Returns layout by index.
// Wraps around or ends run when levels exhausted.
```

### Editor Steps
1. Create SO assets in `ScriptableObjects/` folders:
   - `PlayerStats/DefaultPlayerStats.asset`
   - `EnemyData/BaseAngel.asset` (speed=1.5, hp=2, stunMod=1)
   - `Upgrades/` — 5 assets (StunDuration+, MoveSpeed+, ConeWidth+, ExtraMirror, BeamRange+)
2. Create empty GameObjects in Main.unity: `GameManager`, `PoolManager`, `LevelManager`
3. Assign SO references in inspectors

---

## Phase 2 — Room & Player (6h)

### `RoomConfigurator.cs`
```csharp
// Listens to EventBus.OnRoomReconfigure(LevelLayoutSO)
// 1. Return all active pooled objects (walls, mirrors, pillars) to pools
// 2. Read layout SO
// 3. Spawn wall prefabs at wallPositions (from pool)
// 4. Spawn floor prefabs at floorPositions (or use a single scaled floor plane)
// 5. Spawn mirrors at mirrorPlacements (position + Y rotation)
// 6. Spawn pillars at pillarPositions
// 7. Spawn enemies at enemySpawnPoints
// 8. Set door spawn points (inactive until cleared)
// 9. Move player to playerSpawnPoint
// 10. Rebake NavMesh (NavMeshSurface.BuildNavMesh())
// 11. Fire OnRoomReconfigureComplete
```

### `PlayerController.cs`
```csharp
// [RequireComponent(typeof(CharacterController))] or Rigidbody
// Reads Input System Move action → move on XZ plane
// Speed from PlayerStatsSO.moveSpeed
// Aim: mouse world position via camera raycast to ground plane
// Rotate player to face aim direction
```

### `FlashlightController.cs`
```csharp
// Child of Player. Reads Flashlight input (hold).
// When held: enable BeamCaster, enable spotlight/visual
// Reads PlaceMirror input: if placeableMirrorCount > 0, enter placement mode
// Placement mode: show ghost mirror at mouse position, confirm click to place
// Uses a URP Spot Light for visual cone (angle from PlayerStatsSO.flashlightConeAngle)
```

### Prefabs to Create
- **Wall**: Cube scaled (1,3,1), URP material (dark gray), layer=Wall, BoxCollider
- **Floor**: Quad/plane, URP material (lighter gray), no collider needed (or mesh collider for NavMesh)
- **Mirror**: Thin box (0.1,2,1.5), URP reflective material (cyan/teal emissive), layer=Mirror, BoxCollider
- **Pillar**: Cylinder (0.5,3,0.5), URP material (stone color), layer=Pillar, CapsuleCollider

### Editor Steps
1. Create prefabs in `Prefabs/Room/`
2. Create URP materials in `Materials/` (Wall_Mat, Floor_Mat, Mirror_Mat, Pillar_Mat)
3. Set up Player GameObject:
   - Capsule mesh (visual), CharacterController component
   - Child: Flashlight (empty GO + SpotLight + FlashlightController + BeamCaster)
4. Set player layer = Player
5. Add `NavMeshSurface` component to a Room parent GO (for runtime baking)
6. Install AI Navigation package if not present (Window → Package Manager → AI Navigation)

---

## Phase 3 — Beam System (6h)

### `BeamCaster.cs`
```csharp
// Runs at Script Execution Order -100
// Each frame (when flashlight active):
//   1. Clear AngelIlluminationRegistry
//   2. Clear previous beam segments (return to pool)
//   3. Cast ray from flashlight position in aim direction
//   4. Recursive bounce logic (max 2 bounces):
//      - Hit Wall/Pillar → terminate, draw segment
//      - Hit Mirror → reflect direction, draw segment, recurse
//      - Hit Enemy → register in illumination registry, fire OnBeamHit, draw segment
//      - Hit nothing → draw to max range, terminate
//   5. Use Physics.Raycast with Constants.beamRaycastMask
//   6. Reflection: Vector3.Reflect(direction, hit.normal)
//   7. Each segment = pooled BeamSegment with LineRenderer
```

### `BeamSegment.cs`
```csharp
// MonoBehaviour on prefab with LineRenderer
// SetPositions(Vector3 start, Vector3 end)
// LineRenderer: width 0.05, URP unlit material (white/cyan), Use World Space = true
```

### `BeamHitHandler.cs`
```csharp
// Listens to EventBus.OnBeamHit(BeamHitData)
// bounceCount == 0 → call angel.Stun(playerStats.stunDuration)
// bounceCount == 1 → call angel.TakeDamage(1)
// bounceCount == 2 → call angel.TakeDamage(2) (instant kill on 2HP angel)
```

### `BeamHitData.cs`
```csharp
public struct BeamHitData
{
    public AngelController target;
    public int bounceCount;
    public Vector3 hitPoint;
}
```

### `MirrorReflector.cs`
```csharp
// Component on Mirror prefab. Minimal script.
// The mirror's collider + transform.forward provides the normal for reflection.
// BeamCaster handles all reflection math — this just tags the object.
// Could hold a GetReflectionNormal() helper using transform.forward.
```

### `AngelIlluminationRegistry.cs`
```csharp
// Static class with HashSet<AngelController>
// Clear(), Register(angel), IsIlluminated(angel)
// BeamCaster clears + populates each frame
// AngelController queries IsIlluminated(this) to freeze
```

### Editor Steps
1. Create BeamSegment prefab: Empty GO + LineRenderer (2 positions, width 0.05, URP Unlit material cyan)
2. Set PoolManager to pre-warm 6 BeamSegments
3. Set Script Execution Order: BeamCaster=-100, AngelController=50, GameManager=100

---

## Phase 4 — Angel AI (5h)

### `AngelController.cs`
```csharp
// [RequireComponent(typeof(NavMeshAgent))]
// States: Idle, Pursuing, Stunned, Dead
// 
// Pursuing:
//   - If AngelIlluminationRegistry.IsIlluminated(this) → stop NavMeshAgent
//   - Else → NavMeshAgent.SetDestination(player.position), speed from EnemyDataSO
// 
// Stunned:
//   - Stop agent, start timer (duration from BeamHitHandler call)
//   - After timer → return to Pursuing
// 
// HP System:
//   - _currentHP set from EnemyDataSO.maxHP on spawn
//   - TakeDamage(int amount) → reduce HP, if ≤0 → Die()
//   - Stun(float duration) → enter Stunned state
// 
// Death:
//   - Play VFX (particle burst), disable, return to pool
//   - Fire EventBus.OnAngelKilled(this)
// 
// Player Contact:
//   - OnTriggerEnter(Collider) → if player layer → EventBus.OnPlayerDeath?.Invoke()
//   - Use a trigger collider slightly larger than the visual
```

### Prefab: Angel
- Capsule or custom mesh (white/marble material, creepy vibe)
- NavMeshAgent component (speed from SO, angular speed high)
- Trigger collider for player contact (SphereCollider, isTrigger=true, radius ~0.6)
- Main collider for beam detection (CapsuleCollider, layer=Enemy)
- AngelController component
- Layer = Enemy

### Editor Steps
1. Create Angel prefab in `Prefabs/Enemies/`
2. Create `Materials/Angel_Mat.asset` — URP Lit, white/marble
3. Set PoolManager to pre-warm 8 Angels
4. NavMeshAgent: set stopping distance=0.3, acceleration=8

---

## Phase 5 — Doors & Upgrades (4h)

### `DoorManager.cs`
```csharp
// Listens to EventBus.OnAllAngelsCleared
// Gets 3 UpgradeDoor from pool, places at layout's doorSpawnPoints
// Assigns random non-duplicate UpgradeDefinitionSO from eligible pool
// Tracks used upgrades (HashSet<string> by upgradeID) for the run
// On OnUpgradeSelected: return all 3 doors to pool
```

### `UpgradeDoor.cs`
```csharp
// MonoBehaviour on door prefab
// Setup(UpgradeDefinitionSO upgrade): store ref, update visual (icon + name via world-space canvas)
// OnTriggerEnter: if player → fire EventBus.OnUpgradeSelected(upgrade)
// Has a child Canvas (World Space) showing icon + label
```

### `UpgradeApplier.cs`
```csharp
// Listens to EventBus.OnUpgradeSelected
// Switch on effectType → modify PlayerStatsSO runtime fields
// StunDurationBonus: += effectValue
// MovementSpeedBonus: += effectValue
// FlashlightConeWidth: += effectValue
// ExtraMirrorPlacement: placeableMirrorCount += (int)effectValue
// BeamRangeBonus: += effectValue
```

### `UpgradeRegistry.cs`
```csharp
// Holds UpgradeDefinitionSO[] allUpgrades (assigned in inspector)
// GetRandomUpgrades(int count, HashSet<string> excluded): returns List<UpgradeDefinitionSO>
// Filters out already-taken upgrades by ID
```

### Prefab: UpgradeDoor
- Archway shape (two tall cubes + top cube) or simple door mesh
- Trigger collider covering the opening (BoxCollider, isTrigger)
- World-space Canvas child: Image (icon) + TextMeshPro (upgrade name)
- URP emissive material (golden glow)
- Layer = Door

### Editor Steps
1. Create door prefab in `Prefabs/Doors/`
2. Set PoolManager to pre-warm 3 doors
3. Create UpgradeRegistry GO in scene, assign all 5 upgrade SOs

---

## Phase 6 — UI & Menu Scene (4h)

### Main Scene UI

#### `HUDController.cs`
```csharp
// References: TMP_Text for angel count, TMP_Text for level number
// Listens to OnAngelKilled (update count), OnLevelStart (update level)
// Minimal — top-left corner info
```

#### `GameOverUI.cs`
```csharp
// Canvas (Screen Space Overlay), initially disabled
// Listens to OnPlayerDeath → enable canvas
// Shows "GAME OVER" + level reached + Restart button
// Restart button: PlayerStatsSO.ResetToDefaults(), reload Main scene
```

### Menu Scene

#### `MainMenuUI.cs`
```csharp
// Simple canvas: Title "PRISM PANIC", Start button, Quit button
// Start → SceneManager.LoadScene("Main")
// Quit → Application.Quit()
```

#### `SceneController.cs`
```csharp
// Static helper: LoadMenu(), LoadMain()
// Wraps SceneManager.LoadScene calls
```

### Editor Steps (Menu Scene)
1. Open Menu.unity
2. Create Canvas (Screen Space Overlay)
3. Add title text (TMP), Start button, Quit button
4. Style: dark background, white text, minimal

### Editor Steps (Main Scene)
1. Create HUD_Canvas (Screen Space Overlay) — angel count top-left, level number
2. Create GameOver_Canvas (Screen Space Overlay, disabled by default)
3. Assign references in HUDController and GameOverUI

---

## Phase 7 — Level Content (5h)

### Level Design Strategy

Each level = a `LevelLayoutSO` asset defining exact positions for everything.

#### Map1 — Tutorial
- Small room (10x10 units)
- 2 mirrors placed for easy 1-bounce path
- 1 pillar
- 2 angels (slow)
- Player learns: beam → mirror → kill

#### Map2 — Intermediate
- Medium room (12x12)
- 3 mirrors, some requiring player repositioning
- 2 pillars
- 3 angels

#### Map3 — Advanced
- Larger room (14x12)
- 4 mirrors in tricky positions
- 3 pillars creating narrow corridors
- 4 angels (slightly faster via EnemyDataSO variant)

#### Map4 — Hard
- 14x14 room
- 5 mirrors, complex bounce paths needed
- 4 pillars
- 5-6 angels

### How to Author Layouts
1. Create each SO asset in `ScriptableObjects/LevelLayouts/`
2. Enter positions manually (or build a simple editor scene with gizmos)
3. Best approach: **build a test scene**, place cubes visually, copy Transform positions into the SO arrays
4. Assign EnemyDataSO refs (base or harder variants)

### Editor Steps
1. Create `Map1.asset` through `Map4.asset`
2. Create `BaseAngel.asset` and `FastAngel.asset` EnemyDataSO (speed 1.5 vs 2.0)
3. Assign layout array in LevelManager (Map1→Map4 order)

---

## Phase 8 — Polish (8h)

### VFX (Particle Systems)
- **Angel Death**: White particle burst (Unity Particle System, URP compatible)
- **Door Open**: Golden sparkle particles
- **Room Transition**: Screen flash (UI Image alpha tween 0→1→0)
- **Beam**: Add slight glow — second wider transparent LineRenderer behind main beam

### Camera
- Top-down perspective or isometric angle (45° X rotation)
- Smooth follow player (simple lerp in LateUpdate)
- Shake on player death (small random offset for 0.3s)

### Audio (Stubs Only)
- `AudioManager.cs` — singleton, Play(string clipName) method, no actual clips needed
- Hook calls at: angel death, door open, beam fire, player death

### Game Feel
- Angel movement: slight acceleration, not instant full speed
- Beam: slight fade-in on activation
- Mirror hit: brief flash on the mirror material

### Testing Checklist
- [ ] Player moves on XZ, speed matches SO
- [ ] Beam casts, reflects off mirrors (1 and 2 bounces)
- [ ] Direct beam stuns angel (no damage)
- [ ] 1-bounce beam deals 1 damage
- [ ] 2-bounce beam deals 2 damage (instant kill)
- [ ] Angels freeze when illuminated
- [ ] Angels pursue when not illuminated
- [ ] Angel contact = death
- [ ] All angels dead → 3 doors appear
- [ ] Walking through door grants upgrade
- [ ] Room reconfigures to next layout
- [ ] Game over screen appears on death
- [ ] Restart works (stats reset)
- [ ] Menu scene → Main scene transition works

---

## Open Questions

> [!IMPORTANT]
> 1. **Camera style**: Top-down (90° looking straight down) or isometric (45° angle)? I recommend **isometric** for better visual depth.
> 2. **NavMesh package**: Unity 6 uses the AI Navigation package. Is it already installed, or should I add it?
> 3. **Room geometry**: Should walls be individual cubes placed from SO arrays, or a single room prefab with modular pieces? Individual cubes = more flexible for layouts but more work per layout. I recommend **individual cubes** since we need handcrafted variety.
> 4. **Do you want me to start implementing Phase 0 + 1 immediately after approval?**

---

## Verification Plan

### Automated
- Build the project after each phase — ensure zero compile errors
- Enter Play Mode and test each system incrementally

### Manual (You in Editor)
- Phase 2: WASD movement, flashlight toggle, aim at mouse
- Phase 3: Place a mirror in scene, verify beam bounces
- Phase 4: Angel chases, freezes in beam, takes damage, dies
- Phase 5: Doors appear, upgrade applies, room reconfigures
- Phase 6: HUD updates, game over works, menu transitions
- Phase 7: All 4 maps play in sequence
- Phase 8: VFX visible, no frame drops
