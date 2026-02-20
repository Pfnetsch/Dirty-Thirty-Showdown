using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DirtyThirtyShowdown
{
    /// <summary>
    /// Fades the logo in, holds it, then fades out and transitions to the Title Screen.
    /// Attach to the SplashScreenPanel GameObject alongside a CanvasGroup component.
    /// </summary>
    public class SplashScreenManager : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float holdDuration = 2f;
        [SerializeField] private float fadeOutDuration = 1f;

        [Header("Optional — skip with any key")]
        [SerializeField] private bool allowSkip = true;

        private CanvasGroup canvasGroup;
        private bool skipped = false;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            skipped = false;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            StartCoroutine(PlaySplash());
        }

        private void Update()
        {
            if (allowSkip && !skipped && Input.anyKeyDown)
            {
                skipped = true;
                StopAllCoroutines();
                StartCoroutine(SkipToTitleScreen());
            }
        }

        private IEnumerator PlaySplash()
        {
            // Fade in
            yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));

            // Hold
            float elapsed = 0f;
            while (elapsed < holdDuration && !skipped)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!skipped)
                yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));

            TransitionToTitleScreen();
        }

        private IEnumerator SkipToTitleScreen()
        {
            yield return StartCoroutine(Fade(canvasGroup.alpha, 0f, 0.3f));
            TransitionToTitleScreen();
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = to;
        }

        private void TransitionToTitleScreen()
        {
            GameManager.Instance?.GoToTitleScreen();
        }
    }
}
