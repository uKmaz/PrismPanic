# PrismPanic — Walkthrough

## What Was Done (Code)

**20 scripts created** across the entire project, covering Phases 0–6. All scripts compile-ready (pending AI Navigation package install for `NavMeshSurface`).

### File Map

| Script | Path | Role |
|--------|------|------|
| [EventBus.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Core/EventBus.cs) | Core | All cross-system events + fire helpers |
| [Constants.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Core/Constants.cs) | Core | Layer masks, pool sizes, base values |
| [GameManager.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Core/GameManager.cs) | Core | Game phase state machine |
| [LevelManager.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Core/LevelManager.cs) | Core | Ordered layout provider |
| [SceneController.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Core/SceneController.cs) | Core | Scene loading helper |
| [ObjectPool.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Utilities/ObjectPool.cs) | Utilities | Generic pool |
| [PoolManager.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Utilities/PoolManager.cs) | Utilities | Singleton owning all pools |
| [PlayerStatsSO.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/ScriptableObjects/PlayerStatsSO.cs) | SO | Runtime player stats with reset |
| [EnemyDataSO.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/ScriptableObjects/EnemyDataSO.cs) | SO | Enemy stat definition |
| [LevelLayoutSO.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/ScriptableObjects/LevelLayoutSO.cs) | SO | Handcrafted map data |
| [UpgradeDefinitionSO.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/ScriptableObjects/UpgradeDefinitionSO.cs) | SO | Upgrade definition |
| [RoomConfigurator.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Room/RoomConfigurator.cs) | Room | Builds room from layout SO |
| [PlayerController.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Player/PlayerController.cs) | Player | WASD + mouse aim |
| [FlashlightController.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Player/FlashlightController.cs) | Player | Beam toggle + mirror placement |
| [BeamCaster.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Light/BeamCaster.cs) | Light | Raycast + reflection + illumination |
| [BeamSegment.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Light/BeamSegment.cs) | Light | LineRenderer wrapper |
| [BeamHitHandler.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Light/BeamHitHandler.cs) | Light | Stun/damage logic |
| [MirrorReflector.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Light/MirrorReflector.cs) | Light | Mirror tag + normal helper |
| [AngelIlluminationRegistry.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Light/AngelIlluminationRegistry.cs) | Light | Static illuminated angel set |
| [AngelController.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Enemies/AngelController.cs) | Enemies | AI state machine + NavMesh |
| [DoorManager.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Doors/DoorManager.cs) | Doors | Spawns 3 upgrade doors |
| [UpgradeDoor.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Doors/UpgradeDoor.cs) | Doors | Door trigger + UI display |
| [UpgradeApplier.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Upgrades/UpgradeApplier.cs) | Upgrades | Applies effects to stats |
| [UpgradeRegistry.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/Upgrades/UpgradeRegistry.cs) | Upgrades | Random non-duplicate picker |
| [HUDController.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/UI/HUDController.cs) | UI | Angel count + level display |
| [GameOverUI.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/UI/GameOverUI.cs) | UI | Death screen + restart |
| [MainMenuUI.cs](file:///c:/Users/Ucmaz%20pc/Git/PrismPanic/Assets/_Game/Scripts/UI/MainMenuUI.cs) | UI | Menu scene start/quit |

**Physics layers** also configured in `TagManager.asset` (Player=6, Enemy=7, Mirror=8, Wall=9, Pillar=10, Beam=11, Door=12).

---

## What YOU Need To Do In Unity Editor

### Step 1: Install AI Navigation Package
1. **Window → Package Manager**
2. Click **+** → **Add package by name**
3. Type: `com.unity.ai.navigation` → **Add**
4. Wait for import

### Step 2: Install TextMeshPro (if prompted)
- When Unity prompts to import TMP Essentials → click **Import**

### Step 3: Scenes
1. **Rename** `Assets/Scenes/SampleScene.unity` → `Main.unity`
2. **Create** new scene: Right-click `Assets/_Game/Scenes/` → Create → Scene → name it `Menu`
3. **File → Build Settings** → Add both scenes. **Menu = index 0, Main = index 1**

### Step 4: Create Prefabs

#### Wall Prefab (`Prefabs/Room/Wall`)
1. Create Cube in scene → Scale (1, 3, 1)
2. Create URP Lit material `Materials/Wall_Mat` → dark gray (#333333)
3. Assign material, set layer to **Wall** (index 9)
4. Drag to `Assets/_Game/Prefabs/Room/` → delete from scene

#### Floor Prefab (`Prefabs/Room/Floor`)
1. Create Cube → Scale (1, 0.1, 1)
2. Create material `Materials/Floor_Mat` → light gray (#888888)
3. Layer = **Default** (0)
4. Save as prefab

#### Mirror Prefab (`Prefabs/Room/Mirror`)
1. Create Cube → Scale (0.1, 2, 1.5)
2. Create material `Materials/Mirror_Mat` → URP Lit, Metallic=1, Smoothness=0.9, Base color cyan with Emission enabled (teal glow)
3. Layer = **Mirror** (8) — set on the collider's GO
4. Add component: `MirrorReflector`
5. Save as prefab

#### Pillar Prefab (`Prefabs/Room/Pillar`)
1. Create Cylinder → Scale (0.5, 3, 0.5)
2. Material `Materials/Pillar_Mat` → stone-ish (#999988)
3. Layer = **Pillar** (10)
4. Save as prefab

#### Angel Prefab (`Prefabs/Enemies/Angel`)
1. Create Capsule → Scale (0.6, 1, 0.6) (or use a custom mesh later)
2. Material `Materials/Angel_Mat` → white marble (#EEEEEE), Smoothness=0.7
3. Layer = **Enemy** (7)
4. Add **NavMeshAgent** → Speed=1.5, Angular Speed=360, Stopping Distance=0.3
5. Add **CapsuleCollider** (for beam raycast detection) — NOT trigger
6. Add **SphereCollider** → isTrigger=true, Radius=0.6 (for player contact kill)
7. Add `AngelController` component
8. **Tag**: don't need a tag on angel
9. Save as prefab

#### BeamSegment Prefab (`Prefabs/Light/BeamSegment`)
1. Create empty GO → Add **LineRenderer**
2. LineRenderer settings: Positions=2, Width=0.06, Use World Space=true
3. Create material `Materials/Beam_Mat` → URP Unlit, Color=white
4. Assign to LineRenderer
5. Add `BeamSegment` component
6. Save as prefab

#### UpgradeDoor Prefab (`Prefabs/Doors/UpgradeDoor`)
1. Create empty GO "UpgradeDoor"
2. Child: Cube scaled (1.5, 3, 0.3) — the door frame. Material: golden (#FFD700) with emission
3. Child: **BoxCollider** on parent GO → isTrigger=true, size covering the doorway
4. Layer = **Door** (12)
5. Child: Canvas (World Space) → Render Mode: World Space, width=200, height=150
6. Inside Canvas: **Image** (for icon) + **TMP_Text** (for upgrade name)
7. Add `UpgradeDoor` component → assign label and image refs
8. Save as prefab

### Step 5: Main Scene Hierarchy Setup

Open `Main.unity` and build this hierarchy:

```
[PERSISTENT]
├── GameManager          (add GameManager.cs)
├── LevelManager         (add LevelManager.cs)
├── PoolManager          (add PoolManager.cs)
├── UpgradeApplier       (add UpgradeApplier.cs)
├── UpgradeRegistry      (add UpgradeRegistry.cs)
├── BeamHitHandler       (add BeamHitHandler.cs)
├── DoorManager          (add DoorManager.cs)

[ROOM]
├── RoomConfigurator     (add RoomConfigurator.cs + NavMeshSurface)
├── Walls_Container      (empty — parent for pooled walls)
├── Floors_Container     (empty — parent for pooled floors)
├── Mirrors_Container    (empty)
├── Pillars_Container    (empty)
├── Enemies_Container    (empty)

[PLAYER]
├── Player               (Tag: Player, Layer: Player)
│   ├── CharacterController component
│   ├── PlayerController.cs
│   ├── PlayerInput component (assign input actions)
│   └── Flashlight (child empty GO)
│       ├── Spot Light (URP) — angle=30, range=20, intensity=2
│       ├── FlashlightController.cs
│       └── BeamCaster.cs

[DOORS]
├── Doors_Container      (empty)

[UI]
├── HUD_Canvas           (Screen Space Overlay)
│   ├── AngelCount_Text  (TMP — top-left)
│   └── Level_Text       (TMP — top-left, below count)
│   └── HUDController.cs (on canvas)
├── GameOver_Canvas      (Screen Space Overlay, DISABLED by default)
│   ├── Panel (dark overlay)
│   │   ├── GameOver_Text (TMP — "GAME OVER")
│   │   ├── LevelReached_Text (TMP)
│   │   ├── Restart_Button
│   │   └── Menu_Button
│   └── GameOverUI.cs (on canvas)

[CAMERA]
├── Main Camera
│   └── Position: (0, 15, -10), Rotation: (55, 0, 0) — isometric view
```

### Step 6: Wire Inspector References

**GameManager**: Assign `PlayerStatsSO` asset + `LevelManager` reference
**LevelManager**: Assign layout SO array (empty for now, fill after Step 8)
**PoolManager**: Assign all 7 prefabs + all 7 container transforms
**RoomConfigurator**: Assign Player transform + NavMeshSurface
**BeamCaster**: Assign PlayerStatsSO, FlashlightController, PlayerController, beam origin transform
**FlashlightController**: Assign PlayerStatsSO, SpotLight, RoomConfigurator
**BeamHitHandler**: Assign PlayerStatsSO
**UpgradeApplier**: Assign PlayerStatsSO
**UpgradeRegistry**: Assign all 5 upgrade SO assets (create in Step 8)
**DoorManager**: Assign UpgradeRegistry
**HUDController**: Assign both TMP_Text refs
**GameOverUI**: Assign panel, text, buttons
**Player**: Set **Tag = Player** and **Layer = Player** (index 6)

### Step 7: Input Actions Setup
1. Open/Create `Assets/_Game/Input/ReflectionInputActions.inputactions`
2. Create **Action Map**: "Player"
3. Add actions:

| Action | Type | Binding |
|--------|------|---------|
| Move | Value (Vector2) | WASD / Left Stick |
| Aim | Value (Vector2) | Mouse Position / Right Stick |
| Flashlight | Button (Hold) | Left Mouse Button / Right Trigger |
| PlaceMirror | Button | Right Mouse Button / Right Shoulder |

4. **Generate C# class**: Check "Generate C# Class" in the asset inspector
5. On the **Player** GO: Add **PlayerInput** component → assign this asset → set Behavior to **Send Messages**

### Step 8: Create ScriptableObject Assets

#### PlayerStats
Right-click `ScriptableObjects/PlayerStats/` → Create → PrismPanic → PlayerStats → name: `DefaultPlayerStats`
(Default values are coded in — just create the asset)

#### EnemyData
- `ScriptableObjects/EnemyData/BaseAngel` — speed=1.5, hp=2, stunMod=1
- `ScriptableObjects/EnemyData/FastAngel` — speed=2.0, hp=2, stunMod=0.8

#### Upgrades (5 assets in `ScriptableObjects/Upgrades/`)

| Asset Name | ID | Display Name | Effect Type | Value |
|------------|-----|-------------|-------------|-------|
| StunDurationPlus | stun_dur | Stun Duration+ | StunDurationBonus | 1 |
| MoveSpeedPlus | move_spd | Move Speed+ | MovementSpeedBonus | 0.5 |
| ConeWidthPlus | cone_wid | Flashlight Cone+ | FlashlightConeWidth | 5 |
| ExtraMirror | extra_mir | Extra Mirror | ExtraMirrorPlacement | 1 |
| BeamRangePlus | beam_rng | Beam Range+ | BeamRangeBonus | 5 |

(Leave icon as null for now — you can add sprites later)

#### Level Layouts (4 assets in `ScriptableObjects/LevelLayouts/`)
Create `Map1` through `Map4`. Here's **Map1** data to type in:

**Map1 — Tutorial (10×10 room)**:
- `playerSpawnPoint`: (0, 0.5, 0)
- `wallPositions`: Build a perimeter. For a 10×10 room centered at origin, place walls at:
  - X=-5 to X=5, Z=-5 (bottom wall): (-5,1.5,-5), (-4,1.5,-5), ... (5,1.5,-5) — 11 cubes
  - Same for top wall Z=5, left wall X=-5, right wall X=5
  - (~40 wall cubes total for perimeter)
- `floorPositions`: 10×10 grid from (-4.5, 0, -4.5) to (4.5, 0, 4.5) in 1-unit steps
- `mirrorPlacements`: 2 mirrors — e.g., (2, 1, 0) rotated 45°, (-2, 1, 3) rotated -30°
- `pillarPositions`: 1 pillar at (0, 1.5, 2)
- `enemySpawnPoints`: (-3, 0.5, 3), (3, 0.5, -3)
- `doorSpawnPoints`: (0, 1.5, 4.5), (-3, 1.5, 4.5), (3, 1.5, 4.5)
- `waves`: 1 wave, BaseAngel SO ref, count=2

> [!TIP]
> For faster layout authoring: Place cubes visually in a test scene, copy transform positions, then type them into the SO. You can also write a simple editor script to export positions — but for 4 maps, manual is fine.

### Step 9: Script Execution Order
**Edit → Project Settings → Script Execution Order** → Add:
- `BeamCaster` = **-100**
- `AngelController` = **50**
- `GameManager` = **100**

### Step 10: Physics Collision Matrix
**Edit → Project Settings → Physics** → Collision Matrix:
- Enable: **Player ↔ Enemy**, **Player ↔ Door**
- Disable everything else that's unnecessary (beam never physically collides — it raycasts)

### Step 11: Menu Scene Setup
1. Open `Menu.unity`
2. Create Canvas (Screen Space Overlay)
3. Add dark background panel (#1a1a2e)
4. Add TMP_Text: "PRISM PANIC" (large, white, centered)
5. Add Button: "START" → large
6. Add Button: "QUIT" → below start
7. Create empty GO → Add `MainMenuUI.cs` → Assign button refs

---

## Architecture Flow Recap

```
Menu Scene → [Start Button] → Main Scene
    ↓
GameManager.Start() → PlayerStatsSO.ResetToDefaults()
    ↓
GameManager fires EventBus.OnRoomReconfigure(Map1)
    ↓
RoomConfigurator: Clear room → Place walls/floors/mirrors/pillars → Spawn angels → Bake NavMesh
    ↓
EventBus.OnRoomReconfigureComplete → GameManager enters Combat phase
    ↓
Player moves (WASD), aims (mouse), holds flashlight (LMB)
    ↓
BeamCaster: Raycast → Hit mirror? Reflect. Hit angel? Register illumination + fire OnBeamHit
    ↓
BeamHitHandler: 0 bounce = stun, 1 bounce = 1 dmg, 2 bounce = 2 dmg
    ↓
AngelController: HP ≤ 0 → Die → EventBus.OnAngelKilled
    ↓
GameManager: angelCount-- → if 0 → EventBus.OnAllAngelsCleared → EventBus.OnDoorsOpen
    ↓
DoorManager: Spawn 3 doors with random upgrades
    ↓
Player walks through door → UpgradeDoor fires EventBus.OnUpgradeSelected
    ↓
UpgradeApplier: Modify PlayerStatsSO
GameManager: Advance level → fire EventBus.OnRoomReconfigure(next layout)
    ↓
[LOOP]
```

**Death**: Angel touches player → EventBus.OnPlayerDeath → GameOverUI shows → Restart or Menu
