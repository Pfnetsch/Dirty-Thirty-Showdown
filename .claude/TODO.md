# Dirty Thirty Showdown - TODO

## Open Phases Overview
- [x] Phase 4: Match Flow UI — COMPLETE
- [ ] Phase 5: Visual Polish (Pixel Art)
- [ ] Phase 6: Audio Integration
- [ ] Phase 7: Testing & Balance

---

## Phase 4: Match Flow UI — COMPLETE ✓

### Character Select Screen ✓
- [x] Layout: Two player panels (left/right) with character grid in center
- [x] Wire up `CharacterSelectManager` to actual scene UI elements
- [x] Character button prefab: Portrait image, Name TMP text, Highlight border image
- [x] Builder pre-places 4 buttons in CharacterGrid (visible in editor, repositionable)

### Gameplay HUD ✓
- [x] Symmetric HUD: portraits, names, cooldowns, timer, round text, bar fills, score
- [x] Ability effect overlays: PowerSurge (yellow), ControlsReversed (purple), InputDisabled/Stunned (orange)
- [x] Screen flash overlay for Eli's Flash ability
- [x] `PlayerController.SetCharacter()` called on PreRound via UIManager
- [x] Ability events (OnAbilityActivated/OnAbilityEnded) wired to UIManager indicators

### Round Transition Panels ✓
- [x] Pre-Round panel: "Round X / Get Ready!" — 2s display
- [x] Round End panel: "{Winner} wins Round X!" — 2s display
- [x] Match End panel: "{Winner} WINS!" + victory background image per character

### Rematch / Return Flow ✓
- [x] Rematch button → `GameManager.RestartMatch()`
- [x] Character Select button → `GameManager.ReturnToCharacterSelect()`
- [x] Keyboard shortcuts: Space/Enter = rematch, Esc/Backspace = character select
- [x] CharacterSelectManager auto-resets when returning to character select state
- [x] Both-ready keyboard start: Space or Enter starts match when both confirmed

### Builder ✓
- [x] All panels built and wired by `UISceneBuilder` (menu: Dirty Thirty Showdown > Build Game UI)
- [x] AudioSources auto-wired to AudioManager (music/sfx/voice)
- [x] Victory sprites auto-loaded (import as Sprite 2D and UI first)

---

## Phase 5: Visual Polish (Pixel Art)

### Architecture: Modular Composable Sprites

The arm wrestling scene uses **modular per-character sprites** composited at runtime, NOT pre-rendered per-matchup combinations. This avoids combinatorial explosion (6+ matchups × multiple states) and makes adding characters easy.

**Scene Composition (layered back to front):**
1. **Background / table** (shared)
2. **Left-side character body** (Player 1)
3. **Left-side arm** (Player 1, position driven by bar value)
4. **Clasped hands** (shared, center, position driven by bar value)
5. **Right-side arm** (Player 2, position driven by bar value)
6. **Right-side character body** (Player 2)
7. **Ability VFX overlays** (per ability, on top)

**Bar-to-Sprite Mapping:**
- Bar position `-1` to `+1` (from `ArmWrestleController`) maps to arm positions
- 5 arm positions: far-winning, winning, neutral, losing, far-losing
- Expression/head swaps for straining, winning, using ability

---

### Pixel Art Assets Needed

#### Character Portraits (for UI panels)
- [x] Eli - portrait icon
- [x] Lene - portrait icon
- [x] Nati - portrait icon
- [x] Sabi - portrait icon
> **Status**: Patrick has these already

#### Character Select Icons (for grid buttons)
- [ ] Eli - selectable icon (can reuse portrait or create variant)
- [ ] Lene - selectable icon
- [ ] Nati - selectable icon
- [ ] Sabi - selectable icon

#### Arm Wrestling Body Sprites (per character, 128px)
Each character needs a body sprite (torso + head, seated/standing at table) for both sides.
The body stays mostly static — the arm and expression change.

**Eli:**
- [ ] Body sprite — left side (facing right, for P1 position)
- [ ] Body sprite — right side (facing left, for P2 position)
- [ ] Expression: neutral/idle
- [ ] Expression: straining/effort
- [ ] Expression: winning/confident
- [ ] Expression: losing/struggling
- [ ] Expression: using ability (power-up glow)

**Lene:**
- [ ] Body sprite — left side
- [ ] Body sprite — right side
- [ ] Expression: neutral/idle
- [ ] Expression: straining/effort
- [ ] Expression: winning/confident
- [ ] Expression: losing/struggling
- [ ] Expression: using ability (trash talking / shielded)

**Nati:**
- [ ] Body sprite — left side
- [ ] Body sprite — right side
- [ ] Expression: neutral/idle
- [ ] Expression: straining/effort
- [ ] Expression: winning/confident
- [ ] Expression: losing/struggling
- [ ] Expression: using ability (winking / dancing)

**Sabi:**
- [ ] Body sprite — left side
- [ ] Body sprite — right side
- [ ] Expression: neutral/idle
- [ ] Expression: straining/effort
- [ ] Expression: winning/confident
- [ ] Expression: losing/struggling
- [ ] Expression: using ability (throwing cake / faking out)

#### Arm Sprites (per character, connects to shared hand clasp)
Each character needs arm sprites in 5 positions matching the bar value.

**Per character (×4 characters = 20 arm sprites total):**
- [ ] Arm position: far-winning (bar near ±1.0 in their favor)
- [ ] Arm position: winning (bar ~±0.5 in their favor)
- [ ] Arm position: neutral (bar ~0)
- [ ] Arm position: losing (bar ~±0.5 against them)
- [ ] Arm position: far-losing (bar near ±1.0 against them)

**Progress:**
- [ ] Eli arms (5 positions)
- [ ] Lene arms (5 positions)
- [ ] Nati arms (5 positions)
- [ ] Sabi arms (5 positions)

#### Shared / Center Sprites
- [ ] Clasped hands — neutral position
- [ ] Clasped hands — tilted left (P1 winning)
- [ ] Clasped hands — tilted right (P2 winning)
- [ ] Arm wrestling table / surface
- [ ] Background / stage art

#### Victory / Defeat Poses (full character, for match end screen)
- [ ] Eli - victory celebration sprite
- [ ] Eli - defeat sprite
- [ ] Lene - victory celebration sprite
- [ ] Lene - defeat sprite
- [ ] Nati - victory celebration sprite
- [ ] Nati - defeat sprite
- [ ] Sabi - victory celebration sprite
- [ ] Sabi - defeat sprite

#### Ability Effect Sprites/Animations (overlays)
- [ ] Power Surge — sparkle/glow effect around arm (Eli)
- [ ] Flash — screen flash overlay (Eli) — simple white texture + CanvasGroup
- [ ] Trash Talk — speech bubble with text/symbols (Lene)
- [ ] Shield — protective bubble/aura around Lene (Lene)
- [ ] Shield Break — shatter effect when consumed (Lene)
- [ ] Wink/Flirt — hearts/sparkles floating (Nati)
- [ ] Dance — music notes floating (Nati)
- [ ] Cake Toss — cake projectile + splat on opponent (Sabi)
- [ ] Fake Out — confusion/reverse arrows over opponent (Sabi)

#### UI Elements
- [ ] Tug-of-war bar background
- [ ] Bar indicator/cursor
- [ ] Cooldown radial overlay
- [ ] Round win indicators (dots/stars)
- [ ] Title screen logo "Dirty Thirty Showdown"

### Sprite Specifications
- **Style**: Pixel art, vibrant colors, readable at party distance
- **Canvas size**: 128×128px for body sprites (character is ~60% of canvas height)
- **View**: Side view (arm wrestling viewed from the side)
- **PixelLab settings**: `single color black outline`, `basic shading`, `medium detail`
- **Arms/hands**: 64×64 or smaller, must align at connection points
- **Effects**: 32×32 to 64×64
- **Format**: PNG with transparency

### Generation Order
1. **Eli** (first — validate style and approach) ← CURRENT
2. **Lene** (match Eli's style)
3. **Nati** (match style)
4. **Sabi** (match style)
5. **Shared assets** (hands, table, background)
6. **VFX overlays** (ability effects)

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
- [x] Create `BalanceSimulator` script (Editor-only EditorWindow)
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
