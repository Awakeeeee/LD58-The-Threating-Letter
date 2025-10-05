using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class SFXManager : MonoBehaviourSingleton<SFXManager>
{
    [TitleGroup("音效注册")]
    public List<ClipEntry> clips;

    [TitleGroup("Audio Sources")]
    public AudioSource bgmPlayer;
    public AudioSource sfxPlayer;
    public AudioSource loopSfxPlayer;

    [TitleGroup("Play Settings")]
    public float loopFadeInDuration = 0.1f;
    public float loopFadeOutDuration = 0.2f;

    private Dictionary<string, AudioClip> clipDict;
    private Coroutine loopSfxCoroutine;

    protected override void Awake()
    {
        base.Awake();
        InitializeClipDictionary();
    }

    private void InitializeClipDictionary()
    {
        clipDict = new Dictionary<string, AudioClip>();
        foreach (var entry in clips)
        {
            if (!string.IsNullOrEmpty(entry.name) && entry.clip != null)
            {
                clipDict[entry.name] = entry.clip;
            }
        }
    }

    public void PlaySFX(string clipName, float volume = 1f)
    {
        AudioClip clip = GetClip(clipName);
        if (clip != null && sfxPlayer != null)
        {
            sfxPlayer.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning($"SFXManager: Cannot play clip '{clipName}' - clip or player is null");
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxPlayer != null)
        {
            sfxPlayer.PlayOneShot(clip, volume);
        }
    }

    public void PlayBGM(string clipName, bool loop = true, float volume = 1f)
    {
        AudioClip clip = GetClip(clipName);
        if (clip != null && bgmPlayer != null)
        {
            bgmPlayer.clip = clip;
            bgmPlayer.loop = loop;
            bgmPlayer.volume = volume;
            bgmPlayer.Play();
        }
    }

    public void StopBGM(float fadeDuration = 0f)
    {
        if (bgmPlayer == null) return;

        if (fadeDuration > 0)
        {
            bgmPlayer.DOFade(0f, fadeDuration).OnComplete(() => bgmPlayer.Stop());
        }
        else
        {
            bgmPlayer.Stop();
        }
    }

    public void StartLoopSFX(string clipName, float targetVolume = 1f)
    {
        if (loopSfxPlayer == null)
        {
            Debug.LogWarning("SFXManager: loopSfxPlayer is null");
            return;
        }

        AudioClip clip = GetClip(clipName);
        if (clip == null)
        {
            Debug.LogWarning($"SFXManager: Loop clip '{clipName}' not found");
            return;
        }

        loopSfxPlayer.DOKill();

        if (loopSfxPlayer.isPlaying && loopSfxPlayer.clip == clip)
        {
            loopSfxPlayer.DOFade(targetVolume, loopFadeInDuration);
        }
        else
        {
            loopSfxPlayer.clip = clip;
            loopSfxPlayer.loop = true;
            loopSfxPlayer.volume = 0f;
            loopSfxPlayer.Play();
            loopSfxPlayer.DOFade(targetVolume, loopFadeInDuration);
        }
    }

    public void StopLoopSFX(bool immediate = false)
    {
        if (loopSfxPlayer == null || !loopSfxPlayer.isPlaying) return;

        loopSfxPlayer.DOKill();

        if (immediate)
        {
            loopSfxPlayer.Stop();
            loopSfxPlayer.volume = 0f;
        }
        else
        {
            loopSfxPlayer.DOFade(0f, loopFadeOutDuration).OnComplete(() =>
            {
                loopSfxPlayer.Stop();
            });
        }
    }

    public void PauseLoopSFX()
    {
        if (loopSfxPlayer == null) return;

        loopSfxPlayer.DOKill();
        loopSfxPlayer.DOFade(0f, loopFadeOutDuration).OnComplete(() =>
        {
            loopSfxPlayer.Pause();
        });
    }

    public void ResumeLoopSFX(float targetVolume = 1f)
    {
        if (loopSfxPlayer == null || loopSfxPlayer.clip == null) return;

        loopSfxPlayer.DOKill();
        loopSfxPlayer.UnPause();
        loopSfxPlayer.DOFade(targetVolume, loopFadeInDuration);
    }

    public void FadeSFXVolume(float targetVolume, float duration)
    {
        if (sfxPlayer != null)
        {
            sfxPlayer.DOFade(targetVolume, duration);
        }
    }

    public void FadeBGMVolume(float targetVolume, float duration)
    {
        if (bgmPlayer != null)
        {
            bgmPlayer.DOFade(targetVolume, duration);
        }
    }

    private AudioClip GetClip(string clipName)
    {
        if (clipDict != null && clipDict.TryGetValue(clipName, out AudioClip clip))
        {
            return clip;
        }
        return null;
    }

    public bool IsLoopSFXPlaying()
    {
        return loopSfxPlayer != null && loopSfxPlayer.isPlaying;
    }
}

[System.Serializable]
public class ClipEntry
{
    public string name;
    public AudioClip clip;
}
