using UnityEngine;

/// <summary>
/// Procedural lo-fi audio synthesizer for WW3: Escape from North Korea.
/// Generates 22kHz mono PCM audio clips in memory without external asset dependencies.
/// Tailored for potato hardware (Intel HD 520) with negligible memory footprint.
/// </summary>
public static class ProceduralAudio
{
    private const int SampleRate = 22050; // 22kHz lo-fi mono

    /// <summary>
    /// Generates a metallic / breath whistle tone pulse (e.g. 1.2kHz with slight pitch bend).
    /// </summary>
    public static AudioClip CreateWhistleClip(float duration = 0.6f)
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        float baseFreq = 1250f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            // Slight pitch modulation
            float freq = baseFreq + Mathf.Sin(t * 25f) * 40f;
            float envelope = Mathf.Sin((float)i / sampleCount * Mathf.PI); // Smooth attack/decay
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.45f;
        }

        AudioClip clip = AudioClip.Create("Procedural_Whistle", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Generates a sharp noise burst simulating shattered glass / thrown bottle impact.
    /// </summary>
    public static AudioClip CreateBottleShatterClip(float duration = 0.4f)
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float decay = Mathf.Exp(-8f * (float)i / sampleCount);
            // High frequency noise with metallic resonant chime
            float whiteNoise = (Random.value * 2f - 1f);
            float chime = Mathf.Sin(2f * Mathf.PI * 2800f * ((float)i / SampleRate));
            samples[i] = (whiteNoise * 0.6f + chime * 0.4f) * decay * 0.5f;
        }

        AudioClip clip = AudioClip.Create("Procedural_BottleShatter", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Generates a subtle low-frequency footstep thud pulse.
    /// </summary>
    public static AudioClip CreateFootstepClip(float duration = 0.15f)
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float decay = Mathf.Exp(-18f * (float)i / sampleCount);
            float lowFreq = Mathf.Sin(2f * Mathf.PI * 110f * ((float)i / SampleRate));
            float noise = (Random.value * 2f - 1f) * 0.25f;
            samples[i] = (lowFreq + noise) * decay * 0.35f;
        }

        AudioClip clip = AudioClip.Create("Procedural_Footstep", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Helper to play an AudioClip at a point in world space.
    /// </summary>
    public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}
