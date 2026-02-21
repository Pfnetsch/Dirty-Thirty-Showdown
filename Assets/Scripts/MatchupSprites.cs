using UnityEngine;

namespace DirtyThirtyShowdown
{
    /// <summary>
    /// Holds the pre-rendered arm-wrestling sprites for one character matchup.
    /// Sprites are ordered from char1 dominating on the left to char2 dominating on the right.
    /// char1/char2 must match the order used in the PNG filenames (e.g. eli_vs_lene → char1=Eli, char2=Lene).
    /// </summary>
    [CreateAssetMenu(fileName = "MatchupSprites", menuName = "Dirty Thirty Showdown/Matchup Sprites")]
    public class MatchupSprites : ScriptableObject
    {
        [Tooltip("The 'left' character in the image (first name in the filename)")]
        public CharacterData character1;

        [Tooltip("The 'right' character in the image (second name in the filename)")]
        public CharacterData character2;

        [Header("Sprites — left to right: char1 winning → neutral → char2 winning")]
        [Tooltip("char1 is close to winning (bar ~±0.65). Required when you have 5 states.")]
        public Sprite char1Dominating;

        [Tooltip("char1 is ahead but not dominating (bar ~±0.25). Optional — only for 5-state matchups.")]
        public Sprite char1Winning;

        [Tooltip("Bar is near centre. Always required.")]
        public Sprite neutral;

        [Tooltip("char2 is ahead but not dominating. Optional — only for 5-state matchups.")]
        public Sprite char2Winning;

        [Tooltip("char2 is close to winning. Required when you have 5 states.")]
        public Sprite char2Dominating;

        [Header("Ability Override Sprites")]
        [Tooltip("Shown while char1 uses their Flex ability. Assign for Patz matchups only.")]
        public Sprite char1FlexingSprite;

        [Tooltip("Shown for 1s when char1 uses their Flash ability (before the white screen flash). Assign for Eli matchups.")]
        public Sprite char1FlashingSprite;

        /// <summary>Returns true if this asset covers the given pair (in either order).</summary>
        public bool Matches(CharacterData a, CharacterData b)
        {
            return (character1 == a && character2 == b) ||
                   (character1 == b && character2 == a);
        }

        /// <summary>
        /// Returns true if char1 corresponds to the given player's character.
        /// Used to orient bar-direction correctly when characters are swapped.
        /// </summary>
        public bool Char1Is(CharacterData c) => character1 == c;
    }
}
