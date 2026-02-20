using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DirtyThirtyShowdown
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;

        [Header("Music (drag AudioClip files in when ready)")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip characterSelectMusic;
        [SerializeField] private AudioClip battleMusic;
        [SerializeField] private AudioClip victoryMusic;

        [Header("UI Sounds")]
        [SerializeField] private ProceduralSoundProfile navigateSound;
        [SerializeField] private ProceduralSoundProfile confirmSound;
        [SerializeField] private ProceduralSoundProfile mashHitSound;
        [SerializeField] private ProceduralSoundProfile cooldownReadySound;

        [Header("Match Sounds")]
        [SerializeField] private ProceduralSoundProfile roundStartSound;
        [SerializeField] private ProceduralSoundProfile roundEndSound;
        [SerializeField] private ProceduralSoundProfile matchWinSound;
        [SerializeField] private ProceduralSoundProfile timerWarningSound;

        [Header("Ability Sounds")]
        [SerializeField] private ProceduralSoundProfile powerSurgeSound;
        [SerializeField] private ProceduralSoundProfile flashSound;
        [SerializeField] private ProceduralSoundProfile trashTalkSound;
        [SerializeField] private ProceduralSoundProfile shieldActivateSound;
        [SerializeField] private ProceduralSoundProfile shieldBreakSound;
        [SerializeField] private ProceduralSoundProfile winkFlirtSound;
        [SerializeField] private ProceduralSoundProfile danceSound;
        [SerializeField] private ProceduralSoundProfile cakeTossSound;
        [SerializeField] private ProceduralSoundProfile fakeOutSound;

        [Header("Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float voiceVolume = 1f;
        [SerializeField] private float musicFadeDuration = 1f;
        [Tooltip("Minimum seconds between mash hit sounds (prevents audio spam)")]
        [SerializeField] private float mashSoundInterval = 0.05f;

        // Clip cache: each profile generates N variants up front, we pick randomly per play
        private readonly Dictionary<ProceduralSoundProfile, AudioClip[]> clipCache = new();
        private GameManager gameManager;
        private float lastMashTime = -1f;

        // ─── Lifecycle ───────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            AutoWireAudioSources();
            CreateDefaultProfiles();

            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.OnStateChanged += HandleStateChanged;
                gameManager.OnRoundStart   += HandleRoundStart;
                gameManager.OnRoundEnd     += HandleRoundEnd;
                gameManager.OnMatchEnd     += HandleMatchEnd;
            }

            UpdateVolumes();
            GenerateAllClips();
        }

        private void AutoWireAudioSources()
        {
            var sources = GetComponents<AudioSource>();
            // Add missing sources
            while (sources.Length < 3)
            {
                gameObject.AddComponent<AudioSource>();
                sources = GetComponents<AudioSource>();
            }
            if (musicSource == null) musicSource = sources[0];
            if (sfxSource   == null) sfxSource   = sources[1];
            if (voiceSource == null) voiceSource = sources[2];

            musicSource.loop        = true;
            musicSource.playOnAwake = false;
            sfxSource.playOnAwake   = false;
            voiceSource.playOnAwake = false;
        }

        // ─── Default Profile Creation ─────────────────────────────────────────────

        /// <summary>
        /// Creates in-memory ProceduralSoundProfile instances for any slots left null in the Inspector.
        /// These are not saved as assets — just used at runtime.
        /// </summary>
        private void CreateDefaultProfiles()
        {
            mashHitSound        ??= MakeSimple("MashHit",       OscillatorType.Square,   180f, 0.06f, 0.003f, 0.04f, 0f,   0.015f, 0.55f, lpCutoff: 1400f, pitchEnd: 0.7f);
            navigateSound       ??= MakeSimple("Navigate",      OscillatorType.Sine,     660f, 0.09f, 0.003f, 0.04f, 0f,   0.04f,  0.45f);
            confirmSound        ??= MakeArp("Confirm",          OscillatorType.Sine,     new[]{440f, 660f},        0.12f, 0.55f);
            cooldownReadySound  ??= MakeSimple("CdReady",       OscillatorType.Triangle, 880f, 0.22f, 0.01f,  0.06f, 0.2f, 0.10f,  0.55f, pitchEnd: 1.5f);
            roundStartSound     ??= MakeArp("RoundStart",       OscillatorType.Square,   new[]{261f, 329f, 392f, 523f}, 0.10f, 0.60f, lpCutoff: 1800f);
            roundEndSound       ??= MakeSimple("RoundEnd",      OscillatorType.Square,   440f, 0.30f, 0.01f,  0.05f, 0f,   0.20f,  0.55f, lpCutoff: 1600f, pitchEnd: 0.6f);
            matchWinSound       ??= MakeArp("MatchWin",         OscillatorType.Square,   new[]{392f, 523f, 659f, 784f}, 0.14f, 0.65f, lpCutoff: 2000f);
            timerWarningSound   ??= MakeSimple("TimerWarn",     OscillatorType.Sine,     880f, 0.14f, 0.005f, 0.02f, 0f,   0.06f,  0.50f);
            powerSurgeSound     ??= MakeSimple("PowerSurge",    OscillatorType.Sawtooth, 200f, 0.45f, 0.02f,  0.10f, 0.4f, 0.20f,  0.65f, lpCutoff: 1800f, pitchEnd: 2.2f);
            flashSound          ??= MakeSimple("Flash",         OscillatorType.Sine,    1400f, 0.28f, 0.005f, 0.05f, 0f,   0.18f,  0.65f, pitchEnd: 0.5f, noise: 0.15f);
            trashTalkSound      ??= MakeSimple("TrashTalk",     OscillatorType.Sawtooth, 500f, 0.30f, 0.01f,  0.05f, 0f,   0.20f,  0.60f, lpCutoff: 1600f, pitchEnd: 0.5f);
            shieldActivateSound ??= MakeSimple("ShieldOn",      OscillatorType.Triangle, 660f, 0.35f, 0.01f,  0.08f, 0.3f, 0.18f,  0.55f, pitchEnd: 1.4f);
            shieldBreakSound    ??= MakeSimple("ShieldBreak",   OscillatorType.Noise,    400f, 0.28f, 0.005f, 0.05f, 0f,   0.20f,  0.65f, noise: 0.85f);
            winkFlirtSound      ??= MakeArp("WinkFlirt",        OscillatorType.Sine,     new[]{523f, 659f, 784f},      0.10f, 0.55f);
            danceSound          ??= MakeArp("Dance",            OscillatorType.Square,   new[]{330f, 415f, 330f, 494f}, 0.09f, 0.50f, lpCutoff: 1600f);
            cakeTossSound       ??= MakeSimple("CakeToss",      OscillatorType.Noise,    300f, 0.22f, 0.005f, 0.06f, 0f,   0.14f,  0.65f, noise: 0.80f, pitchEnd: 0.4f);
            fakeOutSound        ??= MakeSimple("FakeOut",       OscillatorType.Sawtooth, 550f, 0.40f, 0.01f,  0.05f, 0f,   0.28f,  0.60f, lpCutoff: 1800f, pitchEnd: 0.35f, vibratoRate: 8f, vibratoDepth: 0.04f);
        }

        private static ProceduralSoundProfile MakeSimple(
            string pName, OscillatorType osc, float hz, float dur,
            float atk, float dcy, float sus, float rel, float vol,
            float pitchEnd = 1f, float noise = 0f, float lpCutoff = 0f,
            float vibratoRate = 0f, float vibratoDepth = 0f)
        {
            var p = ScriptableObject.CreateInstance<ProceduralSoundProfile>();
            p.name                  = pName;
            p.oscillatorType        = osc;
            p.baseFrequency         = hz;
            p.duration              = dur;
            p.attack                = atk;
            p.decay                 = dcy;
            p.sustain               = sus;
            p.release               = rel;
            p.volume                = vol;
            p.pitchStartMultiplier  = 1f;
            p.pitchEndMultiplier    = pitchEnd;
            p.pitchCurve            = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            p.noiseAmount           = noise;
            p.lowPassCutoff         = lpCutoff;
            p.vibratoRate           = vibratoRate;
            p.vibratoDepth          = vibratoDepth;
            p.variantCount          = 3;
            p.frequencyVariation    = 0.02f;
            p.volumeVariation       = 0.05f;
            return p;
        }

        private static ProceduralSoundProfile MakeArp(
            string pName, OscillatorType osc, float[] freqs,
            float noteDur, float vol, float lpCutoff = 0f)
        {
            var p = ScriptableObject.CreateInstance<ProceduralSoundProfile>();
            p.name               = pName;
            p.oscillatorType     = osc;
            p.baseFrequency      = freqs[0];
            p.duration           = noteDur;
            p.pitchStartMultiplier = 1f;
            p.pitchEndMultiplier   = 1f;
            p.pitchCurve         = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            p.volume             = vol;
            p.lowPassCutoff      = lpCutoff;
            p.variantCount       = 1;  // arpeggios sound best without pitch variation
            p.frequencyVariation = 0f;
            p.volumeVariation    = 0.03f;
            var notes = new ArpeggioNote[freqs.Length];
            for (int i = 0; i < freqs.Length; i++)
                notes[i] = new ArpeggioNote { frequency = freqs[i], duration = noteDur };
            p.arpeggio = notes;
            return p;
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnStateChanged -= HandleStateChanged;
                gameManager.OnRoundStart   -= HandleRoundStart;
                gameManager.OnRoundEnd     -= HandleRoundEnd;
                gameManager.OnMatchEnd     -= HandleMatchEnd;
            }
        }

        // ─── Clip Generation ─────────────────────────────────────────────────────

        /// <summary>Pre-generates all variant clips at startup to avoid runtime hitches.</summary>
        private void GenerateAllClips()
        {
            ProceduralSoundProfile[] profiles =
            {
                navigateSound, confirmSound, mashHitSound, cooldownReadySound,
                roundStartSound, roundEndSound, matchWinSound, timerWarningSound,
                powerSurgeSound, flashSound, trashTalkSound,
                shieldActivateSound, shieldBreakSound,
                winkFlirtSound, danceSound, cakeTossSound, fakeOutSound
            };

            foreach (var p in profiles)
            {
                if (p != null) GetOrGenerateClips(p);
            }
        }

        private AudioClip[] GetOrGenerateClips(ProceduralSoundProfile profile)
        {
            if (profile == null) return null;
            if (clipCache.TryGetValue(profile, out AudioClip[] cached)) return cached;

            int count = Mathf.Max(1, profile.variantCount);
            AudioClip[] variants = new AudioClip[count];
            for (int i = 0; i < count; i++)
            {
                // Each variant bakes in a slight random frequency offset
                float freqMult = 1f + Random.Range(-profile.frequencyVariation, profile.frequencyVariation);
                variants[i] = ProceduralSoundGenerator.GenerateClip(profile, freqMult, $"_v{i}");
            }

            clipCache[profile] = variants;
            return variants;
        }

        // ─── Playback ─────────────────────────────────────────────────────────────

        /// <summary>Plays a random variant with slight volume variation.</summary>
        public void PlayWithVariation(ProceduralSoundProfile profile)
        {
            if (profile == null || sfxSource == null) return;
            AudioClip[] variants = GetOrGenerateClips(profile);
            if (variants == null || variants.Length == 0) return;

            AudioClip clip = variants[Random.Range(0, variants.Length)];
            float vol = sfxVolume * masterVolume;
            vol *= 1f + Random.Range(-profile.volumeVariation, profile.volumeVariation);
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(vol));
        }

        /// <summary>Plays a raw AudioClip (used for voice lines, music).</summary>
        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
                sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }

        // ─── Named Sound Methods (called by other systems) ────────────────────────

        public void PlayMashHit()
        {
            if (Time.time - lastMashTime < mashSoundInterval) return;
            lastMashTime = Time.time;
            PlayWithVariation(mashHitSound);
        }

        public void PlayUINavigate()    => PlayWithVariation(navigateSound);
        public void PlayUIConfirm()     => PlayWithVariation(confirmSound);
        public void PlayCooldownReady() => PlayWithVariation(cooldownReadySound);
        public void PlayTimerWarning()  => PlayWithVariation(timerWarningSound);
        public void PlayShieldBreak()   => PlayWithVariation(shieldBreakSound);

        public void PlayAbilitySFX(AbilityType type)
        {
            ProceduralSoundProfile profile = type switch
            {
                AbilityType.PowerSurge => powerSurgeSound,
                AbilityType.Flash      => flashSound,
                AbilityType.TrashTalk  => trashTalkSound,
                AbilityType.Shield     => shieldActivateSound,
                AbilityType.WinkFlirt  => winkFlirtSound,
                AbilityType.Dance      => danceSound,
                AbilityType.CakeToss   => cakeTossSound,
                AbilityType.FakeOut    => fakeOutSound,
                _ => null
            };
            PlayWithVariation(profile);
        }

        // ─── Voice ───────────────────────────────────────────────────────────────

        public void PlayVoiceLine(AudioClip clip)
        {
            if (voiceSource == null || clip == null) return;
            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.volume = voiceVolume * masterVolume;
            voiceSource.Play();
        }

        public void PlayCharacterVoiceLine(CharacterData character, VoiceLineCategory category)
        {
            if (character == null) return;
            AudioClip clip = character.GetRandomVoiceLine(category);
            if (clip != null) PlayVoiceLine(clip);
        }

        // ─── Music ───────────────────────────────────────────────────────────────

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null || clip == null) return;
            StartCoroutine(CrossfadeMusic(clip, loop));
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip, bool loop)
        {
            float startVol = musicSource.volume;
            float halfFade = musicFadeDuration / 2f;
            float elapsed  = 0f;

            while (elapsed < halfFade)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / halfFade);
                yield return null;
            }

            musicSource.clip  = newClip;
            musicSource.loop  = loop;
            musicSource.Play();

            elapsed = 0f;
            float targetVol = musicVolume * masterVolume;
            while (elapsed < halfFade)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, targetVol, elapsed / halfFade);
                yield return null;
            }

            musicSource.volume = targetVol;
        }

        // ─── Game Events ──────────────────────────────────────────────────────────

        private void HandleStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.CharacterSelect:
                    PlayMusic(characterSelectMusic);
                    break;
                case GameState.PreRound:
                case GameState.Playing:
                    if (musicSource != null && musicSource.clip != battleMusic)
                        PlayMusic(battleMusic);
                    break;
                case GameState.MatchEnd:
                    PlayMusic(victoryMusic, false);
                    break;
            }
        }

        private void HandleRoundStart(int round) => PlayWithVariation(roundStartSound);

        private void HandleRoundEnd(int winner, int round)
        {
            PlayWithVariation(roundEndSound);
            CharacterData winnerChar = winner == 1 ? gameManager.Player1Character : gameManager.Player2Character;
            if (winnerChar != null)
                PlayCharacterVoiceLine(winnerChar, VoiceLineCategory.Victory);
        }

        private void HandleMatchEnd(int winner) => PlayWithVariation(matchWinSound);

        // ─── Volume ───────────────────────────────────────────────────────────────

        private void UpdateVolumes()
        {
            if (musicSource  != null) musicSource.volume  = musicVolume  * masterVolume;
            if (sfxSource    != null) sfxSource.volume    = sfxVolume    * masterVolume;
            if (voiceSource  != null) voiceSource.volume  = voiceVolume  * masterVolume;
        }

        public void SetMasterVolume(float v) { masterVolume = Mathf.Clamp01(v); UpdateVolumes(); }
        public void SetMusicVolume(float v)  { musicVolume  = Mathf.Clamp01(v); UpdateVolumes(); }
        public void SetSFXVolume(float v)    { sfxVolume    = Mathf.Clamp01(v); UpdateVolumes(); }
        public void SetVoiceVolume(float v)  { voiceVolume  = Mathf.Clamp01(v); UpdateVolumes(); }
    }

    public enum UISoundType { ButtonClick, ButtonHover, Select, Confirm }
}
