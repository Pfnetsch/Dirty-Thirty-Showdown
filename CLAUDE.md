# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Dirty Thirty Showdown** is a 2-player local multiplayer arm-wrestling party game built in **Unity 6** (6000.3.3f1). Players button-mash to push a bar to the opponent's side while using character-specific abilities. Best of 3 rounds, 45 seconds per round. Four characters: Eli, Lene, Nati, Sabi — each with 2 unique abilities (8 total).

## Development Setup

- **Unity version**: 6000.3.3f1 (Unity 6.0.3)
- **Rendering**: URP (Universal Render Pipeline 17.3.0)
- **UI text**: TextMeshPro (import TMP Essential Resources if prompted)
- **Input**: Legacy `Input.GetKeyDown` (not the new Input System, despite the package being installed)
- **Character data**: ScriptableObject assets in `Assets/Characters/` — create via menu `Assets > Create > Dirty Thirty Showdown > All Characters`

## Architecture

**Single-scene architecture** — all game states managed by `GameManager` state machine within `Assets/Scenes/SampleScene.unity`.

### Game State Flow
```
CharacterSelect → PreRound (2s) → Playing (45s timer) → RoundEnd (2s) → [next round or MatchEnd]
```

### Core Scripts (all in `Assets/Scripts/`)

| Script | Role |
|--------|------|
| `GameManager` | Singleton. State machine, round/match management, scoring, timer |
| `ArmWrestleController` | Bar physics engine. Position (-1 to +1), velocity, momentum, damping |
| `PlayerController` | Per-player input handling, cooldown management, ability triggering |
| `AbilitySystem` | Executes all 8 abilities via modifiers (multipliers, input disable, bar lock, control reversal) |
| `CharacterData` | ScriptableObject defining character name, abilities, cooldowns, sprites, audio |
| `CharacterSelectManager` | Character selection UI with dual-player keyboard navigation |
| `UIManager` | Syncs all gameplay UI: bar, timer, scores, cooldowns, ability indicators, panels |
| `AudioManager` | Singleton. Three sources: music (crossfade), SFX, voice. Per-character voice lines |
| `ScreenShake` | Singleton. Camera shake effects |
| `GameBootstrap` | Scene initialization, auto-creates missing singletons if `autoSetup` is true |
| `QuickTestController` | Debug tool for testing bar mechanics without full UI (R=reset, T=toggle test) |
| `CharacterDataCreator` | Editor-only utility that programmatically creates all 4 character assets |

### Key Patterns

- **Singletons with DontDestroyOnLoad**: GameManager, AudioManager, ScreenShake
- **Event-driven communication**: Systems are loosely coupled via C# events (e.g., `OnStateChanged`, `OnBarPositionChanged`, `OnAbilityActivated`, `OnRoundEnd`, `OnMatchEnd`)
- **ScriptableObject-driven characters**: Character stats, abilities, and asset references stored in `CharacterData` assets
- **Modifier-based ability effects**: Abilities apply modifiers on `ArmWrestleController` (multipliers, input disable flags, bar lock, controls reversed) rather than directly manipulating bar position

### Bar Physics Model
```
NetForce = (P2Power - P1Power) * barSensitivity
BarVelocity += NetForce * deltaTime
BarVelocity *= damping^(deltaTime * 60)    // frame-rate independent
BarPosition = Clamp(position + velocity * dt, -1, +1)
```
Win threshold: bar reaches ±1.0. On timeout, player with bar closer to their side wins.

### Ability System

Abilities modify shared state on ArmWrestleController:
- `player1Multiplier` / `player2Multiplier` — mash power scaling
- `player1InputDisabled` / `player2InputDisabled` — blocks input entirely
- `barLocked` — restricts bar movement to one direction
- `controlsReversed` — inverts power direction for both players

Lene's Shield is unique: persistent until consumed by blocking the next offensive ability targeting her.

To add a new ability: add to `AbilityType` enum, implement in `AbilitySystem.ExecuteAbility()`, create character data with the new type.

## Controls

| Action | Player 1 | Player 2 |
|--------|----------|----------|
| Mash | Space | Enter |
| Ability 1 | Q | O |
| Ability 2 | E | P |
| Navigate (char select) | A / D | Left / Right |

## Development Status

Phases 1-3 (core mechanics, character system, ability system) are complete.

### Open Phases
- [ ] **Phase 4: Match Flow UI** — Build all UI panels (character select, gameplay HUD, round transitions, match end). Priority 1.
- [ ] **Phase 5: Visual Polish (Pixel Art)** — Character portraits exist. Need arm-wrestling pose variants, ability effects, UI elements.
- [ ] **Phase 6: Audio Integration** — Generic SFX/music from free libraries. Voice lines recorded by the 4 girls. Audio is data-driven via ScriptableObjects (drag & drop in Inspector).
- [ ] **Phase 7: Testing & Balance** — Playtesting + automated balance simulation with AI players.

### Detailed TODO
See `.claude/TODO.md` for comprehensive task breakdown including UI layouts, pixel art asset list, audio file inventory, and balance parameters.

## Pixel Art Generation (PixelLab MCP)

A PixelLab MCP server is connected for AI-powered pixel art asset generation. All operations are async (return job IDs, poll with `get_*` tools).

**Available tools**: `create_character`, `animate_character`, `get_character`, `create_isometric_tile`, `create_topdown_tileset`, `create_sidescroller_tileset` + list/delete variants.

**Style guidelines for this project**:
- Use **side** view for character sprites (arm-wrestling is viewed from the side)
- Keep outline, shading, and detail settings consistent across all characters
- Recommended starting style: `single color black outline`, `basic shading`, `medium detail`, 48px canvas
- Useful animation templates: `fight-stance-idle-8-frames`, `breathing-idle`, `cross-punch`, `lead-jab`, `taking-punch`
- Characters: Eli, Lene, Nati, Sabi — each needs a distinct visual description

### Reference Documents
- `Assets/Docu/game-design-document.md` — Full design document
- `.claude/TODO.md` — Detailed phase-by-phase task list
