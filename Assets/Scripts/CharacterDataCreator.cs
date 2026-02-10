using UnityEngine;

namespace DirtyThirtyShowdown
{
#if UNITY_EDITOR
    /// <summary>
    /// Editor utility to create pre-configured character data assets.
    /// Use menu: Assets > Create > Dirty Thirty Showdown > All Characters
    /// </summary>
    public static class CharacterDataCreator
    {
        [UnityEditor.MenuItem("Assets/Create/Dirty Thirty Showdown/All Characters")]
        public static void CreateAllCharacters()
        {
            CreateEli();
            CreateLene();
            CreateNati();
            CreateSabi();
            Debug.Log("Created all 4 character data assets in Assets/ScriptableObjects/Characters/");
        }

        [UnityEditor.MenuItem("Assets/Create/Dirty Thirty Showdown/Character - Eli")]
        public static void CreateEli()
        {
            CharacterData eli = ScriptableObject.CreateInstance<CharacterData>();
            eli.characterName = "Eli";
            eli.description = "Overwhelming offensive power with high aggression";
            eli.ability1Type = AbilityType.PowerSurge;
            eli.ability1Name = "Power Surge";
            eli.ability1Description = "Mashing counts 2x for 3 seconds";
            eli.ability1Cooldown = 15f;
            eli.ability1Duration = 3f;
            eli.ability2Type = AbilityType.Flash;
            eli.ability2Name = "Flash";
            eli.ability2Description = "Screen flash/shake to distract opponent";
            eli.ability2Cooldown = 10f;
            eli.ability2Duration = 0.5f;
            eli.characterColor = new Color(1f, 0.84f, 0f); // Gold
            SaveCharacterAsset(eli, "Eli");
        }

        [UnityEditor.MenuItem("Assets/Create/Dirty Thirty Showdown/Character - Lene")]
        public static void CreateLene()
        {
            CharacterData lene = ScriptableObject.CreateInstance<CharacterData>();
            lene.characterName = "Lene";
            lene.description = "Tactical defense with psychological pressure";
            lene.ability1Type = AbilityType.TrashTalk;
            lene.ability1Name = "Trash Talk";
            lene.ability1Description = "Speech bubble + opponent gets 25% slowdown for 2 seconds";
            lene.ability1Cooldown = 16f;
            lene.ability1Duration = 2f;
            lene.ability2Type = AbilityType.Shield;
            lene.ability2Name = "Shield";
            lene.ability2Description = "Block the next ability used against you";
            lene.ability2Cooldown = 22f;
            lene.ability2Duration = 0f;
            lene.characterColor = new Color(0.54f, 0.17f, 0.89f); // Purple
            SaveCharacterAsset(lene, "Lene");
        }

        [UnityEditor.MenuItem("Assets/Create/Dirty Thirty Showdown/Character - Nati")]
        public static void CreateNati()
        {
            CharacterData nati = ScriptableObject.CreateInstance<CharacterData>();
            nati.characterName = "Nati";
            nati.description = "Charm-based disruption with endurance advantage";
            nati.ability1Type = AbilityType.WinkFlirt;
            nati.ability1Name = "Wink";
            nati.ability1Description = "Opponent's input disabled for 1.5 seconds";
            nati.ability1Cooldown = 12f;
            nati.ability1Duration = 1.5f;
            nati.ability2Type = AbilityType.Dance;
            nati.ability2Name = "Dance";
            nati.ability2Description = "Opponent needs 50% more mashing for 4 seconds";
            nati.ability2Cooldown = 18f;
            nati.ability2Duration = 4f;
            nati.characterColor = new Color(1f, 0.41f, 0.71f); // Hot Pink
            SaveCharacterAsset(nati, "Nati");
        }

        [UnityEditor.MenuItem("Assets/Create/Dirty Thirty Showdown/Character - Sabi")]
        public static void CreateSabi()
        {
            CharacterData sabi = ScriptableObject.CreateInstance<CharacterData>();
            sabi.characterName = "Sabi";
            sabi.description = "Defensive and unpredictable counter-play specialist";
            sabi.ability1Type = AbilityType.CakeToss;
            sabi.ability1Name = "Cake Toss";
            sabi.ability1Description = "Bar can only move back toward you for 2 seconds";
            sabi.ability1Cooldown = 20f;
            sabi.ability1Duration = 2f;
            sabi.ability2Type = AbilityType.FakeOut;
            sabi.ability2Name = "Fake Out";
            sabi.ability2Description = "Controls reversed for 2 seconds";
            sabi.ability2Cooldown = 24f;
            sabi.ability2Duration = 2f;
            sabi.characterColor = new Color(0.55f, 0.27f, 0.07f); // Brown/Chocolate
            SaveCharacterAsset(sabi, "Sabi");
        }

        private static void SaveCharacterAsset(CharacterData data, string name)
        {
            string folderPath = "Assets/ScriptableObjects/Characters";
            
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                UnityEditor.AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
                UnityEditor.AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Characters");

            string assetPath = $"{folderPath}/{name}.asset";
            UnityEditor.AssetDatabase.CreateAsset(data, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"Created character asset: {assetPath}");
        }
    }
#endif
}
