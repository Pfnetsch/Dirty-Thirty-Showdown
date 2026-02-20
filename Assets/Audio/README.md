# Audio File Guide

## Folder Structure

```
Audio/
  Music/              ← Looping background tracks
  SFX/
    UI/               ← Navigation, confirm, ping sounds
    Match/            ← Round start/end, match win, timer beep
    Abilities/        ← One SFX per ability activation/effect
  Voice/
    Eli/              ← Eli's recorded voice lines
    Lene/             ← Lene's recorded voice lines
    Nati/             ← Nati's recorded voice lines
    Sabi/             ← Sabi's recorded voice lines
```

## Naming Conventions

### SFX
| File | Description |
|------|-------------|
| `SFX/UI/ui_navigate.wav` | Character select navigate |
| `SFX/UI/ui_confirm.wav` | Character select confirm |
| `SFX/UI/mash_hit.wav` | Button mash hit (plays rapidly!) |
| `SFX/UI/cooldown_ready.wav` | Ability cooldown ping |
| `SFX/Match/round_start.wav` | Round start bell/horn |
| `SFX/Match/round_end.wav` | Round end buzzer |
| `SFX/Match/match_win.wav` | Match win fanfare |
| `SFX/Match/timer_warning.wav` | Last 10 seconds beep |
| `SFX/Abilities/power_surge.wav` | Eli — Power Surge |
| `SFX/Abilities/flash.wav` | Eli — Flash |
| `SFX/Abilities/trash_talk.wav` | Lene — Trash Talk |
| `SFX/Abilities/shield_activate.wav` | Lene — Shield on |
| `SFX/Abilities/shield_break.wav` | Lene — Shield broken |
| `SFX/Abilities/wink_flirt.wav` | Nati — Wink/Flirt |
| `SFX/Abilities/dance.wav` | Nati — Celebration Dance |
| `SFX/Abilities/cake_toss.wav` | Sabi — Cake Toss |
| `SFX/Abilities/fake_out.wav` | Sabi — Fake Out |

### Voice Lines
Format: `{character}_{category}_{number}.wav`

| Category | Example | Trigger |
|----------|---------|---------|
| `select` | `eli_select_01.wav` | Picked on character select |
| `ability1` | `lene_ability1_01.wav` | Uses ability 1 |
| `ability2` | `nati_ability2_02.wav` | Uses ability 2 |
| `victory` | `sabi_victory_01.wav` | Wins a round/match |
| `defeat` | `eli_defeat_01.wav` | Loses |
| `trashtalk` | `lene_trashtalk_03.wav` | Lene only — during Trash Talk ability |

### Recording Tips
- Format: WAV preferred (OGG or MP3 also fine — Unity handles all three)
- Mono, 44.1kHz
- 1–3 seconds max per clip
- Record in a quiet room
- Leave a tiny bit of silence at start/end

### Music
| File | Description |
|------|-------------|
| `Music/character_select.wav` | Character select screen (looping) |
| `Music/battle.wav` | Gameplay (looping) |
| `Music/victory.wav` | Match end (short, non-looping) |
