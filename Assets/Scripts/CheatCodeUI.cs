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

        private void OpenPanel()
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

        private void Submit()
        {
            if (cheatInput == null || CheatManager.Instance == null) return;

            string input = cheatInput.text;
            bool success = CheatManager.Instance.TryCheatCode(input);

            StopAllCoroutines();
            StartCoroutine(ShowFeedback(success));

            if (success)
            {
                cheatInput.text = "";
            }
        }

        private IEnumerator ShowFeedback(bool success)
        {
            if (feedbackText == null) yield break;

            feedbackText.gameObject.SetActive(true);
            feedbackText.text = success ? "CHEAT ACTIVATED!" : "Invalid code.";
            feedbackText.color = success ? Color.green : Color.red;

            yield return new WaitForSeconds(feedbackDuration);

            feedbackText.gameObject.SetActive(false);

            if (success)
                ClosePanel();
        }
    }
}
