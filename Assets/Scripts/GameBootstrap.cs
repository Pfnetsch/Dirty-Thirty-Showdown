using UnityEngine;

namespace PartyArmWrestling
{
    /// <summary>
    /// Bootstraps the game scene with all necessary references.
    /// Attach this to a GameObject in your main game scene.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private ArmWrestleController armWrestleController;
        [SerializeField] private AbilitySystem abilitySystem;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private CharacterSelectManager characterSelectManager;

        [Header("Players")]
        [SerializeField] private PlayerController player1;
        [SerializeField] private PlayerController player2;

        [Header("Auto-Create Missing Components")]
        [SerializeField] private bool autoSetup = true;

        private void Awake()
        {
            if (autoSetup) SetupMissingComponents();
            ValidateSetup();
        }

        private void SetupMissingComponents()
        {
            if (gameManager == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gameManager = gmObj.AddComponent<GameManager>();
            }

            if (audioManager == null)
            {
                GameObject audioObj = new GameObject("AudioManager");
                audioManager = audioObj.AddComponent<AudioManager>();
                audioObj.AddComponent<AudioSource>();
                audioObj.AddComponent<AudioSource>();
                audioObj.AddComponent<AudioSource>();
            }
        }

        private void ValidateSetup()
        {
            bool hasErrors = false;

            if (gameManager == null)
            {
                Debug.LogError("[GameBootstrap] GameManager is missing!");
                hasErrors = true;
            }

            if (armWrestleController == null)
                Debug.LogWarning("[GameBootstrap] ArmWrestleController is missing - create in scene");

            if (player1 == null || player2 == null)
                Debug.LogWarning("[GameBootstrap] Player controllers not assigned");

            if (uiManager == null)
                Debug.LogWarning("[GameBootstrap] UIManager not assigned");

            if (!hasErrors)
                Debug.Log("[GameBootstrap] Game setup validated successfully!");
        }

        [ContextMenu("Create Minimal Test Setup")]
        public void CreateMinimalTestSetup()
        {
            if (gameManager == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gameManager = gmObj.AddComponent<GameManager>();
            }

            if (armWrestleController == null)
            {
                GameObject armObj = new GameObject("ArmWrestleController");
                armWrestleController = armObj.AddComponent<ArmWrestleController>();
            }

            if (abilitySystem == null)
            {
                GameObject abilityObj = new GameObject("AbilitySystem");
                abilitySystem = abilityObj.AddComponent<AbilitySystem>();
            }

            if (player1 == null)
            {
                GameObject p1Obj = new GameObject("Player1");
                player1 = p1Obj.AddComponent<PlayerController>();
            }

            if (player2 == null)
            {
                GameObject p2Obj = new GameObject("Player2");
                player2 = p2Obj.AddComponent<PlayerController>();
            }

            Debug.Log("[GameBootstrap] Minimal test setup created!");
        }
    }
}
