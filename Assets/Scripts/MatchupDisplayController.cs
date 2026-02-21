using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace DirtyThirtyShowdown
{
    /// <summary>
    /// Drives the central arm-wrestling image by mapping ArmWrestleController.BarPosition
    /// to the correct pre-rendered matchup sprite.
    ///
    /// Bar convention:  -1 = P1 wins,  0 = neutral,  +1 = P2 wins.
    ///
    /// The asset stores sprites from char1's perspective (char1 winning = large negative bar
    /// when char1 is P1, or large positive bar when char1 is P2).  This class handles both
    /// cases automatically by checking which player char1 maps to.
    /// </summary>
    public class MatchupDisplayController : MonoBehaviour
    {
        [SerializeField] private Image displayImage;
        [SerializeField] private ArmWrestleController armWrestleController;
        [SerializeField] private AbilitySystem abilitySystem;
        [SerializeField] private List<MatchupSprites> allMatchups = new();

        [Header("Thresholds (fraction of ±1 bar range)")]
        [Tooltip("Bar fraction at which the 'dominating' sprite activates (e.g. 0.6 = 60% pushed)")]
        [SerializeField] private float dominatingThreshold = 0.6f;

        [Tooltip("Bar fraction at which the 'winning' sprite activates (ignored when sprite is null)")]
        [SerializeField] private float winningThreshold = 0.25f;

        private MatchupSprites currentMatchup;
        private bool char1IsP1;   // orientation flag — set when matchup is selected
        private Sprite lastSprite;
        private bool flexActive;
        private bool flashingActive;

        private void Start()
        {
            if (displayImage != null)
                displayImage.preserveAspect = false;

            if (abilitySystem == null)
                abilitySystem = FindFirstObjectByType<AbilitySystem>();

            if (abilitySystem != null)
            {
                abilitySystem.OnAbilityActivated += OnAbilityActivated;
                abilitySystem.OnAbilityEnded     += OnAbilityEnded;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnStateChanged;

                // The GameplayPanel is inactive until UIManager enables it, so Start() fires
                // after the first PreRound event has already been sent. Catch up here.
                var state = GameManager.Instance.CurrentState;
                if (state == GameState.PreRound || state == GameState.Playing)
                {
                    SetupForCurrentMatchup();
                    return;
                }
            }

            if (displayImage != null)
                displayImage.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnStateChanged;

            if (abilitySystem != null)
            {
                abilitySystem.OnAbilityActivated -= OnAbilityActivated;
                abilitySystem.OnAbilityEnded     -= OnAbilityEnded;
            }
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.PreRound)
                SetupForCurrentMatchup();
            else if (state == GameState.CharacterSelect || state == GameState.TitleScreen)
                HideDisplay();
        }

        private void SetupForCurrentMatchup()
        {
            var gm = GameManager.Instance;
            if (gm?.Player1Character == null || gm?.Player2Character == null)
            {
                HideDisplay();
                return;
            }

            var p1 = gm.Player1Character;
            var p2 = gm.Player2Character;

            // First pass: prefer the asset where char1 == P1 (correct visual orientation).
            // Second pass: fall back to the reversed asset if no direct match exists.
            currentMatchup = null;
            MatchupSprites reversedFallback = null;
            foreach (var m in allMatchups)
            {
                if (m == null || m.character1 == null || m.character2 == null) continue;

                if (m.character1 == p1 && m.character2 == p2)
                {
                    currentMatchup = m;
                    char1IsP1 = true;
                    break;
                }
                if (reversedFallback == null && m.character1 == p2 && m.character2 == p1)
                    reversedFallback = m;
            }

            if (currentMatchup == null && reversedFallback != null)
            {
                currentMatchup = reversedFallback;
                char1IsP1 = false;
            }

            if (currentMatchup == null || currentMatchup.neutral == null)
            {
                HideDisplay();
                return;
            }

            if (displayImage != null)
                displayImage.gameObject.SetActive(true);

            lastSprite = null;
            RefreshSprite(0f);
        }

        private void Update()
        {
            if (currentMatchup == null || armWrestleController == null) return;
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

            RefreshSprite(armWrestleController.BarPosition);
        }

        private void RefreshSprite(float barPos)
        {
            if (displayImage == null || currentMatchup == null) return;

            // Flash ability override — show the flashing sprite for 1s before the white screen flash
            if (flashingActive && currentMatchup.char1FlashingSprite != null)
            {
                if (currentMatchup.char1FlashingSprite == lastSprite) return;
                displayImage.sprite = currentMatchup.char1FlashingSprite;
                lastSprite = currentMatchup.char1FlashingSprite;
                return;
            }

            // Flex ability override — show the flexing sprite while active
            if (flexActive && currentMatchup.char1FlexingSprite != null)
            {
                Sprite flexSprite = currentMatchup.char1FlexingSprite;
                if (flexSprite == lastSprite) return;
                displayImage.sprite = flexSprite;
                lastSprite = flexSprite;
                return;
            }

            // Convert bar position to char1's winning direction.
            // bar negative = P1 winning.
            // If char1 is P1: char1 winning when bar is negative → invert sign so positive = char1 winning.
            // If char1 is P2: char1 winning when bar is positive → keep sign.
            float p = char1IsP1 ? -barPos : barPos;

            Sprite next;
            if (p >= dominatingThreshold)
                next = currentMatchup.char1Dominating != null ? currentMatchup.char1Dominating : currentMatchup.neutral;
            else if (p >= winningThreshold && currentMatchup.char1Winning != null)
                next = currentMatchup.char1Winning;
            else if (p <= -dominatingThreshold)
                next = currentMatchup.char2Dominating != null ? currentMatchup.char2Dominating : currentMatchup.neutral;
            else if (p <= -winningThreshold && currentMatchup.char2Winning != null)
                next = currentMatchup.char2Winning;
            else
                next = currentMatchup.neutral;

            if (next == lastSprite) return;
            displayImage.sprite = next;
            lastSprite = next;
        }

        private void HideDisplay()
        {
            currentMatchup = null;
            flexActive = false;
            flashingActive = false;
            StopAllCoroutines();
            if (displayImage != null)
                displayImage.gameObject.SetActive(false);
        }

        private void OnAbilityActivated(AbilityType type, int player)
        {
            if (type == AbilityType.Flex)
            {
                flexActive = true;
                lastSprite = null;
            }
            else if (type == AbilityType.Flash && currentMatchup != null)
            {
                // Show the flashing image only when the user is the char1 of the current asset
                // (the flashing sprite is authored from char1's perspective)
                bool userIsChar1 = (char1IsP1 && player == 1) || (!char1IsP1 && player == 2);
                if (userIsChar1 && currentMatchup.char1FlashingSprite != null)
                {
                    flashingActive = true;
                    lastSprite = null;
                    StartCoroutine(ClearFlashingAfter(1f));
                }
            }
        }

        private IEnumerator ClearFlashingAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            flashingActive = false;
            lastSprite = null; // force return to bar-based sprite
        }

        private void OnAbilityEnded(AbilityType type, int player)
        {
            if (type == AbilityType.Flex)
            {
                flexActive = false;
                lastSprite = null;
            }
        }
    }
}
