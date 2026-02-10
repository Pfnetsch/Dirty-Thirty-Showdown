using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DirtyThirtyShowdown
{
    public class UIManager : MonoBehaviour
    {
        [Header("Bar Display")]
        [SerializeField] private RectTransform barIndicator;
        [SerializeField] private RectTransform barTrack;
        [SerializeField] private Image barFillLeft;
        [SerializeField] private Image barFillRight;
        [SerializeField] private Color player1Color = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color player2Color = new Color(1f, 0.4f, 0.4f);

        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI player1ScoreText;
        [SerializeField] private TextMeshProUGUI player2ScoreText;
        [SerializeField] private TextMeshProUGUI roundText;

        [Header("Timer")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerFill;

        [Header("Player 1 UI")]
        [SerializeField] private Image p1Portrait;
        [SerializeField] private TextMeshProUGUI p1NameText;
        [SerializeField] private Image p1Ability1Cooldown;
        [SerializeField] private Image p1Ability2Cooldown;
        [SerializeField] private TextMeshProUGUI p1Ability1KeyText;
        [SerializeField] private TextMeshProUGUI p1Ability2KeyText;
        [SerializeField] private GameObject p1ShieldIndicator;

        [Header("Player 2 UI")]
        [SerializeField] private Image p2Portrait;
        [SerializeField] private TextMeshProUGUI p2NameText;
        [SerializeField] private Image p2Ability1Cooldown;
        [SerializeField] private Image p2Ability2Cooldown;
        [SerializeField] private TextMeshProUGUI p2Ability1KeyText;
        [SerializeField] private TextMeshProUGUI p2Ability2KeyText;
        [SerializeField] private GameObject p2ShieldIndicator;

        [Header("State Panels")]
        [SerializeField] private GameObject characterSelectPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject roundStartPanel;
        [SerializeField] private GameObject roundEndPanel;
        [SerializeField] private GameObject matchEndPanel;

        [Header("Round Start/End")]
        [SerializeField] private TextMeshProUGUI roundStartText;
        [SerializeField] private TextMeshProUGUI roundEndText;
        [SerializeField] private TextMeshProUGUI matchWinnerText;

        [Header("Ability Effect Indicators")]
        [SerializeField] private GameObject p1PowerSurgeIndicator;
        [SerializeField] private GameObject p2PowerSurgeIndicator;
        [SerializeField] private GameObject controlsReversedIndicator;

        [Header("References")]
        [SerializeField] private PlayerController player1Controller;
        [SerializeField] private PlayerController player2Controller;
        [SerializeField] private ArmWrestleController armWrestleController;

        private GameManager gameManager;
        private float maxRoundTime;

        private void Start()
        {
            gameManager = GameManager.Instance;

            if (gameManager != null)
            {
                gameManager.OnStateChanged += HandleStateChanged;
                gameManager.OnRoundStart += HandleRoundStart;
                gameManager.OnRoundEnd += HandleRoundEnd;
                gameManager.OnMatchEnd += HandleMatchEnd;
                gameManager.OnTimerUpdate += UpdateTimer;
            }

            if (armWrestleController != null)
            {
                armWrestleController.OnBarPositionChanged += UpdateBarDisplay;
            }

            if (player1Controller != null)
            {
                player1Controller.OnCooldownUpdate += (ability, remaining, total) => UpdateCooldown(1, ability, remaining, total);
                player1Controller.OnShieldActivated += () => SetShieldIndicator(1, true);
                player1Controller.OnShieldConsumed += () => SetShieldIndicator(1, false);
            }

            if (player2Controller != null)
            {
                player2Controller.OnCooldownUpdate += (ability, remaining, total) => UpdateCooldown(2, ability, remaining, total);
                player2Controller.OnShieldActivated += () => SetShieldIndicator(2, true);
                player2Controller.OnShieldConsumed += () => SetShieldIndicator(2, false);
            }

            // Initialize UI
            SetAllPanelsInactive();
            if (characterSelectPanel != null)
                characterSelectPanel.SetActive(true);
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnStateChanged -= HandleStateChanged;
                gameManager.OnRoundStart -= HandleRoundStart;
                gameManager.OnRoundEnd -= HandleRoundEnd;
                gameManager.OnMatchEnd -= HandleMatchEnd;
                gameManager.OnTimerUpdate -= UpdateTimer;
            }

            if (armWrestleController != null)
            {
                armWrestleController.OnBarPositionChanged -= UpdateBarDisplay;
            }
        }

        #region State Handling

        private void HandleStateChanged(GameState newState)
        {
            SetAllPanelsInactive();

            switch (newState)
            {
                case GameState.CharacterSelect:
                    characterSelectPanel?.SetActive(true);
                    break;
                case GameState.PreRound:
                    gameplayPanel?.SetActive(true);
                    roundStartPanel?.SetActive(true);
                    UpdateRoundStartText();
                    break;
                case GameState.Playing:
                    gameplayPanel?.SetActive(true);
                    roundStartPanel?.SetActive(false);
                    break;
                case GameState.RoundEnd:
                    gameplayPanel?.SetActive(true);
                    roundEndPanel?.SetActive(true);
                    break;
                case GameState.MatchEnd:
                    matchEndPanel?.SetActive(true);
                    break;
            }

            UpdateScoreDisplay();
        }

        private void SetAllPanelsInactive()
        {
            characterSelectPanel?.SetActive(false);
            gameplayPanel?.SetActive(false);
            roundStartPanel?.SetActive(false);
            roundEndPanel?.SetActive(false);
            matchEndPanel?.SetActive(false);
        }

        #endregion

        #region Bar Display

        private void UpdateBarDisplay(float position)
        {
            if (barIndicator == null || barTrack == null) return;

            // Position is -1 to 1, map to bar position
            float normalizedPosition = (position + 1f) / 2f; // 0 to 1
            float trackWidth = barTrack.rect.width;
            float indicatorX = Mathf.Lerp(-trackWidth / 2f, trackWidth / 2f, normalizedPosition);

            barIndicator.anchoredPosition = new Vector2(indicatorX, barIndicator.anchoredPosition.y);

            // Update fill amounts
            if (barFillLeft != null)
                barFillLeft.fillAmount = (1f - normalizedPosition);
            if (barFillRight != null)
                barFillRight.fillAmount = normalizedPosition;
        }

        #endregion

        #region Score & Round Display

        private void UpdateScoreDisplay()
        {
            if (gameManager == null) return;

            if (player1ScoreText != null)
                player1ScoreText.text = gameManager.Player1Score.ToString();
            if (player2ScoreText != null)
                player2ScoreText.text = gameManager.Player2Score.ToString();
            if (roundText != null)
                roundText.text = $"Round {gameManager.CurrentRound}";
        }

        private void HandleRoundStart(int round)
        {
            maxRoundTime = gameManager.RoundTimer;
            UpdateScoreDisplay();
        }

        private void HandleRoundEnd(int winner, int round)
        {
            if (roundEndText != null)
            {
                string winnerName = winner == 1 ?
                    (gameManager.Player1Character?.characterName ?? "Player 1") :
                    (gameManager.Player2Character?.characterName ?? "Player 2");
                roundEndText.text = $"{winnerName} wins Round {round}!";
            }
            UpdateScoreDisplay();
        }

        private void HandleMatchEnd(int winner)
        {
            if (matchWinnerText != null)
            {
                string winnerName = winner == 1 ?
                    (gameManager.Player1Character?.characterName ?? "Player 1") :
                    (gameManager.Player2Character?.characterName ?? "Player 2");
                matchWinnerText.text = $"{winnerName} WINS!";
            }
        }

        private void UpdateRoundStartText()
        {
            if (roundStartText != null && gameManager != null)
            {
                roundStartText.text = $"Round {gameManager.CurrentRound}\nGet Ready!";
            }
        }

        #endregion

        #region Timer

        private void UpdateTimer(float timeRemaining)
        {
            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(timeRemaining);
                timerText.text = seconds.ToString();

                // Flash red when low
                if (seconds <= 10)
                {
                    timerText.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * 4f, 1f));
                }
                else
                {
                    timerText.color = Color.white;
                }
            }

            if (timerFill != null && maxRoundTime > 0)
            {
                timerFill.fillAmount = timeRemaining / maxRoundTime;
            }
        }

        #endregion

        #region Cooldowns

        private void UpdateCooldown(int playerNumber, int ability, float remaining, float total)
        {
            Image cooldownImage = null;

            if (playerNumber == 1)
            {
                cooldownImage = ability == 1 ? p1Ability1Cooldown : p1Ability2Cooldown;
            }
            else
            {
                cooldownImage = ability == 1 ? p2Ability1Cooldown : p2Ability2Cooldown;
            }

            if (cooldownImage != null)
            {
                cooldownImage.fillAmount = remaining > 0 ? remaining / total : 0f;
            }
        }

        private void SetShieldIndicator(int playerNumber, bool active)
        {
            GameObject indicator = playerNumber == 1 ? p1ShieldIndicator : p2ShieldIndicator;
            if (indicator != null)
            {
                indicator.SetActive(active);
            }
        }

        #endregion

        #region Character Setup

        public void SetupPlayerUI(int playerNumber, CharacterData character)
        {
            if (character == null) return;

            if (playerNumber == 1)
            {
                if (p1Portrait != null) p1Portrait.sprite = character.characterPortrait;
                if (p1NameText != null) p1NameText.text = character.characterName;
                if (p1Ability1KeyText != null) p1Ability1KeyText.text = "Q";
                if (p1Ability2KeyText != null) p1Ability2KeyText.text = "E";
            }
            else
            {
                if (p2Portrait != null) p2Portrait.sprite = character.characterPortrait;
                if (p2NameText != null) p2NameText.text = character.characterName;
                if (p2Ability1KeyText != null) p2Ability1KeyText.text = "O";
                if (p2Ability2KeyText != null) p2Ability2KeyText.text = "P";
            }
        }

        #endregion

        #region Ability Effects Display

        public void ShowPowerSurgeEffect(int playerNumber, bool show)
        {
            GameObject indicator = playerNumber == 1 ? p1PowerSurgeIndicator : p2PowerSurgeIndicator;
            if (indicator != null)
            {
                indicator.SetActive(show);
            }
        }

        public void ShowControlsReversed(bool show)
        {
            if (controlsReversedIndicator != null)
            {
                controlsReversedIndicator.SetActive(show);
            }
        }

        #endregion
    }
}
