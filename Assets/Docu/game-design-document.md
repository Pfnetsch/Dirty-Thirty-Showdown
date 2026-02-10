# Dirty Thirty Showdown - Design Document

## Game Concept

**Genre:** Party Game / Competitive Multiplayer  
**Platform:** PC (Unity)  
**Players:** 2 players per match  
**Target Audience:** Party entertainment, casual players

### Core Concept
A competitive arm wrestling game featuring 4 characters (birthday celebrants: Eli, Lene, Nati, Sabi) where players button-mash to overpower their opponent while strategically using unique character abilities to gain advantage.

---

## Game Overview

### Core Mechanic
- Two players compete in arm wrestling matches by button-mashing
- A visual bar/indicator moves left or right based on mashing intensity
- First player to push the bar completely to their side wins the round
- Each character has 2 unique abilities to disrupt opponents or gain advantages
- Best of 3 rounds wins the match

### Design Pillars
1. **Instant Accessibility** - Anyone can learn in 10 seconds
2. **Spectator-Friendly** - Exciting to watch, clear visual feedback
3. **Strategic Depth** - Ability timing creates skill ceiling
4. **Party Entertainment** - Fun, chaotic, social experience

---

## Match Structure

### Flow
1. **Character Select Screen**
   - Player 1 selects character
   - Player 2 selects character
   
2. **Round 1**
   - 30-45 second timer (prevents stalemates)
   - Winner gets celebration/taunt animation
   
3. **Round 2**
   - If needed (score 1-0)
   
4. **Round 3**
   - If needed (score 1-1)
   - Tiebreaker round
   
5. **Victory Screen**
   - Winner celebration
   - Match statistics (optional)

### Win Conditions
- Push bar completely to your side = Round win
- Win 2 rounds = Match victory
- If timer expires: Player with bar closer to their side wins round

---

## Characters & Abilities

### Eli - "Birthday Wish" (Aggressive Force)

**Ability 1: Power Surge**
- Effect: Eli's button mashing counts 2x for 3 seconds
- Cooldown: 15 seconds
- Visual: Sparkles/stars around character
- Strategy: Use when opponent is vulnerable or pushing hard

**Ability 2: Flash**
- Effect: Screen flash/shake for opponent (brief distraction)
- Cooldown: 10 seconds
- Visual: Bright flash, screen shake effect
- Strategy: Quick disruption, breaks opponent's rhythm

**Playstyle:** Overwhelming offensive power, high aggression

---

### Lene - "Party Master" (Mind Games & Defense)

**Ability 1: Trash Talk**
- Effect: Speech bubble appears + opponent gets 25% slowdown for 2 seconds
- Cooldown: 16 seconds
- Visual: Speech bubble with trash talk text
- **Voice Line Integration:** Real voice lines like:
  - "Is that all you got?"
  - "My grandma pushes harder!"
  - "Getting tired already?"
- Strategy: Psychological warfare, sustained advantage

**Ability 2: Shield**
- Effect: Immune to next ability used against Lene
- Cooldown: 22 seconds
- Visual: Protective bubble/aura around character
- Strategy: Counter high-value abilities, defensive play

**Playstyle:** Tactical defense with psychological pressure

---

### Nati - "Dance Queen" (Distraction & Pressure)

**Ability 1: Wink/Flirt**
- Effect: Opponent's button becomes completely unresponsive for 1.5 seconds
- Cooldown: 12 seconds
- Visual: Wink animation, hearts/sparkles
- **Voice Line Integration:** Flirty/playful sounds
- Strategy: Create openings, clutch ability for comebacks

**Ability 2: Celebration Dance**
- Effect: Opponent needs 50% more button mashing to get same force for 4 seconds
- Cooldown: 18 seconds
- Visual: Music notes, dancing animation
- Strategy: Sustained pressure, wear down opponent

**Playstyle:** Charm-based disruption with endurance advantage

---

### Sabi - "Cake Boss" (Tactical Chaos)

**Ability 1: Cake Toss (Defensive Lock)**
- Effect: Bar cannot move TOWARD opponent's side, only back toward Sabi
- Duration: 2 seconds
- Cooldown: 20 seconds
- Visual: Cake splat on screen, bar locked indicator
- Strategy: Stop momentum when losing, defensive comeback tool

**Ability 2: Fake Out**
- Effect: Bar reverses direction briefly (momentum/controls reversed)
- Duration: 2 seconds
- Cooldown: 24 seconds
- Visual: Confusion effect, reverse arrows
- Strategy: Turn opponent's strength against them, chaos creation

**Playstyle:** Defensive and unpredictable, counter-play specialist

---

## Input Controls

### Player 1
- **Mash Button:** Space
- **Ability 1:** Q
- **Ability 2:** E

### Player 2
- **Mash Button:** Enter
- **Ability 1:** O
- **Ability 2:** P

### Gamepad Support (Future)
- **Mash:** A button (Xbox) / X button (PlayStation)
- **Ability 1:** Left Bumper
- **Ability 2:** Right Bumper

---

## Game Systems

### Bar/Arm Wrestling System
- Bar position ranges from -1.0 (P1 wins) to +1.0 (P2 wins)
- Neutral position: 0.0
- Mash detection counts button presses per frame
- Force applied to bar based on mash intensity
- Smooth momentum system (not instant snap)
- Visual feedback: Arms pushing, character strain animations

### Ability System
- Each ability has independent cooldown timer
- Cooldowns displayed as circular progress indicators
- Abilities can be queued but not stacked
- Some abilities affect bar physics, others affect opponent input
- Clear visual/audio feedback when abilities activate

### Cooldown Balance

**Short Cooldowns (10-12s):** 
- Flash (10s)
- Wink/Flirt (12s)

**Medium Cooldowns (15-18s):**
- Power Surge (15s)
- Trash Talk (16s)
- Celebration Dance (18s)

**Long Cooldowns (20-25s):**
- Cake Toss (20s)
- Shield (22s)
- Fake Out (24s)

---

## Visual & Audio Design

### Art Style
- **Pixel Art** characters (to be created/commissioned later)
- Clean, vibrant UI
- Clear visual feedback for all actions
- Particle effects for abilities

### Character Visuals
- Idle animations
- Pushing/straining animations
- Victory celebrations
- Taunt animations
- Ability-specific animations (wink, talking, dancing, etc.)

### Audio
- **Voice Lines** (Real voice recordings from Eli, Lene, Nati, Sabi)
  - Character select
  - Ability activation
  - Victory/defeat
  - Trash talk lines
- **SFX**
  - Button mash sounds
  - Ability activation sounds
  - Bar movement/strain sounds
  - Victory/defeat stings
- **Music**
  - Character select theme
  - Match music (builds tension)
  - Victory theme

---

## Technical Implementation

### Unity Script Structure

```
Scripts/
├── GameManager.cs
│   └── Handles: Round management, win conditions, scene flow
│
├── ArmWrestleController.cs
│   └── Handles: Bar position, mashing detection, physics
│
├── PlayerController.cs
│   └── Handles: Input detection, ability triggering, cooldown tracking
│
├── CharacterData.cs (ScriptableObject)
│   └── Stores: Character stats, ability types, cooldowns, sprites
│
├── AbilitySystem.cs
│   └── Handles: Ability effects, duration timers, ability interactions
│
└── UIManager.cs
    └── Handles: Bar visualization, cooldown indicators, round counter, score
```

### Key Data Structure

```csharp
public enum AbilityType {
    PowerSurge,      // Eli - Ability 1
    Flash,           // Eli - Ability 2
    TrashTalk,       // Lene - Ability 1
    Shield,          // Lene - Ability 2
    WinkFlirt,       // Nati - Ability 1
    Dance,           // Nati - Ability 2
    CakeToss,        // Sabi - Ability 1
    FakeOut          // Sabi - Ability 2
}

[CreateAssetMenu]
public class CharacterData : ScriptableObject {
    public string characterName;
    
    // Ability 1
    public AbilityType ability1Type;
    public float ability1Cooldown;
    public float ability1Duration;
    
    // Ability 2
    public AbilityType ability2Type;
    public float ability2Cooldown;
    public float ability2Duration;
    
    // Visuals
    public Sprite characterSprite;
    public RuntimeAnimatorController characterAnimator;
    
    // Audio
    public AudioClip[] voiceLines;
}
```

---

## Development Phases

### Phase 1: Core Mechanic (MVP)
- [ ] Basic bar movement system
- [ ] Button mashing detection
- [ ] Simple win condition
- [ ] 2 placeholder characters
- [ ] Basic UI

### Phase 2: Character System
- [ ] Character selection screen
- [ ] 4 character data assets (Eli, Lene, Nati, Sabi)
- [ ] Character-specific stats

### Phase 3: Ability System
- [ ] Ability infrastructure
- [ ] Implement all 8 abilities
- [ ] Cooldown system
- [ ] Visual feedback for abilities

### Phase 4: Match Flow
- [ ] Best of 3 rounds
- [ ] Round transitions
- [ ] Score tracking
- [ ] Victory screen

### Phase 5: Visual Polish
- [ ] Pixel art character sprites
- [ ] Character animations
- [ ] Particle effects
- [ ] UI polish

### Phase 6: Audio Integration
- [ ] Record voice lines
- [ ] Implement voice line system
- [ ] Add SFX
- [ ] Add music

### Phase 7: Testing & Balance
- [ ] Playtest at party
- [ ] Balance ability cooldowns
- [ ] Polish gameplay feel
- [ ] Bug fixes

---

## Future Enhancements (Post-Launch)

- **More Characters:** Additional birthday celebrants or guests
- **Tournament Mode:** 4-player bracket system
- **Power-ups:** Random items that appear during matches
- **Stage Variations:** Different backgrounds/themes
- **Combo System:** Reward consistent mashing rhythm
- **Online Multiplayer:** Remote play capability
- **Replay System:** Save and watch best moments
- **Custom Voice Lines:** Record new lines over time

---

## Pixel Art Animation Resources

### AI-Assisted Tools
- **Pixel It** - Converts images to pixel art
- **Aseprite** - Industry standard pixel art tool (recommended)
- **Piskel** - Free browser-based pixel art tool
- **PixelMe** - AI pixel art generator
- **Midjourney/DALL-E** → Convert to pixel art in post-processing

### Recommendation
Hand-crafted pixel art in **Aseprite** typically yields better results for game animations with consistent quality and proper frame-by-frame control.

---

## Success Metrics

### Party Entertainment Goals
- Players understand controls within 10 seconds
- Matches generate laughter and excitement
- Spectators are engaged watching
- Players want rematches
- All abilities feel distinct and useful

### Technical Goals
- Responsive input (< 16ms latency)
- Smooth 60 FPS performance
- No bugs during party
- Quick match times (2-4 minutes per match)

---

## Notes

- This game is specifically designed for a birthday party featuring Eli, Lene, Nati, and Sabi
- Voice lines will add significant personality and entertainment value
- Balance may need adjustment after playtesting
- Keep mechanics simple - complexity comes from ability timing, not controls
- Visual clarity is crucial for party environment (people may be standing far from screen)

---

**Document Version:** 1.0  
**Last Updated:** November 12, 2025  
**Status:** Design Phase
