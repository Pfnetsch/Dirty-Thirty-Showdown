using UnityEngine;
using System;

namespace DirtyThirtyShowdown
{
    public class CheatManager : MonoBehaviour
    {
        public static CheatManager Instance { get; private set; }

        public bool PatzUnlocked { get; private set; } = false;

        public event Action OnPatzUnlocked;

        private static readonly string[] ValidCodes =
        {
            "oh mighty patz please help",
            "patz please help",
            "patz help",
            "patz is the best",
            "patz wuhu",
        };

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

            string normalized = input.Trim().ToLowerInvariant();
            bool matched = false;
            foreach (var code in ValidCodes)
            {
                if (normalized == code) { matched = true; break; }
            }

            if (matched)
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
