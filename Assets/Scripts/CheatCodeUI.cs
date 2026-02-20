using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace DirtyThirtyShowdown
{
    public class CheatCodeUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject cheatPanel;
        [SerializeField] private Button openButton;

        [Header("Input")]
        [SerializeField] private TMP_InputField cheatInput;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button closeButton;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private float feedbackDuration = 2f;

        private void Start()
        {
            if (cheatPanel != null)
                cheatPanel.SetActive(false);

            if (openButton != null)
                openButton.onClick.AddListener(OpenPanel);

            if (submitButton != null)
                submitButton.onClick.AddListener(Submit);

            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);

            if (cheatInput != null)
                cheatInput.onSubmit.AddListener(_ => Submit());

            if (feedbackText != null)
                feedbackText.gameObject.SetActive(false);
        }

        public void OpenPanel()
        {
            if (cheatPanel != null)
            {
                cheatPanel.SetActive(true);
                if (cheatInput != null)
                {
                    cheatInput.text = "";
                    cheatInput.ActivateInputField();
                }
            }
        }

        public void ClosePanel()
        {
            if (cheatPanel != null)
                cheatPanel.SetActive(false);
        }

        public bool IsPanelOpen => cheatPanel != null && cheatPanel.activeSelf;

        private void Update()
        {
            if (IsPanelOpen && Input.GetKeyDown(KeyCode.Escape))
                ClosePanel();
        }

        private void Submit()
        {
            if (cheatInput == null || CheatManager.Instance == null) return;

            string input = cheatInput.text;
            bool success = CheatManager.Instance.TryCheatCode(input);

            if (success)
            {
                cheatInput.text = "";
                StartCoroutine(ClosePanelNextFrame());
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(ShowFeedback());
            }
        }

        private IEnumerator ClosePanelNextFrame()
        {
            yield return null;
            ClosePanel();
        }

        private IEnumerator ShowFeedback()
        {
            if (feedbackText == null) yield break;

            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Invalid code.";
            feedbackText.color = Color.red;

            yield return new WaitForSeconds(feedbackDuration);

            feedbackText.gameObject.SetActive(false);
        }
    }
}
