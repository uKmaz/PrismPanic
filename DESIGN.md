# DESIGN.md
# Authoritative Game Design Reference — Reflection (Working Title)

This document is the source of truth for all gameplay decisions. If code conflicts with this document, the code is wrong. Agents must not implement features not described here without explicit approval.

---

## Core Pillars

1. **Reflection is everything** — mirrors and light beams are the solution to every problem in the game
2. **Tension through pursuit** — Angels never rush, but they never stop
3. **Scope is a feature** — one room, tight systems, high polish over breadth

---

## Game Loop

```
Enter reconfigured room
    → Survive Angel pursuit
    → Use mirrors to kill all Angels
    → Three upgrade doors open immediately
    → Choose one door (grant upgrade + reconfigure room)
    → Repeat
```

No downtime between combat and progression. Doors open the moment the last Angel dies.

---

## The Room

- There is **one scene** and **one room**.
- The room is reconfigured between levels strictly by loading handcrafted map data. There is NO procedural generation.
- Progression is linear: Level 1 loads Map1, Level 2 loads Map2, etc. 
- Each handcrafted configuration is stored as a `LevelLayoutSO` (ScriptableObject).
- Room reconfiguration is instant — no loading screen, no transition pause beyond a brief VFX flash. The `RoomConfigurator` simply reads the active `LevelLayoutSO` and moves the pooled 3D environment pieces, mirrors, and pillars into their exact authored coordinates.

---

## The Flashlight — Beam Tiers

| Tier | Bounces Required | Effect |
|------|-----------------|--------|
| Basic | 0 (direct) | Stun only — 3 seconds base duration, no damage |
| 1-Bounce | 1 mirror | Base damage |
| 2-Bounce | 2 mirrors | 2× base damage |

- Beams terminate on walls and pillars
- Beams reflect off mirrors (max 2 reflections tracked)
- A beam hitting an Angel beyond 2 bounces does nothing (out of scope — do not implement)
- The flashlight is always active while the input is held — there is no cooldown or battery

---

## Angels

- Inspired by Weeping Angels — freeze when illuminated by the player's flashlight beam
- Move slowly toward the player when **not** illuminated
- An Angel touching the player = **instant death** (no HP for player)
- Angels have HP; only 1-bounce and 2-bounce beams deal damage
- Angels do not interact with each other — no flocking, no collision avoidance between Angels
- On death: play death VFX, return to pool, decrement active angel count

### Angel Stats (base, before upgrades)
| Stat | Value |
|------|-------|
| Move speed | 1.5 units/sec |
| HP | 2 hits (1-bounce = 1 hit, 2-bounce = instant kill) |
| Stun duration | 3 seconds |

---

## Mirrors

- **Fixed per layout** — defined in `LevelLayoutSO`, cannot be moved by the player normally
- Orientation is set at layout authoring time (rotation in degrees)
- One upgrade allows the player to place **one additional mirror** per level — free placement anywhere in the room
- Placed mirrors are removed when the room reconfigures
- Mirrors do not break, do not have HP

---

## Pillars

- Static obstacles placed per layout
- Block both enemy movement and beam paths
- No interaction beyond collision — they do not move, rotate, or change

---

## Upgrade Doors

- Exactly **3 doors** open after all Angels are cleared
- Each door is assigned one upgrade from the eligible pool (no duplicates in the same offer)
- Upgrades are **automatic on entry** — walk through the door to receive it
- The door visually communicates the upgrade (icon + label displayed on the door prefab)
- Once a door is chosen, the other two close/disappear before room reconfiguration begins

### Upgrade Pool

| Upgrade | Effect |
|---------|--------|
| Stun Duration+ | +1 second stun duration |
| Move Speed+ | +0.5 units/sec player speed |
| Flashlight Cone+ | Widens flashlight cone angle |
| Extra Mirror | +1 placeable mirror for next level |
| Beam Range+ | Extends max beam travel distance |

- Pool can grow — add new `UpgradeDefinitionSO` assets without code changes
- No upgrade may appear more than once per run (track by upgradeID)

---

## Phases — No Exit Puzzle

There is **no exit puzzle phase**. The original design included a switch/button mechanism, but this has been cut. Once all Angels are dead, the room is "won" and doors open immediately. Do not implement any switch, button, or exit-unlock mechanic.

---

## Death & Restart

- Player has no HP — one Angel touch = death
- On death: show game over screen, offer restart from Level 1
- All runtime stats reset to base values (`PlayerStatsSO.ResetToDefaults()`)
- No permadeath unlocks at jam scope — future feature

---

## Difficulty Scaling

Difficulty scales per level via the `LevelLayoutSO`:
- More Angels per wave
- More complex mirror layouts (fewer "easy" bounce paths)
- Faster Angel base speed (modified in `EnemyDataSO` referenced by layout)
- Tighter room configurations that limit safe zones

No dynamic difficulty adjustment — difficulty is authored per layout.

---

## Out of Scope (Do Not Implement)

- Exit puzzle / switch mechanic (cut)
- Angel-to-Angel interactions
- Beam damage beyond 2 bounces
- Multiple scenes or level select
- Save/load system
- Sound design implementation (placeholder only)
- Dialogue or narrative text
- Boss enemies
- Player HP / health pickups
- Beam cooldown or battery mechanic
- Any feature not described in this document
