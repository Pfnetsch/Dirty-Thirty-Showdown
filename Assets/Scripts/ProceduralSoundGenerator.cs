using UnityEngine;

namespace DirtyThirtyShowdown
{
    /// <summary>
    /// Synthesises AudioClips from ProceduralSoundProfile data.
    /// Supports: 2-oscillator mixing, ADSR envelope, pitch envelope,
    /// vibrato LFO, white noise blend, one-pole low-pass filter, arpeggio sequences.
    /// All generation runs on the main thread (called from AudioManager.Start).
    /// </summary>
    public static class ProceduralSoundGenerator
    {
        private const int SampleRate = 44100;

        // ─── Public Entry Point ───────────────────────────────────────────────────

        public static AudioClip GenerateClip(ProceduralSoundProfile profile,
            float freqMultiplier = 1f, string nameSuffix = "")
        {
            if (profile == null) return null;

            return profile.arpeggio != null && profile.arpeggio.Length > 0
                ? GenerateArpeggio(profile, freqMultiplier, nameSuffix)
                : GenerateSingle(profile, freqMultiplier, nameSuffix);
        }

        // ─── Single-Tone Generation ───────────────────────────────────────────────

        private static AudioClip GenerateSingle(ProceduralSoundProfile p,
            float freqMult, string suffix)
        {
            int total = Mathf.Max(1, Mathf.CeilToInt(p.duration * SampleRate));
            float[] samples = FillSamples(p, p.baseFrequency * freqMult, p.duration, total);

            return MakeClip($"{p.name}{suffix}", samples);
        }

        // ─── Arpeggio Generation ──────────────────────────────────────────────────

        private static AudioClip GenerateArpeggio(ProceduralSoundProfile p,
            float freqMult, string suffix)
        {
            // Compute total sample count across all notes
            int totalSamples = 0;
            foreach (var note in p.arpeggio)
                totalSamples += Mathf.Max(1, Mathf.CeilToInt(note.duration * SampleRate));

            float[] all = new float[totalSamples];
            int offset = 0;

            foreach (var note in p.arpeggio)
            {
                if (offset >= totalSamples) break;
                int count = Mathf.Max(1, Mathf.CeilToInt(note.duration * SampleRate));
                float[] noteData = FillNoteSamples(p, note.frequency * freqMult, note.duration, count);
                int copy = Mathf.Min(count, totalSamples - offset);
                System.Array.Copy(noteData, 0, all, offset, copy);
                offset += copy;
            }

            return MakeClip($"{p.name}{suffix}", all);
        }

        // ─── Core Sample Generation ───────────────────────────────────────────────

        /// <summary>
        /// Full synthesis for a single-note clip using the profile's ADSR + all modifiers.
        /// </summary>
        private static float[] FillSamples(ProceduralSoundProfile p,
            float baseFreq, float dur, int sampleCount)
        {
            float[] samples = new float[sampleCount];

            // Envelope boundaries
            float aTime  = Mathf.Max(0.001f, p.attack);
            float dTime  = Mathf.Max(0f,     p.decay);
            float rTime  = Mathf.Max(0.001f, p.release);
            float sLevel = p.sustain;
            float sStart = aTime + dTime;
            float sEnd   = Mathf.Max(sStart, dur - rTime);

            // LP filter coefficient (one-pole IIR)
            float lpAlpha = ComputeLPAlpha(p.lowPassCutoff);
            float prevLP  = 0f;

            bool hasSecondary = p.secondaryFrequency > 0f && p.secondaryVolume > 0f;
            double phase1 = 0, phase2 = 0;

            for (int i = 0; i < sampleCount; i++)
            {
                float t           = (float)i / SampleRate;
                float normalizedT = t / dur;

                // --- Vibrato LFO ---
                float vibrato = 1f;
                if (p.vibratoRate > 0f && p.vibratoDepth > 0f)
                    vibrato = 1f + Mathf.Sin(t * p.vibratoRate * Mathf.PI * 2f) * p.vibratoDepth;

                // --- Pitch envelope ---
                float pitchMult = Mathf.Lerp(p.pitchStartMultiplier, p.pitchEndMultiplier,
                                      p.pitchCurve.Evaluate(normalizedT)) * vibrato;

                // --- Primary oscillator ---
                double freq1 = baseFreq * pitchMult;
                phase1 += freq1 / SampleRate;
                float osc1 = SampleOsc(p.oscillatorType, (float)(phase1 % 1.0));

                // --- Secondary oscillator (tracks same pitch multiplier) ---
                float oscMix;
                if (hasSecondary)
                {
                    double freq2 = p.secondaryFrequency * pitchMult;
                    phase2 += freq2 / SampleRate;
                    float osc2 = SampleOsc(p.secondaryOscType, (float)(phase2 % 1.0));
                    // Normalise so total volume stays controlled
                    oscMix = (osc1 + osc2 * p.secondaryVolume) / (1f + p.secondaryVolume);
                }
                else
                {
                    oscMix = osc1;
                }

                // --- Noise blend ---
                float noise  = Random.Range(-1f, 1f);
                float signal = Mathf.Lerp(oscMix, noise, p.noiseAmount);

                // --- Low-pass filter ---
                prevLP = prevLP + lpAlpha * (signal - prevLP);
                signal  = prevLP;

                // --- ADSR envelope ---
                float env;
                if (t < aTime)
                    env = t / aTime;
                else if (t < sStart)
                    env = Mathf.Lerp(1f, sLevel, (t - aTime) / Mathf.Max(0.0001f, dTime));
                else if (t < sEnd)
                    env = sLevel;
                else
                    env = Mathf.Lerp(sLevel, 0f, (t - sEnd) / Mathf.Max(0.0001f, rTime));

                samples[i] = signal * env * p.volume;
            }

            return samples;
        }

        /// <summary>
        /// Per-note synthesis for arpeggio notes: simple attack + exponential decay to 0.
        /// Reuses oscillator, vibrato, noise and filter settings from the profile.
        /// </summary>
        private static float[] FillNoteSamples(ProceduralSoundProfile p,
            float freq, float dur, int sampleCount)
        {
            float[] samples = new float[sampleCount];

            float aTime  = Mathf.Min(0.006f, dur * 0.08f);
            float dTime  = Mathf.Max(0.001f, dur - aTime);

            float lpAlpha = ComputeLPAlpha(p.lowPassCutoff);
            float prevLP  = 0f;
            double phase  = 0;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;

                float vibrato = 1f;
                if (p.vibratoRate > 0f && p.vibratoDepth > 0f)
                    vibrato = 1f + Mathf.Sin(t * p.vibratoRate * Mathf.PI * 2f) * p.vibratoDepth;

                phase += (double)(freq * vibrato) / SampleRate;
                float osc    = SampleOsc(p.oscillatorType, (float)(phase % 1.0));
                float noise  = Random.Range(-1f, 1f);
                float signal = Mathf.Lerp(osc, noise, p.noiseAmount);

                prevLP = prevLP + lpAlpha * (signal - prevLP);
                signal  = prevLP;

                float env = t < aTime
                    ? t / aTime
                    : Mathf.Max(0f, 1f - (t - aTime) / dTime);

                samples[i] = signal * env * p.volume;
            }

            return samples;
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private static float SampleOsc(OscillatorType type, float phase)
        {
            return type switch
            {
                OscillatorType.Sine     => Mathf.Sin(phase * Mathf.PI * 2f),
                OscillatorType.Triangle => phase < 0.5f ? phase * 4f - 1f : (1f - phase) * 4f - 1f,
                OscillatorType.Square   => Mathf.Sin(phase * Mathf.PI * 2f) >= 0f ? 1f : -1f,
                OscillatorType.Sawtooth => phase * 2f - 1f,
                OscillatorType.Noise    => Random.Range(-1f, 1f),
                _ => 0f
            };
        }

        /// <summary>
        /// One-pole IIR low-pass: alpha=1 → bypass, alpha→0 → heavy filtering.
        /// cutoffHz=0 returns alpha=1 (bypass).
        /// </summary>
        private static float ComputeLPAlpha(float cutoffHz)
        {
            if (cutoffHz <= 0f) return 1f;
            float omega = 2f * Mathf.PI * cutoffHz / SampleRate;
            return omega / (1f + omega);
        }

        private static AudioClip MakeClip(string name, float[] samples)
        {
            var clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
