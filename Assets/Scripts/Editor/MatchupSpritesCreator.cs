using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DirtyThirtyShowdown
{
    /// <summary>
    /// Scans Assets/Art/ArmWrestling/ for matchup PNGs and auto-creates or updates
    /// MatchupSprites ScriptableObject assets in Assets/Art/ArmWrestling/Matchups/.
    ///
    /// Expected filename convention:
    ///   {char1}_vs_{char2}_{state}.png
    ///
    /// Supported state suffixes:
    ///   neutral
    ///   {char1}_dominating  →  char1Dominating
    ///   {char1}_winning     →  char1Winning
    ///   {char2}_dominating  →  char2Dominating
    ///   {char2}_winning     →  char2Winning
    ///
    /// Character names must match the characterName field in CharacterData assets
    /// (case-insensitive compare, filename uses lowercase).
    /// </summary>
    public static class MatchupSpritesCreator
    {
        private const string ArtRoot    = "Assets/Art/ArmWrestling";
        private const string OutputRoot = "Assets/Art/ArmWrestling/Matchups";
        private const string CharRoot   = "Assets/Characters";

        [MenuItem("Dirty Thirty Showdown/Create Matchup Sprite Assets")]
        public static void CreateMatchupAssets()
        {
            // Ensure output folder exists
            if (!AssetDatabase.IsValidFolder(OutputRoot))
                AssetDatabase.CreateFolder("Assets/Art/ArmWrestling", "Matchups");

            // Load all CharacterData assets keyed by lowercase name
            var characters = LoadAllCharacters();
            if (characters.Count == 0)
            {
                Debug.LogWarning("[MatchupSpritesCreator] No CharacterData assets found in Assets/Characters/.");
                return;
            }

            // Find all PNG files in ArtRoot (top-level only — not subfolders)
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { ArtRoot });
            var spritesByMatchup = new Dictionary<string, Dictionary<string, Sprite>>();

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Skip files in subfolders (only want top-level matchup PNGs)
                string relativePath = path.Substring(ArtRoot.Length + 1);
                if (relativePath.Contains("/")) continue;

                string filename = Path.GetFileNameWithoutExtension(path).ToLower();

                // Must contain "_vs_"
                if (!filename.Contains("_vs_")) continue;

                // Split into matchup key and state: "{char1}_vs_{char2}_{state}"
                int vsIdx = filename.IndexOf("_vs_");
                string afterVs = filename.Substring(vsIdx + 4); // everything after "_vs_"

                // Find the second character name by checking all known characters
                string char1Name = filename.Substring(0, vsIdx);
                string char2Name = null;
                string state = null;

                foreach (var knownName in characters.Keys)
                {
                    if (afterVs.StartsWith(knownName))
                    {
                        char2Name = knownName;
                        // State is whatever comes after "{char2}" (skip leading underscore)
                        state = afterVs.Substring(knownName.Length).TrimStart('_');
                        break;
                    }
                }

                if (char2Name == null)
                {
                    Debug.LogWarning($"[MatchupSpritesCreator] Could not parse character name from: {filename}");
                    continue;
                }

                string matchupKey = $"{char1Name}_vs_{char2Name}";
                if (!spritesByMatchup.ContainsKey(matchupKey))
                    spritesByMatchup[matchupKey] = new Dictionary<string, Sprite>();

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                    spritesByMatchup[matchupKey][state] = sprite;
            }

            if (spritesByMatchup.Count == 0)
            {
                Debug.LogWarning("[MatchupSpritesCreator] No matchup PNGs found. Make sure files follow the naming convention: {char1}_vs_{char2}_{state}.png");
                return;
            }

            int created = 0, updated = 0;

            foreach (var kvp in spritesByMatchup)
            {
                string matchupKey = kvp.Key; // e.g. "eli_vs_lene"
                var sprites = kvp.Value;

                // Parse char names from key
                int vs = matchupKey.IndexOf("_vs_");
                string c1Name = matchupKey.Substring(0, vs);
                string c2Name = matchupKey.Substring(vs + 4);

                if (!characters.TryGetValue(c1Name, out var char1Data))
                {
                    Debug.LogWarning($"[MatchupSpritesCreator] No CharacterData found for '{c1Name}' (matchup: {matchupKey})");
                    continue;
                }
                if (!characters.TryGetValue(c2Name, out var char2Data))
                {
                    Debug.LogWarning($"[MatchupSpritesCreator] No CharacterData found for '{c2Name}' (matchup: {matchupKey})");
                    continue;
                }

                string assetPath = $"{OutputRoot}/{matchupKey}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<MatchupSprites>(assetPath);
                bool isNew = asset == null;

                if (isNew)
                {
                    asset = ScriptableObject.CreateInstance<MatchupSprites>();
                    asset.name = matchupKey;
                }

                asset.character1 = char1Data;
                asset.character2 = char2Data;

                // Assign sprites by state name
                sprites.TryGetValue("neutral",              out asset.neutral);
                sprites.TryGetValue($"{c1Name}_dominating", out asset.char1Dominating);
                sprites.TryGetValue($"{c1Name}_winning",    out asset.char1Winning);
                sprites.TryGetValue($"{c2Name}_dominating", out asset.char2Dominating);
                sprites.TryGetValue($"{c2Name}_winning",    out asset.char2Winning);

                if (isNew)
                {
                    AssetDatabase.CreateAsset(asset, assetPath);
                    created++;
                }
                else
                {
                    EditorUtility.SetDirty(asset);
                    updated++;
                }

                Debug.Log($"[MatchupSpritesCreator] {(isNew ? "Created" : "Updated")} {assetPath} " +
                          $"[neutral={asset.neutral != null}, " +
                          $"char1Dom={asset.char1Dominating != null}, char1Win={asset.char1Winning != null}, " +
                          $"char2Win={asset.char2Winning != null}, char2Dom={asset.char2Dominating != null}]");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[MatchupSpritesCreator] Done. Created: {created}, Updated: {updated}. Assets saved to {OutputRoot}/");
        }

        /// <summary>Returns all CharacterData assets keyed by lowercase characterName.</summary>
        public static Dictionary<string, CharacterData> LoadAllCharacters()
        {
            var result = new Dictionary<string, CharacterData>();
            var guids = AssetDatabase.FindAssets("t:CharacterData", new[] { CharRoot });
            foreach (var guid in guids)
            {
                var cd = AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid));
                if (cd != null && !string.IsNullOrEmpty(cd.characterName))
                    result[cd.characterName.ToLower()] = cd;
            }
            return result;
        }

        /// <summary>Loads all existing MatchupSprites assets from the output folder.</summary>
        public static MatchupSprites[] LoadAllMatchupAssets()
        {
            if (!AssetDatabase.IsValidFolder(OutputRoot)) return new MatchupSprites[0];
            var guids = AssetDatabase.FindAssets("t:MatchupSprites", new[] { OutputRoot });
            return guids
                .Select(g => AssetDatabase.LoadAssetAtPath<MatchupSprites>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(m => m != null)
                .ToArray();
        }
    }
}
