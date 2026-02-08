using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace PartyArmWrestling
{
    public class CharacterSelectManager : MonoBehaviour
    {
        [Header("Character Data")]
        [SerializeField] private CharacterData[] availableCharacters;

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
        private Image[] characterButtonHighlights;

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
        }

        private void Update()
        {
            HandlePlayer1Input();
            HandlePlayer2Input();
        }

        private void SetupCharacterGrid()
        {
            if (characterGridParent == null || characterButtonPrefab == null || availableCharacters == null)
                return;

            // Clear existing buttons
            foreach (Transform child in characterGridParent)
            {
                Destroy(child.gameObject);
            }

            characterButtons = new Button[availableCharacters.Length];
            characterButtonHighlights = new Image[availableCharacters.Length];

            for (int i = 0; i < availableCharacters.Length; i++)
            {
                int index = i; // Capture for lambda
                GameObject buttonObj = Instantiate(characterButtonPrefab, characterGridParent);

                // Setup button
                Button button = buttonObj.GetComponent<Button>();
                characterButtons[i] = button;

                // Setup portrait image
                Image portrait = buttonObj.transform.Find("Portrait")?.GetComponent<Image>();
                if (portrait != null && availableCharacters[i].characterPortrait != null)
                {
                    portrait.sprite = availableCharacters[i].characterPortrait;
                }

                // Setup name text
                TextMeshProUGUI nameText = buttonObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = availableCharacters[i].characterName;
                }

                // Setup highlight for selection indication
                Image highlight = buttonObj.transform.Find("Highlight")?.GetComponent<Image>();
                characterButtonHighlights[i] = highlight;
                if (highlight != null)
                {
                    highlight.gameObject.SetActive(false);
                }
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
                p1ReadyIndicator.color = p1Ready ? Color.green : Color.gray;

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
                p2ReadyIndicator.color = p2Ready ? Color.green : Color.gray;

            // Update character grid highlights
            for (int i = 0; i < characterButtonHighlights.Length; i++)
            {
                if (characterButtonHighlights[i] == null) continue;

                bool isP1Selection = i == p1SelectionIndex;
                bool isP2Selection = i == p2SelectionIndex;

                if (isP1Selection && isP2Selection)
                {
                    characterButtonHighlights[i].gameObject.SetActive(true);
                    characterButtonHighlights[i].color = Color.Lerp(p1HighlightColor, p2HighlightColor, 0.5f);
                }
                else if (isP1Selection)
                {
                    characterButtonHighlights[i].gameObject.SetActive(true);
                    characterButtonHighlights[i].color = p1HighlightColor;
                }
                else if (isP2Selection)
                {
                    characterButtonHighlights[i].gameObject.SetActive(true);
                    characterButtonHighlights[i].color = p2HighlightColor;
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
                instructionsText.text = "P1: A/D to select, SPACE to confirm\nP2: Arrows to select, ENTER to confirm";
            }
            else if (!p1Ready)
            {
                instructionsText.text = "P1: A/D to select, SPACE to confirm\nP2: READY!";
            }
            else if (!p2Ready)
            {
                instructionsText.text = "P1: READY!\nP2: Arrows to select, ENTER to confirm";
            }
            else
            {
                instructionsText.text = "BOTH PLAYERS READY!\nPress START to begin!";
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
