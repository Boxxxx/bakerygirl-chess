using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundChannel : MonoBehaviour
{
    public enum SoundType
    {
        BGM, Sound, Voice
    }

    public delegate void OnSoundPlayEnd(SoundChannel channel);
    public delegate void OnVolumeTweenEnd();

    string _key = "";
    public string Key
    {
        get
        {
            return _key;
        }
        set
        {
            _key = value;
            gameObject.name = "SoundChannel_" + _key;
        }
    }

    List<AudioSource> m_audioSources = new List<AudioSource>();
    float AudioSourcesVolume
    {
        set
        {
            for (int i = 0; i < m_audioSources.Count; i++)
                m_audioSources[i].volume = value;
        }
    }

    float m_channelVolume = 1;
    public float ChannelVolume
    {
        get
        {
            return m_channelVolume;
        }
        set
        {
            m_channelVolume = value;
        }
    }
    public float RealVolume
    {
        get
        {
            switch (m_soundType)
            {
                case SoundType.BGM:
                    return ChannelVolume * SoundManager.Instance.BGMVolume;
                case SoundType.Sound:
                    return ChannelVolume * SoundManager.Instance.SoundVolume;
                case SoundType.Voice:
                    return ChannelVolume * SoundManager.Instance.VoiceVolume;
            }
            return 0;
        }
    }

    public OnSoundPlayEnd onSoundPlayEnd = null;

    //channel play properties
    SoundType m_soundType;
    List<AudioClip> m_currentClips = new List<AudioClip>();
    List<double> m_currentClipsScheduledDspTime = new List<double>();
    int m_currentClipIndex = 0;
    bool m_looplast = false;
    float m_delay = 0;

    //tween properties
    bool m_tweenVolume = false;
    float m_tweenDestVolume, m_tweenStartVolume, m_tweenTime = 0, m_tweenCurrentTime = 0;
    iTween.EaseType m_tweenEaseType;
    OnVolumeTweenEnd m_tweenEndCb = null;

    public void TweenVolume(float destVolume, float time, iTween.EaseType easeType, OnVolumeTweenEnd endCB = null)
    {
        m_tweenVolume = true;
        m_tweenCurrentTime = 0;

        destVolume = Mathf.Clamp(destVolume, 0, 1);
        m_tweenDestVolume = destVolume;
        m_tweenStartVolume = ChannelVolume;
        m_tweenTime = time;
        m_tweenEaseType = easeType;
        m_tweenEndCb = endCB;
    }

    public bool IsPlaying
    {
        get
        {
            for (int i = 0; i < m_audioSources.Count; i++)
                if (m_audioSources[i].isPlaying)
                    return true;
            return false;
        }
    }

    void ExecuteStop()
    {
        for (int i = 0; i < m_audioSources.Count; i++)
            m_audioSources[i].Stop();
    }
    public void Stop(float fadeOutTime = 0)
    {
        if (fadeOutTime == 0)
        {
            ExecuteStop();
        }
        else
        {
            TweenVolume(0, fadeOutTime, iTween.EaseType.linear, ExecuteStop);
        }
    }

    float m_executeChannelVolume = 1;
    void ExecutePlayClips()
    {
        Stop();
        ChannelVolume = m_executeChannelVolume;
        AudioSourcesVolume = RealVolume;
        var t_scheduleTime = AudioSettings.dspTime + m_delay;
        for (int i = 0; i < m_currentClips.Count; i++)
        {
            if (i >= m_audioSources.Count)
            {
                m_audioSources.Add(gameObject.AddComponent<AudioSource>());
                m_audioSources[i].playOnAwake = false;
            }
            m_audioSources[i].clip = m_currentClips[i];
            m_audioSources[i].Stop();
            m_currentClipsScheduledDspTime.Add(t_scheduleTime);
            if (m_audioSources[i].clip != null)
                t_scheduleTime += m_currentClips[i].length;
            if (i == m_currentClips.Count - 1)
                m_audioSources[i].loop = m_looplast;
            else
                m_audioSources[i].loop = false;
        }
        m_audioSources[0].PlayScheduled(m_currentClipsScheduledDspTime[0]);
        m_currentClipIndex = 1;
    }
    public void PlayClip(SoundType type, AudioClip clip, OnSoundPlayEnd playEndCB = null, bool loop = false, float channelvolume = 1, float currentFadeOutTime = 0, float delay = 0)
    {
        PlayClips(type, new AudioClip[] { clip }, playEndCB, loop, channelvolume, currentFadeOutTime, delay);
    }
    public void PlayClips(SoundType type, AudioClip[] clips, OnSoundPlayEnd playEndCB = null, bool looplast = false, float channelvolume = 1, float currentFadeOutTime = 0, float delay = 0)
    {
        m_soundType = type;
        m_looplast = looplast;
        m_executeChannelVolume = channelvolume;
        onSoundPlayEnd = playEndCB;

        if (clips.Length == 0)
        {
            Stop();
            return;
        }

        //check if the same clips
        bool sameclips = true;
        if (m_currentClips.Count == clips.Length)
        {
            for (int i = 0; i < m_currentClips.Count; i++)
                if (m_currentClips[i] != clips[i])
                {
                    sameclips = false;
                    break;
                }
        }
        else
        {
            sameclips = false;
        }

        if (sameclips && IsPlaying)
        {
            return;
        }
        else
        {
            m_delay = delay;
            m_currentClips.Clear();
            m_currentClipsScheduledDspTime.Clear();
            m_currentClips.AddRange(clips);

            if (!IsPlaying || currentFadeOutTime <= 0)
            {
                ExecutePlayClips();
            }
            else
            {
                TweenVolume(0, currentFadeOutTime, iTween.EaseType.linear, ExecutePlayClips);
            }
        }
    }

    void Update()
    {
        if (m_tweenVolume)
        {
            m_tweenCurrentTime += Time.deltaTime;
            if (m_tweenTime <= 0)
                ChannelVolume = m_tweenDestVolume;
            else
                ChannelVolume = iTween.GetEasingValue(m_tweenStartVolume, m_tweenDestVolume, m_tweenCurrentTime / m_tweenTime, m_tweenEaseType);
            if (m_tweenCurrentTime >= m_tweenTime)
            {
                ChannelVolume = m_tweenDestVolume;
                m_tweenVolume = false;
                if (m_tweenEndCb != null)
                    m_tweenEndCb();
            }
        }
        AudioSourcesVolume = RealVolume;

        if (m_currentClipIndex < m_currentClipsScheduledDspTime.Count)
        {
            var nowDspTime = AudioSettings.dspTime;
            if (nowDspTime + 1.0 > m_currentClipsScheduledDspTime[m_currentClipIndex])
            {
                m_audioSources[m_currentClipIndex].PlayScheduled(m_currentClipsScheduledDspTime[m_currentClipIndex]);
                m_currentClipIndex++;
            }
        }

        if (onSoundPlayEnd != null && !IsPlaying)
        {
            onSoundPlayEnd(this);
            onSoundPlayEnd = null;
        }
    }
}
