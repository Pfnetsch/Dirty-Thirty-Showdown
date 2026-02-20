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

        [Header("Idle Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite[] titleScreenSprites;
        [SerializeField] private float idleInterval = 15f;

        private int _lastSpriteIndex = -1;

        private void Start()
        {
            playButton?.onClick.AddListener(OnPlay);
            optionsButton?.onClick.AddListener(OnOptions);
            exitButton?.onClick.AddListener(OnExit);
            optionsCloseButton?.onClick.AddListener(CloseOptions);

            if (optionsPanel != null)
                optionsPanel.SetActive(false);

            if (titleScreenSprites != null && titleScreenSprites.Length > 0)
                StartCoroutine(IdleCycle());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && optionsPanel != null && optionsPanel.activeSelf)
                CloseOptions();
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
