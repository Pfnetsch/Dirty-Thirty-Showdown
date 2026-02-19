using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DirtyThirtyShowdown
{
    public class CharacterSelectManager : MonoBehaviour
    {
        [Header("Character Data")]
        [SerializeField] private CharacterData[] availableCharacters;

        [Header("Easter Egg")]
        [SerializeField] private CharacterData patzCharacter;

        [Header("Player 1 Selection UI")]
        [SerializeField] private Image p1SelectedPortrait;
        [SerializeField] private TextMeshProUGUI p1SelectedName;
        [SerializeField] private TextMeshProUGUI p1Ability1Text;
        [SerializeField] private TextMeshProUGUI p1Ability2Text;
        [SerializeField] private Image p1ReadyIndicator;
        [SerializeField] private Color p1HighlightColor = new Color(0.2f, 0.6f, 1f);

        [Header("Player 2 Selection UI")]
        [SerializeField] private Image p2SelectedPortrait;
        [SerializeField] private TextMeshProUGUI p2SelectedName;
        [SerializeField] private TextMeshProUGUI p2Ability1Text;
        [SerializeField] private TextMeshProUGUI p2Ability2Text;
        [SerializeField] private Image p2ReadyIndicator;
        [SerializeField] private Color p2HighlightColor = new Color(1f, 0.4f, 0.4f);

        [Header("Character Grid")]
        [SerializeField] private Transform characterGridParent;
        [SerializeField] private GameObject characterButtonPrefab;

        [Header("Input")]
        [SerializeField] private KeyCode p1SelectKey = KeyCode.Space;
        [SerializeField] private KeyCode p2SelectKey = KeyCode.Return;
        [SerializeField] private KeyCode p1LeftKey = KeyCode.A;
        [SerializeField] private KeyCode p1RightKey = KeyCode.D;
        [SerializeField] private KeyCode p2LeftKey = KeyCode.LeftArrow;
        [SerializeField] private KeyCode p2RightKey = KeyCode.RightArrow;

        [Header("Start Match")]
        [SerializeField] private Button startMatchButton;
        [SerializeField] private TextMeshProUGUI instructionsText;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip confirmSound;

        // Selection state
        private int p1SelectionIndex = 0;
        private int p2SelectionIndex = 1;
        private bool p1Ready = false;
        private bool p2Ready = false;

        // Character button references
        private Button[] characterButtons;
        private Transform[] characterButtonHighlights;

        public event Action<CharacterData, CharacterData> OnBothPlayersReady;

        private void Start()
        {
            SetupCharacterGrid();
            UpdateSelectionUI();

            if (startMatchButton != null)
            {
                startMatchButton.onClick.AddListener(TryStartMatch);
                startMatchButton.interactable = false;
            }

            // Listen for state changes to auto-reset when returning to character select
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }

            // Listen for cheat code activation
            if (CheatManager.Instance != null)
            {
                CheatManager.Instance.OnPatzUnlocked += UnlockPatz;
                // Re-unlock if already unlocked from a previous visit to this scene
                if (CheatManager.Instance.PatzUnlocked)
                    UnlockPatz();
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
            if (CheatManager.Instance != null)
            {
                CheatManager.Instance.OnPatzUnlocked -= UnlockPatz;
            }
        }

        private void HandleStateChanged(GameState newState)
        {
            if (newState == GameState.CharacterSelect)
            {
                ResetSelection();
            }
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentState != GameState.CharacterSelect) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.Instance.GoToTitleScreen();
                return;
            }

            HandlePlayer1Input();
            HandlePlayer2Input();

            // When both ready, any confirm key starts the match
            if (p1Ready && p2Ready)
            {
                if (Input.GetKeyDown(p1SelectKey) || Input.GetKeyDown(p2SelectKey))
                {
                    TryStartMatch();
                }
            }
        }

        private void SetupCharacterGrid()
        {
            if (characterGridParent == null || availableCharacters == null)
                return;

            // If the grid is empty (no pre-placed buttons from the builder), instantiate from prefab
            if (characterGridParent.childCount == 0)
            {
                if (characterButtonPrefab == null) return;
                for (int i = 0; i < availableCharacters.Length; i++)
                    Instantiate(characterButtonPrefab, characterGridParent);
            }
            else
            {
                // Add any buttons missing beyond what was pre-placed (e.g. Patz unlocked at runtime)
                while (characterGridParent.childCount < availableCharacters.Length)
                {
                    if (characterButtonPrefab == null) break;
                    Instantiate(characterButtonPrefab, characterGridParent);
                }
            }

            characterButtons = new Button[availableCharacters.Length];
            characterButtonHighlights = new Transform[availableCharacters.Length];

            for (int i = 0; i < availableCharacters.Length; i++)
            {
                if (i >= characterGridParent.childCount) break;

                GameObject buttonObj = characterGridParent.GetChild(i).gameObject;
                characterButtons[i] = buttonObj.GetComponent<Button>();

                Image portrait = buttonObj.transform.Find("Portrait")?.GetComponent<Image>();
                if (portrait != null && availableCharacters[i].characterPortrait != null)
                    portrait.sprite = availableCharacters[i].characterPortrait;

                TextMeshProUGUI nameText = buttonObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = availableCharacters[i].characterName;

                Transform highlight = buttonObj.transform.Find("Highlight");
                characterButtonHighlights[i] = highlight;
                if (highlight != null)
                    highlight.gameObject.SetActive(false);
            }
        }

        #region Input Handling

        private void HandlePlayer1Input()
        {
            if (p1Ready) return;

            if (Input.GetKeyDown(p1LeftKey))
            {
                p1SelectionIndex = (p1SelectionIndex - 1 + availableCharacters.Length) % availableCharacters.Length;
                PlaySelectSound();
                UpdateSelectionUI();
            }
            else if (Input.GetKeyDown(p1RightKey))
            {
                p1SelectionIndex = (p1SelectionIndex + 1) % availableCharacters.Length;
                PlaySelectSound();
                UpdateSelectionUI();
            }
            else if (Input.GetKeyDown(p1SelectKey))
            {
                ConfirmPlayer1Selection();
            }
        }

        private void HandlePlayer2Input()
        {
            if (p2Ready) return;

            if (Input.GetKeyDown(p2LeftKey))
            {
                p2SelectionIndex = (p2SelectionIndex - 1 + availableCharacters.Length) % availableCharacters.Length;
                PlaySelectSound();
                UpdateSelectionUI();
            }
            else if (Input.GetKeyDown(p2RightKey))
            {
                p2SelectionIndex = (p2SelectionIndex + 1) % availableCharacters.Length;
                PlaySelectSound();
                UpdateSelectionUI();
            }
            else if (Input.GetKeyDown(p2SelectKey))
            {
                ConfirmPlayer2Selection();
            }
        }

        private void ConfirmPlayer1Selection()
        {
            p1Ready = true;
            PlayConfirmSound();
            PlayCharacterVoiceLine(availableCharacters[p1SelectionIndex]);
            UpdateSelectionUI();
            CheckBothReady();
        }

        private void ConfirmPlayer2Selection()
        {
            p2Ready = true;
            PlayConfirmSound();
            PlayCharacterVoiceLine(availableCharacters[p2SelectionIndex]);
            UpdateSelectionUI();
            CheckBothReady();
        }

        #endregion

        #region UI Updates

        private void UpdateSelectionUI()
        {
            // Update P1 selection display
            CharacterData p1Character = availableCharacters[p1SelectionIndex];
            if (p1SelectedPortrait != null && p1Character.characterPortrait != null)
                p1SelectedPortrait.sprite = p1Character.characterPortrait;
            if (p1SelectedName != null)
                p1SelectedName.text = p1Character.characterName;
            if (p1Ability1Text != null)
                p1Ability1Text.text = $"Q: {p1Character.ability1Name}";
            if (p1Ability2Text != null)
                p1Ability2Text.text = $"E: {p1Character.ability2Name}";
            if (p1ReadyIndicator != null)
            {
                p1ReadyIndicator.color = p1Ready ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.25f, 0.25f, 0.3f, 0.8f);
                var p1ReadyText = p1ReadyIndicator.GetComponentInChildren<TextMeshProUGUI>();
                if (p1ReadyText != null)
                    p1ReadyText.color = p1Ready ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }

            // Update P2 selection display
            CharacterData p2Character = availableCharacters[p2SelectionIndex];
            if (p2SelectedPortrait != null && p2Character.characterPortrait != null)
                p2SelectedPortrait.sprite = p2Character.characterPortrait;
            if (p2SelectedName != null)
                p2SelectedName.text = p2Character.characterName;
            if (p2Ability1Text != null)
                p2Ability1Text.text = $"O: {p2Character.ability1Name}";
            if (p2Ability2Text != null)
                p2Ability2Text.text = $"P: {p2Character.ability2Name}";
            if (p2ReadyIndicator != null)
            {
                p2ReadyIndicator.color = p2Ready ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.25f, 0.25f, 0.3f, 0.8f);
                var p2ReadyText = p2ReadyIndicator.GetComponentInChildren<TextMeshProUGUI>();
                if (p2ReadyText != null)
                    p2ReadyText.color = p2Ready ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }

            // Update character grid highlights (border-style)
            for (int i = 0; i < characterButtonHighlights.Length; i++)
            {
                if (characterButtonHighlights[i] == null) continue;

                bool isP1Selection = i == p1SelectionIndex;
                bool isP2Selection = i == p2SelectionIndex;

                if (isP1Selection || isP2Selection)
                {
                    characterButtonHighlights[i].gameObject.SetActive(true);
                    Color borderColor;
                    if (isP1Selection && isP2Selection)
                        borderColor = Color.Lerp(p1HighlightColor, p2HighlightColor, 0.5f);
                    else if (isP1Selection)
                        borderColor = p1HighlightColor;
                    else
                        borderColor = p2HighlightColor;

                    foreach (var img in characterButtonHighlights[i].GetComponentsInChildren<Image>())
                        img.color = borderColor;
                }
                else
                {
                    characterButtonHighlights[i].gameObject.SetActive(false);
                }
            }

            UpdateInstructions();
        }

        private void UpdateInstructions()
        {
            if (instructionsText == null) return;

            if (!p1Ready && !p2Ready)
            {
                instructionsText.text = "P1: A/D = navigate  |  SPACE = confirm  |  Mash SPACE to arm wrestle!\nP2: ←/→ = navigate  |  ENTER = confirm  |  Mash ENTER to arm wrestle!\nESC = back to title";
            }
            else if (!p1Ready)
            {
                instructionsText.text = "P1: A/D = navigate  |  SPACE = confirm  |  Mash SPACE to arm wrestle!\nP2: READY!";
            }
            else if (!p2Ready)
            {
                instructionsText.text = "P1: READY!\nP2: ←/→ = navigate  |  ENTER = confirm  |  Mash ENTER to arm wrestle!";
            }
            else
            {
                instructionsText.text = "BOTH PLAYERS READY!\nPress SPACE or ENTER to begin!";
            }
        }

        #endregion

        #region Match Start

        private void CheckBothReady()
        {
            if (startMatchButton != null)
            {
                startMatchButton.interactable = p1Ready && p2Ready;
            }
        }

        private void TryStartMatch()
        {
            if (!p1Ready || !p2Ready) return;

            CharacterData p1Character = availableCharacters[p1SelectionIndex];
            CharacterData p2Character = availableCharacters[p2SelectionIndex];

            GameManager.Instance?.SetCharacterSelections(p1Character, p2Character);
            GameManager.Instance?.StartMatch();

            OnBothPlayersReady?.Invoke(p1Character, p2Character);
        }

        private void UnlockPatz()
        {
            if (patzCharacter == null) return;

            // Check if already in the roster
            foreach (var c in availableCharacters)
            {
                if (c == patzCharacter) return;
            }

            // Append Patz to the available characters array
            var newArray = new CharacterData[availableCharacters.Length + 1];
            availableCharacters.CopyTo(newArray, 0);
            newArray[newArray.Length - 1] = patzCharacter;
            availableCharacters = newArray;

            // Auto-select Patz as P2's character and rebuild grid
            SetupCharacterGrid();
            p2SelectionIndex = availableCharacters.Length - 1;
            UpdateSelectionUI();

            Debug.Log("[CharacterSelectManager] Patz has entered the arena!");
        }

        public void ResetSelection()
        {
            p1Ready = false;
            p2Ready = false;
            p1SelectionIndex = 0;
            p2SelectionIndex = Mathf.Min(1, availableCharacters.Length - 1);
            UpdateSelectionUI();

            if (startMatchButton != null)
            {
                startMatchButton.interactable = false;
            }
        }

        #endregion

        #region Audio

        private void PlaySelectSound()
        {
            if (audioSource != null && selectSound != null)
            {
                audioSource.PlayOneShot(selectSound);
            }
        }

        private void PlayConfirmSound()
        {
            if (audioSource != null && confirmSound != null)
            {
                audioSource.PlayOneShot(confirmSound);
            }
        }

        private void PlayCharacterVoiceLine(CharacterData character)
        {
            if (audioSource != null && character != null)
            {
                AudioClip voiceLine = character.GetRandomVoiceLine(VoiceLineCategory.Select);
                if (voiceLine != null)
                {
                    audioSource.PlayOneShot(voiceLine);
                }
            }
        }

        #endregion
    }
}
