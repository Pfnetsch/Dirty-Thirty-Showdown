using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private List<MatchupSprites> allMatchups = new();

        [Header("Thresholds (fraction of ±1 bar range)")]
        [Tooltip("Bar fraction at which the 'dominating' sprite activates (e.g. 0.6 = 60% pushed)")]
        [SerializeField] private float dominatingThreshold = 0.6f;

        [Tooltip("Bar fraction at which the 'winning' sprite activates (ignored when sprite is null)")]
        [SerializeField] private float winningThreshold = 0.25f;

        private MatchupSprites currentMatchup;
        private bool char1IsP1;   // orientation flag — set when matchup is selected
        private Sprite lastSprite;

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnStateChanged;

            if (displayImage != null)
                displayImage.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnStateChanged;
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

            currentMatchup = null;
            foreach (var m in allMatchups)
            {
                if (m == null || m.character1 == null || m.character2 == null) continue;

                if (m.character1 == p1 && m.character2 == p2)
                {
                    currentMatchup = m;
                    char1IsP1 = true;
                    break;
                }
                if (m.character1 == p2 && m.character2 == p1)
                {
                    currentMatchup = m;
                    char1IsP1 = false;
                    break;
                }
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
            if (displayImage != null)
                displayImage.gameObject.SetActive(false);
        }
    }
}
