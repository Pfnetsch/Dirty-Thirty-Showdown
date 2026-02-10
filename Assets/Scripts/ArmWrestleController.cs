using UnityEngine;
using System;

namespace DirtyThirtyShowdown
{
    public class ArmWrestleController : MonoBehaviour
    {
        [Header("Bar Settings")]
        [SerializeField] private float barSensitivity = 0.1f;
        [SerializeField] private float barDamping = 0.95f;
        [SerializeField] private float winThreshold = 1.0f;
        [SerializeField] private float momentumDecay = 2f;

        [Header("Visual References")]
        [SerializeField] private RectTransform barIndicator;
        [SerializeField] private RectTransform barTrack;

        // Bar state: -1 = P1 wins, +1 = P2 wins, 0 = neutral
        public float BarPosition { get; private set; } = 0f;
        public float BarVelocity { get; private set; } = 0f;

        // Mashing state
        private float player1MashPower = 0f;
        private float player2MashPower = 0f;

        // Modifiers from abilities
        private float player1Multiplier = 1f;
        private float player2Multiplier = 1f;
        private bool player1InputDisabled = false;
        private bool player2InputDisabled = false;
        private bool barLocked = false;
        private int barLockedForPlayer = 0; // 1 = can't move toward P2, 2 = can't move toward P1
        private bool controlsReversed = false;

        // Events
        public event Action<float> OnBarPositionChanged;
        public event Action<int> OnPlayerWin; // 1 or 2

        private GameManager gameManager;

        private void Start()
        {
            gameManager = GameManager.Instance;
        }

        private void Update()
        {
            if (gameManager == null || !gameManager.IsPlaying) return;

            UpdateBarPhysics();
            CheckWinCondition();
            UpdateVisuals();
        }

        public void AddMashPower(int playerNumber, float power)
        {
            if (playerNumber == 1 && !player1InputDisabled)
            {
                float actualPower = controlsReversed ? -power : power;
                player1MashPower += actualPower * player1Multiplier;
            }
            else if (playerNumber == 2 && !player2InputDisabled)
            {
                float actualPower = controlsReversed ? -power : power;
                player2MashPower += actualPower * player2Multiplier;
            }
        }

        private void UpdateBarPhysics()
        {
            // Calculate net force (P1 pushes negative, P2 pushes positive)
            float netForce = (player2MashPower - player1MashPower) * barSensitivity;

            // Apply force to velocity
            BarVelocity += netForce * Time.deltaTime;

            // Apply damping
            BarVelocity *= Mathf.Pow(barDamping, Time.deltaTime * 60f);

            // Calculate new position
            float newPosition = BarPosition + BarVelocity * Time.deltaTime;

            // Handle bar lock (Cake Toss ability)
            if (barLocked)
            {
                if (barLockedForPlayer == 1)
                {
                    // Bar can only move toward P1 (negative) or stay
                    newPosition = Mathf.Min(newPosition, BarPosition);
                }
                else if (barLockedForPlayer == 2)
                {
                    // Bar can only move toward P2 (positive) or stay
                    newPosition = Mathf.Max(newPosition, BarPosition);
                }
            }

            // Clamp position
            BarPosition = Mathf.Clamp(newPosition, -winThreshold, winThreshold);

            // Reset mash power for next frame
            player1MashPower = Mathf.Lerp(player1MashPower, 0f, momentumDecay * Time.deltaTime);
            player2MashPower = Mathf.Lerp(player2MashPower, 0f, momentumDecay * Time.deltaTime);

            OnBarPositionChanged?.Invoke(BarPosition);
        }

        private void CheckWinCondition()
        {
            if (BarPosition <= -winThreshold)
            {
                OnPlayerWin?.Invoke(1);
                gameManager.EndRound(1);
            }
            else if (BarPosition >= winThreshold)
            {
                OnPlayerWin?.Invoke(2);
                gameManager.EndRound(2);
            }
        }

        private void UpdateVisuals()
        {
            if (barIndicator == null || barTrack == null) return;

            // Map bar position to visual position
            float trackWidth = barTrack.rect.width;
            float normalizedPosition = (BarPosition + winThreshold) / (2f * winThreshold);
            float xPosition = Mathf.Lerp(-trackWidth / 2f, trackWidth / 2f, normalizedPosition);

            barIndicator.anchoredPosition = new Vector2(xPosition, barIndicator.anchoredPosition.y);
        }

        public void ForceBarPosition(float position)
        {
            BarPosition = Mathf.Clamp(position, -winThreshold, winThreshold);
            BarVelocity = 0f;
            OnBarPositionChanged?.Invoke(BarPosition);
        }

        public void ResetBar()
        {
            BarPosition = 0f;
            BarVelocity = 0f;
            player1MashPower = 0f;
            player2MashPower = 0f;
            ResetAllModifiers();
            UpdateVisuals();
        }

        #region Ability Modifiers

        public void SetMashMultiplier(int playerNumber, float multiplier, float duration)
        {
            if (playerNumber == 1)
            {
                player1Multiplier = multiplier;
                Invoke(nameof(ResetPlayer1Multiplier), duration);
            }
            else
            {
                player2Multiplier = multiplier;
                Invoke(nameof(ResetPlayer2Multiplier), duration);
            }
        }

        public void SetInputDisabled(int playerNumber, bool disabled, float duration)
        {
            if (playerNumber == 1)
            {
                player1InputDisabled = disabled;
                if (disabled) Invoke(nameof(EnablePlayer1Input), duration);
            }
            else
            {
                player2InputDisabled = disabled;
                if (disabled) Invoke(nameof(EnablePlayer2Input), duration);
            }
        }

        public void SetBarLock(int protectedPlayer, float duration)
        {
            barLocked = true;
            barLockedForPlayer = protectedPlayer;
            Invoke(nameof(UnlockBar), duration);
        }

        public void SetControlsReversed(bool reversed, float duration)
        {
            controlsReversed = reversed;
            if (reversed) Invoke(nameof(ResetControlsReversed), duration);
        }

        private void ResetPlayer1Multiplier() => player1Multiplier = 1f;
        private void ResetPlayer2Multiplier() => player2Multiplier = 1f;
        private void EnablePlayer1Input() => player1InputDisabled = false;
        private void EnablePlayer2Input() => player2InputDisabled = false;
        private void UnlockBar() { barLocked = false; barLockedForPlayer = 0; }
        private void ResetControlsReversed() => controlsReversed = false;

        private void ResetAllModifiers()
        {
            player1Multiplier = 1f;
            player2Multiplier = 1f;
            player1InputDisabled = false;
            player2InputDisabled = false;
            barLocked = false;
            barLockedForPlayer = 0;
            controlsReversed = false;
            CancelInvoke();
        }

        #endregion
    }
}
