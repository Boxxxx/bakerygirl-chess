using UnityEngine;
using UnityEngine.UI;

public class UIMusicBox : MonoBehaviour {
    public const string kBgmChannel = "bgm";
    public const float kBgmFadeOutTime = 0.5f;

    [System.Serializable]
    public class SongInfo {
        public string songName;
        public string bgmIntroFile;
        public string bgmLoopFile;
    }

    public Text songName;
    public SongInfo[] musicInfos = new SongInfo[] { };

    public bool autoPlay = true;

    private bool m_paused = true;
    private bool m_playing = false;
    private int m_currentIndex = 0;

    public bool Paused {
        get {
            return m_paused;
        }
        set {
            m_paused = value;
        }
    }

    public bool IsPlaying {
        get {
            return m_playing;
        }
        set {
            if (m_playing != value) {
                m_playing = value;
                if (m_playing) {
                    if (musicInfos.Length == 0) {
                        m_playing = false;
                        return;
                    }
                    PlaySong(m_currentIndex);
                }
                else {
                    SoundManager.Instance.StopBGM(kBgmFadeOutTime);
                }
            }
        }
    }

    public void PlaySong(int index) {
        if (musicInfos.Length == 0) {
            return;
        }
        m_currentIndex = (index % musicInfos.Length + musicInfos.Length) % musicInfos.Length;
        _PlaySong(musicInfos[m_currentIndex]);
    }

	public void NextSong() {
        if (musicInfos.Length == 0) {
            return;
        }
        m_currentIndex = (m_currentIndex + 1) % musicInfos.Length;
        _PlaySong(musicInfos[m_currentIndex]);
    }

    public void LastSong() {
        if (musicInfos.Length == 0) {
            return;
        }
        m_currentIndex = (m_currentIndex - 1 + musicInfos.Length) % musicInfos.Length;
        _PlaySong(musicInfos[m_currentIndex]);
    }

    public void PauseSong() {
        IsPlaying = false;
    }

    public void ResumeSong() {
        IsPlaying = true;
    }

    private void _PlaySong(SongInfo songInfo) {
        m_playing = true;
        songName.text = songInfo.songName;
        if (!string.IsNullOrEmpty(songInfo.bgmIntroFile)) {
            SoundManager.Instance.PlayBGMWithIntro(songInfo.bgmIntroFile, songInfo.bgmLoopFile, 0, kBgmChannel, kBgmFadeOutTime);
        }
        else {
            SoundManager.Instance.PlayBGM(songInfo.bgmLoopFile, 0, kBgmChannel, kBgmFadeOutTime);
        }
    }

    void Start() {
        if (autoPlay) {
            IsPlaying = true;
        }
    }
}
