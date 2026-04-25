using UnityEngine;
using System.Xml;
using System.IO;
using System.Globalization;

namespace DirtyThirtyShowdown
{
    /// <summary>
    /// Loads game settings from StreamingAssets/config.xml at startup.
    /// Edit config.xml with any text editor — no recompile needed.
    /// </summary>
    public class GameConfig : MonoBehaviour
    {
        public static GameConfig Instance { get; private set; }

        // Bar physics
        public float BarMovementMultiplier { get; private set; } = 3f;
        public float BarSensitivity       { get; private set; } = 0.1f;
        public float BarDamping           { get; private set; } = 0.95f;
        public float MomentumDecay        { get; private set; } = 2f;

        // Match settings
        public float RoundTimeLimit { get; private set; } = 45f;
        public int   RoundsToWin   { get; private set; } = 2;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfig();
        }

        private void LoadConfig()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "config.xml");
            if (!File.Exists(path))
            {
                Debug.LogWarning("[GameConfig] config.xml not found — using built-in defaults.");
                return;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlElement root = doc.DocumentElement;

                BarMovementMultiplier = ReadFloat(root, "BarMovementMultiplier", BarMovementMultiplier);
                BarSensitivity        = ReadFloat(root, "BarSensitivity",        BarSensitivity);
                BarDamping            = ReadFloat(root, "BarDamping",            BarDamping);
                MomentumDecay         = ReadFloat(root, "MomentumDecay",         MomentumDecay);
                RoundTimeLimit        = ReadFloat(root, "RoundTimeLimit",        RoundTimeLimit);
                RoundsToWin           = ReadInt  (root, "RoundsToWin",           RoundsToWin);

                Debug.Log($"[GameConfig] Loaded config.xml — " +
                          $"BarMovementMultiplier={BarMovementMultiplier}, " +
                          $"BarSensitivity={BarSensitivity}, " +
                          $"BarDamping={BarDamping}, " +
                          $"MomentumDecay={MomentumDecay}, " +
                          $"RoundTimeLimit={RoundTimeLimit}s, " +
                          $"RoundsToWin={RoundsToWin}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameConfig] Failed to parse config.xml: {e.Message} — using defaults.");
            }
        }

        private float ReadFloat(XmlElement root, string key, float fallback)
        {
            var node = root.SelectSingleNode(key);
            if (node != null && float.TryParse(node.InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                return val;
            return fallback;
        }

        private int ReadInt(XmlElement root, string key, int fallback)
        {
            var node = root.SelectSingleNode(key);
            if (node != null && int.TryParse(node.InnerText, out int val))
                return val;
            return fallback;
        }
    }
}
