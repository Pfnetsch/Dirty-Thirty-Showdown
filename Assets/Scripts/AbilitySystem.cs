using UnityEngine;
using System;
using System.Collections;

namespace DirtyThirtyShowdown
{
    public class AbilitySystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ArmWrestleController armWrestleController;
        [SerializeField] private PlayerController player1Controller;
        [SerializeField] private PlayerController player2Controller;
        [SerializeField] private AudioSource sfxAudioSource;

        [Header("Ability Settings")]
        [SerializeField] private float powerSurgeMultiplier = 2f;
        [SerializeField] private float trashTalkSlowdown = 0.75f; // 25% slowdown
        [SerializeField] private float danceEfficiency = 0.5f; // Need 50% more mashing

        [Header("Visual Effects")]
        [SerializeField] private GameObject flashEffectPrefab;
        [SerializeField] private GameObject shieldEffectPrefab;
        [SerializeField] private GameObject cakeEffectPrefab;
        [SerializeField] private CanvasGroup screenFlashOverlay;

        // Events
        public event Action<AbilityType, int> OnAbilityActivated; // type, player
        public event Action<AbilityType, int> OnAbilityEnded; // type, player
        public event Action<int> OnShieldBlocked; // player who blocked

        private void Start()
        {
            if (armWrestleController == null)
                armWrestleController = FindFirstObjectByType<ArmWrestleController>();
        }

        /// <summary>
        /// Execute an ability for the specified player
        /// </summary>
        public void ExecuteAbility(AbilityType type, int playerNumber, float duration)
        {
            int targetPlayer = playerNumber == 1 ? 2 : 1;
            PlayerController targetController = targetPlayer == 1 ? player1Controller : player2Controller;
            PlayerController userController = playerNumber == 1 ? player1Controller : player2Controller;

            // Check if target has shield (except for Shield ability itself)
            if (type != AbilityType.Shield && IsOffensiveAbility(type))
            {
                if (targetController != null && targetController.TryConsumeShield())
                {
                    OnShieldBlocked?.Invoke(targetPlayer);
                    PlayShieldBlockEffect(targetPlayer);
                    return;
                }
            }

            OnAbilityActivated?.Invoke(type, playerNumber);

            switch (type)
            {
                case AbilityType.PowerSurge:
                    ExecutePowerSurge(playerNumber, duration);
                    break;
                case AbilityType.Flash:
                    ExecuteFlash(targetPlayer, duration);
                    break;
                case AbilityType.TrashTalk:
                    ExecuteTrashTalk(targetPlayer, duration, userController);
                    break;
                case AbilityType.Shield:
                    ExecuteShield(userController);
                    break;
                case AbilityType.WinkFlirt:
                    ExecuteWinkFlirt(targetPlayer, duration);
                    break;
                case AbilityType.Dance:
                    ExecuteDance(targetPlayer, duration);
                    break;
                case AbilityType.CakeToss:
                    ExecuteCakeToss(playerNumber, duration);
                    break;
                case AbilityType.FakeOut:
                    ExecuteFakeOut(duration);
                    break;
                case AbilityType.DivineSmash:
                    ExecuteDivineSmash(playerNumber);
                    break;
                case AbilityType.Flex:
                    ExecuteFlex(playerNumber, duration);
                    break;
            }
        }

        private bool IsOffensiveAbility(AbilityType type)
        {
            return type switch
            {
                AbilityType.Flash => true,
                AbilityType.TrashTalk => true,
                AbilityType.WinkFlirt => true,
                AbilityType.Dance => true,
                AbilityType.FakeOut => true,
                _ => false
            };
        }

        #region Eli Abilities

        private void ExecutePowerSurge(int playerNumber, float duration)
        {
            // Double mashing power for the user
            armWrestleController.SetMashMultiplier(playerNumber, powerSurgeMultiplier, duration);
            StartCoroutine(AbilityDurationCoroutine(AbilityType.PowerSurge, playerNumber, duration));
        }

        private void ExecuteFlash(int targetPlayer, float duration)
        {
            // Screen flash/shake effect
            StartCoroutine(FlashEffect(duration));
            StartCoroutine(AbilityDurationCoroutine(AbilityType.Flash, targetPlayer, duration));
        }

        private IEnumerator FlashEffect(float duration)
        {
            if (screenFlashOverlay != null)
            {
                screenFlashOverlay.alpha = 1f;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    screenFlashOverlay.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                    yield return null;
                }

                screenFlashOverlay.alpha = 0f;
            }
        }

        #endregion

        #region Lene Abilities

        private void ExecuteTrashTalk(int targetPlayer, float duration, PlayerController user)
        {
            // Slowdown opponent's mashing effectiveness
            armWrestleController.SetMashMultiplier(targetPlayer, trashTalkSlowdown, duration);

            // Play trash talk voice line
            if (user != null && user.Character != null)
            {
                AudioClip trashTalkClip = user.Character.GetRandomVoiceLine(VoiceLineCategory.TrashTalk);
                if (trashTalkClip != null && sfxAudioSource != null)
                {
                    sfxAudioSource.PlayOneShot(trashTalkClip);
                }
            }

            StartCoroutine(AbilityDurationCoroutine(AbilityType.TrashTalk, targetPlayer, duration));
        }

        private void ExecuteShield(PlayerController user)
        {
            if (user != null)
            {
                user.ActivateShield();
                // Shield doesn't have a duration - it stays until consumed or round ends
            }
        }

        private void PlayShieldBlockEffect(int player)
        {
            // Visual/audio feedback when shield blocks an ability
            Debug.Log($"Player {player}'s shield blocked an ability!");
            // Instantiate shield break effect, play sound, etc.
        }

        #endregion

        #region Nati Abilities

        private void ExecuteWinkFlirt(int targetPlayer, float duration)
        {
            // Completely disable opponent's input
            armWrestleController.SetInputDisabled(targetPlayer, true, duration);
            StartCoroutine(AbilityDurationCoroutine(AbilityType.WinkFlirt, targetPlayer, duration));
        }

        private void ExecuteDance(int targetPlayer, float duration)
        {
            // Opponent needs more mashing for same effect
            armWrestleController.SetMashMultiplier(targetPlayer, danceEfficiency, duration);
            StartCoroutine(AbilityDurationCoroutine(AbilityType.Dance, targetPlayer, duration));
        }

        #endregion

        #region Sabi Abilities

        private void ExecuteCakeToss(int playerNumber, float duration)
        {
            // Bar can only move toward the user (defensive lock)
            armWrestleController.SetBarLock(playerNumber, duration);
            StartCoroutine(AbilityDurationCoroutine(AbilityType.CakeToss, playerNumber, duration));
        }

        private void ExecuteFakeOut(float duration)
        {
            // Reverse controls/momentum for both players
            armWrestleController.SetControlsReversed(true, duration);
            StartCoroutine(AbilityDurationCoroutine(AbilityType.FakeOut, 0, duration));
        }

        #endregion

        #region Patz Abilities (Easter Egg)

        private void ExecuteDivineSmash(int playerNumber)
        {
            // Instantly slam the bar to win position
            float winPosition = playerNumber == 1 ? -1f : 1f;
            armWrestleController.ForceBarPosition(winPosition);

            if (ScreenShake.Instance != null)
                ScreenShake.Instance.Shake(0.5f, 0.3f);

            OnAbilityActivated?.Invoke(AbilityType.DivineSmash, playerNumber);
        }

        private void ExecuteFlex(int playerNumber, float duration)
        {
            // 10x mashing power — completely overpowered
            armWrestleController.SetMashMultiplier(playerNumber, 10f, duration);
            StartCoroutine(AbilityDurationCoroutine(AbilityType.Flex, playerNumber, duration));
        }

        #endregion

        private IEnumerator AbilityDurationCoroutine(AbilityType type, int player, float duration)
        {
            yield return new WaitForSeconds(duration);
            OnAbilityEnded?.Invoke(type, player);
        }
    }
}
