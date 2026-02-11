using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

namespace DirtyThirtyShowdown
{
    public static class BackgroundBuilder
    {
        private const string ART_PATH = "Assets/Art/Background";
        private const string CHAR_PATH = ART_PATH + "/Characters";
        private const string TILE_PATH = ART_PATH + "/Tiles";

        // Sky colors (warm evening party vibe)
        private static readonly Color SkyTop = new Color(0.15f, 0.18f, 0.45f);
        private static readonly Color SkyBottom = new Color(0.85f, 0.55f, 0.35f);
        private static readonly Color GrassColor = new Color(0.28f, 0.55f, 0.22f);
        private static readonly Color GrassDark = new Color(0.22f, 0.45f, 0.18f);

        // Character sprite entries: file, anchorX, anchorY (normalized 0-1 of screen), scale, flipX
        private static readonly (string file, float x, float y, float scale, bool flip)[] crowdLayout = new[]
        {
            // Back row (smaller, higher up = further away)
            ("GuestA_south",  0.10f, 0.38f, 0.65f, false),
            ("GuestD_south",  0.20f, 0.40f, 0.60f, false),
            ("GuestB_south",  0.30f, 0.37f, 0.65f, true),
            ("Mario_south",   0.40f, 0.39f, 0.60f, false),  // Easter egg - hidden in back row
            ("GuestF_south",  0.50f, 0.38f, 0.65f, false),
            ("GuestH_south",  0.60f, 0.40f, 0.60f, true),
            ("GuestE_south",  0.70f, 0.37f, 0.65f, false),
            ("Guest1_west",   0.80f, 0.39f, 0.60f, false),
            ("GuestJ_south",  0.90f, 0.38f, 0.65f, true),

            // Front row (larger, lower = closer)
            ("GuestG_south",  0.06f, 0.22f, 0.90f, false),
            ("GuestI_south",  0.18f, 0.24f, 0.85f, false),
            ("GuestC_south",  0.30f, 0.22f, 0.90f, true),
            ("Luigi_south",   0.42f, 0.23f, 0.85f, false),  // Easter egg - hidden in front
            ("Guest1_south",  0.54f, 0.24f, 0.90f, false),
            ("GuestA_south",  0.66f, 0.22f, 0.85f, true),
            ("GuestB_south",  0.78f, 0.23f, 0.90f, false),
            ("Mario_east",    0.90f, 0.24f, 0.85f, false),  // Mario side view, edge of crowd
        };

        [MenuItem("Dirty Thirty Showdown/Build Background")]
        public static void BuildBackground()
        {
            // Ensure all sprites are imported correctly
            EnsureSpriteImports();

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[BackgroundBuilder] No Canvas found. Run 'Build Game UI' first.");
                return;
            }

            // Remove existing background if any
            var existing = canvas.transform.Find("Background");
            if (existing != null)
                Undo.DestroyObjectImmediate(existing.gameObject);

            // Create background root (first sibling = renders behind everything)
            var bgRoot = new GameObject("Background");
            bgRoot.transform.SetParent(canvas.transform, false);
            bgRoot.transform.SetAsFirstSibling();
            var bgRT = bgRoot.AddComponent<RectTransform>();
            StretchFill(bgRT);

            // 1. Sky gradient
            BuildSky(bgRoot.transform);

            // 2. Grass ground
            BuildGrass(bgRoot.transform);

            // 3. Decorative elements (barn, table, fence)
            BuildDecorations(bgRoot.transform);

            // 4. Party bunting / string lights across the top
            BuildBunting(bgRoot.transform);

            // 5. Party crowd (including hidden Mario & Luigi)
            BuildCrowd(bgRoot.transform);

            // 6. Make the CharacterSelectPanel semi-transparent so background shows
            MakeCharSelectTransparent(canvas.transform);

            Undo.RegisterCreatedObjectUndo(bgRoot, "Build Background");

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[BackgroundBuilder] Farm party background built! Mario & Luigi are hiding in the crowd.");
        }

        private static void BuildSky(Transform parent)
        {
            // Sky is top 55% of screen - use two overlapping images for gradient effect
            var skyObj = new GameObject("Sky");
            skyObj.transform.SetParent(parent, false);
            var skyRT = skyObj.AddComponent<RectTransform>();
            skyRT.anchorMin = new Vector2(0, 0.35f);
            skyRT.anchorMax = Vector2.one;
            skyRT.offsetMin = Vector2.zero;
            skyRT.offsetMax = Vector2.zero;

            var skyImg = skyObj.AddComponent<Image>();
            skyImg.color = SkyBottom;
            skyImg.raycastTarget = false;

            // Top overlay for darker sky
            var skyTopObj = new GameObject("SkyTop");
            skyTopObj.transform.SetParent(skyObj.transform, false);
            var skyTopRT = skyTopObj.AddComponent<RectTransform>();
            skyTopRT.anchorMin = new Vector2(0, 0.4f);
            skyTopRT.anchorMax = Vector2.one;
            skyTopRT.offsetMin = Vector2.zero;
            skyTopRT.offsetMax = Vector2.zero;

            var skyTopImg = skyTopObj.AddComponent<Image>();
            skyTopImg.color = SkyTop;
            skyTopImg.raycastTarget = false;

            // Stars (small white dots scattered in the sky)
            AddStars(skyObj.transform);
        }

        private static void AddStars(Transform parent)
        {
            var starsObj = new GameObject("Stars");
            starsObj.transform.SetParent(parent, false);
            StretchFill(starsObj.AddComponent<RectTransform>());

            // Seed for consistent star placement
            var rng = new System.Random(42);
            for (int i = 0; i < 25; i++)
            {
                var star = new GameObject($"Star_{i}");
                star.transform.SetParent(starsObj.transform, false);
                var starRT = star.AddComponent<RectTransform>();

                float x = (float)rng.NextDouble();
                float y = 0.3f + (float)rng.NextDouble() * 0.7f; // upper portion only
                float size = 2f + (float)rng.NextDouble() * 3f;

                starRT.anchorMin = new Vector2(x, y);
                starRT.anchorMax = new Vector2(x, y);
                starRT.sizeDelta = new Vector2(size, size);

                var starImg = star.AddComponent<Image>();
                float brightness = 0.6f + (float)rng.NextDouble() * 0.4f;
                starImg.color = new Color(1f, 1f, brightness, 0.7f + (float)rng.NextDouble() * 0.3f);
                starImg.raycastTarget = false;
            }
        }

        private static void BuildGrass(Transform parent)
        {
            // Main grass area - bottom 40%
            var grassObj = new GameObject("Grass");
            grassObj.transform.SetParent(parent, false);
            var grassRT = grassObj.AddComponent<RectTransform>();
            grassRT.anchorMin = Vector2.zero;
            grassRT.anchorMax = new Vector2(1, 0.40f);
            grassRT.offsetMin = Vector2.zero;
            grassRT.offsetMax = Vector2.zero;

            var grassImg = grassObj.AddComponent<Image>();
            grassImg.color = GrassColor;
            grassImg.raycastTarget = false;

            // Darker grass stripe for depth
            var grassDarkObj = new GameObject("GrassDark");
            grassDarkObj.transform.SetParent(grassObj.transform, false);
            var grassDarkRT = grassDarkObj.AddComponent<RectTransform>();
            grassDarkRT.anchorMin = Vector2.zero;
            grassDarkRT.anchorMax = new Vector2(1, 0.3f);
            grassDarkRT.offsetMin = Vector2.zero;
            grassDarkRT.offsetMax = Vector2.zero;

            var grassDarkImg = grassDarkObj.AddComponent<Image>();
            grassDarkImg.color = GrassDark;
            grassDarkImg.raycastTarget = false;

            // Grass detail line at horizon
            var horizonObj = new GameObject("HorizonLine");
            horizonObj.transform.SetParent(parent, false);
            var horizonRT = horizonObj.AddComponent<RectTransform>();
            horizonRT.anchorMin = new Vector2(0, 0.395f);
            horizonRT.anchorMax = new Vector2(1, 0.405f);
            horizonRT.offsetMin = Vector2.zero;
            horizonRT.offsetMax = Vector2.zero;

            var horizonImg = horizonObj.AddComponent<Image>();
            horizonImg.color = new Color(0.35f, 0.65f, 0.28f);
            horizonImg.raycastTarget = false;
        }

        private static void BuildDecorations(Transform parent)
        {
            var decoParent = new GameObject("Decorations");
            decoParent.transform.SetParent(parent, false);
            StretchFill(decoParent.AddComponent<RectTransform>());

            // Barn (back left)
            PlaceSprite(decoParent.transform, "Barn", TILE_PATH + "/Barn.png",
                0.08f, 0.35f, 0.22f, 0.65f, false);

            // Party Table (center-right)
            PlaceSprite(decoParent.transform, "PartyTable", TILE_PATH + "/PartyTable.png",
                0.55f, 0.18f, 0.7f, 0.38f, false);

            // Fence sections
            PlaceSprite(decoParent.transform, "Fence1", TILE_PATH + "/PartyFence.png",
                0.0f, 0.30f, 0.12f, 0.42f, false);

            PlaceSprite(decoParent.transform, "Fence2", TILE_PATH + "/PartyFence.png",
                0.88f, 0.30f, 1.0f, 0.42f, true);

            // Extra tree-like elements (simple green circles for background trees)
            AddBackgroundTrees(decoParent.transform);
        }

        private static void AddBackgroundTrees(Transform parent)
        {
            float[] treePositions = { 0.02f, 0.18f, 0.35f, 0.75f, 0.92f };
            float[] treeSizes = { 80, 60, 70, 65, 75 };

            for (int i = 0; i < treePositions.Length; i++)
            {
                // Tree canopy (circle)
                var tree = new GameObject($"Tree_{i}");
                tree.transform.SetParent(parent, false);
                var treeRT = tree.AddComponent<RectTransform>();
                treeRT.anchorMin = new Vector2(treePositions[i], 0.40f);
                treeRT.anchorMax = new Vector2(treePositions[i], 0.40f);
                treeRT.sizeDelta = new Vector2(treeSizes[i], treeSizes[i] * 1.2f);
                treeRT.pivot = new Vector2(0.5f, 0f);

                var treeImg = tree.AddComponent<Image>();
                float shade = 0.18f + (i % 3) * 0.06f;
                treeImg.color = new Color(shade, 0.4f + shade, shade * 0.8f);
                treeImg.raycastTarget = false;

                // Trunk
                var trunk = new GameObject("Trunk");
                trunk.transform.SetParent(tree.transform, false);
                var trunkRT = trunk.AddComponent<RectTransform>();
                trunkRT.anchorMin = new Vector2(0.4f, -0.3f);
                trunkRT.anchorMax = new Vector2(0.6f, 0.15f);
                trunkRT.offsetMin = Vector2.zero;
                trunkRT.offsetMax = Vector2.zero;

                var trunkImg = trunk.AddComponent<Image>();
                trunkImg.color = new Color(0.35f, 0.22f, 0.12f);
                trunkImg.raycastTarget = false;
                trunk.transform.SetAsFirstSibling(); // trunk behind canopy
            }
        }

        private static void BuildBunting(Transform parent)
        {
            // Colorful party bunting across the top area
            var buntingParent = new GameObject("Bunting");
            buntingParent.transform.SetParent(parent, false);
            StretchFill(buntingParent.AddComponent<RectTransform>());

            Color[] flagColors = {
                new Color(1f, 0.3f, 0.3f),   // Red
                new Color(1f, 0.85f, 0.2f),  // Yellow
                new Color(0.3f, 0.8f, 0.3f), // Green
                new Color(0.3f, 0.5f, 1f),   // Blue
                new Color(1f, 0.5f, 0.8f),   // Pink
                new Color(0.9f, 0.5f, 0.1f), // Orange
            };

            // String line
            var line = new GameObject("StringLine");
            line.transform.SetParent(buntingParent.transform, false);
            var lineRT = line.AddComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0.0f, 0.72f);
            lineRT.anchorMax = new Vector2(1.0f, 0.723f);
            lineRT.offsetMin = Vector2.zero;
            lineRT.offsetMax = Vector2.zero;
            var lineImg = line.AddComponent<Image>();
            lineImg.color = new Color(0.3f, 0.3f, 0.3f);
            lineImg.raycastTarget = false;

            // Triangle flags along the string
            int flagCount = 18;
            for (int i = 0; i < flagCount; i++)
            {
                var flag = new GameObject($"Flag_{i}");
                flag.transform.SetParent(buntingParent.transform, false);
                var flagRT = flag.AddComponent<RectTransform>();

                float t = (i + 0.5f) / flagCount;
                flagRT.anchorMin = new Vector2(t - 0.015f, 0.68f);
                flagRT.anchorMax = new Vector2(t + 0.015f, 0.72f);
                flagRT.offsetMin = Vector2.zero;
                flagRT.offsetMax = Vector2.zero;

                var flagImg = flag.AddComponent<Image>();
                flagImg.color = flagColors[i % flagColors.Length];
                flagImg.raycastTarget = false;
            }

            // Second bunting line lower
            var line2 = new GameObject("StringLine2");
            line2.transform.SetParent(buntingParent.transform, false);
            var line2RT = line2.AddComponent<RectTransform>();
            line2RT.anchorMin = new Vector2(0.05f, 0.58f);
            line2RT.anchorMax = new Vector2(0.95f, 0.583f);
            line2RT.offsetMin = Vector2.zero;
            line2RT.offsetMax = Vector2.zero;
            var line2Img = line2.AddComponent<Image>();
            line2Img.color = new Color(0.3f, 0.3f, 0.3f);
            line2Img.raycastTarget = false;

            int flagCount2 = 14;
            for (int i = 0; i < flagCount2; i++)
            {
                var flag = new GameObject($"Flag2_{i}");
                flag.transform.SetParent(buntingParent.transform, false);
                var flagRT = flag.AddComponent<RectTransform>();

                float t = 0.05f + (i + 0.5f) / flagCount2 * 0.9f;
                flagRT.anchorMin = new Vector2(t - 0.018f, 0.54f);
                flagRT.anchorMax = new Vector2(t + 0.018f, 0.58f);
                flagRT.offsetMin = Vector2.zero;
                flagRT.offsetMax = Vector2.zero;

                var flagImg = flag.AddComponent<Image>();
                flagImg.color = flagColors[(i + 3) % flagColors.Length];
                flagImg.raycastTarget = false;
            }
        }

        private static void BuildCrowd(Transform parent)
        {
            var crowdParent = new GameObject("Crowd");
            crowdParent.transform.SetParent(parent, false);
            StretchFill(crowdParent.AddComponent<RectTransform>());

            for (int i = 0; i < crowdLayout.Length; i++)
            {
                var entry = crowdLayout[i];
                string path = $"{CHAR_PATH}/{entry.file}.png";

                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[BackgroundBuilder] Missing sprite: {path}, skipping");
                    continue;
                }

                float charSize = 64 * entry.scale; // Base size for 32px sprites upscaled
                string label = entry.file.Contains("Mario") || entry.file.Contains("Luigi")
                    ? $"__{entry.file}" // Underscore prefix to not be obvious in hierarchy
                    : entry.file;

                var charObj = new GameObject(label);
                charObj.transform.SetParent(crowdParent.transform, false);
                var charRT = charObj.AddComponent<RectTransform>();
                charRT.anchorMin = new Vector2(entry.x, entry.y);
                charRT.anchorMax = new Vector2(entry.x, entry.y);
                charRT.sizeDelta = new Vector2(charSize, charSize);
                charRT.pivot = new Vector2(0.5f, 0f);

                Sprite sprite = LoadSpriteAtPath(path);
                if (sprite != null)
                {
                    var img = charObj.AddComponent<Image>();
                    img.sprite = sprite;
                    img.preserveAspect = true;
                    img.raycastTarget = false;

                    // Flip by negative scale
                    if (entry.flip)
                    {
                        charRT.localScale = new Vector3(-1, 1, 1);
                    }
                }
            }
        }

        private static void MakeCharSelectTransparent(Transform canvasTransform)
        {
            var csPanel = canvasTransform.Find("CharacterSelectPanel");
            if (csPanel == null) return;

            var bg = csPanel.GetComponent<Image>();
            if (bg != null)
            {
                Undo.RecordObject(bg, "Make CharSelect Transparent");
                bg.color = new Color(0.05f, 0.05f, 0.1f, 0.55f);
            }

            // Also make player panels more transparent
            var p1Panel = csPanel.Find("P1Panel");
            var p2Panel = csPanel.Find("P2Panel");
            if (p1Panel != null)
            {
                var p1Bg = p1Panel.GetComponent<Image>();
                if (p1Bg != null)
                {
                    Undo.RecordObject(p1Bg, "P1 Panel Transparent");
                    p1Bg.color = new Color(0.1f, 0.1f, 0.15f, 0.7f);
                }
            }
            if (p2Panel != null)
            {
                var p2Bg = p2Panel.GetComponent<Image>();
                if (p2Bg != null)
                {
                    Undo.RecordObject(p2Bg, "P2 Panel Transparent");
                    p2Bg.color = new Color(0.1f, 0.1f, 0.15f, 0.7f);
                }
            }
        }

        #region Helpers

        private static void PlaceSprite(Transform parent, string name, string assetPath,
            float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY, bool flip)
        {
            Sprite sprite = LoadSpriteAtPath(assetPath);

            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(anchorMinX, anchorMinY);
            rt.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = obj.AddComponent<Image>();
            img.preserveAspect = true;
            img.raycastTarget = false;

            if (sprite != null)
                img.sprite = sprite;
            else
                img.color = new Color(0.5f, 0.3f, 0.2f, 0.5f); // Placeholder brown

            if (flip)
                rt.localScale = new Vector3(-1, 1, 1);
        }

        private static Sprite LoadSpriteAtPath(string path)
        {
            // Ensure proper import settings first
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point; // Pixel art!
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureSpriteImports()
        {
            // Find all PNGs in the background art folder and ensure they're imported as sprites
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ART_PATH });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 32;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }
            }
        }

        private static void StretchFill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        #endregion
    }
}
