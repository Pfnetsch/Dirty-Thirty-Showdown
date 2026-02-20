using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

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
        [SerializeField] private TextMeshProUGUI p1Ability1NameText;
        [SerializeField] private TextMeshProUGUI p1Ability2NameText;
        [SerializeField] private GameObject p1ShieldIndicator;

        [Header("Player 2 UI")]
        [SerializeField] private Image p2Portrait;
        [SerializeField] private TextMeshProUGUI p2NameText;
        [SerializeField] private Image p2Ability1Cooldown;
        [SerializeField] private Image p2Ability2Cooldown;
        [SerializeField] private TextMeshProUGUI p2Ability1KeyText;
        [SerializeField] private TextMeshProUGUI p2Ability2KeyText;
        [SerializeField] private TextMeshProUGUI p2Ability1NameText;
        [SerializeField] private TextMeshProUGUI p2Ability2NameText;
        [SerializeField] private GameObject p2ShieldIndicator;

        [Header("State Panels")]
        [SerializeField] private GameObject splashScreenPanel;
        [SerializeField] private GameObject titleScreenPanel;
        [SerializeField] private GameObject characterSelectPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject roundStartPanel;
        [SerializeField] private GameObject roundEndPanel;
        [SerializeField] private GameObject matchEndPanel;

        [Header("Round Start/End")]
        [SerializeField] private TextMeshProUGUI roundStartText;
        [SerializeField] private TextMeshProUGUI roundEndText;
        [SerializeField] private TextMeshProUGUI matchWinnerText;
        [SerializeField] private TextMeshProUGUI matchEndInstructionsText;

        [Header("Victory Backgrounds")]
        [SerializeField] private Image victoryBackgroundImage;
        [SerializeField] private Sprite eliVictorySprite;
        [SerializeField] private Sprite leneVictorySprite;
        [SerializeField] private Sprite natiVictorySprite;
        [SerializeField] private Sprite sabiVictorySprite;
        [SerializeField] private Sprite patzVictorySprite;

        [Header("Ability Effect Indicators")]
        [SerializeField] private GameObject p1PowerSurgeIndicator;
        [SerializeField] private GameObject p2PowerSurgeIndicator;
        [SerializeField] private GameObject p1InputDisabledIndicator;
        [SerializeField] private GameObject p2InputDisabledIndicator;
        [SerializeField] private GameObject controlsReversedIndicator;

        [Header("VFX Overlays (new abilities)")]
        [SerializeField] private GameObject p1TrashTalkOverlay;
        [SerializeField] private GameObject p2TrashTalkOverlay;
        [SerializeField] private GameObject p1DanceOverlay;
        [SerializeField] private GameObject p2DanceOverlay;
        [SerializeField] private GameObject p1CakeSplatOverlay;
        [SerializeField] private GameObject p2CakeSplatOverlay;

        [Header("References")]
        [SerializeField] private PlayerController player1Controller;
        [SerializeField] private PlayerController player2Controller;
        [SerializeField] private ArmWrestleController armWrestleController;
        [SerializeField] private AbilitySystem abilitySystem;

        private GameManager gameManager;
        private float maxRoundTime;
        private Coroutine countdownCoroutine;

        private void AutoFindPanels()
        {
            if (splashScreenPanel == null)    splashScreenPanel    = GameObject.Find("SplashScreenPanel");
            if (titleScreenPanel == null)     titleScreenPanel     = GameObject.Find("TitleScreenPanel");
            if (characterSelectPanel == null) characterSelectPanel = GameObject.Find("CharacterSelectPanel");
            if (gameplayPanel == null)        gameplayPanel        = GameObject.Find("GameplayPanel");
            if (roundStartPanel == null)      roundStartPanel      = GameObject.Find("RoundStartPanel");
            if (roundEndPanel == null)        roundEndPanel        = GameObject.Find("RoundEndPanel");
            if (matchEndPanel == null)        matchEndPanel        = GameObject.Find("MatchEndPanel");
        }

        private void Start()
        {
            AutoFindPanels();
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

            if (abilitySystem != null)
            {
                abilitySystem.OnAbilityActivated += HandleAbilityActivated;
                abilitySystem.OnAbilityEnded += HandleAbilityEnded;
            }

            // Initialize UI
            SetAllPanelsInactive();
            if (splashScreenPanel != null)
                splashScreenPanel.SetActive(true);
            else if (titleScreenPanel != null)
                titleScreenPanel.SetActive(true);
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

            if (abilitySystem != null)
            {
                abilitySystem.OnAbilityActivated -= HandleAbilityActivated;
                abilitySystem.OnAbilityEnded -= HandleAbilityEnded;
            }
        }

        #region State Handling

        private void HandleStateChanged(GameState newState)
        {
            SetAllPanelsInactive();

            switch (newState)
            {
                case GameState.SplashScreen:
                    if (splashScreenPanel != null) splashScreenPanel.SetActive(true);
                    break;
                case GameState.TitleScreen:
                    titleScreenPanel?.SetActive(true);
                    break;
                case GameState.CharacterSelect:
                    characterSelectPanel?.SetActive(true);
                    break;
                case GameState.PreRound:
                    gameplayPanel?.SetActive(true);
                    roundStartPanel?.SetActive(true);
                    HideAllAbilityOverlays();
                    if (gameManager.Player1Character != null)
                    {
                        SetupPlayerUI(1, gameManager.Player1Character);
                        player1Controller?.SetCharacter(gameManager.Player1Character);
                    }
                    if (gameManager.Player2Character != null)
                    {
                        SetupPlayerUI(2, gameManager.Player2Character);
                        player2Controller?.SetCharacter(gameManager.Player2Character);
                    }
                    if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
                    countdownCoroutine = StartCoroutine(CountdownCoroutine());
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
            if (splashScreenPanel != null) splashScreenPanel.SetActive(false);
            titleScreenPanel?.SetActive(false);
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
            CharacterData winnerCharacter = winner == 1 ? gameManager.Player1Character : gameManager.Player2Character;

            if (matchWinnerText != null)
            {
                string winnerName = winnerCharacter?.characterName ?? (winner == 1 ? "Player 1" : "Player 2");
                matchWinnerText.text = $"{winnerName} WINS!";
            }

            if (matchEndInstructionsText != null)
                matchEndInstructionsText.text = "SPACE / ENTER = Rematch\nESC / BACKSPACE = Character Select";

            if (victoryBackgroundImage != null && winnerCharacter != null)
            {
                Sprite victorySprite = GetVictorySprite(winnerCharacter.characterName);
                if (victorySprite != null)
                {
                    victoryBackgroundImage.sprite = victorySprite;
                    victoryBackgroundImage.gameObject.SetActive(true);
                }
            }
        }

        private Sprite GetVictorySprite(string characterName) => characterName switch
        {
            "Eli"  => eliVictorySprite,
            "Lene" => leneVictorySprite,
            "Nati" => natiVictorySprite,
            "Sabi" => sabiVictorySprite,
            "Patz" => patzVictorySprite,
            _      => null
        };

        private void UpdateRoundStartText()
        {
            if (roundStartText != null && gameManager != null)
            {
                roundStartText.text = $"Round {gameManager.CurrentRound}\nGet Ready!";
            }
        }

        private IEnumerator CountdownCoroutine()
        {
            if (roundStartText == null) yield break;

            roundStartText.text = $"Round {gameManager?.CurrentRound}\nGet Ready!";
            yield return new WaitForSeconds(0.5f);

            for (int i = 3; i >= 1; i--)
            {
                roundStartText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }

            roundStartText.text = "GO!";
            // Panel stays visible for ~1 more second before Playing state hides it
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
                if (p1Ability1NameText != null) p1Ability1NameText.text = character.ability1Name;
                if (p1Ability2NameText != null) p1Ability2NameText.text = character.ability2Name;
            }
            else
            {
                if (p2Portrait != null) p2Portrait.sprite = character.characterPortrait;
                if (p2NameText != null) p2NameText.text = character.characterName;
                if (p2Ability1KeyText != null) p2Ability1KeyText.text = "O";
                if (p2Ability2KeyText != null) p2Ability2KeyText.text = "P";
                if (p2Ability1NameText != null) p2Ability1NameText.text = character.ability1Name;
                if (p2Ability2NameText != null) p2Ability2NameText.text = character.ability2Name;
            }
        }

        #endregion

        #region Ability Effects Display

        private void HandleAbilityActivated(AbilityType type, int player)
        {
            // NOTE: player = the user who triggered the ability.
            // For offensive abilities, the target is (3 - player).
            switch (type)
            {
                case AbilityType.PowerSurge:
                case AbilityType.Flex:
                    ShowPowerSurgeEffect(player, true);
                    break;
                case AbilityType.Flash:
                case AbilityType.WinkFlirt:
                    ShowInputDisabledEffect(3 - player, true);
                    break;
                case AbilityType.TrashTalk:
                    SetOverlay(player == 1 ? p2TrashTalkOverlay : p1TrashTalkOverlay, true);
                    break;
                case AbilityType.Dance:
                    SetOverlay(player == 1 ? p2DanceOverlay : p1DanceOverlay, true);
                    break;
                case AbilityType.CakeToss:
                    SetOverlay(player == 1 ? p2CakeSplatOverlay : p1CakeSplatOverlay, true);
                    break;
                case AbilityType.FakeOut:
                    ShowControlsReversed(true);
                    break;
            }
        }

        private void HandleAbilityEnded(AbilityType type, int player)
        {
            // NOTE: for TrashTalk/Dance/WinkFlirt/Flash, player = the target (the affected one).
            // For PowerSurge/Flex/CakeToss/FakeOut, player = the user.
            switch (type)
            {
                case AbilityType.PowerSurge:
                case AbilityType.Flex:
                    ShowPowerSurgeEffect(player, false);
                    break;
                case AbilityType.Flash:
                case AbilityType.WinkFlirt:
                    ShowInputDisabledEffect(player, false);
                    break;
                case AbilityType.TrashTalk:
                    SetOverlay(player == 1 ? p1TrashTalkOverlay : p2TrashTalkOverlay, false);
                    break;
                case AbilityType.Dance:
                    SetOverlay(player == 1 ? p1DanceOverlay : p2DanceOverlay, false);
                    break;
                case AbilityType.CakeToss:
                    // player = user, so opponent is (3 - player)
                    SetOverlay(player == 1 ? p2CakeSplatOverlay : p1CakeSplatOverlay, false);
                    break;
                case AbilityType.FakeOut:
                    ShowControlsReversed(false);
                    break;
            }
        }

        public void ShowPowerSurgeEffect(int playerNumber, bool show)
        {
            GameObject indicator = playerNumber == 1 ? p1PowerSurgeIndicator : p2PowerSurgeIndicator;
            if (indicator != null) indicator.SetActive(show);
        }

        public void ShowInputDisabledEffect(int playerNumber, bool show)
        {
            GameObject indicator = playerNumber == 1 ? p1InputDisabledIndicator : p2InputDisabledIndicator;
            if (indicator != null) indicator.SetActive(show);
        }

        public void ShowControlsReversed(bool show)
        {
            if (controlsReversedIndicator != null) controlsReversedIndicator.SetActive(show);
        }

        private void SetOverlay(GameObject overlay, bool show)
        {
            if (overlay != null) overlay.SetActive(show);
        }

        private void HideAllAbilityOverlays()
        {
            ShowPowerSurgeEffect(1, false);
            ShowPowerSurgeEffect(2, false);
            ShowInputDisabledEffect(1, false);
            ShowInputDisabledEffect(2, false);
            ShowControlsReversed(false);
            SetOverlay(p1TrashTalkOverlay, false);
            SetOverlay(p2TrashTalkOverlay, false);
            SetOverlay(p1DanceOverlay, false);
            SetOverlay(p2DanceOverlay, false);
            SetOverlay(p1CakeSplatOverlay, false);
            SetOverlay(p2CakeSplatOverlay, false);
        }

        #endregion
    }
}
