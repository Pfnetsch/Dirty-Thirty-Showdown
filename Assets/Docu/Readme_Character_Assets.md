# Character ScriptableObject Setup Guide

## Quick Setup in Unity

### Step 1: Import Files
1. Copy all files from this folder structure into your Unity project
2. Unity will automatically detect the `.asset` files

### Step 2: Fix Script References
The `.asset` files reference the `CharacterData` script. You need to update the GUID:

1. In Unity, find your `CharacterData.cs` script
2. Right-click → Properties (or View in Explorer/Finder)
3. Open the `.meta` file next to `CharacterData.cs`
4. Copy the `guid:` value (looks like: `guid: 1234567890abcdef1234567890abcdef`)
5. Replace `CHARACTERDATA_GUID_HERE` in each `.asset` file with your actual GUID

**OR** (Easier Method):
1. Delete all 4 `.asset` files
2. In Unity Editor: Right-click in Project window
3. Create → Dirty Thirty Showdown → Character Data
4. Create 4 new assets and manually configure them using the values below

---

## Character Configurations

### 🎂 Eli - "Birthday Wish"
**Theme:** Aggressive Force  
**Color:** Gold (255, 215, 0)

**Ability 1: Power Surge**
- Type: `PowerSurge` (enum value 0)
- Cooldown: 15 seconds
- Duration: 3 seconds
- Effect: Mashing counts 2x

**Ability 2: Flash**
- Type: `Flash` (enum value 1)
- Cooldown: 10 seconds
- Duration: 0.5 seconds
- Effect: Screen flash/shake distraction

---

### 🎉 Lene - "Party Master"
**Theme:** Mind Games & Defense  
**Color:** Purple (138, 43, 226)

**Ability 1: Trash Talk**
- Type: `TrashTalk` (enum value 2)
- Cooldown: 16 seconds
- Duration: 2 seconds
- Effect: 25% opponent slowdown + speech bubble

**Ability 2: Shield**
- Type: `Shield` (enum value 3)
- Cooldown: 22 seconds
- Duration: 0 (passive until triggered)
- Effect: Block next ability

---

### 💃 Nati - "Dance Queen"
**Theme:** Distraction & Pressure  
**Color:** Hot Pink (255, 106, 180)

**Ability 1: Wink/Flirt**
- Type: `WinkFlirt` (enum value 4)
- Cooldown: 12 seconds
- Duration: 1.5 seconds
- Effect: Disable opponent input completely

**Ability 2: Celebration Dance**
- Type: `Dance` (enum value 5)
- Cooldown: 18 seconds
- Duration: 4 seconds
- Effect: Opponent needs 50% more mashing

---

### 🎂 Sabi - "Cake Boss"
**Theme:** Tactical Chaos  
**Color:** Orange (255, 165, 0)

**Ability 1: Cake Toss**
- Type: `CakeToss` (enum value 6)
- Cooldown: 20 seconds
- Duration: 2 seconds
- Effect: Bar can only move toward Sabi

**Ability 2: Fake Out**
- Type: `FakeOut` (enum value 7)
- Cooldown: 24 seconds
- Duration: 2 seconds
- Effect: Reverse controls/momentum

---

## AbilityType Enum Reference

```csharp
public enum AbilityType
{
    PowerSurge,    // 0 - Eli Ability 1
    Flash,         // 1 - Eli Ability 2
    TrashTalk,     // 2 - Lene Ability 1
    Shield,        // 3 - Lene Ability 2
    WinkFlirt,     // 4 - Nati Ability 1
    Dance,         // 5 - Nati Ability 2
    CakeToss,      // 6 - Sabi Ability 1
    FakeOut        // 7 - Sabi Ability 2
}
```

---

## Audio Setup (Future Phase)

When you're ready to add audio:

### Voice Lines to Record:
- **Select Lines** (when choosing character)
- **Ability 1 Lines** (when using first ability)
- **Ability 2 Lines** (when using second ability)
- **Victory Lines** (when winning match)
- **Defeat Lines** (when losing match)
- **Trash Talk Lines** (Lene only - appears in speech bubble)

### Recommended Voice Line Examples:

**Lene Trash Talk:**
- "Is that all you got?"
- "My grandma pushes harder!"
- "Getting tired already?"
- "You call that arm wrestling?"

**Nati Flirt/Wink:**
- *Playful giggle*
- "Like what you see?"
- "Eyes over here!"
- *Kiss sound effect*

---

## Testing Checklist

After setting up all 4 characters:

- [ ] All 4 character assets appear in Project window
- [ ] No missing script references (should show CharacterData)
- [ ] All ability types are correctly assigned
- [ ] All cooldowns match design document values
- [ ] Character colors are visually distinct
- [ ] Can assign characters to players in GameManager

---

## Next Steps

1. Create placeholder sprites for each character
2. Set up character select UI that displays these characters
3. Test ability cooldowns in gameplay
4. Record voice lines
5. Add particle effects for abilities
6. Polish animations

---

**Note:** The sprite, animator, and audio fields can be left empty for now. The game will work without them, they just won't have visual/audio feedback yet.
