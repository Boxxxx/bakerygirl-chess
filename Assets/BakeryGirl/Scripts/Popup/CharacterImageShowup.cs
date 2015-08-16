using UnityEngine;
using System.Collections.Generic;
using FullInspector;

public class CharacterImageShowup : BaseBehavior {
    public static readonly Vector3 kShowingOffset = new Vector3(-4, 0, 0);
    public const float kShowingTime = 0.25f;

    public SpriteRenderer sprite;

    public enum ShowState {
        NotShow, Showing, Showed, ShowingOff
    };

    public ShowState m_showState = ShowState.NotShow;
    public bool m_queuedShowing = false;
    public string m_queuedCardName;
    public string m_showedCardName;

    public void Show(string cardName) {
        if (m_showState == ShowState.Showed || m_showState == ShowState.Showing) {
            if (m_showedCardName != cardName) {
                _ShowOff();
                m_queuedShowing = true;
                m_queuedCardName = cardName;
            }
        }
        else if (m_showState == ShowState.NotShow) {
            _Show(cardName);
            m_queuedShowing = false;
            m_queuedCardName = "";
        }
        else {
            m_queuedShowing = true;
            m_queuedCardName = cardName;
        }
    }

    public void ShowOff() {
        if (m_showState != ShowState.NotShow) {
            _ShowOff();
            m_queuedShowing = false;
            m_queuedCardName = "";
        }
    }

    void OnShowingOffComplete() {
        if (m_queuedShowing) {
            _Show(m_queuedCardName);
        }
        else {
            m_showState = ShowState.NotShow;
        }
    }

    void OnShowingComplete() {
        m_showState = ShowState.Showed;
    }

    void _ShowOff() {
        iTween.MoveTo(sprite.gameObject, iTween.Hash("position", kShowingOffset, "time", kShowingTime, "oncomplete", "OnShowingOffComplete", "oncompletetarget", gameObject, "islocal", true));
        m_showState = ShowState.ShowingOff;
    }

    void _Show(string name) {
        var characterImage = ArtManager.Instance.GetCharacterImage(name);
        if (characterImage == null) {
            sprite.sprite = null;
            m_showState = ShowState.NotShow;
            m_showedCardName = "";
            return;
        }
        m_showedCardName = name;
        sprite.sprite = characterImage;
        sprite.transform.localPosition = kShowingOffset;
        iTween.MoveTo(sprite.gameObject, iTween.Hash("position", Vector3.zero, "time", kShowingTime, "oncomplete", "OnShowingComplete", "oncompletetarget", gameObject, "islocal", true));
        m_showState = ShowState.Showing;
    }

    protected override void Awake() {
        base.Awake();
    }
}
