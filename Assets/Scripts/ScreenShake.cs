using UnityEngine;
using System.Collections;

namespace DirtyThirtyShowdown
{
    public class ScreenShake : MonoBehaviour
    {
        public static ScreenShake Instance { get; private set; }

        [SerializeField] private float defaultDuration = 0.3f;
        [SerializeField] private float defaultMagnitude = 0.1f;
        [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        private Vector3 originalPosition;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            originalPosition = transform.localPosition;
        }

        public void Shake() => Shake(defaultDuration, defaultMagnitude);

        public void Shake(float duration, float magnitude)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                transform.localPosition = originalPosition;
            }
            shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                float curveValue = shakeCurve.Evaluate(progress);
                float x = Random.Range(-1f, 1f) * magnitude * curveValue;
                float y = Random.Range(-1f, 1f) * magnitude * curveValue;
                transform.localPosition = originalPosition + new Vector3(x, y, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = originalPosition;
            shakeCoroutine = null;
        }

        public void StopShake()
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }
            transform.localPosition = originalPosition;
        }
    }
}
