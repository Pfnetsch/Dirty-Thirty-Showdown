using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace DirtyThirtyShowdown
{
    public static class PlayerHUDPatcher
    {
        private const string PrefabPath = "Assets/Prefabs/PlayerHUD.prefab";

        [MenuItem("Dirty Thirty Showdown/Fix PlayerHUD Cooldown Circles")]
        static void FixCooldownCircles()
        {
            using (var scope = new PrefabUtility.EditPrefabContentsScope(PrefabPath))
            {
                var root = scope.prefabContentsRoot;
                PatchAbilityGroup(root.transform.Find("Ability1Group"));
                PatchAbilityGroup(root.transform.Find("Ability2Group"));
            }

            Debug.Log("[PlayerHUDPatcher] PlayerHUD cooldown circles fixed and saved to prefab.");
        }

        static void PatchAbilityGroup(Transform group)
        {
            if (group == null) { Debug.LogWarning("[PlayerHUDPatcher] AbilityGroup not found"); return; }

            Transform cdTransform = group.Find("Cooldown");
            if (cdTransform == null) { Debug.LogWarning($"[PlayerHUDPatcher] No Cooldown child in {group.name}"); return; }

            // Unity's built-in circular sprite — required for radial fill to render as a circle
            var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            var cdRT = cdTransform.GetComponent<RectTransform>();

            // Add CooldownBg dark ring if missing
            Transform bgTransform = group.Find("CooldownBg");
            if (bgTransform == null)
            {
                var bgObj = new GameObject("CooldownBg");
                bgObj.transform.SetParent(group, false);
                bgTransform = bgObj.transform;
            }

            var bgRT = bgTransform.GetComponent<RectTransform>() ?? bgTransform.gameObject.AddComponent<RectTransform>();
            bgRT.anchorMin        = cdRT.anchorMin;
            bgRT.anchorMax        = cdRT.anchorMax;
            bgRT.anchoredPosition = cdRT.anchoredPosition;
            bgRT.sizeDelta        = cdRT.sizeDelta;
            bgRT.pivot            = cdRT.pivot;

            var bgImg = bgTransform.GetComponent<Image>() ?? bgTransform.gameObject.AddComponent<Image>();
            bgImg.sprite      = knob;
            bgImg.color       = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            bgImg.type        = Image.Type.Filled;
            bgImg.fillMethod  = Image.FillMethod.Radial360;
            bgImg.fillAmount  = 1f;
            bgImg.raycastTarget = false;

            // Place CooldownBg just before Cooldown in the hierarchy
            bgTransform.SetSiblingIndex(cdTransform.GetSiblingIndex());

            // Fix the Cooldown fill: assign knob sprite, green, radial, starts full (= ability ready)
            var cdImg = cdTransform.GetComponent<Image>();
            if (cdImg != null)
            {
                cdImg.sprite        = knob;
                cdImg.color         = new Color(0.3f, 0.9f, 0.4f, 0.9f);
                cdImg.type          = Image.Type.Filled;
                cdImg.fillMethod    = Image.FillMethod.Radial360;
                cdImg.fillOrigin    = (int)Image.Origin360.Top;
                cdImg.fillClockwise = true;
                cdImg.fillAmount    = 1f;
            }
        }
    }
}
