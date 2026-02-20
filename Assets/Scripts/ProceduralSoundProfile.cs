using UnityEngine;

namespace DirtyThirtyShowdown
{
    public enum OscillatorType
    {
        Sine,
        Triangle,
        Square,     // Use lowPassCutoff to soften
        Sawtooth,
        Noise
    }

    [System.Serializable]
    public struct ArpeggioNote
    {
        [Tooltip("Frequency in Hz for this note")]
        public float frequency;
        [Tooltip("Duration in seconds for this note")]
        public float duration;
    }

    [CreateAssetMenu(menuName = "Dirty Thirty Showdown/Procedural Sound Profile", fileName = "NewSoundProfile")]
    public class ProceduralSoundProfile : ScriptableObject
    {
        [Header("Primary Oscillator")]
        public OscillatorType oscillatorType = OscillatorType.Sine;
        public float baseFrequency = 440f;

        [Header("Secondary Oscillator (set secondaryFrequency > 0 to enable)")]
        public OscillatorType secondaryOscType = OscillatorType.Sine;
        [Tooltip("Hz — 0 disables secondary oscillator")]
        public float secondaryFrequency = 0f;
        [Range(0f, 1f)] public float secondaryVolume = 0f;

        [Header("Amplitude Envelope (ADSR)")]
        [Tooltip("Total clip length in seconds")]
        public float duration = 0.3f;
        public float attack = 0.01f;
        public float decay = 0.1f;
        [Range(0f, 1f)] public float sustain = 0f;
        public float release = 0.05f;

        [Header("Pitch Envelope")]
        [Tooltip("Pitch multiplier at clip start (1 = base frequency)")]
        public float pitchStartMultiplier = 1f;
        [Tooltip("Pitch multiplier at clip end")]
        public float pitchEndMultiplier = 1f;
        [Tooltip("Easing curve for pitch sweep (X=normalised time, Y=blend 0→1)")]
        public AnimationCurve pitchCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Vibrato (LFO on pitch)")]
        [Tooltip("LFO frequency in Hz — 0 disables vibrato")]
        public float vibratoRate = 0f;
        [Range(0f, 0.15f), Tooltip("Depth as fraction of base frequency (0.01 = 1%)")]
        public float vibratoDepth = 0f;

        [Header("Noise")]
        [Range(0f, 1f), Tooltip("0 = pure oscillator, 1 = pure white noise")]
        public float noiseAmount = 0f;

        [Header("Filter")]
        [Tooltip("One-pole low-pass cutoff Hz — 0 bypasses (use to soften Square waves)")]
        public float lowPassCutoff = 0f;

        [Header("Arpeggio (overrides single frequency when entries are present)")]
        [Tooltip("List of notes to play in sequence. Leave empty for single-tone sounds.")]
        public ArpeggioNote[] arpeggio;

        [Header("Variation (baked into cached variants)")]
        [Range(0f, 0.1f), Tooltip("±frequency multiplier randomised per cached variant")]
        public float frequencyVariation = 0.03f;
        [Range(0f, 0.15f), Tooltip("±volume randomised at playback time")]
        public float volumeVariation = 0.05f;
        [Tooltip("Number of pre-generated variants (one picked randomly per play)")]
        public int variantCount = 3;

        [Header("Output")]
        [Range(0f, 1f)]
        public float volume = 0.75f;
    }
}
