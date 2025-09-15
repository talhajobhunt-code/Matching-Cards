using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip flipSound;
    public AudioClip matchSound;
    public AudioClip mismatchSound;
    public AudioClip gameOverSound;

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 2f)] public float pitch = 1f;
    public bool enableAudio = true;

    [Header("Audio Sources")]
    public int audioSourceCount = 4;

    // Audio Source Pool
    private AudioSource[] audioSources;
    private int currentSourceIndex = 0;

    // Audio Source Types
    private AudioSource flipAudioSource;
    private AudioSource matchAudioSource;
    private AudioSource mismatchAudioSource;
    private AudioSource gameOverAudioSource;

    private void Awake()
    {
        InitializeAudioSources();
        LoadAudioSettings();
    }

    private void InitializeAudioSources()
    {
        // Create audio source pool for overlapping sounds
        audioSources = new AudioSource[audioSourceCount];

        for (int i = 0; i < audioSourceCount; i++)
        {
            GameObject audioSourceObject = new GameObject($"AudioSource_{i}");
            audioSourceObject.transform.SetParent(transform);

            audioSources[i] = audioSourceObject.AddComponent<AudioSource>();
            audioSources[i].playOnAwake = false;
            audioSources[i].volume = sfxVolume * masterVolume;
            audioSources[i].pitch = pitch;
        }

        // Create dedicated audio sources for specific sounds
        flipAudioSource = CreateDedicatedAudioSource("FlipAudioSource");
        matchAudioSource = CreateDedicatedAudioSource("MatchAudioSource");
        mismatchAudioSource = CreateDedicatedAudioSource("MismatchAudioSource");
        gameOverAudioSource = CreateDedicatedAudioSource("GameOverAudioSource");
    }

    private AudioSource CreateDedicatedAudioSource(string name)
    {
        GameObject sourceObject = new GameObject(name);
        sourceObject.transform.SetParent(transform);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.volume = sfxVolume * masterVolume;
        source.pitch = pitch;

        return source;
    }

    public void PlayFlipSound()
    {
        PlaySound(flipSound, flipAudioSource);
    }

    public void PlayMatchSound()
    {
        PlaySound(matchSound, matchAudioSource, 1.1f); // Slightly higher pitch for positive feedback
    }

    public void PlayMismatchSound()
    {
        PlaySound(mismatchSound, mismatchAudioSource, 0.9f); // Slightly lower pitch for negative feedback
    }

    public void PlayGameOverSound()
    {
        PlaySound(gameOverSound, gameOverAudioSource);
    }

    private void PlaySound(AudioClip clip, AudioSource dedicatedSource, float pitchModifier = 1f)
    {
        if (!enableAudio || clip == null) return;

        if (dedicatedSource != null)
        {
            dedicatedSource.clip = clip;
            dedicatedSource.volume = sfxVolume * masterVolume;
            dedicatedSource.pitch = pitch * pitchModifier;
            dedicatedSource.Play();
        }
        else
        {
            // Fallback to pooled audio sources
            PlaySoundPooled(clip, pitchModifier);
        }
    }

    private void PlaySoundPooled(AudioClip clip, float pitchModifier = 1f)
    {
        if (!enableAudio || clip == null) return;

        AudioSource source = GetNextAudioSource();
        source.clip = clip;
        source.volume = sfxVolume * masterVolume;
        source.pitch = pitch * pitchModifier;
        source.Play();
    }

    private AudioSource GetNextAudioSource()
    {
        AudioSource source = audioSources[currentSourceIndex];
        currentSourceIndex = (currentSourceIndex + 1) % audioSources.Length;
        return source;
    }

    // Volume and settings control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        SaveAudioSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        SaveAudioSettings();
    }

    public void SetPitch(float newPitch)
    {
        pitch = Mathf.Clamp(newPitch, 0.5f, 2f);
        UpdateAllPitch();
        SaveAudioSettings();
    }

    public void SetAudioEnabled(bool enabled)
    {
        enableAudio = enabled;
        SaveAudioSettings();
    }

    private void UpdateAllVolumes()
    {
        float finalVolume = sfxVolume * masterVolume;

        foreach (AudioSource source in audioSources)
        {
            if (source != null)
            {
                source.volume = finalVolume;
            }
        }

        if (flipAudioSource != null) flipAudioSource.volume = finalVolume;
        if (matchAudioSource != null) matchAudioSource.volume = finalVolume;
        if (mismatchAudioSource != null) mismatchAudioSource.volume = finalVolume;
        if (gameOverAudioSource != null) gameOverAudioSource.volume = finalVolume;
    }

    private void UpdateAllPitch()
    {
        foreach (AudioSource source in audioSources)
        {
            if (source != null)
            {
                source.pitch = pitch;
            }
        }

        if (flipAudioSource != null) flipAudioSource.pitch = pitch;
        if (matchAudioSource != null) matchAudioSource.pitch = pitch;
        if (mismatchAudioSource != null) mismatchAudioSource.pitch = pitch;
        if (gameOverAudioSource != null) gameOverAudioSource.pitch = pitch;
    }

    // Fade effects
    public void FadeOut(float duration = 1f)
    {
        StartCoroutine(FadeVolumeCoroutine(sfxVolume * masterVolume, 0f, duration));
    }

    public void FadeIn(float duration = 1f)
    {
        StartCoroutine(FadeVolumeCoroutine(0f, sfxVolume * masterVolume, duration));
    }

    private IEnumerator FadeVolumeCoroutine(float startVolume, float targetVolume, float duration)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / duration;
            float currentVolume = Mathf.Lerp(startVolume, targetVolume, t);

            foreach (AudioSource source in audioSources)
            {
                if (source != null)
                {
                    source.volume = currentVolume;
                }
            }

            yield return null;
        }

        // Ensure final volume is set
        foreach (AudioSource source in audioSources)
        {
            if (source != null)
            {
                source.volume = targetVolume;
            }
        }
    }

    // Save/Load settings
    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("AudioManager_MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("AudioManager_SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("AudioManager_Pitch", pitch);
        PlayerPrefs.SetInt("AudioManager_EnableAudio", enableAudio ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("AudioManager_MasterVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("AudioManager_SFXVolume", 1f);
        pitch = PlayerPrefs.GetFloat("AudioManager_Pitch", 1f);
        enableAudio = PlayerPrefs.GetInt("AudioManager_EnableAudio", 1) == 1;

        UpdateAllVolumes();
        UpdateAllPitch();
    }

    // Utility methods
    public void StopAllSounds()
    {
        foreach (AudioSource source in audioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }

        if (flipAudioSource != null && flipAudioSource.isPlaying) flipAudioSource.Stop();
        if (matchAudioSource != null && matchAudioSource.isPlaying) matchAudioSource.Stop();
        if (mismatchAudioSource != null && mismatchAudioSource.isPlaying) mismatchAudioSource.Stop();
        if (gameOverAudioSource != null && gameOverAudioSource.isPlaying) gameOverAudioSource.Stop();
    }

    public bool IsAnyAudioPlaying()
    {
        foreach (AudioSource source in audioSources)
        {
            if (source != null && source.isPlaying)
            {
                return true;
            }
        }

        return (flipAudioSource != null && flipAudioSource.isPlaying) ||
               (matchAudioSource != null && matchAudioSource.isPlaying) ||
               (mismatchAudioSource != null && mismatchAudioSource.isPlaying) ||
               (gameOverAudioSource != null && gameOverAudioSource.isPlaying);
    }

    // Debug information
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugAudioInfo()
    {
        Debug.Log($"Audio Manager Status:");
        Debug.Log($"  Master Volume: {masterVolume}");
        Debug.Log($"  SFX Volume: {sfxVolume}");
        Debug.Log($"  Pitch: {pitch}");
        Debug.Log($"  Audio Enabled: {enableAudio}");
        Debug.Log($"  Any Audio Playing: {IsAnyAudioPlaying()}");
    }
}