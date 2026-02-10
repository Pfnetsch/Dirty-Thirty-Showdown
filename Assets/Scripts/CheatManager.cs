using UnityEngine;
using System;

namespace DirtyThirtyShowdown
{
    public class CheatManager : MonoBehaviour
    {
        public static CheatManager Instance { get; private set; }

        public bool PatzUnlocked { get; private set; } = false;

        public event Action OnPatzUnlocked;

        private const string CheatCode = "oh mighty patz please help";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool TryCheatCode(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            if (input.Trim().ToLowerInvariant() == CheatCode)
            {
                if (!PatzUnlocked)
                {
                    PatzUnlocked = true;
                    OnPatzUnlocked?.Invoke();
                    Debug.Log("[CheatManager] Patz has been unleashed!");
                }
                return true;
            }

            return false;
        }
    }
}
