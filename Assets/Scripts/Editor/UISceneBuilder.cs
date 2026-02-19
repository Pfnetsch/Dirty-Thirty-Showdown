using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using System.Linq;

namespace DirtyThirtyShowdown
{
    public static class UISceneBuilder
    {
        // Colors
        private static readonly Color P1Color = new Color(0.2f, 0.6f, 1f);
        private static readonly Color P2Color = new Color(1f, 0.4f, 0.4f);
        private static readonly Color PanelBg = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        private static readonly Color OverlayBg = new Color(0f, 0f, 0f, 0.7f);
        private static readonly Color BarTrackColor = new Color(0.2f, 0.2f, 0.25f);

        private static TMP_FontAsset defaultFont;  // Pixel Operator Bold — general HUD text
        private static TMP_FontAsset titleFont;    // Press Start 2P — titles, big numbers
        private static TMP_FontAsset displayFont;  // Dogica Pixel Bold — buttons, headers, names
        private static TMP_FontAsset smallFont;    // m5x7 — key hints, instructions, small labels

        [MenuItem("Dirty Thirty Showdown/Build Game UI")]
        public static void BuildGameUI()
        {
            // Load pixel art fonts
            defaultFont  = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PixelOperator-Bold.asset");
            titleFont    = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PressStart2P-Regular.asset");
            displayFont  = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/dogicapixelbold.asset");
            smallFont    = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/m5x7.asset");

            if (defaultFont  == null) Debug.LogWarning("[UISceneBuilder] PixelOperator-Bold.asset not found.");
            if (titleFont    == null) Debug.LogWarning("[UISceneBuilder] PressStart2P-Regular.asset not found.");
            if (displayFont  == null) Debug.LogWarning("[UISceneBuilder] dogicapixelbold.asset not found.");
            if (smallFont    == null) Debug.LogWarning("[UISceneBuilder] m5x7.asset not found.");

            // Create or find Canvas
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            GameObject canvasObj;
            if (canvas != null)
            {
                canvasObj = canvas.gameObject;
                // Clear existing UI children
                for (int i = canvasObj.transform.childCount - 1; i >= 0; i--)
                {
                    Undo.DestroyObjectImmediate(canvasObj.transform.GetChild(i).gameObject);
                }
            }
            else
            {
                canvasObj = new GameObject("Canvas");
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (canvasObj.GetComponent<GraphicRaycaster>() == null)
                canvasObj.AddComponent<GraphicRaycaster>();

            // Ensure EventSystem exists
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esObj = new GameObject("EventSystem");
                Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Build all panels
            var titleScreenPanel = BuildTitleScreenPanel(canvasObj.transform);
            var charSelectPanel = BuildCharacterSelectPanel(canvasObj.transform);
            var gameplayPanel = BuildGameplayPanel(canvasObj.transform,
                out var barTrack, out var barFillLeft, out var barFillRight, out var barIndicator,
                out var roundText, out var timerText, out var timerFill,
                out var p1Portrait, out var p1NameText,
                out var p1Ability1Cooldown, out var p1Ability2Cooldown,
                out var p1Ability1KeyText, out var p1Ability2KeyText,
                out var p1Ability1NameText, out var p1Ability2NameText,
                out var p1ShieldIndicator, out var p1PowerSurgeIndicator,
                out var p2Portrait, out var p2NameText,
                out var p2Ability1Cooldown, out var p2Ability2Cooldown,
                out var p2Ability1KeyText, out var p2Ability2KeyText,
                out var p2Ability1NameText, out var p2Ability2NameText,
                out var p2ShieldIndicator, out var p2PowerSurgeIndicator,
                out var p1ScoreText, out var p2ScoreText,
                out var controlsReversedIndicator);

            // Matchup display image (center of gameplay panel)
            var matchupImageObj = BuildMatchupImage(gameplayPanel.transform);

            // Embed VFX sprites into existing ability indicators
            AddVFXSpritesToExistingIndicators(gameplayPanel.transform);

            // Build new per-player VFX overlays (TrashTalk, Dance, CakeToss)
            BuildVFXOverlays(gameplayPanel.transform,
                out var p1TrashTalk, out var p2TrashTalk,
                out var p1Dance,     out var p2Dance,
                out var p1Cake,      out var p2Cake);

            var roundStartPanel = BuildOverlayPanel(canvasObj.transform, "RoundStartPanel",
                out var roundStartText, "Round 1\nGet Ready!", 72);
            var roundEndPanel = BuildOverlayPanel(canvasObj.transform, "RoundEndPanel",
                out var roundEndText, "Player Wins Round!", 64);
            var matchEndPanel = BuildMatchEndPanel(canvasObj.transform,
                out var matchWinnerText, out var matchEndInstructionsText,
                out var victoryBgImage);

            // Load victory sprites
            var eliVictory  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Background/VictoryEli.png");
            var leneVictory = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Background/VictoryLene.png");
            var natiVictory = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Background/VictoryNati.png");
            var sabiVictory = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Background/VictorySabi.png");
            var patzVictory = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Background/VictoryPatz.png");

            var screenFlashOverlay = BuildScreenFlashOverlay(canvasObj.transform);

            // Deactivate non-default panels (TitleScreen is the default active panel)
            charSelectPanel.SetActive(false);
            gameplayPanel.SetActive(false);
            roundStartPanel.SetActive(false);
            roundEndPanel.SetActive(false);
            matchEndPanel.SetActive(false);

            // Extract CharacterSelect sub-elements
            var csPanel = charSelectPanel.transform;
            var p1CSPortrait = csPanel.Find("P1Panel/Portrait").GetComponent<Image>();
            var p1CSName = csPanel.Find("P1Panel/NameText").GetComponent<TextMeshProUGUI>();
            var p1CSAbility1 = csPanel.Find("P1Panel/Ability1Text").GetComponent<TextMeshProUGUI>();
            var p1CSAbility2 = csPanel.Find("P1Panel/Ability2Text").GetComponent<TextMeshProUGUI>();
            var p1CSReady = csPanel.Find("P1Panel/ReadyIndicator").GetComponent<Image>();
            var p2CSPortrait = csPanel.Find("P2Panel/Portrait").GetComponent<Image>();
            var p2CSName = csPanel.Find("P2Panel/NameText").GetComponent<TextMeshProUGUI>();
            var p2CSAbility1 = csPanel.Find("P2Panel/Ability1Text").GetComponent<TextMeshProUGUI>();
            var p2CSAbility2 = csPanel.Find("P2Panel/Ability2Text").GetComponent<TextMeshProUGUI>();
            var p2CSReady = csPanel.Find("P2Panel/ReadyIndicator").GetComponent<Image>();
            var charGrid = csPanel.Find("CharacterGrid");
            var startButton = csPanel.Find("StartButton").GetComponent<Button>();
            var instrText = csPanel.Find("InstructionsText").GetComponent<TextMeshProUGUI>();

            // Create CharacterButton prefab
            GameObject prefab = CreateCharacterButtonPrefab();

            // Load character data assets
            var characterAssets = LoadCharacterAssets();
            CharacterData patzAsset = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Characters/Patz.asset");

            // Pre-place the 4 character buttons so they're visible in the editor
            PlaceCharacterButtonsInGrid(charGrid, characterAssets, prefab);

            // Find or create scene systems
            var bootstrap = FindOrCreateComponent<GameBootstrap>("GameBootstrap");
            var gm = FindOrCreateComponent<GameManager>("GameManager");
            FindOrCreateComponent<ScreenShake>("ScreenShake");
            var awc = FindOrCreateComponent<ArmWrestleController>("ArmWrestleController");
            var abilitySys = FindOrCreateComponent<AbilitySystem>("AbilitySystem");
            var uiMgr = FindOrCreateComponent<UIManager>("UIManager");
            var charSelectMgr = FindOrCreateComponent<CharacterSelectManager>("CharacterSelectManager");
            var audioMgr = FindOrCreateComponent<AudioManager>("AudioManager");
            var cheatMgr = FindOrCreateComponent<CheatManager>("CheatManager");

            // Create player controllers
            var p1Controller = FindOrCreatePlayerController(1);
            var p2Controller = FindOrCreatePlayerController(2);

            // Ensure AudioManager has audio sources and wire them
            EnsureAudioSources(audioMgr.gameObject, 3);
            WireAudioManager(audioMgr);

            // Wire UIManager
            WireUIManager(uiMgr,
                barIndicator.GetComponent<RectTransform>(), barTrack.GetComponent<RectTransform>(),
                barFillLeft, barFillRight,
                p1ScoreText, p2ScoreText, roundText, timerText, timerFill,
                p1Portrait, p1NameText, p1Ability1Cooldown, p1Ability2Cooldown,
                p1Ability1KeyText, p1Ability2KeyText, p1Ability1NameText, p1Ability2NameText,
                p1ShieldIndicator,
                p2Portrait, p2NameText, p2Ability1Cooldown, p2Ability2Cooldown,
                p2Ability1KeyText, p2Ability2KeyText, p2Ability1NameText, p2Ability2NameText,
                p2ShieldIndicator,
                titleScreenPanel, charSelectPanel, gameplayPanel, roundStartPanel, roundEndPanel, matchEndPanel,
                roundStartText, roundEndText, matchWinnerText, matchEndInstructionsText,
                p1PowerSurgeIndicator, p2PowerSurgeIndicator, controlsReversedIndicator,
                p1Controller, p2Controller, awc,
                victoryBgImage, eliVictory, leneVictory, natiVictory, sabiVictory, patzVictory);

            // Wire UIManager ability system, stunned indicators, and new VFX overlays
            var gpTransform = gameplayPanel.transform;
            var p1InputDisabledInd = gpTransform.Find("P1HUD/InputDisabledIndicator")?.gameObject;
            var p2InputDisabledInd = gpTransform.Find("P2HUD/InputDisabledIndicator")?.gameObject;
            var uiExtraSO = new SerializedObject(uiMgr);
            SetRef(uiExtraSO, "abilitySystem",           abilitySys);
            SetRef(uiExtraSO, "p1InputDisabledIndicator", p1InputDisabledInd);
            SetRef(uiExtraSO, "p2InputDisabledIndicator", p2InputDisabledInd);
            SetRef(uiExtraSO, "p1TrashTalkOverlay",       p1TrashTalk);
            SetRef(uiExtraSO, "p2TrashTalkOverlay",       p2TrashTalk);
            SetRef(uiExtraSO, "p1DanceOverlay",           p1Dance);
            SetRef(uiExtraSO, "p2DanceOverlay",           p2Dance);
            SetRef(uiExtraSO, "p1CakeSplatOverlay",       p1Cake);
            SetRef(uiExtraSO, "p2CakeSplatOverlay",       p2Cake);
            uiExtraSO.ApplyModifiedProperties();

            // Wire CharacterSelectManager
            WireCharacterSelectManager(charSelectMgr,
                characterAssets, patzAsset,
                p1CSPortrait, p1CSName, p1CSAbility1, p1CSAbility2, p1CSReady,
                p2CSPortrait, p2CSName, p2CSAbility1, p2CSAbility2, p2CSReady,
                charGrid, prefab, startButton, instrText);

            // Wire GameManager
            WireGameManager(gm, awc, uiMgr);

            // Wire AbilitySystem
            WireAbilitySystem(abilitySys, awc, p1Controller, p2Controller,
                screenFlashOverlay.GetComponent<CanvasGroup>());

            // Wire ArmWrestleController bar visuals
            WireArmWrestleController(awc,
                barIndicator.GetComponent<RectTransform>(),
                barTrack.GetComponent<RectTransform>());

            // Wire MatchupDisplayController
            var matchupCtrl = gameplayPanel.GetComponent<MatchupDisplayController>();
            if (matchupCtrl == null) matchupCtrl = gameplayPanel.AddComponent<MatchupDisplayController>();
            WireMatchupDisplayController(matchupCtrl, matchupImageObj.GetComponent<Image>(), awc);

            // Wire PlayerControllers
            WirePlayerController(p1Controller, 1, awc, abilitySys);
            WirePlayerController(p2Controller, 2, awc, abilitySys);

            // Wire GameBootstrap
            WireGameBootstrap(bootstrap, gm, awc, abilitySys, uiMgr, audioMgr,
                charSelectMgr, cheatMgr, p1Controller, p2Controller);

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[UISceneBuilder] Game UI built successfully! Save the scene to preserve changes.");
        }

        #region Panel Builders

        private static GameObject BuildCharacterSelectPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "CharacterSelectPanel", true);
            StretchFill(panel);

            // Background
            var bg = panel.AddComponent<Image>();
            var charSelectSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Background/CharacterSelection.png");
            if (charSelectSprite != null)
            {
                bg.sprite = charSelectSprite;
                bg.color = Color.white;
                bg.type = Image.Type.Simple;
                bg.preserveAspect = false;
            }
            else
            {
                bg.color = PanelBg;
            }

            // P1 Panel (left)
            var p1Panel = BuildPlayerSelectPanel(panel.transform, "P1Panel", true);

            // Character Grid (center)
            var gridObj = new GameObject("CharacterGrid");
            gridObj.transform.SetParent(panel.transform, false);
            var gridRT = gridObj.AddComponent<RectTransform>();
            gridRT.anchorMin = new Vector2(0.25f, 0.3f);
            gridRT.anchorMax = new Vector2(0.75f, 0.75f);
            gridRT.anchoredPosition = new Vector2(0f, -12.89f);
            gridRT.sizeDelta = new Vector2(199.28f, -331.70f);
            var hlg = gridObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // P2 Panel (right)
            var p2Panel = BuildPlayerSelectPanel(panel.transform, "P2Panel", false);

            // Instructions
            var instr = CreateTMP(panel.transform, "InstructionsText",
                "P1: A/D to select, SPACE to ready up\nP2: Arrows to select, ENTER to ready up", 40, FontStyles.Normal, smallFont);
            var instrRT = instr.GetComponent<RectTransform>();
            instrRT.anchorMin = new Vector2(0.1f, 0.02f);
            instrRT.anchorMax = new Vector2(0.9f, 0.12f);
            instrRT.offsetMin = Vector2.zero;
            instrRT.offsetMax = Vector2.zero;
            instr.alignment = TextAlignmentOptions.Center;
            instr.color = Color.white;

            // Cheats button (bottom-left corner, small and subtle — it's an easter egg)
            var cheatsBtnObj = new GameObject("CheatsButton");
            cheatsBtnObj.transform.SetParent(panel.transform, false);
            var cheatsBtnRT = cheatsBtnObj.AddComponent<RectTransform>();
            cheatsBtnRT.anchorMin = new Vector2(0f, 0f);
            cheatsBtnRT.anchorMax = new Vector2(0.1f, 0.06f);
            cheatsBtnRT.offsetMin = new Vector2(8, 8);
            cheatsBtnRT.offsetMax = new Vector2(-4, -4);
            var cheatsBtnImg = cheatsBtnObj.AddComponent<Image>();
            cheatsBtnImg.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            var cheatsBtn = cheatsBtnObj.AddComponent<Button>();
            cheatsBtn.targetGraphic = cheatsBtnImg;
            var cheatsBtnText = CreateTMP(cheatsBtnObj.transform, "Text", "?", 20, FontStyles.Bold, displayFont);
            StretchFill(cheatsBtnText.gameObject);
            cheatsBtnText.alignment = TextAlignmentOptions.Center;

            // Cheat panel (floats over the whole char select screen)
            var cheatPanel = BuildCheatPanel(panel.transform,
                out var cheatInput, out var cheatSubmitBtn, out var cheatCloseBtn, out var cheatFeedback);

            // Attach and wire CheatCodeUI — openButton is the "?" cheats button
            var cheatCodeUI = panel.AddComponent<CheatCodeUI>();
            var cheatSO = new SerializedObject(cheatCodeUI);
            SetRef(cheatSO, "cheatPanel",    cheatPanel);
            SetRef(cheatSO, "openButton",    cheatsBtn);
            SetRef(cheatSO, "cheatInput",    cheatInput);
            SetRef(cheatSO, "submitButton",  cheatSubmitBtn);
            SetRef(cheatSO, "closeButton",   cheatCloseBtn);
            SetRef(cheatSO, "feedbackText",  cheatFeedback);
            cheatSO.ApplyModifiedProperties();

            // Start Button
            var startBtnObj = new GameObject("StartButton");
            startBtnObj.transform.SetParent(panel.transform, false);
            var startBtnRT = startBtnObj.AddComponent<RectTransform>();
            startBtnRT.anchorMin = new Vector2(0.35f, 0.13f);
            startBtnRT.anchorMax = new Vector2(0.65f, 0.22f);
            startBtnRT.offsetMin = Vector2.zero;
            startBtnRT.offsetMax = Vector2.zero;
            var startBtnImg = startBtnObj.AddComponent<Image>();
            startBtnImg.color = new Color(0.2f, 0.7f, 0.3f);
            var startBtn = startBtnObj.AddComponent<Button>();
            startBtn.targetGraphic = startBtnImg;
            startBtn.interactable = false;

            var startBtnText = CreateTMP(startBtnObj.transform, "Text", "START", 32, FontStyles.Bold, displayFont);
            StretchFill(startBtnText.gameObject);
            startBtnText.alignment = TextAlignmentOptions.Center;

            return panel;
        }

        private static GameObject BuildPlayerSelectPanel(Transform parent, string name, bool isLeft)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rt = panel.AddComponent<RectTransform>();

            if (isLeft)
            {
                rt.anchorMin = new Vector2(0.02f, 0.15f);
                rt.anchorMax = new Vector2(0.22f, 0.82f);
            }
            else
            {
                rt.anchorMin = new Vector2(0.78f, 0.15f);
                rt.anchorMax = new Vector2(0.98f, 0.82f);
            }
            rt.anchoredPosition = new Vector2(0f, 319f);
            rt.sizeDelta = new Vector2(0f, -334.01f);

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Portrait
            var portraitObj = new GameObject("Portrait");
            portraitObj.transform.SetParent(panel.transform, false);
            var portraitImg = portraitObj.AddComponent<Image>();
            portraitImg.color = Color.white;
            portraitImg.preserveAspect = true;
            var portraitLE = portraitObj.AddComponent<LayoutElement>();
            portraitLE.preferredHeight = 128;
            portraitLE.preferredWidth = 128;

            // Name
            var nameText = CreateTMP(panel.transform, "NameText", isLeft ? "Player 1" : "Player 2", 28, FontStyles.Bold, displayFont);
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = isLeft ? P1Color : P2Color;
            var nameLE = nameText.gameObject.AddComponent<LayoutElement>();
            nameLE.preferredHeight = 36;

            // Ability 1
            var ab1 = CreateTMP(panel.transform, "Ability1Text", isLeft ? "Q: ---" : "O: ---", 20);
            ab1.alignment = TextAlignmentOptions.Center;
            var ab1LE = ab1.gameObject.AddComponent<LayoutElement>();
            ab1LE.preferredHeight = 28;

            // Ability 2
            var ab2 = CreateTMP(panel.transform, "Ability2Text", isLeft ? "E: ---" : "P: ---", 20);
            ab2.alignment = TextAlignmentOptions.Center;
            var ab2LE = ab2.gameObject.AddComponent<LayoutElement>();
            ab2LE.preferredHeight = 28;

            // Ready Indicator
            var readyObj = new GameObject("ReadyIndicator");
            readyObj.transform.SetParent(panel.transform, false);
            var readyImg = readyObj.AddComponent<Image>();
            readyImg.color = new Color(0.25f, 0.25f, 0.3f, 0.8f);
            var readyRT = readyObj.GetComponent<RectTransform>();
            readyRT.sizeDelta = new Vector2(readyRT.sizeDelta.x, 60f);
            var readyLE = readyObj.AddComponent<LayoutElement>();
            readyLE.preferredHeight = 60;
            var readyText = CreateTMP(readyObj.transform, "Text", "READY", 28, FontStyles.Bold, displayFont);
            StretchFill(readyText.gameObject);
            readyText.alignment = TextAlignmentOptions.Center;
            readyText.color = new Color(0.5f, 0.5f, 0.5f);

            return panel;
        }

        private static GameObject BuildGameplayPanel(Transform parent,
            out Image barTrack, out Image barFillLeft, out Image barFillRight, out Image barIndicator,
            out TextMeshProUGUI roundText, out TextMeshProUGUI timerText, out Image timerFill,
            out Image p1Portrait, out TextMeshProUGUI p1NameText,
            out Image p1Ab1Cd, out Image p1Ab2Cd,
            out TextMeshProUGUI p1Ab1Key, out TextMeshProUGUI p1Ab2Key,
            out TextMeshProUGUI p1Ab1Name, out TextMeshProUGUI p1Ab2Name,
            out GameObject p1ShieldInd, out GameObject p1PowerSurgeInd,
            out Image p2Portrait, out TextMeshProUGUI p2NameText,
            out Image p2Ab1Cd, out Image p2Ab2Cd,
            out TextMeshProUGUI p2Ab1Key, out TextMeshProUGUI p2Ab2Key,
            out TextMeshProUGUI p2Ab1Name, out TextMeshProUGUI p2Ab2Name,
            out GameObject p2ShieldInd, out GameObject p2PowerSurgeInd,
            out TextMeshProUGUI p1ScoreText, out TextMeshProUGUI p2ScoreText,
            out GameObject controlsReversedInd)
        {
            var panel = CreatePanel(parent, "GameplayPanel", true);
            StretchFill(panel);

            // Top Bar
            var topBar = new GameObject("TopBar");
            topBar.transform.SetParent(panel.transform, false);
            var topBarRT = topBar.AddComponent<RectTransform>();
            topBarRT.anchorMin = new Vector2(0f, 0.9f);
            topBarRT.anchorMax = new Vector2(1f, 1f);
            topBarRT.offsetMin = Vector2.zero;
            topBarRT.offsetMax = Vector2.zero;
            var topBarBg = topBar.AddComponent<Image>();
            topBarBg.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

            // Round Text
            var roundTmp = CreateTMP(topBar.transform, "RoundText", "Round 1", 28, FontStyles.Bold);
            var roundRT = roundTmp.GetComponent<RectTransform>();
            roundRT.anchorMin = new Vector2(0.02f, 0f);
            roundRT.anchorMax = new Vector2(0.25f, 1f);
            roundRT.offsetMin = Vector2.zero;
            roundRT.offsetMax = Vector2.zero;
            roundTmp.alignment = TextAlignmentOptions.MidlineLeft;
            roundText = roundTmp;

            // Timer Text (large, center)
            var timerTmp = CreateTMP(topBar.transform, "TimerText", "45", 48, FontStyles.Bold, titleFont);
            var timerRT = timerTmp.GetComponent<RectTransform>();
            timerRT.anchorMin = new Vector2(0.4f, 0f);
            timerRT.anchorMax = new Vector2(0.6f, 1f);
            timerRT.offsetMin = Vector2.zero;
            timerRT.offsetMax = Vector2.zero;
            timerTmp.alignment = TextAlignmentOptions.Center;
            timerText = timerTmp;

            // Timer Fill (background bar behind timer)
            var timerFillObj = new GameObject("TimerFill");
            timerFillObj.transform.SetParent(topBar.transform, false);
            timerFillObj.transform.SetAsFirstSibling(); // behind text
            var timerFillRT = timerFillObj.AddComponent<RectTransform>();
            timerFillRT.anchorMin = new Vector2(0.25f, 0.1f);
            timerFillRT.anchorMax = new Vector2(0.75f, 0.9f);
            timerFillRT.offsetMin = Vector2.zero;
            timerFillRT.offsetMax = Vector2.zero;
            var timerFillImg = timerFillObj.AddComponent<Image>();
            timerFillImg.color = new Color(0.3f, 0.8f, 0.3f, 0.4f);
            timerFillImg.type = Image.Type.Filled;
            timerFillImg.fillMethod = Image.FillMethod.Horizontal;
            timerFillImg.fillAmount = 1f;
            timerFill = timerFillImg;

            // P1 HUD (left)
            BuildPlayerHUD(panel.transform, "P1HUD", true,
                out p1Portrait, out p1NameText, out p1Ab1Cd, out p1Ab2Cd,
                out p1Ab1Key, out p1Ab2Key, out p1Ab1Name, out p1Ab2Name,
                out p1ShieldInd, out p1PowerSurgeInd);

            // P2 HUD (right)
            BuildPlayerHUD(panel.transform, "P2HUD", false,
                out p2Portrait, out p2NameText, out p2Ab1Cd, out p2Ab2Cd,
                out p2Ab1Key, out p2Ab2Key, out p2Ab1Name, out p2Ab2Name,
                out p2ShieldInd, out p2PowerSurgeInd);

            // Bar Area (center-bottom)
            var barArea = new GameObject("BarArea");
            barArea.transform.SetParent(panel.transform, false);
            var barAreaRT = barArea.AddComponent<RectTransform>();
            barAreaRT.anchorMin = new Vector2(0.15f, 0.1f);
            barAreaRT.anchorMax = new Vector2(0.85f, 0.22f);
            barAreaRT.offsetMin = Vector2.zero;
            barAreaRT.offsetMax = Vector2.zero;

            // Bar Track
            var trackObj = new GameObject("BarTrack");
            trackObj.transform.SetParent(barArea.transform, false);
            var trackRT = trackObj.AddComponent<RectTransform>();
            trackRT.anchorMin = new Vector2(0f, 0.15f);
            trackRT.anchorMax = new Vector2(1f, 0.85f);
            trackRT.offsetMin = Vector2.zero;
            trackRT.offsetMax = Vector2.zero;
            var trackImg = trackObj.AddComponent<Image>();
            trackImg.color = BarTrackColor;
            barTrack = trackImg;

            // Bar Fill Left (P1 blue, fills from right to left)
            var fillLeftObj = new GameObject("BarFillLeft");
            fillLeftObj.transform.SetParent(trackObj.transform, false);
            StretchFill(fillLeftObj);
            var fillLeftImg = fillLeftObj.AddComponent<Image>();
            fillLeftImg.color = new Color(P1Color.r, P1Color.g, P1Color.b, 0.6f);
            fillLeftImg.type = Image.Type.Filled;
            fillLeftImg.fillMethod = Image.FillMethod.Horizontal;
            fillLeftImg.fillOrigin = (int)Image.OriginHorizontal.Right;
            fillLeftImg.fillAmount = 0.5f;
            barFillLeft = fillLeftImg;

            // Bar Fill Right (P2 red, fills from left to right)
            var fillRightObj = new GameObject("BarFillRight");
            fillRightObj.transform.SetParent(trackObj.transform, false);
            StretchFill(fillRightObj);
            var fillRightImg = fillRightObj.AddComponent<Image>();
            fillRightImg.color = new Color(P2Color.r, P2Color.g, P2Color.b, 0.6f);
            fillRightImg.type = Image.Type.Filled;
            fillRightImg.fillMethod = Image.FillMethod.Horizontal;
            fillRightImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillRightImg.fillAmount = 0.5f;
            barFillRight = fillRightImg;

            // Bar Indicator (white marker)
            var indicatorObj = new GameObject("BarIndicator");
            indicatorObj.transform.SetParent(barArea.transform, false);
            var indicatorRT = indicatorObj.AddComponent<RectTransform>();
            indicatorRT.anchorMin = new Vector2(0.5f, 0f);
            indicatorRT.anchorMax = new Vector2(0.5f, 1f);
            indicatorRT.sizeDelta = new Vector2(10, 0);
            indicatorRT.anchoredPosition = Vector2.zero;
            var indicatorImg = indicatorObj.AddComponent<Image>();
            indicatorImg.color = Color.white;
            barIndicator = indicatorImg;

            // Score Display
            var scoreArea = new GameObject("ScoreDisplay");
            scoreArea.transform.SetParent(panel.transform, false);
            var scoreRT = scoreArea.AddComponent<RectTransform>();
            scoreRT.anchorMin = new Vector2(0.15f, 0.22f);
            scoreRT.anchorMax = new Vector2(0.85f, 0.3f);
            scoreRT.offsetMin = Vector2.zero;
            scoreRT.offsetMax = Vector2.zero;

            var p1Score = CreateTMP(scoreArea.transform, "P1ScoreText", "0", 36, FontStyles.Bold, titleFont);
            var p1ScoreRT = p1Score.GetComponent<RectTransform>();
            p1ScoreRT.anchorMin = new Vector2(0f, 0f);
            p1ScoreRT.anchorMax = new Vector2(0.3f, 1f);
            p1ScoreRT.offsetMin = Vector2.zero;
            p1ScoreRT.offsetMax = Vector2.zero;
            p1Score.alignment = TextAlignmentOptions.Center;
            p1Score.color = P1Color;
            p1ScoreText = p1Score;

            var p2Score = CreateTMP(scoreArea.transform, "P2ScoreText", "0", 36, FontStyles.Bold, titleFont);
            var p2ScoreRT = p2Score.GetComponent<RectTransform>();
            p2ScoreRT.anchorMin = new Vector2(0.7f, 0f);
            p2ScoreRT.anchorMax = new Vector2(1f, 1f);
            p2ScoreRT.offsetMin = Vector2.zero;
            p2ScoreRT.offsetMax = Vector2.zero;
            p2Score.alignment = TextAlignmentOptions.Center;
            p2Score.color = P2Color;
            p2ScoreText = p2Score;

            // Controls Reversed Indicator (center overlay)
            var reversedObj = new GameObject("ControlsReversedIndicator");
            reversedObj.transform.SetParent(panel.transform, false);
            var reversedRT = reversedObj.AddComponent<RectTransform>();
            reversedRT.anchorMin = new Vector2(0.3f, 0.4f);
            reversedRT.anchorMax = new Vector2(0.7f, 0.55f);
            reversedRT.offsetMin = Vector2.zero;
            reversedRT.offsetMax = Vector2.zero;
            var reversedBg = reversedObj.AddComponent<Image>();
            reversedBg.color = new Color(0.8f, 0.2f, 0.8f, 0.7f);
            var reversedText = CreateTMP(reversedObj.transform, "Text", "CONTROLS REVERSED!", 30, FontStyles.Bold, displayFont);
            StretchFill(reversedText.gameObject);
            reversedText.alignment = TextAlignmentOptions.Center;
            reversedText.color = Color.white;
            controlsReversedInd = reversedObj;
            reversedObj.SetActive(false);

            return panel;
        }

        private static void BuildPlayerHUD(Transform parent, string name, bool isLeft,
            out Image portrait, out TextMeshProUGUI nameText,
            out Image ab1Cd, out Image ab2Cd,
            out TextMeshProUGUI ab1Key, out TextMeshProUGUI ab2Key,
            out TextMeshProUGUI ab1Name, out TextMeshProUGUI ab2Name,
            out GameObject shieldInd, out GameObject powerSurgeInd)
        {
            var hud = new GameObject(name);
            hud.transform.SetParent(parent, false);
            var hudRT = hud.AddComponent<RectTransform>();

            if (isLeft)
            {
                hudRT.anchorMin = new Vector2(0.02f, 0.3f);
                hudRT.anchorMax = new Vector2(0.15f, 0.88f);
            }
            else
            {
                hudRT.anchorMin = new Vector2(0.85f, 0.3f);
                hudRT.anchorMax = new Vector2(0.98f, 0.88f);
            }
            hudRT.offsetMin = Vector2.zero;
            hudRT.offsetMax = Vector2.zero;

            var bg = hud.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.18f, 0.8f);

            var vlg = hud.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Portrait
            var portraitObj = new GameObject("Portrait");
            portraitObj.transform.SetParent(hud.transform, false);
            portrait = portraitObj.AddComponent<Image>();
            portrait.color = Color.gray;
            portrait.preserveAspect = true;
            var portraitLE = portraitObj.AddComponent<LayoutElement>();
            portraitLE.preferredHeight = 80;

            // Name
            nameText = CreateTMP(hud.transform, "NameText", isLeft ? "P1" : "P2", 22, FontStyles.Bold, displayFont);
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = isLeft ? P1Color : P2Color;
            var nameLE = nameText.gameObject.AddComponent<LayoutElement>();
            nameLE.preferredHeight = 28;

            // Ability 1 Group
            BuildAbilityGroup(hud.transform, "Ability1Group", isLeft ? "Q" : "O", "Ability 1",
                out ab1Cd, out ab1Key, out ab1Name);

            // Ability 2 Group
            BuildAbilityGroup(hud.transform, "Ability2Group", isLeft ? "E" : "P", "Ability 2",
                out ab2Cd, out ab2Key, out ab2Name);

            // Shield Indicator
            var shieldObj = new GameObject("ShieldIndicator");
            shieldObj.transform.SetParent(hud.transform, false);
            var shieldImg = shieldObj.AddComponent<Image>();
            shieldImg.color = new Color(0.3f, 0.8f, 1f, 0.8f);
            var shieldLE = shieldObj.AddComponent<LayoutElement>();
            shieldLE.preferredHeight = 24;
            var shieldText = CreateTMP(shieldObj.transform, "Text", "SHIELD", 14, FontStyles.Bold, smallFont);
            StretchFill(shieldText.gameObject);
            shieldText.alignment = TextAlignmentOptions.Center;
            shieldInd = shieldObj;
            shieldObj.SetActive(false);

            // Power Surge Indicator
            var surgeObj = new GameObject("PowerSurgeIndicator");
            surgeObj.transform.SetParent(hud.transform, false);
            var surgeImg = surgeObj.AddComponent<Image>();
            surgeImg.color = new Color(1f, 0.8f, 0.2f, 0.8f);
            var surgeLE = surgeObj.AddComponent<LayoutElement>();
            surgeLE.preferredHeight = 24;
            var surgeText = CreateTMP(surgeObj.transform, "Text", "POWER SURGE!", 14, FontStyles.Bold, smallFont);
            StretchFill(surgeText.gameObject);
            surgeText.alignment = TextAlignmentOptions.Center;
            powerSurgeInd = surgeObj;
            surgeObj.SetActive(false);

            // Input Disabled (Stunned) Indicator
            var stunnedObj = new GameObject("InputDisabledIndicator");
            stunnedObj.transform.SetParent(hud.transform, false);
            var stunnedImg = stunnedObj.AddComponent<Image>();
            stunnedImg.color = new Color(0.8f, 0.5f, 0.1f, 0.8f);
            var stunnedLE = stunnedObj.AddComponent<LayoutElement>();
            stunnedLE.preferredHeight = 24;
            var stunnedText = CreateTMP(stunnedObj.transform, "Text", "STUNNED!", 14, FontStyles.Bold, smallFont);
            StretchFill(stunnedText.gameObject);
            stunnedText.alignment = TextAlignmentOptions.Center;
            stunnedObj.SetActive(false);
        }

        private static void BuildAbilityGroup(Transform parent, string name, string key, string abilityName,
            out Image cooldownImg, out TextMeshProUGUI keyText, out TextMeshProUGUI nameText)
        {
            var group = new GameObject(name);
            group.transform.SetParent(parent, false);
            group.AddComponent<RectTransform>();
            var groupLE = group.AddComponent<LayoutElement>();
            groupLE.preferredHeight = 50;

            // Cooldown overlay (radial fill)
            var cdObj = new GameObject("Cooldown");
            cdObj.transform.SetParent(group.transform, false);
            var cdRT = cdObj.AddComponent<RectTransform>();
            cdRT.anchorMin = new Vector2(0f, 0f);
            cdRT.anchorMax = new Vector2(0.35f, 1f);
            cdRT.offsetMin = Vector2.zero;
            cdRT.offsetMax = Vector2.zero;
            cooldownImg = cdObj.AddComponent<Image>();
            cooldownImg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            cooldownImg.type = Image.Type.Filled;
            cooldownImg.fillMethod = Image.FillMethod.Radial360;
            cooldownImg.fillAmount = 0f;

            // Key text (over cooldown area)
            keyText = CreateTMP(cdObj.transform, "KeyText", key, 20, FontStyles.Bold, smallFont);
            StretchFill(keyText.gameObject);
            keyText.alignment = TextAlignmentOptions.Center;
            keyText.color = Color.white;

            // Name text (right side)
            nameText = CreateTMP(group.transform, "NameText", abilityName, 16);
            var nameRT = nameText.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.38f, 0f);
            nameRT.anchorMax = new Vector2(1f, 1f);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            nameText.color = new Color(0.85f, 0.85f, 0.85f);
        }

        private static void AddVFXSpritesToExistingIndicators(Transform gameplayRoot)
        {
            // Load VFX sprites
            var surgeSpr   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/vfx_power_surge.png");
            var heartsSpr  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/vfx_wink_hearts.png");
            var arrowsSpr  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/vfx_fake_out.png");
            var shieldSpr  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/vfx_shield.png");

            // Power Surge: add sprite to P1 and P2 surge indicators
            EmbedVFXSprite(gameplayRoot.Find("P1HUD/PowerSurgeIndicator"), surgeSpr);
            EmbedVFXSprite(gameplayRoot.Find("P2HUD/PowerSurgeIndicator"), surgeSpr);

            // Input Disabled (Wink/Flash): add hearts sprite
            EmbedVFXSprite(gameplayRoot.Find("P1HUD/InputDisabledIndicator"), heartsSpr);
            EmbedVFXSprite(gameplayRoot.Find("P2HUD/InputDisabledIndicator"), heartsSpr);

            // Shield: add sprite to shield indicators
            EmbedVFXSprite(gameplayRoot.Find("P1HUD/ShieldIndicator"), shieldSpr);
            EmbedVFXSprite(gameplayRoot.Find("P2HUD/ShieldIndicator"), shieldSpr);

            // Controls Reversed: add arrows sprite
            EmbedVFXSprite(gameplayRoot.Find("ControlsReversedIndicator"), arrowsSpr);
        }

        private static void EmbedVFXSprite(Transform indicator, Sprite sprite)
        {
            if (indicator == null || sprite == null) return;

            // Remove any existing VFX child to avoid duplicates on rebuild
            var existing = indicator.Find("VFXSprite");
            if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);

            var vfxObj = new GameObject("VFXSprite");
            vfxObj.transform.SetParent(indicator, false);
            // Ignore layout so it floats over the panel text
            var le = vfxObj.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            // Stretch to fill the indicator
            var rt = vfxObj.GetComponent<RectTransform>();
            if (rt == null) rt = vfxObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = vfxObj.AddComponent<Image>();
            img.sprite = sprite;
            img.color = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        private static void BuildVFXOverlays(Transform gameplayRoot,
            out GameObject p1TrashTalk, out GameObject p2TrashTalk,
            out GameObject p1Dance,     out GameObject p2Dance,
            out GameObject p1Cake,      out GameObject p2Cake)
        {
            var trashSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/vfx_trash_talk.png");
            var noteSpr  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/vfx_dance_notes.png");
            var cakeSpr  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/vfx_cake_splat.png");

            // P1-side overlays (appear on P1's half when P1 is the target)
            p1TrashTalk = BuildVFXOverlayObject(gameplayRoot, "P1TrashTalkOverlay", trashSpr, isLeft: true);
            p1Dance     = BuildVFXOverlayObject(gameplayRoot, "P1DanceOverlay",     noteSpr,  isLeft: true);
            p1Cake      = BuildVFXOverlayObject(gameplayRoot, "P1CakeSplatOverlay", cakeSpr,  isLeft: true);

            // P2-side overlays
            p2TrashTalk = BuildVFXOverlayObject(gameplayRoot, "P2TrashTalkOverlay", trashSpr, isLeft: false);
            p2Dance     = BuildVFXOverlayObject(gameplayRoot, "P2DanceOverlay",     noteSpr,  isLeft: false);
            p2Cake      = BuildVFXOverlayObject(gameplayRoot, "P2CakeSplatOverlay", cakeSpr,  isLeft: false);
        }

        private static GameObject BuildVFXOverlayObject(Transform parent, string name, Sprite sprite, bool isLeft)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rt = obj.AddComponent<RectTransform>();

            // Position over the left or right HUD area, slightly inset into the center
            if (isLeft)
            {
                rt.anchorMin = new Vector2(0.02f, 0.5f);
                rt.anchorMax = new Vector2(0.22f, 0.85f);
            }
            else
            {
                rt.anchorMin = new Vector2(0.78f, 0.5f);
                rt.anchorMax = new Vector2(0.98f, 0.85f);
            }
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.color = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;

            obj.SetActive(false);
            return obj;
        }

        private static GameObject BuildMatchupImage(Transform parent)
        {
            var obj = new GameObject("MatchupDisplay");
            obj.transform.SetParent(parent, false);
            var rt = obj.AddComponent<RectTransform>();
            // Centre area: between the two HUDs and above bar/score
            rt.anchorMin = new Vector2(0.15f, 0.3f);
            rt.anchorMax = new Vector2(0.85f, 0.88f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = obj.AddComponent<Image>();
            img.color = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;
            obj.SetActive(false); // hidden until a matchup is loaded at round start
            return obj;
        }

        private static GameObject BuildOverlayPanel(Transform parent, string name,
            out TextMeshProUGUI mainText, string defaultText, int fontSize)
        {
            var panel = CreatePanel(parent, name, true);
            StretchFill(panel);

            var bg = panel.AddComponent<Image>();
            bg.color = OverlayBg;

            var cg = panel.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            mainText = CreateTMP(panel.transform, name.Replace("Panel", "Text"), defaultText, fontSize, FontStyles.Bold, titleFont);
            var textRT = mainText.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.1f, 0.3f);
            textRT.anchorMax = new Vector2(0.9f, 0.7f);
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
            mainText.alignment = TextAlignmentOptions.Center;
            mainText.color = Color.white;

            return panel;
        }

        private static GameObject BuildMatchEndPanel(Transform parent,
            out TextMeshProUGUI winnerText, out TextMeshProUGUI instrText,
            out Image victoryBgImage)
        {
            var panel = CreatePanel(parent, "MatchEndPanel", true);
            StretchFill(panel);

            // Victory background (hidden until match ends, swapped by UIManager)
            var victoryBgObj = new GameObject("VictoryBackground");
            victoryBgObj.transform.SetParent(panel.transform, false);
            victoryBgObj.transform.SetAsFirstSibling();
            StretchFill(victoryBgObj);
            victoryBgImage = victoryBgObj.AddComponent<Image>();
            victoryBgImage.color = Color.white;
            victoryBgImage.preserveAspect = false;
            victoryBgObj.SetActive(false);

            // Dark overlay on top of victory bg
            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.45f);

            var cg = panel.AddComponent<CanvasGroup>();

            winnerText = CreateTMP(panel.transform, "MatchWinnerText", "PLAYER WINS!", 72, FontStyles.Bold, titleFont);
            var winRT = winnerText.GetComponent<RectTransform>();
            winRT.anchorMin = new Vector2(0.1f, 0.45f);
            winRT.anchorMax = new Vector2(0.9f, 0.75f);
            winRT.offsetMin = Vector2.zero;
            winRT.offsetMax = Vector2.zero;
            winnerText.alignment = TextAlignmentOptions.Center;
            winnerText.color = new Color(1f, 0.85f, 0.2f);

            instrText = CreateTMP(panel.transform, "MatchEndInstructionsText",
                "SPACE / ENTER = Rematch\nESC / BACKSPACE = Character Select", 28, FontStyles.Normal, displayFont);
            var instrRT = instrText.GetComponent<RectTransform>();
            instrRT.anchorMin = new Vector2(0.15f, 0.2f);
            instrRT.anchorMax = new Vector2(0.85f, 0.42f);
            instrRT.offsetMin = Vector2.zero;
            instrRT.offsetMax = Vector2.zero;
            instrText.alignment = TextAlignmentOptions.Center;
            instrText.color = Color.gray;

            return panel;
        }

        private static GameObject BuildTitleScreenPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "TitleScreenPanel", true);
            StretchFill(panel);

            // Background
            var bg = panel.AddComponent<Image>();
            var titleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Background/TitleScreen.png");
            if (titleSprite != null)
            {
                bg.sprite = titleSprite;
                bg.color = Color.white;
                bg.type = Image.Type.Simple;
                bg.preserveAspect = false;
            }
            else
            {
                bg.color = new Color(0.08f, 0.05f, 0.02f);
            }

            // Title text
            var title = CreateTMP(panel.transform, "TitleText", "DIRTY THIRTY\nSHOWDOWN", 36, FontStyles.Bold, titleFont);
            var titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.1f, 0.65f);
            titleRT.anchorMax = new Vector2(0.9f, 0.93f);
            titleRT.anchoredPosition = new Vector2(-748.63f, 161.61f);
            titleRT.sizeDelta = new Vector2(-1098.696f, -177.5493f);
            title.alignment = TextAlignmentOptions.Center;
            title.color = Color.white;

            // Button container (centered, stacked vertically)
            var btnContainer = new GameObject("ButtonContainer");
            btnContainer.transform.SetParent(panel.transform, false);
            var btnContainerRT = btnContainer.AddComponent<RectTransform>();
            btnContainerRT.anchorMin = new Vector2(0.35f, 0.2f);
            btnContainerRT.anchorMax = new Vector2(0.65f, 0.62f);
            btnContainerRT.anchoredPosition = new Vector2(-777f, -245f);
            btnContainerRT.sizeDelta = new Vector2(-213.1123f, -135.7732f);
            var vlg = btnContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 16;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = true;

            var playBtn    = BuildMenuButton(btnContainer.transform, "PlayButton",    "PLAY",    new Color(0.2f, 0.65f, 0.25f));
            var optionsBtn = BuildMenuButton(btnContainer.transform, "OptionsButton", "OPTIONS", new Color(0.2f, 0.35f, 0.65f));
            var exitBtn    = BuildMenuButton(btnContainer.transform, "ExitButton",    "EXIT",    new Color(0.55f, 0.12f, 0.12f));

            // Options panel (stub)
            var optionsPanel = CreatePanel(panel.transform, "OptionsPanel", true);
            StretchFill(optionsPanel);
            optionsPanel.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

            var optionsTitle = CreateTMP(optionsPanel.transform, "OptionsTitle", "OPTIONS", 48, FontStyles.Bold, titleFont);
            var optTitleRT = optionsTitle.GetComponent<RectTransform>();
            optTitleRT.anchorMin = new Vector2(0.2f, 0.75f);
            optTitleRT.anchorMax = new Vector2(0.8f, 0.9f);
            optTitleRT.offsetMin = Vector2.zero;
            optTitleRT.offsetMax = Vector2.zero;
            optionsTitle.alignment = TextAlignmentOptions.Center;
            optionsTitle.color = new Color(1f, 0.85f, 0.2f);

            var optionsContent = CreateTMP(optionsPanel.transform, "OptionsContent", "Coming soon...", 28);
            var optContentRT = optionsContent.GetComponent<RectTransform>();
            optContentRT.anchorMin = new Vector2(0.2f, 0.4f);
            optContentRT.anchorMax = new Vector2(0.8f, 0.72f);
            optContentRT.offsetMin = Vector2.zero;
            optContentRT.offsetMax = Vector2.zero;
            optionsContent.alignment = TextAlignmentOptions.Center;
            optionsContent.color = Color.gray;

            var optionsCloseBtn = BuildMenuButton(optionsPanel.transform, "CloseButton", "CLOSE", new Color(0.45f, 0.45f, 0.45f));
            var optionsCloseBtnRT = optionsCloseBtn.GetComponent<RectTransform>();
            optionsCloseBtnRT.anchorMin = new Vector2(0.35f, 0.1f);
            optionsCloseBtnRT.anchorMax = new Vector2(0.65f, 0.22f);
            optionsCloseBtnRT.offsetMin = Vector2.zero;
            optionsCloseBtnRT.offsetMax = Vector2.zero;
            optionsPanel.SetActive(false);

            // Attach and wire TitleScreenManager
            var tsm = panel.AddComponent<TitleScreenManager>();
            var tsmSO = new SerializedObject(tsm);
            SetRef(tsmSO, "playButton",        playBtn);
            SetRef(tsmSO, "optionsButton",     optionsBtn);
            SetRef(tsmSO, "exitButton",        exitBtn);
            SetRef(tsmSO, "optionsPanel",      optionsPanel);
            SetRef(tsmSO, "optionsCloseButton", optionsCloseBtn);
            tsmSO.ApplyModifiedProperties();

            return panel;
        }

        private static Button BuildMenuButton(Transform parent, string name, string label, Color bgColor)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            btnObj.AddComponent<RectTransform>();
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = bgColor;
            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var text = CreateTMP(btnObj.transform, "Text", label, 32, FontStyles.Bold, displayFont);
            StretchFill(text.gameObject);
            text.alignment = TextAlignmentOptions.Center;
            return btn;
        }

        private static GameObject BuildCheatPanel(Transform parent,
            out TMP_InputField inputField, out Button submitBtn, out Button closeBtn,
            out TextMeshProUGUI feedbackText)
        {
            var panel = CreatePanel(parent, "CheatPanel", true);
            StretchFill(panel);
            panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);

            var title = CreateTMP(panel.transform, "CheatTitle", "ENTER CHEAT CODE", 40, FontStyles.Bold, titleFont);
            var titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.1f, 0.6f);
            titleRT.anchorMax = new Vector2(0.9f, 0.75f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            title.alignment = TextAlignmentOptions.Center;
            title.color = new Color(1f, 0.85f, 0.2f);

            // Input field
            var inputObj = new GameObject("CheatInput");
            inputObj.transform.SetParent(panel.transform, false);
            var inputRT = inputObj.AddComponent<RectTransform>();
            inputRT.anchorMin = new Vector2(0.15f, 0.45f);
            inputRT.anchorMax = new Vector2(0.85f, 0.58f);
            inputRT.offsetMin = Vector2.zero;
            inputRT.offsetMax = Vector2.zero;
            var inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(0.15f, 0.15f, 0.2f);

            inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.targetGraphic = inputBg;

            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            StretchFill(textArea);

            var inputText = CreateTMP(textArea.transform, "Text", "", 32, FontStyles.Normal, smallFont);
            StretchFill(inputText.gameObject);

            var placeholder = CreateTMP(textArea.transform, "Placeholder", "type cheat code here...", 32, FontStyles.Normal, smallFont);
            StretchFill(placeholder.gameObject);
            placeholder.color = Color.gray;
            placeholder.fontStyle = FontStyles.Italic;

            inputField.textViewport = textArea.GetComponent<RectTransform>();
            inputField.textComponent = inputText;
            inputField.placeholder = placeholder;

            // Submit button
            submitBtn = BuildMenuButton(panel.transform, "SubmitButton", "SUBMIT", new Color(0.2f, 0.65f, 0.25f));
            var submitRT = submitBtn.GetComponent<RectTransform>();
            submitRT.anchorMin = new Vector2(0.2f, 0.28f);
            submitRT.anchorMax = new Vector2(0.48f, 0.42f);
            submitRT.offsetMin = Vector2.zero;
            submitRT.offsetMax = Vector2.zero;

            // Close button
            closeBtn = BuildMenuButton(panel.transform, "CloseButton", "CLOSE", new Color(0.5f, 0.18f, 0.18f));
            var closeRT = closeBtn.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(0.52f, 0.28f);
            closeRT.anchorMax = new Vector2(0.8f, 0.42f);
            closeRT.offsetMin = Vector2.zero;
            closeRT.offsetMax = Vector2.zero;

            // Feedback text — sits just below the input field
            feedbackText = CreateTMP(panel.transform, "FeedbackText", "", 24, FontStyles.Normal, displayFont);
            var feedbackRT = feedbackText.GetComponent<RectTransform>();
            feedbackRT.anchorMin = new Vector2(0.1f, 0.43f);
            feedbackRT.anchorMax = new Vector2(0.9f, 0.47f);
            feedbackRT.offsetMin = Vector2.zero;
            feedbackRT.offsetMax = Vector2.zero;
            feedbackText.alignment = TextAlignmentOptions.Center;
            feedbackText.gameObject.SetActive(false);

            panel.SetActive(false);
            return panel;
        }

        private static GameObject BuildScreenFlashOverlay(Transform parent)
        {
            var obj = new GameObject("ScreenFlashOverlay");
            obj.transform.SetParent(parent, false);
            StretchFill(obj);

            var img = obj.AddComponent<Image>();
            img.color = Color.white;
            img.raycastTarget = false;

            var cg = obj.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            return obj;
        }

        #endregion

        #region Character Button Prefab

        private static void PlaceCharacterButtonsInGrid(Transform gridParent, CharacterData[] characters, GameObject buttonPrefab)
        {
            if (gridParent == null || characters == null || buttonPrefab == null) return;

            // Clear any stale children from a previous builder run
            for (int i = gridParent.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(gridParent.GetChild(i).gameObject);

            foreach (var character in characters)
            {
                if (character == null) continue;

                var btnObj = (GameObject)PrefabUtility.InstantiatePrefab(buttonPrefab, gridParent);
                Undo.RegisterCreatedObjectUndo(btnObj, $"Create {character.characterName} Button");
                btnObj.name = $"CharButton_{character.characterName}";

                var portrait = btnObj.transform.Find("Portrait")?.GetComponent<Image>();
                if (portrait != null && character.characterPortrait != null)
                    portrait.sprite = character.characterPortrait;

                var nameText = btnObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = character.characterName;
            }
        }

        private static GameObject CreateCharacterButtonPrefab()
        {
            string prefabPath = "Assets/Prefabs/CharacterButton.prefab";

            // Check if prefab already exists
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                return existing;
            }

            // Create a temporary GameObject for the prefab
            var btnObj = new GameObject("CharacterButton");

            var btnRT = btnObj.AddComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(150, 200);

            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.2f, 0.25f);

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var vlg = btnObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.padding = new RectOffset(5, 5, 10, 5);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Portrait — must be named "Portrait"
            var portraitObj = new GameObject("Portrait");
            portraitObj.transform.SetParent(btnObj.transform, false);
            var portraitImg = portraitObj.AddComponent<Image>();
            portraitImg.color = Color.white;
            portraitImg.preserveAspect = true;
            var portraitLE = portraitObj.AddComponent<LayoutElement>();
            portraitLE.preferredHeight = 120;

            // Name — must be named "Name"
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(btnObj.transform, false);
            nameObj.AddComponent<RectTransform>();
            var nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = "Character";
            nameTMP.fontSize = 20;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.color = Color.white;
            var nameFont = displayFont ?? defaultFont;
            if (nameFont != null) nameTMP.font = nameFont;
            var nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.preferredHeight = 30;

            // Highlight — border-style selection indicator (named "Highlight")
            var highlightObj = new GameObject("Highlight");
            highlightObj.transform.SetParent(btnObj.transform, false);
            var highlightRT = highlightObj.AddComponent<RectTransform>();
            highlightRT.anchorMin = Vector2.zero;
            highlightRT.anchorMax = Vector2.one;
            highlightRT.offsetMin = Vector2.zero;
            highlightRT.offsetMax = Vector2.zero;
            CreateBorderImages(highlightObj.transform, 3f);
            highlightObj.SetActive(false);
            // Remove from layout
            var highlightLI = highlightObj.AddComponent<LayoutElement>();
            highlightLI.ignoreLayout = true;

            // Save as prefab
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");

            var prefab = PrefabUtility.SaveAsPrefabAsset(btnObj, prefabPath);
            Object.DestroyImmediate(btnObj);

            Debug.Log($"[UISceneBuilder] Created CharacterButton prefab at {prefabPath}");
            return prefab;
        }

        #endregion

        #region Reference Wiring

        private static void WireAudioManager(AudioManager audioMgr)
        {
            var sources = audioMgr.GetComponents<AudioSource>();
            if (sources.Length < 3)
            {
                Debug.LogWarning("[UISceneBuilder] AudioManager needs 3 AudioSources.");
                return;
            }
            var so = new SerializedObject(audioMgr);
            SetRef(so, "musicSource", sources[0]);
            SetRef(so, "sfxSource",   sources[1]);
            SetRef(so, "voiceSource", sources[2]);
            so.ApplyModifiedProperties();
        }

        private static void WireUIManager(UIManager uiMgr,
            RectTransform barIndicator, RectTransform barTrack,
            Image barFillLeft, Image barFillRight,
            TextMeshProUGUI p1ScoreText, TextMeshProUGUI p2ScoreText,
            TextMeshProUGUI roundText, TextMeshProUGUI timerText, Image timerFill,
            Image p1Portrait, TextMeshProUGUI p1NameText,
            Image p1Ab1Cd, Image p1Ab2Cd,
            TextMeshProUGUI p1Ab1Key, TextMeshProUGUI p1Ab2Key,
            TextMeshProUGUI p1Ab1Name, TextMeshProUGUI p1Ab2Name,
            GameObject p1ShieldInd,
            Image p2Portrait, TextMeshProUGUI p2NameText,
            Image p2Ab1Cd, Image p2Ab2Cd,
            TextMeshProUGUI p2Ab1Key, TextMeshProUGUI p2Ab2Key,
            TextMeshProUGUI p2Ab1Name, TextMeshProUGUI p2Ab2Name,
            GameObject p2ShieldInd,
            GameObject titleScreenPanel, GameObject charSelectPanel, GameObject gameplayPanel,
            GameObject roundStartPanel, GameObject roundEndPanel, GameObject matchEndPanel,
            TextMeshProUGUI roundStartText, TextMeshProUGUI roundEndText,
            TextMeshProUGUI matchWinnerText, TextMeshProUGUI matchEndInstrText,
            GameObject p1PowerSurgeInd, GameObject p2PowerSurgeInd,
            GameObject controlsReversedInd,
            PlayerController p1Controller, PlayerController p2Controller,
            ArmWrestleController awc,
            Image victoryBgImage,
            Sprite eliVictory, Sprite leneVictory, Sprite natiVictory, Sprite sabiVictory, Sprite patzVictory)
        {
            var so = new SerializedObject(uiMgr);

            SetRef(so, "barIndicator", barIndicator);
            SetRef(so, "barTrack", barTrack);
            SetRef(so, "barFillLeft", barFillLeft);
            SetRef(so, "barFillRight", barFillRight);
            SetRef(so, "player1ScoreText", p1ScoreText);
            SetRef(so, "player2ScoreText", p2ScoreText);
            SetRef(so, "roundText", roundText);
            SetRef(so, "timerText", timerText);
            SetRef(so, "timerFill", timerFill);
            SetRef(so, "p1Portrait", p1Portrait);
            SetRef(so, "p1NameText", p1NameText);
            SetRef(so, "p1Ability1Cooldown", p1Ab1Cd);
            SetRef(so, "p1Ability2Cooldown", p1Ab2Cd);
            SetRef(so, "p1Ability1KeyText", p1Ab1Key);
            SetRef(so, "p1Ability2KeyText", p1Ab2Key);
            SetRef(so, "p1Ability1NameText", p1Ab1Name);
            SetRef(so, "p1Ability2NameText", p1Ab2Name);
            SetRef(so, "p1ShieldIndicator", p1ShieldInd);
            SetRef(so, "p2Portrait", p2Portrait);
            SetRef(so, "p2NameText", p2NameText);
            SetRef(so, "p2Ability1Cooldown", p2Ab1Cd);
            SetRef(so, "p2Ability2Cooldown", p2Ab2Cd);
            SetRef(so, "p2Ability1KeyText", p2Ab1Key);
            SetRef(so, "p2Ability2KeyText", p2Ab2Key);
            SetRef(so, "p2Ability1NameText", p2Ab1Name);
            SetRef(so, "p2Ability2NameText", p2Ab2Name);
            SetRef(so, "p2ShieldIndicator", p2ShieldInd);
            SetRef(so, "titleScreenPanel", titleScreenPanel);
            SetRef(so, "characterSelectPanel", charSelectPanel);
            SetRef(so, "gameplayPanel", gameplayPanel);
            SetRef(so, "roundStartPanel", roundStartPanel);
            SetRef(so, "roundEndPanel", roundEndPanel);
            SetRef(so, "matchEndPanel", matchEndPanel);
            SetRef(so, "roundStartText", roundStartText);
            SetRef(so, "roundEndText", roundEndText);
            SetRef(so, "matchWinnerText", matchWinnerText);
            SetRef(so, "matchEndInstructionsText", matchEndInstrText);
            SetRef(so, "p1PowerSurgeIndicator", p1PowerSurgeInd);
            SetRef(so, "p2PowerSurgeIndicator", p2PowerSurgeInd);
            SetRef(so, "controlsReversedIndicator", controlsReversedInd);
            SetRef(so, "player1Controller", p1Controller);
            SetRef(so, "player2Controller", p2Controller);
            SetRef(so, "armWrestleController", awc);
            SetRef(so, "victoryBackgroundImage", victoryBgImage);
            SetRef(so, "eliVictorySprite",  eliVictory);
            SetRef(so, "leneVictorySprite", leneVictory);
            SetRef(so, "natiVictorySprite", natiVictory);
            SetRef(so, "sabiVictorySprite", sabiVictory);
            SetRef(so, "patzVictorySprite", patzVictory);

            so.ApplyModifiedProperties();
        }

        private static void WireCharacterSelectManager(CharacterSelectManager csm,
            CharacterData[] characters, CharacterData patzCharacter,
            Image p1Portrait, TextMeshProUGUI p1Name,
            TextMeshProUGUI p1Ab1, TextMeshProUGUI p1Ab2, Image p1Ready,
            Image p2Portrait, TextMeshProUGUI p2Name,
            TextMeshProUGUI p2Ab1, TextMeshProUGUI p2Ab2, Image p2Ready,
            Transform gridParent, GameObject prefab, Button startBtn,
            TextMeshProUGUI instrText)
        {
            var so = new SerializedObject(csm);

            // Set character array
            var charProp = so.FindProperty("availableCharacters");
            charProp.arraySize = characters.Length;
            for (int i = 0; i < characters.Length; i++)
            {
                charProp.GetArrayElementAtIndex(i).objectReferenceValue = characters[i];
            }

            if (patzCharacter != null)
                SetRef(so, "patzCharacter", patzCharacter);

            SetRef(so, "p1SelectedPortrait", p1Portrait);
            SetRef(so, "p1SelectedName", p1Name);
            SetRef(so, "p1Ability1Text", p1Ab1);
            SetRef(so, "p1Ability2Text", p1Ab2);
            SetRef(so, "p1ReadyIndicator", p1Ready);
            SetRef(so, "p2SelectedPortrait", p2Portrait);
            SetRef(so, "p2SelectedName", p2Name);
            SetRef(so, "p2Ability1Text", p2Ab1);
            SetRef(so, "p2Ability2Text", p2Ab2);
            SetRef(so, "p2ReadyIndicator", p2Ready);
            SetRef(so, "characterGridParent", gridParent);
            SetRef(so, "characterButtonPrefab", prefab);
            SetRef(so, "startMatchButton", startBtn);
            SetRef(so, "instructionsText", instrText);

            so.ApplyModifiedProperties();
        }

        private static void WireMatchupDisplayController(MatchupDisplayController ctrl, Image displayImage, ArmWrestleController awc)
        {
            var so = new SerializedObject(ctrl);
            SetRef(so, "displayImage", displayImage);
            SetRef(so, "armWrestleController", awc);

            // Auto-populate list from existing matchup assets
            var matchupAssets = MatchupSpritesCreator.LoadAllMatchupAssets();
            var listProp = so.FindProperty("allMatchups");
            listProp.arraySize = matchupAssets.Length;
            for (int i = 0; i < matchupAssets.Length; i++)
                listProp.GetArrayElementAtIndex(i).objectReferenceValue = matchupAssets[i];

            so.ApplyModifiedProperties();

            if (matchupAssets.Length > 0)
                Debug.Log($"[UISceneBuilder] Wired {matchupAssets.Length} matchup sprite asset(s) to MatchupDisplayController.");
            else
                Debug.Log("[UISceneBuilder] No matchup assets found yet. Run 'Dirty Thirty Showdown > Create Matchup Sprite Assets' first, then rebuild UI.");
        }

        private static void WireGameManager(GameManager gm, ArmWrestleController awc, UIManager uiMgr)
        {
            var so = new SerializedObject(gm);
            SetRef(so, "armWrestleController", awc);
            SetRef(so, "uiManager", uiMgr);
            so.ApplyModifiedProperties();
        }

        private static void WireAbilitySystem(AbilitySystem abSys, ArmWrestleController awc,
            PlayerController p1, PlayerController p2, CanvasGroup flashOverlay)
        {
            var so = new SerializedObject(abSys);
            SetRef(so, "armWrestleController", awc);
            SetRef(so, "player1Controller", p1);
            SetRef(so, "player2Controller", p2);
            SetRef(so, "screenFlashOverlay", flashOverlay);
            so.ApplyModifiedProperties();
        }

        private static void WireArmWrestleController(ArmWrestleController awc,
            RectTransform barIndicator, RectTransform barTrack)
        {
            var so = new SerializedObject(awc);
            SetRef(so, "barIndicator", barIndicator);
            SetRef(so, "barTrack", barTrack);
            so.ApplyModifiedProperties();
        }

        private static void WirePlayerController(PlayerController pc, int playerNumber,
            ArmWrestleController awc, AbilitySystem abSys)
        {
            var so = new SerializedObject(pc);
            so.FindProperty("playerNumber").intValue = playerNumber;
            SetRef(so, "armWrestleController", awc);
            SetRef(so, "abilitySystem", abSys);
            so.ApplyModifiedProperties();
        }

        private static void WireGameBootstrap(GameBootstrap bootstrap,
            GameManager gm, ArmWrestleController awc, AbilitySystem abSys,
            UIManager uiMgr, AudioManager audioMgr, CharacterSelectManager csm,
            CheatManager cheatMgr, PlayerController p1, PlayerController p2)
        {
            var so = new SerializedObject(bootstrap);
            SetRef(so, "gameManager", gm);
            SetRef(so, "armWrestleController", awc);
            SetRef(so, "abilitySystem", abSys);
            SetRef(so, "uiManager", uiMgr);
            SetRef(so, "audioManager", audioMgr);
            SetRef(so, "characterSelectManager", csm);
            SetRef(so, "cheatManager", cheatMgr);
            SetRef(so, "player1", p1);
            SetRef(so, "player2", p2);
            so.ApplyModifiedProperties();
        }

        #endregion

        #region Helpers

        private static T FindOrCreateComponent<T>(string objName) where T : MonoBehaviour
        {
            var existing = Object.FindFirstObjectByType<T>();
            if (existing != null) return existing;

            var obj = new GameObject(objName);
            Undo.RegisterCreatedObjectUndo(obj, $"Create {objName}");
            return obj.AddComponent<T>();
        }

        private static PlayerController FindOrCreatePlayerController(int playerNumber)
        {
            var all = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var pc in all)
            {
                // Check playerNumber via SerializedObject
                var so = new SerializedObject(pc);
                if (so.FindProperty("playerNumber").intValue == playerNumber)
                    return pc;
            }

            var obj = new GameObject($"Player{playerNumber}");
            Undo.RegisterCreatedObjectUndo(obj, $"Create Player{playerNumber}");
            var controller = obj.AddComponent<PlayerController>();

            // Set player number
            var cso = new SerializedObject(controller);
            cso.FindProperty("playerNumber").intValue = playerNumber;
            cso.ApplyModifiedProperties();

            return controller;
        }

        private static void EnsureAudioSources(GameObject obj, int count)
        {
            var sources = obj.GetComponents<AudioSource>();
            for (int i = sources.Length; i < count; i++)
            {
                obj.AddComponent<AudioSource>();
            }
        }

        private static CharacterData[] LoadCharacterAssets()
        {
            // Load the 4 main characters (not Patz)
            string[] names = { "Eli", "Lene", "Nati", "Sabi" };
            var list = new System.Collections.Generic.List<CharacterData>();

            foreach (var name in names)
            {
                var asset = AssetDatabase.LoadAssetAtPath<CharacterData>($"Assets/Characters/{name}.asset");
                if (asset != null)
                    list.Add(asset);
                else
                    Debug.LogWarning($"[UISceneBuilder] Could not find character asset: Assets/Characters/{name}.asset");
            }

            return list.ToArray();
        }

        private static GameObject CreatePanel(Transform parent, string name, bool addRT)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            if (addRT) obj.AddComponent<RectTransform>();
            return obj;
        }

        private static TextMeshProUGUI CreateTMP(Transform parent, string name, string text,
            int fontSize, FontStyles style = FontStyles.Normal, TMP_FontAsset font = null)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            var resolvedFont = font ?? defaultFont;
            if (resolvedFont != null) tmp.font = resolvedFont;
            return tmp;
        }

        private static void StretchFill(GameObject obj)
        {
            var rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void CreateBorderImages(Transform parent, float thickness)
        {
            // Top
            var top = new GameObject("TopBorder");
            top.transform.SetParent(parent, false);
            var topRT = top.AddComponent<RectTransform>();
            topRT.anchorMin = new Vector2(0, 1);
            topRT.anchorMax = Vector2.one;
            topRT.offsetMin = new Vector2(0, -thickness);
            topRT.offsetMax = Vector2.zero;
            top.AddComponent<Image>().raycastTarget = false;

            // Bottom
            var bottom = new GameObject("BottomBorder");
            bottom.transform.SetParent(parent, false);
            var bottomRT = bottom.AddComponent<RectTransform>();
            bottomRT.anchorMin = Vector2.zero;
            bottomRT.anchorMax = new Vector2(1, 0);
            bottomRT.offsetMin = Vector2.zero;
            bottomRT.offsetMax = new Vector2(0, thickness);
            bottom.AddComponent<Image>().raycastTarget = false;

            // Left
            var left = new GameObject("LeftBorder");
            left.transform.SetParent(parent, false);
            var leftRT = left.AddComponent<RectTransform>();
            leftRT.anchorMin = Vector2.zero;
            leftRT.anchorMax = new Vector2(0, 1);
            leftRT.offsetMin = Vector2.zero;
            leftRT.offsetMax = new Vector2(thickness, 0);
            left.AddComponent<Image>().raycastTarget = false;

            // Right
            var right = new GameObject("RightBorder");
            right.transform.SetParent(parent, false);
            var rightRT = right.AddComponent<RectTransform>();
            rightRT.anchorMin = new Vector2(1, 0);
            rightRT.anchorMax = Vector2.one;
            rightRT.offsetMin = new Vector2(-thickness, 0);
            rightRT.offsetMax = Vector2.zero;
            right.AddComponent<Image>().raycastTarget = false;
        }

        private static void SetRef(SerializedObject so, string propertyName, Object value)
        {
            var prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
            else
            {
                Debug.LogWarning($"[UISceneBuilder] Property '{propertyName}' not found on {so.targetObject.GetType().Name}");
            }
        }

        #endregion
    }
}
