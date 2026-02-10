# Dirty Thirty Showdown - TODO

## Open Phases Overview
- [ ] Phase 4: Match Flow UI
- [ ] Phase 5: Visual Polish (Pixel Art)
- [ ] Phase 6: Audio Integration
- [ ] Phase 7: Testing & Balance

---

## Phase 4: Match Flow UI

### Character Select Screen
- [ ] Layout: Two player panels (left/right) with character grid in center
  - P1 panel (left): Portrait, name, ability 1 name + key (Q), ability 2 name + key (E), READY indicator
  - P2 panel (right): Portrait, name, ability 1 name + key (O), ability 2 name + key (P), READY indicator
  - Center grid: 4 character buttons (2x2 or 1x4) with portrait + name, highlight borders for P1 (blue) / P2 (red)
  - Bottom: Instructions text, START button (appears when both ready)
- [ ] Wire up `CharacterSelectManager` to actual scene UI elements
- [ ] Character button prefab: Portrait image, Name TMP text, Highlight border image

### Gameplay HUD
- [ ] Layout: Symmetric HUD for 2 players
  - **Top center**: Round indicator ("Round 1 / 2 / 3"), Timer (large, countdown from 45)
  - **Left side (P1)**: Portrait, character name, ability 1 cooldown (radial fill + key label "Q"), ability 2 cooldown (radial fill + key label "E"), shield indicator (if Lene)
  - **Right side (P2)**: Mirror of P1 panel with keys "O" and "P"
  - **Center**: Tug-of-war bar (horizontal), bar indicator/cursor, P1 color fill (blue) vs P2 color fill (red)
  - **Score display**: Round wins per player (dots or checkmarks)
- [ ] Ability effect overlays: PowerSurge glow, ControlsReversed arrows, InputDisabled "stunned" icon
- [ ] Screen flash overlay (CanvasGroup for Eli's Flash ability - already in AbilitySystem)

### Round Transition Panels
- [ ] **Pre-Round panel**: "Round X - Get Ready!" centered text, 2-second display
- [ ] **Round End panel**: "{Winner} wins Round X!" centered text, 2-second display
- [ ] **Match End panel**: Large "{Winner} WINS!" text, rematch button, character select button
- [ ] Animations/transitions between panels (fade in/out)

### Rematch / Return Flow
- [ ] Rematch button → `GameManager.RestartMatch()`
- [ ] Character Select button → `GameManager.ReturnToCharacterSelect()`
- [ ] Keyboard shortcuts for rematch (e.g., Space/Enter to rematch from MatchEnd)

---

## Phase 5: Visual Polish (Pixel Art)

### Pixel Art Assets Needed

#### Character Portraits (for UI panels)
- [ ] Eli - portrait icon (headshot/bust, ~64x64 or 128x128)
- [ ] Lene - portrait icon
- [ ] Nati - portrait icon
- [ ] Sabi - portrait icon
> **Status**: Patrick has these already

#### Character Select Icons (for grid buttons)
- [ ] Eli - selectable icon (can reuse portrait or create variant)
- [ ] Lene - selectable icon
- [ ] Nati - selectable icon
- [ ] Sabi - selectable icon

#### Arm Wrestling Sprites (gameplay scene)
- [ ] Eli - arm wrestling pose (idle/neutral)
- [ ] Eli - arm wrestling pose (pushing/winning)
- [ ] Eli - arm wrestling pose (losing/strained)
- [ ] Lene - arm wrestling pose (idle/neutral)
- [ ] Lene - arm wrestling pose (pushing/winning)
- [ ] Lene - arm wrestling pose (losing/strained)
- [ ] Nati - arm wrestling pose (idle/neutral)
- [ ] Nati - arm wrestling pose (pushing/winning)
- [ ] Nati - arm wrestling pose (losing/strained)
- [ ] Sabi - arm wrestling pose (idle/neutral)
- [ ] Sabi - arm wrestling pose (pushing/winning)
- [ ] Sabi - arm wrestling pose (losing/strained)
> **Note**: Patrick will generate variants from existing character icons. 3 states per character = 12 sprites total.

#### Victory / Defeat Poses
- [ ] Eli - victory celebration sprite
- [ ] Eli - defeat sprite
- [ ] Lene - victory celebration sprite
- [ ] Lene - defeat sprite
- [ ] Nati - victory celebration sprite
- [ ] Nati - defeat sprite
- [ ] Sabi - victory celebration sprite
- [ ] Sabi - defeat sprite

#### Ability Effect Sprites/Animations
- [ ] Power Surge - sparkle/glow effect (Eli)
- [ ] Flash - screen flash overlay (Eli) — can be a simple white texture + CanvasGroup
- [ ] Trash Talk - speech bubble with text (Lene)
- [ ] Shield - protective bubble/aura (Lene)
- [ ] Shield Break - shatter effect when consumed (Lene)
- [ ] Wink/Flirt - hearts/sparkles (Nati)
- [ ] Dance - music notes (Nati)
- [ ] Cake Toss - cake splat (Sabi)
- [ ] Fake Out - confusion/reverse arrows (Sabi)

#### UI Elements
- [ ] Tug-of-war bar background
- [ ] Bar indicator/cursor
- [ ] Cooldown radial overlay
- [ ] Round win indicators (dots/stars)
- [ ] Background / stage art
- [ ] Title screen logo "Dirty Thirty Showdown"

### Sprite Specifications
- **Style**: Pixel art, vibrant colors, readable at party distance
- **Portrait size**: 128x128 recommended (scales well)
- **Gameplay sprites**: 64x64 or 128x128 per character
- **Effects**: Can be smaller, 32x32 to 64x64
- **Format**: PNG with transparency

---

## Phase 6: Audio Integration

### Audio File Inventory

#### Music Tracks (generic/royalty-free OK)
- [ ] Character select theme (looping, upbeat)
- [ ] Battle/gameplay music (looping, energetic, builds tension)
- [ ] Victory jingle (short, celebratory, non-looping)
- [ ] Menu music (optional, looping, chill)

#### Generic SFX (can source from free libraries)
- [ ] Button mash hit sound (subtle, non-annoying since it plays rapidly)
- [ ] Character select navigate sound (click/beep)
- [ ] Character select confirm sound (heavier click/chime)
- [ ] Round start sound (bell/horn)
- [ ] Round end sound (buzzer/whistle)
- [ ] Match win fanfare
- [ ] Cooldown ready ping (ability available again)
- [ ] Timer warning beep (last 10 seconds)

#### Ability SFX (generic, one per ability)
- [ ] Power Surge activation (power-up whoosh)
- [ ] Flash activation (camera flash / bright burst)
- [ ] Trash Talk activation (can be generic taunt sound, or just use voice line)
- [ ] Shield activation (magic barrier sound)
- [ ] Shield block/break (glass shatter or energy pop)
- [ ] Wink/Flirt (kiss/charm sparkle)
- [ ] Celebration Dance (short music/dance beat)
- [ ] Cake Toss (splat sound)
- [ ] Fake Out (reverse/warp sound)

#### Voice Lines (RECORDED BY THE GIRLS)
Each character needs voice lines for these categories (stored in `CharacterData` ScriptableObject arrays):

**Per Character:**
- [ ] Select voice lines (1-3 clips) — said when picked on character select
- [ ] Ability 1 voice lines (1-3 clips) — said when using ability 1
- [ ] Ability 2 voice lines (1-3 clips) — said when using ability 2
- [ ] Victory voice lines (1-3 clips) — said when winning a round/match
- [ ] Defeat voice lines (1-3 clips) — said when losing
- [ ] Trash talk lines (Lene only, 3-5 clips) — played during Trash Talk ability

**Recording Guidelines for Patrick:**
- Format: WAV or MP3, mono, 44.1kHz
- Keep clips short: 1-3 seconds max
- Record in a quiet room
- Leave a tiny bit of silence at start/end
- Naming convention: `{character}_{category}_{number}.wav`
  - e.g., `eli_select_01.wav`, `lene_trashtalk_03.wav`, `nati_ability1_02.wav`

### Hot-Swap Audio System
- [ ] All audio clips are already SerializeField references on `AudioManager` and `CharacterData`
- [ ] To swap audio: just drag new clips onto the Inspector fields in Unity
- [ ] Consider: Create an `Assets/Audio/` folder structure for organization:
  ```
  Assets/Audio/
    Music/
      character_select.wav
      battle.wav
      victory.wav
    SFX/
      mash_hit.wav
      round_start.wav
      ...
    Voice/
      Eli/
        eli_select_01.wav
        eli_ability1_01.wav
        ...
      Lene/
        lene_select_01.wav
        lene_trashtalk_01.wav
        ...
      Nati/
      Sabi/
  ```
- [ ] The current `CharacterData` already supports multiple voice line arrays per category with random selection — just populate the arrays in the Inspector
- [ ] No code changes needed for swapping — it's already data-driven via ScriptableObjects

---

## Phase 7: Testing & Balance

### Manual Playtesting
- [ ] Play all 6 character matchups (4 choose 2 = 6 combinations)
- [ ] Test mirror matches (same character vs same character)
- [ ] Check ability timing feels right
- [ ] Check bar sensitivity / mash rate feels satisfying
- [ ] Verify rounds don't end too fast or too slow
- [ ] Test edge cases: both abilities at once, shield vs each ability, etc.

### Balance Simulation (Automated)
- [ ] Create `BalanceSimulator` script (Editor-only or standalone)
- [ ] Simulate matches with AI players using different strategies:
  - Random masher (constant random input)
  - Optimal masher (max speed input)
  - Ability spammer (use abilities on cooldown)
  - Strategic player (save abilities for key moments)
- [ ] Run 1000+ simulated matches per matchup
- [ ] Track win rates per character per matchup
- [ ] Output balance report: character win rates, ability usage stats, average match duration
- [ ] Use data to tune: cooldowns, durations, multiplier values, bar sensitivity

### Key Balance Parameters (in Inspector)
| Parameter | Script | Current Value |
|-----------|--------|---------------|
| Bar Sensitivity | ArmWrestleController | 0.1 |
| Bar Damping | ArmWrestleController | 0.95 |
| Momentum Decay | ArmWrestleController | 2.0 |
| Round Time | GameManager | 45s |
| Power Surge Multiplier | AbilitySystem | 2.0x |
| Trash Talk Slowdown | AbilitySystem | 0.75 (25% slow) |
| Dance Efficiency | AbilitySystem | 0.5 (50% penalty) |
| Per-character cooldowns | CharacterData assets | Varies |
| Per-character durations | CharacterData assets | Varies |

### Bug Fixes
- [ ] Fix any issues found during playtesting
- [ ] Verify all state transitions work cleanly
- [ ] Check for modifier stacking bugs (multiple abilities active)
- [ ] Ensure `CancelInvoke` in `ResetAllModifiers` doesn't cancel unrelated invokes

---

## Priority Order
1. **Phase 4** (UI) — needed to actually play the game properly
2. **Phase 6** (Audio) — set up folder structure early so Patrick can record voice lines in parallel
3. **Phase 5** (Pixel Art) — Patrick generates art in parallel, we integrate as available
4. **Phase 7** (Balance) — last, after everything is playable
