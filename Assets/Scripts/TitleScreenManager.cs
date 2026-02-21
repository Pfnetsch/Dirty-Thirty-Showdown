using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace DirtyThirtyShowdown
{
    public class TitleScreenManager : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button exitButton;

        [Header("Options Panel")]
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private Button optionsCloseButton;

        [Header("Cheat Panel")]
        [SerializeField] private CheatCodeUI cheatCodeUI;

        [Header("Idle Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite[] titleScreenSprites;
        [SerializeField] private float idleInterval = 15f;

        private int _lastSpriteIndex = -1;
        private int _selectedIndex = 0;
        private Button[] _menuButtons;
        private Image[] _menuImages;
        private Color[] _menuOriginalColors;

        private void Start()
        {
            playButton?.onClick.AddListener(OnPlay);
            optionsButton?.onClick.AddListener(OnOptions);
            exitButton?.onClick.AddListener(OnExit);
            optionsCloseButton?.onClick.AddListener(CloseOptions);

            _menuButtons = new Button[] { playButton, optionsButton, exitButton };

            _menuImages = new Image[_menuButtons.Length];
            _menuOriginalColors = new Color[_menuButtons.Length];
            for (int i = 0; i < _menuButtons.Length; i++)
            {
                if (_menuButtons[i] == null) continue;
                _menuImages[i] = _menuButtons[i].GetComponent<Image>();
                if (_menuImages[i] != null)
                    _menuOriginalColors[i] = _menuImages[i].color;
            }

            if (optionsPanel != null)
                optionsPanel.SetActive(false);

            if (titleScreenSprites != null && titleScreenSprites.Length > 0)
                StartCoroutine(IdleCycle());

            SelectButton(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (cheatCodeUI != null && cheatCodeUI.IsPanelOpen)
                    cheatCodeUI.ClosePanel();
                else if (optionsPanel != null && optionsPanel.activeSelf)
                    CloseOptions();
                return;
            }

            // ^ key opens the cheat panel
            if (Input.GetKeyDown(KeyCode.Caret))
            {
                if (cheatCodeUI != null && !cheatCodeUI.IsPanelOpen)
                    cheatCodeUI.OpenPanel();
                return;
            }

            // Block menu nav while a sub-panel is open
            bool subPanelOpen = (optionsPanel != null && optionsPanel.activeSelf)
                             || (cheatCodeUI != null && cheatCodeUI.IsPanelOpen);
            if (subPanelOpen) return;

            // Arrow-key navigation
            if (Input.GetKeyDown(KeyCode.DownArrow))
                SelectButton((_selectedIndex + 1) % _menuButtons.Length);
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                SelectButton((_selectedIndex - 1 + _menuButtons.Length) % _menuButtons.Length);
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                _menuButtons[_selectedIndex]?.onClick.Invoke();
        }

        private void SelectButton(int index)
        {
            _selectedIndex = index;
            for (int i = 0; i < _menuButtons.Length; i++)
            {
                bool selected = i == index;
                if (_menuImages[i] != null)
                    _menuImages[i].color = selected ? _menuOriginalColors[i] : _menuOriginalColors[i] * 0.7f;
                if (_menuButtons[i] != null)
                    _menuButtons[i].transform.localScale = selected ? Vector3.one * 1.1f : Vector3.one;
            }
        }

        private IEnumerator IdleCycle()
        {
            while (true)
            {
                yield return new WaitForSeconds(idleInterval);

                if (backgroundImage == null || titleScreenSprites.Length < 2) yield break;

                // Pick a random index that differs from the last one
                int next;
                do { next = Random.Range(0, titleScreenSprites.Length); }
                while (next == _lastSpriteIndex);

                _lastSpriteIndex = next;
                backgroundImage.sprite = titleScreenSprites[next];
            }
        }

        private void OnPlay()
        {
            GameManager.Instance?.GoToCharacterSelect();
        }

        private void OnOptions()
        {
            if (optionsPanel != null)
                optionsPanel.SetActive(true);
        }

        private void CloseOptions()
        {
            if (optionsPanel != null)
                optionsPanel.SetActive(false);
        }

        private void OnExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
