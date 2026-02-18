using UnityEngine;
using UnityEngine.UI;

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

        private void Start()
        {
            playButton?.onClick.AddListener(OnPlay);
            optionsButton?.onClick.AddListener(OnOptions);
            exitButton?.onClick.AddListener(OnExit);
            optionsCloseButton?.onClick.AddListener(CloseOptions);

            if (optionsPanel != null)
                optionsPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && optionsPanel != null && optionsPanel.activeSelf)
                CloseOptions();
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
