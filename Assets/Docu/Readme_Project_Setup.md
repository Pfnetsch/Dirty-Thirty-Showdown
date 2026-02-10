# Dirty Thirty Showdown

A competitive 2-player arm wrestling party game built in Unity 6.

## Quick Start

### Setting Up in Unity

1. **Create a new Unity 6 project** (2D or 3D URP)
2. **Copy the Assets folder** into your Unity project
3. **Install TextMeshPro** if prompted (Window > TextMeshPro > Import TMP Essential Resources)
4. **Create Character Data Assets**:
   - In Unity, go to `Assets > Create > Dirty Thirty Showdown > All Characters`
   - This creates 4 character ScriptableObjects (Eli, Lene, Nati, Sabi)

### Scene Setup

1. Create a new scene or use the SampleScene
2. Add an empty GameObject and attach `GameBootstrap.cs`
3. Right-click the GameBootstrap component > "Create Minimal Test Setup"
4. For quick testing, add `QuickTestController.cs` to any GameObject

### Quick Test Controls

| Player 1 | Action |
|----------|--------|
| Space | Mash |
| Q | Ability 1 |
| E | Ability 2 |

| Player 2 | Action |
|----------|--------|
| Enter | Mash |
| O | Ability 1 |
| P | Ability 2 |

| Debug | Action |
|-------|--------|
| R | Reset bar |
| T | Toggle test match |

## Project Structure

```
Assets/
├── Scripts/
│   ├── GameManager.cs          # Round/match management
│   ├── ArmWrestleController.cs # Core bar physics
│   ├── PlayerController.cs     # Input handling
│   ├── CharacterData.cs        # ScriptableObject for characters
│   ├── AbilitySystem.cs        # All 8 ability implementations
│   ├── UIManager.cs            # UI management
│   ├── CharacterSelectManager.cs
│   ├── AudioManager.cs
│   ├── ScreenShake.cs
│   ├── GameBootstrap.cs        # Scene setup helper
│   ├── QuickTestController.cs  # Debug testing
│   └── CharacterDataCreator.cs # Editor utility
├── ScriptableObjects/
│   └── Characters/             # Character data assets
├── Prefabs/
├── Scenes/
├── Sprites/
├── Audio/
│   ├── Music/
│   ├── SFX/
│   └── VoiceLines/
├── Animations/
└── UI/
```

## Characters

### Eli - "Birthday Wish" (Aggressive)
- **Power Surge** (Q): 2x mashing power for 3s (15s cooldown)
- **Flash** (E): Screen flash distraction (10s cooldown)

### Lene - "Party Master" (Defensive)
- **Trash Talk** (Q): 25% slowdown + speech bubble for 2s (16s cooldown)
- **Shield** (E): Block next ability (22s cooldown)

### Nati - "Dance Queen" (Disruption)
- **Wink** (Q): Disable opponent input for 1.5s (12s cooldown)
- **Dance** (E): Opponent needs 50% more mashing for 4s (18s cooldown)

### Sabi - "Cake Boss" (Chaos)
- **Cake Toss** (Q): Bar can only move toward you for 2s (20s cooldown)
- **Fake Out** (E): Reverse controls for 2s (24s cooldown)

## Game Flow

1. Character Select
2. Best of 3 Rounds (45s each)
3. Button mash to push bar to opponent's side
4. Use abilities strategically
5. Match winner celebrated!

## Development Phases

- [x] Phase 1: Core Mechanics
- [x] Phase 2: Character System
- [x] Phase 3: Ability System
- [ ] Phase 4: Match Flow UI
- [ ] Phase 5: Visual Polish (Pixel Art)
- [ ] Phase 6: Audio Integration
- [ ] Phase 7: Testing & Balance

## Using with Claude Code

This project is set up to work well with Claude Code for iterative development:

```bash
# Navigate to your Unity project
cd /path/to/your/unity/project

# Copy the scripts
cp -r DirtyThirtyShowdown/Assets/Scripts/* Assets/Scripts/

# Open Unity and let it compile
```

Then use Claude Code to:
- Add new abilities
- Create UI layouts
- Implement animations
- Add audio systems
- Balance gameplay

## License

This is a private party game for birthday entertainment.
