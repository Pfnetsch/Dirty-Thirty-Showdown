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

        [Header("Player HUD Roots (PlayerHUD prefab instances)")]
        [SerializeField] private Transform p1HUDRoot;
        [SerializeField] private Transform p2HUDRoot;

        // Auto-wired from prefab at runtime — do not set in Inspector
        private PlayerHUDRefs p1HUD;
        private PlayerHUDRefs p2HUD;

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
        [SerializeField] private GameObject controlsReversedIndicator;

        [Header("VFX Overlays (new abilities)")]
        [SerializeField] private GameObject p1TrashTalkOverlay;
        [SerializeField] private GameObject p2TrashTalkOverlay;

        // Cached subtitle text components inside each TrashTalk overlay (auto-found at Start)
        private TextMeshProUGUI p1TrashTalkText;
        private TextMeshProUGUI p2TrashTalkText;
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

        private struct PlayerHUDRefs
        {
            public Image portrait;
            public TextMeshProUGUI nameText;
            public Image ability1Cooldown;
            public Image ability2Cooldown;
            public TextMeshProUGUI ability1KeyText;
            public TextMeshProUGUI ability2KeyText;
            public TextMeshProUGUI ability1NameText;
            public TextMeshProUGUI ability2NameText;
            public GameObject ability1ReadyFlash;
            public GameObject ability2ReadyFlash;
            public GameObject shieldIndicator;
            public GameObject powerSurgeIndicator;
            public GameObject inputDisabledIndicator;
        }

        private void SetupHUDReferences()
        {
            if (p1HUDRoot != null) p1HUD = WireHUDRefs(p1HUDRoot, 1);
            if (p2HUDRoot != null) p2HUD = WireHUDRefs(p2HUDRoot, 2);
        }

        private PlayerHUDRefs WireHUDRefs(Transform root, int playerNum)
        {
            var refs = new PlayerHUDRefs();

            refs.portrait         = FindDeep<Image>(root, "Portrait");
            refs.nameText         = root.Find("NameText")?.GetComponent<TextMeshProUGUI>();

            Transform ab1 = root.Find("Ability1Group");
            if (ab1 != null)
            {
                refs.ability1NameText = ab1.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                refs.ability1KeyText  = FindDeep<TextMeshProUGUI>(ab1, "KeyText");
                refs.ability1Cooldown = FindDeep<Image>(ab1, "Cooldown");
            }

            Transform ab2 = root.Find("Ability2Group");
            if (ab2 != null)
            {
                refs.ability2NameText = ab2.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                refs.ability2KeyText  = FindDeep<TextMeshProUGUI>(ab2, "KeyText");
                refs.ability2Cooldown = FindDeep<Image>(ab2, "Cooldown");
            }

            refs.ability1ReadyFlash   = ab1?.Find("ReadyFlash")?.gameObject;
            refs.ability2ReadyFlash   = ab2?.Find("ReadyFlash")?.gameObject;
            refs.powerSurgeIndicator  = root.Find("PowerSurgeIndicator")?.gameObject;
            refs.shieldIndicator      = root.Find("ShieldIndicator")?.gameObject;
            refs.inputDisabledIndicator = root.Find("InputDisabledIndicator")?.gameObject;

            // Warn about anything that failed to wire
            if (refs.portrait == null)             Debug.LogWarning($"[UIManager] P{playerNum}HUD: Portrait not found");
            if (refs.nameText == null)             Debug.LogWarning($"[UIManager] P{playerNum}HUD: NameText not found");
            if (refs.ability1Cooldown == null)     Debug.LogWarning($"[UIManager] P{playerNum}HUD: Ability1Group/Cooldown not found");
            if (refs.ability2Cooldown == null)     Debug.LogWarning($"[UIManager] P{playerNum}HUD: Ability2Group/Cooldown not found");
            if (refs.ability1KeyText == null)      Debug.LogWarning($"[UIManager] P{playerNum}HUD: Ability1Group/KeyText not found");
            if (refs.ability2KeyText == null)      Debug.LogWarning($"[UIManager] P{playerNum}HUD: Ability2Group/KeyText not found");
            if (refs.powerSurgeIndicator == null)  Debug.LogWarning($"[UIManager] P{playerNum}HUD: PowerSurgeIndicator not found");
            if (refs.shieldIndicator == null)      Debug.LogWarning($"[UIManager] P{playerNum}HUD: ShieldIndicator not found");
            if (refs.inputDisabledIndicator == null) Debug.LogWarning($"[UIManager] P{playerNum}HUD: InputDisabledIndicator not found");

            return refs;
        }

        private static T FindDeep<T>(Transform root, string name) where T : Component
        {
            if (root.name == name)
            {
                var c = root.GetComponent<T>();
                if (c != null) return c;
            }
            for (int i = 0; i < root.childCount; i++)
            {
                var result = FindDeep<T>(root.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }

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
            SetupHUDReferences();

            if (p1TrashTalkOverlay != null)
                p1TrashTalkText = p1TrashTalkOverlay.GetComponentInChildren<TextMeshProUGUI>(true);
            if (p2TrashTalkOverlay != null)
                p2TrashTalkText = p2TrashTalkOverlay.GetComponentInChildren<TextMeshProUGUI>(true);
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
                player1Controller.OnAbilityReady   += (ability) => SetAbilityReady(1, ability, true);
                player1Controller.OnShieldActivated += () => SetShieldIndicator(1, true);
                player1Controller.OnShieldConsumed  += () => SetShieldIndicator(1, false);
            }

            if (player2Controller != null)
            {
                player2Controller.OnCooldownUpdate += (ability, remaining, total) => UpdateCooldown(2, ability, remaining, total);
                player2Controller.OnAbilityReady   += (ability) => SetAbilityReady(2, ability, true);
                player2Controller.OnShieldActivated += () => SetShieldIndicator(2, true);
                player2Controller.OnShieldConsumed  += () => SetShieldIndicator(2, false);
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
                matchEndInstructionsText.text = "SPACE / ENTER = Rematch\n\nESC / BACKSPACE = Character Select";
                matchEndInstructionsText.color = new Color(0xCE / 255f, 0xCE / 255f, 0xCE / 255f, 1f);

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
            ref PlayerHUDRefs hud = ref (playerNumber == 1 ? ref p1HUD : ref p2HUD);
            Image cooldownImage = ability == 1 ? hud.ability1Cooldown : hud.ability2Cooldown;
            if (cooldownImage != null)
                // Fill increases as ability recharges: 0 = just used, 1 = ready
                cooldownImage.fillAmount = remaining > 0 ? (1f - remaining / total) : 1f;
            // Hide ready flash while ability is on cooldown
            if (remaining > 0)
                SetAbilityReady(playerNumber, ability, false);
        }

        private void SetAbilityReady(int playerNumber, int ability, bool ready)
        {
            // Full radial ring is sufficient ready indicator — no flash overlay needed
        }

        private void SetShieldIndicator(int playerNumber, bool active)
        {
            ref PlayerHUDRefs hud = ref (playerNumber == 1 ? ref p1HUD : ref p2HUD);
            hud.shieldIndicator?.SetActive(active);
        }

        #endregion

        #region Character Setup

        public void SetupPlayerUI(int playerNumber, CharacterData character)
        {
            if (character == null) return;

            ref PlayerHUDRefs hud = ref (playerNumber == 1 ? ref p1HUD : ref p2HUD);
            string key1 = playerNumber == 1 ? "Q" : "O";
            string key2 = playerNumber == 1 ? "E" : "P";

            if (hud.portrait != null)        hud.portrait.sprite    = character.characterPortrait;
            if (hud.nameText != null)        hud.nameText.text       = character.characterName;
            if (hud.ability1KeyText != null) hud.ability1KeyText.text = key1;
            if (hud.ability2KeyText != null) hud.ability2KeyText.text = key2;
            if (hud.ability1NameText != null) hud.ability1NameText.text = character.ability1Name;
            if (hud.ability2NameText != null) hud.ability2NameText.text = character.ability2Name;

            // Abilities start ready at round start — show full rings and ready flash
            if (hud.ability1Cooldown != null) hud.ability1Cooldown.fillAmount = 1f;
            if (hud.ability2Cooldown != null) hud.ability2Cooldown.fillAmount = 1f;
            SetAbilityReady(playerNumber, 1, true);
            SetAbilityReady(playerNumber, 2, true);
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
                {
                    // Speech bubble is a quote FROM the user, so it appears on the user's side.
                    // player is the user here.
                    var userController = player == 1 ? player1Controller : player2Controller;
                    var subtitleText   = player == 1 ? p1TrashTalkText   : p2TrashTalkText;
                    if (subtitleText != null && userController != null && userController.Character != null)
                    {
                        var lines = userController.Character.trashTalkSubtitles;
                        if (lines != null && lines.Length > 0)
                            subtitleText.text = lines[UnityEngine.Random.Range(0, lines.Length)];
                    }
                    SetOverlay(player == 1 ? p1TrashTalkOverlay : p2TrashTalkOverlay, true);
                    break;
                }
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
                    // player is the TARGET here; user = (3 - player), so hide the user's overlay.
                    SetOverlay(player == 1 ? p2TrashTalkOverlay : p1TrashTalkOverlay, false);
                    if (player == 1 && p2TrashTalkText != null) p2TrashTalkText.text = "";
                    if (player == 2 && p1TrashTalkText != null) p1TrashTalkText.text = "";
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
            ref PlayerHUDRefs hud = ref (playerNumber == 1 ? ref p1HUD : ref p2HUD);
            hud.powerSurgeIndicator?.SetActive(show);
        }

        public void ShowInputDisabledEffect(int playerNumber, bool show)
        {
            ref PlayerHUDRefs hud = ref (playerNumber == 1 ? ref p1HUD : ref p2HUD);
            hud.inputDisabledIndicator?.SetActive(show);
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
