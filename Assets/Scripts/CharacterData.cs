using UnityEngine;

namespace PartyArmWrestling
{
    public enum AbilityType
    {
        // Eli abilities
        PowerSurge,     // 2x mashing power for duration
        Flash,          // Screen flash/shake distraction

        // Lene abilities
        TrashTalk,      // Slowdown opponent + speech bubble
        Shield,         // Block next ability

        // Nati abilities
        WinkFlirt,      // Disable opponent input
        Dance,          // Opponent needs more mashing for same effect

        // Sabi abilities
        CakeToss,       // Bar can only move toward user
        FakeOut         // Reverse controls/momentum
    }

    [CreateAssetMenu(fileName = "New Character", menuName = "Party Arm Wrestling/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Basic Info")]
        public string characterName;
        [TextArea(2, 4)]
        public string description;

        [Header("Ability 1")]
        public AbilityType ability1Type;
        public string ability1Name;
        [TextArea(2, 3)]
        public string ability1Description;
        public float ability1Cooldown = 15f;
        public float ability1Duration = 3f;

        [Header("Ability 2")]
        public AbilityType ability2Type;
        public string ability2Name;
        [TextArea(2, 3)]
        public string ability2Description;
        public float ability2Cooldown = 15f;
        public float ability2Duration = 2f;

        [Header("Visuals")]
        public Sprite characterPortrait;
        public Sprite characterSprite;
        public Color characterColor = Color.white;
        public RuntimeAnimatorController characterAnimator;

        [Header("Audio - Voice Lines")]
        public AudioClip[] selectVoiceLines;
        public AudioClip[] ability1VoiceLines;
        public AudioClip[] ability2VoiceLines;
        public AudioClip[] victoryVoiceLines;
        public AudioClip[] defeatVoiceLines;
        public AudioClip[] trashTalkLines; // For Lene specifically

        [Header("Audio - SFX")]
        public AudioClip ability1SFX;
        public AudioClip ability2SFX;

        /// <summary>
        /// Gets a random voice line from the specified category
        /// </summary>
        public AudioClip GetRandomVoiceLine(VoiceLineCategory category)
        {
            AudioClip[] clips = category switch
            {
                VoiceLineCategory.Select => selectVoiceLines,
                VoiceLineCategory.Ability1 => ability1VoiceLines,
                VoiceLineCategory.Ability2 => ability2VoiceLines,
                VoiceLineCategory.Victory => victoryVoiceLines,
                VoiceLineCategory.Defeat => defeatVoiceLines,
                VoiceLineCategory.TrashTalk => trashTalkLines,
                _ => null
            };

            if (clips == null || clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        }
    }

    public enum VoiceLineCategory
    {
        Select,
        Ability1,
        Ability2,
        Victory,
        Defeat,
        TrashTalk
    }
}
