using UnityEngine;
using System;

namespace DirtyThirtyShowdown
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Settings")]
        [SerializeField] private int playerNumber = 1; // 1 or 2

        [Header("Input Keys - Player 1 Defaults")]
        [SerializeField] private KeyCode mashKey1 = KeyCode.A;
        [SerializeField] private KeyCode mashKey2 = KeyCode.D;
        [SerializeField] private KeyCode ability1Key = KeyCode.Q;
        [SerializeField] private KeyCode ability2Key = KeyCode.E;

        [Header("Mashing Settings")]
        [SerializeField] private float mashPowerPerPress = 1f;
        [SerializeField] private float maxMashRate = 20f; // Max presses per second
        [SerializeField] private float mashDecay = 5f;

        [Header("References")]
        [SerializeField] private ArmWrestleController armWrestleController;
        [SerializeField] private AbilitySystem abilitySystem;

        // Character data
        public CharacterData Character { get; private set; }

        // Cooldown state
        public float Ability1CooldownRemaining { get; private set; } = 0f;
        public float Ability2CooldownRemaining { get; private set; } = 0f;
        public bool Ability1Ready => Ability1CooldownRemaining <= 0f;
        public bool Ability2Ready => Ability2CooldownRemaining <= 0f;

        // Shield state (for Lene's Shield ability)
        public bool HasShield { get; private set; } = false;

        // Mashing state
        private float currentMashPower = 0f;
        private float lastMashTime = 0f;
        private KeyCode lastMashKey = KeyCode.None; // enforces alternation

        // Events
        public event Action<int, float, float> OnCooldownUpdate; // ability (1/2), remaining, total
        public event Action<int> OnAbilityUsed;   // ability (1/2)
        public event Action<int> OnAbilityReady;  // ability (1/2) — fires once when cooldown hits 0
        public event Action OnShieldActivated;
        public event Action OnShieldConsumed;

        private GameManager gameManager;

        private void Start()
        {
            gameManager = GameManager.Instance;
            SetupInputKeys();
        }

        private void SetupInputKeys()
        {
            if (playerNumber == 1)
            {
                mashKey1 = KeyCode.A;
                mashKey2 = KeyCode.D;
                ability1Key = KeyCode.Q;
                ability2Key = KeyCode.E;
            }
            else
            {
                mashKey1 = KeyCode.LeftArrow;
                mashKey2 = KeyCode.RightArrow;
                ability1Key = KeyCode.O;
                ability2Key = KeyCode.P;
            }
        }

        private void Update()
        {
            if (gameManager == null) return;

            UpdateCooldowns();

            if (gameManager.IsPlaying)
            {
                HandleMashInput();
                HandleAbilityInput();
            }

            UpdateMashPower();
        }

        public void SetCharacter(CharacterData character)
        {
            Character = character;
            ResetCooldowns();
        }

        #region Input Handling

        private void HandleMashInput()
        {
            KeyCode pressed = KeyCode.None;
            if (Input.GetKeyDown(mashKey1)) pressed = mashKey1;
            else if (Input.GetKeyDown(mashKey2)) pressed = mashKey2;

            if (pressed == KeyCode.None) return;

            // Only count if this is the OTHER key from the last press (alternation enforced)
            if (pressed == lastMashKey) return;

            float timeSinceLastMash = Time.time - lastMashTime;
            if (timeSinceLastMash < 1f / maxMashRate) return;

            lastMashKey = pressed;
            lastMashTime = Time.time;
            currentMashPower += mashPowerPerPress;

            AudioManager.Instance?.PlayMashHit();

            if (armWrestleController != null)
                armWrestleController.AddMashPower(playerNumber, mashPowerPerPress);
        }

        private void HandleAbilityInput()
        {
            if (Character == null) return;

            if (Input.GetKeyDown(ability1Key) && Ability1Ready)
            {
                UseAbility(1);
            }

            if (Input.GetKeyDown(ability2Key) && Ability2Ready)
            {
                UseAbility(2);
            }
        }

        private void UpdateMashPower()
        {
            currentMashPower = Mathf.Lerp(currentMashPower, 0f, mashDecay * Time.deltaTime);
        }

        #endregion

        #region Ability System

        private void UseAbility(int abilityNumber)
        {
            if (Character == null || abilitySystem == null) return;

            AbilityType abilityType;
            float cooldown;
            float duration;

            if (abilityNumber == 1)
            {
                abilityType = Character.ability1Type;
                cooldown = Character.ability1Cooldown;
                duration = Character.ability1Duration;
                Ability1CooldownRemaining = cooldown;
            }
            else
            {
                abilityType = Character.ability2Type;
                cooldown = Character.ability2Cooldown;
                duration = Character.ability2Duration;
                Ability2CooldownRemaining = cooldown;
            }

            // Execute the ability
            abilitySystem.ExecuteAbility(abilityType, playerNumber, duration);

            OnAbilityUsed?.Invoke(abilityNumber);
        }

        private void UpdateCooldowns()
        {
            if (Ability1CooldownRemaining > 0)
            {
                Ability1CooldownRemaining -= Time.deltaTime;
                if (Character != null)
                    OnCooldownUpdate?.Invoke(1, Ability1CooldownRemaining, Character.ability1Cooldown);
                if (Ability1CooldownRemaining <= 0f)
                {
                    Ability1CooldownRemaining = 0f;
                    OnAbilityReady?.Invoke(1);
                }
            }

            if (Ability2CooldownRemaining > 0)
            {
                Ability2CooldownRemaining -= Time.deltaTime;
                if (Character != null)
                    OnCooldownUpdate?.Invoke(2, Ability2CooldownRemaining, Character.ability2Cooldown);
                if (Ability2CooldownRemaining <= 0f)
                {
                    Ability2CooldownRemaining = 0f;
                    OnAbilityReady?.Invoke(2);
                }
            }
        }

        public void ResetCooldowns()
        {
            Ability1CooldownRemaining = 0f;
            Ability2CooldownRemaining = 0f;
            HasShield = false;
        }

        #endregion

        #region Shield (Lene's Ability)

        public void ActivateShield()
        {
            HasShield = true;
            OnShieldActivated?.Invoke();
        }

        public bool TryConsumeShield()
        {
            if (HasShield)
            {
                HasShield = false;
                OnShieldConsumed?.Invoke();
                return true;
            }
            return false;
        }

        #endregion

        #region Public Accessors

        public int PlayerNumber => playerNumber;

        public float GetAbilityCooldownProgress(int abilityNumber)
        {
            if (Character == null) return 1f;

            if (abilityNumber == 1)
            {
                return 1f - (Ability1CooldownRemaining / Character.ability1Cooldown);
            }
            else
            {
                return 1f - (Ability2CooldownRemaining / Character.ability2Cooldown);
            }
        }

        #endregion
    }
}
