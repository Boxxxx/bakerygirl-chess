using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour {
    static SoundManager m_instance = null;
    public static SoundManager Instance {
        get {
            if (m_instance == null) {
                var obj = new GameObject();
                DontDestroyOnLoad(obj);
                obj.name = "SoundManager";
                m_instance = obj.AddComponent<SoundManager>();
                m_instance.pool_channels = obj.AddComponent<InstanceReusePool>();
                m_instance.pool_channels.instance_prefab = Resources.Load<UnityEngine.Object>("Sound/soundchannel");
            }
            return m_instance;
        }
    }

    public InstanceReusePool pool_channels;
    Dictionary<string, SoundChannel> soundChannels = new Dictionary<string, SoundChannel>();

    int m_soundAllocateId = 0, m_voiceAllocateId = 0;
    string m_soundResourcesPath = "Sound/";

    public SoundChannel this[string key] {
        get {
            if (!soundChannels.ContainsKey(key)) {
                var obj = pool_channels.AllocateObject();
                var channel = obj.GetComponent<SoundChannel>();
                channel.Key = key;
                soundChannels[key] = channel;
            }
            return soundChannels[key];
        }
        set {
            if (soundChannels.ContainsKey(value.Key))
                soundChannels.Remove(value.Key);
            value.Key = key;
            soundChannels[key] = value;
        }
    }
    public SoundChannel TryGetChannel(string key) {
        if (soundChannels.ContainsKey(key))
            return soundChannels[key];
        return null;
    }

    void Awake() {
        BGMVolume = 0.5f;
        SoundVolume = 0.45f;
        VoiceVolume = 0;
    }

    public float BGMVolume { get; set; }
    public float SoundVolume { get; set; }
    public float VoiceVolume { get; set; }
    public float AllVolume {
        get { return AudioListener.volume; }
        set { AudioListener.volume = value; }
    }

    AudioClip GetClipByKey(string key) {
        return Resources.Load<AudioClip>(key);
    }

    void OnChannelPlayEnd(SoundChannel channel) {
        soundChannels.Remove(channel.Key);
        pool_channels.RecycleObject(channel.gameObject);
    }

    public void PlayBGM(AudioClip clip, float delay = 0, string channel = "bgm", float fadeOutTime = 0) {
        this[channel].PlayClip(SoundChannel.SoundType.BGM, clip, OnChannelPlayEnd, true, 1, fadeOutTime, delay);
    }
    public void PlayBGM(string key, float delay = 0, string channel = "bgm", float fadeOutTime = 0) {
        PlayBGM(GetClipByKey(m_soundResourcesPath + "bgm/" + key), delay, channel, fadeOutTime);
    }
    public void PlayBGMWithIntro(AudioClip clipIntro, AudioClip clipLoop, float delay = 0, string channel = "bgm", float fadeOutTime = 0) {
        this[channel].PlayClips(SoundChannel.SoundType.BGM, new[] { clipIntro, clipLoop }, OnChannelPlayEnd, true, 1, fadeOutTime, delay);
    }
    public void PlayBGMWithIntro(string introKey, string loopKey, float delay = 0, string channel = "bgm", float fadeOutTime = 0) {
        PlayBGMWithIntro(GetClipByKey(m_soundResourcesPath + "bgm/" + introKey), GetClipByKey(m_soundResourcesPath + "bgm/" + loopKey),
            delay, channel, fadeOutTime);
    }

    public string PlayBGSound(string key, float delay = 0, string channel = "") {
        return PlaySound(GetClipByKey(m_soundResourcesPath + "se/" + key), delay, channel, true);
    }
    public string PlayBGSound(AudioClip clip, float delay = 0, string channel = "") {
        return PlaySound(clip, delay, channel, true);
    }
    public string PlaySound(string key, float delay = 0, string channel = "") {
        return PlaySound(GetClipByKey(m_soundResourcesPath + "se/" + key), delay, channel);
    }
    public string PlaySound(AudioClip clip, float delay = 0, string channel = "", bool loop = false) {
        if (string.IsNullOrEmpty(channel)) {
            channel = "sound_" + (++m_soundAllocateId).ToString();
        }
        this[channel].PlayClip(SoundChannel.SoundType.Sound, clip, OnChannelPlayEnd, loop, 1, 0, delay);
        return channel;
    }

    public string PlayVoice(string key, float delay = 0, string channel = "") {
        if (string.IsNullOrEmpty(channel)) {
            channel = "voice_" + (++m_voiceAllocateId).ToString();
        }
        this[channel].PlayClip(SoundChannel.SoundType.Voice, GetClipByKey(m_soundResourcesPath + "voice/" + key), OnChannelPlayEnd, false, 1,
            0, delay);
        return channel;
    }

    public void StopBGM(float fadeTime = 0) {
        StopChannel("bgm", fadeTime);
    }
    public void PauseBGM(float fadeTime = 0) {
        PauseChannel("bgm", fadeTime);
    }
    public void ResumeBGM(float fadeTime = 0) {
        ResumeChannel("bgm", fadeTime);
    }
    public void StopChannel(string channel, float fadeTime = 0) {
        var soundChannel = TryGetChannel(channel);
        if (soundChannel != null)
            soundChannel.Stop(fadeTime);
    }
    public void PauseChannel(string channel, float fadeTime = 0) {
        var soundChannel = TryGetChannel(channel);
        if (soundChannel != null)
            soundChannel.Pause(fadeTime);
    }
    public void ResumeChannel(string channel, float fadeInTime = 0) {
        var soundChannel = TryGetChannel(channel);
        if (soundChannel != null)
            soundChannel.Resume(fadeInTime);
    }

    void Start() {

    }
    void Update() {

    }
}
