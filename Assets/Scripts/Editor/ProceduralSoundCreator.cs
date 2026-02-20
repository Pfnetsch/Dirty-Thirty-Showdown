#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace DirtyThirtyShowdown
{
    /// <summary>
    /// Creates all ProceduralSoundProfile assets with "Neon Party Arcade" presets and
    /// auto-wires them into the AudioManager in the current scene.
    /// Menu: Dirty Thirty Showdown > Create Sound Profiles
    /// </summary>
    public static class ProceduralSoundCreator
    {
        private const string ProfilesPath = "Assets/Audio/Profiles";

        [MenuItem("Dirty Thirty Showdown/Create Sound Profiles")]
        public static void CreateAllProfiles()
        {
            if (!Directory.Exists(ProfilesPath))
                Directory.CreateDirectory(ProfilesPath);

            // ── UI ────────────────────────────────────────────────────────────────

            // Soft "bup" — rapid-fire safe, low sine + light noise, pitch drop 110→70
            var mashHit = Make("mashHit",
                osc: OscillatorType.Sine, freq: 110f, dur: 0.10f,
                a: 0.001f, d: 0.08f, s: 0f, r: 0.015f,
                pStart: 1.0f, pEnd: 0.636f,                // 70/110
                noise: 0.28f, fVar: 0.03f, vVar: 0.05f, variants: 5, vol: 0.55f);

            // Subtle tick — plays constantly, keep very light
            var navigate = Make("navigate",
                osc: OscillatorType.Sine, freq: 900f, dur: 0.06f,
                a: 0.001f, d: 0.05f, s: 0f, r: 0.008f,
                pStart: 1.0f, pEnd: 1.03f,
                noise: 0f, fVar: 0.02f, vVar: 0.05f, variants: 3, vol: 0.45f);

            // Bright affirmative pop — 700 Hz sine + 1400 Hz harmonic, pitch rise 700→820
            var confirm = Make("confirm",
                osc: OscillatorType.Sine, freq: 700f, dur: 0.14f,
                a: 0.001f, d: 0.12f, s: 0f, r: 0.015f,
                pStart: 1.0f, pEnd: 1.171f,                // 820/700
                noise: 0f, fVar: 0.02f, vVar: 0.05f, variants: 2, vol: 0.75f,
                secOsc: OscillatorType.Sine, secFreq: 1400f, secVol: 0.35f);

            // ── Match ─────────────────────────────────────────────────────────────

            // Signature moment — triangle 440+660, slight upward ramp, soft noise layer
            var roundStart = Make("roundStart",
                osc: OscillatorType.Triangle, freq: 440f, dur: 0.45f,
                a: 0.005f, d: 0.38f, s: 0f, r: 0.06f,
                pStart: 1.0f, pEnd: 1.02f,
                noise: 0.06f, fVar: 0.02f, vVar: 0.04f, variants: 2, vol: 0.80f,
                secOsc: OscillatorType.Triangle, secFreq: 660f, secVol: 0.55f);

            // Game show buzzer — softened square, pitch drop 220→180
            var roundEnd = Make("roundEnd",
                osc: OscillatorType.Square, freq: 220f, dur: 0.35f,
                a: 0.001f, d: 0.30f, s: 0f, r: 0.04f,
                pStart: 1.0f, pEnd: 0.818f,                // 180/220
                noise: 0f, fVar: 0.02f, vVar: 0.04f, variants: 2, vol: 0.75f,
                lpCutoff: 1800f);                          // soften the square

            // Short 3-note triangle arpeggio: 880 → 1100 → 1320 Hz, total 0.36s
            var matchWin = MakeArpeggio("matchWin",
                osc: OscillatorType.Triangle,
                notes: new[] {
                    new ArpeggioNote { frequency = 880f,  duration = 0.12f },
                    new ArpeggioNote { frequency = 1100f, duration = 0.12f },
                    new ArpeggioNote { frequency = 1320f, duration = 0.12f }
                },
                noise: 0f, fVar: 0.02f, vVar: 0.04f, variants: 2, vol: 0.85f);

            // High sparkle "ready!" — sine, pitch drop 1400→1200
            var cooldownReady = Make("cooldownReady",
                osc: OscillatorType.Sine, freq: 1400f, dur: 0.22f,
                a: 0.001f, d: 0.20f, s: 0f, r: 0.012f,
                pStart: 1.0f, pEnd: 0.857f,                // 1200/1400
                noise: 0f, fVar: 0.02f, vVar: 0.04f, variants: 2, vol: 0.65f);

            // Short urgent pulse — softened square 500Hz, repeated by game timer code
            var timerWarning = Make("timerWarning",
                osc: OscillatorType.Square, freq: 500f, dur: 0.08f,
                a: 0.001f, d: 0.06f, s: 0f, r: 0.012f,
                pStart: 1.0f, pEnd: 1.0f,
                noise: 0f, fVar: 0.02f, vVar: 0.04f, variants: 2, vol: 0.65f,
                lpCutoff: 2200f);                          // soften the square

            // ── Abilities ─────────────────────────────────────────────────────────

            // Eli — Power Surge: triangle ramp 200→900 Hz + noise layer rising
            var powerSurge = Make("powerSurge",
                osc: OscillatorType.Triangle, freq: 200f, dur: 0.35f,
                a: 0.02f, d: 0.28f, s: 0f, r: 0.06f,
                pStart: 1.0f, pEnd: 4.5f,                  // 900/200
                noise: 0.22f, fVar: 0.03f, vVar: 0.05f, variants: 2, vol: 0.85f);

            // Eli — Flash: sharp sine 1600 Hz zap + noise burst, quick pitch drop
            var flash = Make("flash",
                osc: OscillatorType.Sine, freq: 1600f, dur: 0.14f,
                a: 0.001f, d: 0.10f, s: 0f, r: 0.035f,
                pStart: 1.0f, pEnd: 0.72f,
                noise: 0.38f, fVar: 0.04f, vVar: 0.06f, variants: 3, vol: 0.88f);

            // Lene — Trash Talk: softened square 300 Hz + fast pitch wobble (vibrato)
            var trashTalk = Make("trashTalk",
                osc: OscillatorType.Square, freq: 300f, dur: 0.18f,
                a: 0.001f, d: 0.15f, s: 0f, r: 0.025f,
                pStart: 1.0f, pEnd: 1.0f,
                noise: 0f, fVar: 0.04f, vVar: 0.06f, variants: 3, vol: 0.72f,
                vibRate: 13f, vibDepth: 0.07f,
                lpCutoff: 1500f);

            // Lene — Shield Activate: two sines 260+390 Hz with gentle vibrato, sustain tail
            var shieldActivate = Make("shieldActivate",
                osc: OscillatorType.Sine, freq: 260f, dur: 0.70f,
                a: 0.015f, d: 0.15f, s: 0.70f, r: 0.38f,
                pStart: 1.0f, pEnd: 1.0f,
                noise: 0f, fVar: 0.02f, vVar: 0.04f, variants: 2, vol: 0.65f,
                secOsc: OscillatorType.Sine, secFreq: 390f, secVol: 0.60f,
                vibRate: 5f, vibDepth: 0.01f);

            // Lene — Shield Break: noise burst + sine 500→200 Hz pitch drop
            var shieldBreak = Make("shieldBreak",
                osc: OscillatorType.Sine, freq: 500f, dur: 0.28f,
                a: 0.001f, d: 0.22f, s: 0f, r: 0.05f,
                pStart: 1.0f, pEnd: 0.40f,                 // 200/500
                noise: 0.72f, fVar: 0.06f, vVar: 0.06f, variants: 3, vol: 0.85f);

            // Nati — Wink/Flirt: sine 1200→1500 Hz + tiny octave chime
            var winkFlirt = Make("winkFlirt",
                osc: OscillatorType.Sine, freq: 1200f, dur: 0.18f,
                a: 0.002f, d: 0.15f, s: 0f, r: 0.022f,
                pStart: 1.0f, pEnd: 1.25f,                 // 1500/1200
                noise: 0f, fVar: 0.03f, vVar: 0.05f, variants: 3, vol: 0.68f,
                secOsc: OscillatorType.Sine, secFreq: 2400f, secVol: 0.18f);

            // Nati — Dance: goofy triangle arpeggio 660 → 880 → 740 Hz, total 0.4s
            var dance = MakeArpeggio("dance",
                osc: OscillatorType.Triangle,
                notes: new[] {
                    new ArpeggioNote { frequency = 660f, duration = 0.13f },
                    new ArpeggioNote { frequency = 880f, duration = 0.13f },
                    new ArpeggioNote { frequency = 740f, duration = 0.14f }
                },
                noise: 0f, fVar: 0.03f, vVar: 0.05f, variants: 2, vol: 0.70f);

            // Sabi — Cake Toss: filtered noise burst + low sine 90 Hz, cartoony splat
            var cakeToss = Make("cakeToss",
                osc: OscillatorType.Noise, freq: 90f, dur: 0.25f,
                a: 0.001f, d: 0.20f, s: 0f, r: 0.04f,
                pStart: 1.0f, pEnd: 0.82f,
                noise: 0.78f, fVar: 0.10f, vVar: 0.07f, variants: 4, vol: 0.80f,
                secOsc: OscillatorType.Sine, secFreq: 90f, secVol: 0.42f,
                lpCutoff: 900f);

            // Sabi — Fake Out: sine sweep 1500→300 Hz with fade-in feel ("whoooop")
            var fakeOut = Make("fakeOut",
                osc: OscillatorType.Sine, freq: 1500f, dur: 0.22f,
                a: 0.07f, d: 0.12f, s: 0f, r: 0.03f,
                pStart: 1.0f, pEnd: 0.20f,                 // 300/1500
                noise: 0.04f, fVar: 0.03f, vVar: 0.05f, variants: 2, vol: 0.80f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WireIntoAudioManager(
                navigate, confirm, mashHit, cooldownReady,
                roundStart, roundEnd, matchWin, timerWarning,
                powerSurge, flash, trashTalk, shieldActivate, shieldBreak,
                winkFlirt, dance, cakeToss, fakeOut);

            Debug.Log($"[ProceduralSoundCreator] All profiles created in {ProfilesPath} and wired into AudioManager.");
        }

        // ─── Profile Factories ────────────────────────────────────────────────────

        private static ProceduralSoundProfile Make(
            string filename,
            OscillatorType osc, float freq, float dur,
            float a, float d, float s, float r,
            float pStart = 1f, float pEnd = 1f,
            float noise = 0f,
            float fVar = 0.03f, float vVar = 0.05f, int variants = 3, float vol = 0.75f,
            OscillatorType secOsc = OscillatorType.Sine, float secFreq = 0f, float secVol = 0f,
            float vibRate = 0f, float vibDepth = 0f,
            float lpCutoff = 0f)
        {
            var p = GetOrCreate(filename);

            p.oscillatorType       = osc;
            p.baseFrequency        = freq;
            p.duration             = dur;
            p.attack               = a;
            p.decay                = d;
            p.sustain              = s;
            p.release              = r;
            p.pitchStartMultiplier = pStart;
            p.pitchEndMultiplier   = pEnd;
            p.pitchCurve           = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            p.noiseAmount          = noise;
            p.frequencyVariation   = fVar;
            p.volumeVariation      = vVar;
            p.variantCount         = variants;
            p.volume               = vol;
            p.secondaryOscType     = secOsc;
            p.secondaryFrequency   = secFreq;
            p.secondaryVolume      = secVol;
            p.vibratoRate          = vibRate;
            p.vibratoDepth         = vibDepth;
            p.lowPassCutoff        = lpCutoff;
            p.arpeggio             = null;

            EditorUtility.SetDirty(p);
            return p;
        }

        private static ProceduralSoundProfile MakeArpeggio(
            string filename,
            OscillatorType osc,
            ArpeggioNote[] notes,
            float noise = 0f,
            float fVar = 0.03f, float vVar = 0.05f, int variants = 2, float vol = 0.75f)
        {
            var p = GetOrCreate(filename);

            // Sum note durations for the asset duration field (informational)
            float totalDur = 0f;
            foreach (var n in notes) totalDur += n.duration;

            p.oscillatorType       = osc;
            p.baseFrequency        = notes.Length > 0 ? notes[0].frequency : 440f;
            p.duration             = totalDur;
            p.attack               = 0.005f;
            p.decay                = 0.1f;
            p.sustain              = 0f;
            p.release              = 0.02f;
            p.pitchStartMultiplier = 1f;
            p.pitchEndMultiplier   = 1f;
            p.pitchCurve           = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            p.noiseAmount          = noise;
            p.frequencyVariation   = fVar;
            p.volumeVariation      = vVar;
            p.variantCount         = variants;
            p.volume               = vol;
            p.secondaryFrequency   = 0f;
            p.secondaryVolume      = 0f;
            p.vibratoRate          = 0f;
            p.vibratoDepth         = 0f;
            p.lowPassCutoff        = 0f;
            p.arpeggio             = notes;

            EditorUtility.SetDirty(p);
            return p;
        }

        /// <summary>Loads existing asset or creates a new one. Avoids losing Inspector tweaks on re-run.</summary>
        private static ProceduralSoundProfile GetOrCreate(string filename)
        {
            string path = $"{ProfilesPath}/{filename}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ProceduralSoundProfile>(path);
            if (existing != null) return existing;

            var profile = ScriptableObject.CreateInstance<ProceduralSoundProfile>();
            AssetDatabase.CreateAsset(profile, path);
            return profile;
        }

        // ─── Auto-Wire ────────────────────────────────────────────────────────────

        private static void WireIntoAudioManager(
            ProceduralSoundProfile navigate, ProceduralSoundProfile confirm,
            ProceduralSoundProfile mashHit,  ProceduralSoundProfile cooldownReady,
            ProceduralSoundProfile roundStart, ProceduralSoundProfile roundEnd,
            ProceduralSoundProfile matchWin,   ProceduralSoundProfile timerWarning,
            ProceduralSoundProfile powerSurge, ProceduralSoundProfile flash,
            ProceduralSoundProfile trashTalk,
            ProceduralSoundProfile shieldActivate, ProceduralSoundProfile shieldBreak,
            ProceduralSoundProfile winkFlirt, ProceduralSoundProfile dance,
            ProceduralSoundProfile cakeToss,  ProceduralSoundProfile fakeOut)
        {
#pragma warning disable CS0618
            var audioManager = Object.FindObjectOfType<AudioManager>();
#pragma warning restore CS0618
            if (audioManager == null)
            {
                Debug.LogWarning("[ProceduralSoundCreator] AudioManager not found — open the game scene and run again to auto-wire.");
                return;
            }

            var so = new SerializedObject(audioManager);
            so.FindProperty("navigateSound").objectReferenceValue       = navigate;
            so.FindProperty("confirmSound").objectReferenceValue        = confirm;
            so.FindProperty("mashHitSound").objectReferenceValue        = mashHit;
            so.FindProperty("cooldownReadySound").objectReferenceValue  = cooldownReady;
            so.FindProperty("roundStartSound").objectReferenceValue     = roundStart;
            so.FindProperty("roundEndSound").objectReferenceValue       = roundEnd;
            so.FindProperty("matchWinSound").objectReferenceValue       = matchWin;
            so.FindProperty("timerWarningSound").objectReferenceValue   = timerWarning;
            so.FindProperty("powerSurgeSound").objectReferenceValue     = powerSurge;
            so.FindProperty("flashSound").objectReferenceValue          = flash;
            so.FindProperty("trashTalkSound").objectReferenceValue      = trashTalk;
            so.FindProperty("shieldActivateSound").objectReferenceValue = shieldActivate;
            so.FindProperty("shieldBreakSound").objectReferenceValue    = shieldBreak;
            so.FindProperty("winkFlirtSound").objectReferenceValue      = winkFlirt;
            so.FindProperty("danceSound").objectReferenceValue          = dance;
            so.FindProperty("cakeTossSound").objectReferenceValue       = cakeToss;
            so.FindProperty("fakeOutSound").objectReferenceValue        = fakeOut;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(audioManager);
            Debug.Log("[ProceduralSoundCreator] AudioManager wired successfully.");
        }
    }
}
#endif
