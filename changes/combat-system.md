# Combat System — Initial Implementation

**File:** `Ruleset/Combat.cs`  
**Date:** 2026-04-02

## Overview

Implemented a Mythras Imperative-inspired combat system with a strong limb-focused design, built on top of the existing `Entity`, `Limb`, `BodyPart`, and `BaseAttributes` classes.

---

## New Types

### `WoundLevel` (enum)
Per-limb wound state derived from current vs. max HP.

| Value | Condition |
|---|---|
| `None` | HP = max |
| `Wounded` | HP < max |
| `Serious` | HP ≤ max / 2 |
| `Maimed` | HP ≤ 0 |

### `CombatResult` (enum)
Outcome of a single opposed roll.

| Value | Meaning |
|---|---|
| `Fumble` | Attacker rolled > 95 |
| `Miss` | Attacker failed or defender out-succeeded |
| `Hit` | Attacker succeeded |
| `CriticalHit` | Attacker rolled ≤ skill / 10 |

### `AttackResult` (record)
Returned by `PerformAttack`. Contains:
- `Result` — `CombatResult`
- `HitLocation` — `BodyParts?`
- `RawDamage`, `ArmorMitigation`, `FinalDamage`
- `WoundCaused` — `WoundLevel` applied to the hit limb
- `LimbMaimed` — true if a peripheral limb reached 0 HP
- `Fatal` — true if a vital location (Head/Chest/Abdomen) reached 0 HP

---

## `Combat` Static Class

### Derived Stats

| Method | Formula |
|---|---|
| `CombatSkill(entity)` | `(STR + DEX) * 2` as d100 target |
| `ActionPoints(entity)` | `1 + max(0, (DEX - 10) / 10)` |
| `RollInitiative(entity)` | `DEX + 1d10` |
| `DamageBonus(entity)` | `max(0, (STR - 10) / 5)` added flat to damage |

### Limb HP

| Method | Behaviour |
|---|---|
| `MaxLimbHP(limb)` | `max(1, Size / 5)` |
| `InitializeLimbHP(entity)` | Sets all `Limb.HitPoint` to their max — call once before combat |

### Hit Location (`RollHitLocation`)
d20 table covering all 8 body parts. Tail is only a valid location when its Size > 0.

### Opposed Roll Resolution (`ResolveOpposed`)
Mirrors Mythras Imperative rules:
- **Critical** = roll ≤ skill / 10
- **Hit** = roll ≤ skill
- **Miss** = roll > skill
- **Fumble** = roll > 95
- Both succeed → higher degree wins; same degree → higher absolute roll wins; attacker wins exact ties.

### Core Methods

| Method | Description |
|---|---|
| `PerformAttack(attacker, defender, targetLimb?)` | Resolves one attack. Targeting a specific limb costs −20 to attack skill. Deals 1d4 + damage bonus; crits double damage. |
| `CombatRound(entityA, entityB)` | Runs a full round. Rolls initiative, alternates actions one-for-one based on AP, stops when an entity is incapacitated. Returns all `AttackResult`s. |
| `IsIncapacitated(entity)` | True when Head, Chest, or Abdomen HP ≤ 0. |
| `MaimedLimbs(entity)` | Returns peripheral limbs (arms, legs, tail) with HP ≤ 0. |
| `IsVitalLocation(part)` | True for Head, Chest, Abdomen. |
| `GetLimb(entity, part)` | Retrieves the `Limb` for a given `BodyParts` value. |
