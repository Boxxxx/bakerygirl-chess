using UnityEngine;
using System.Collections;

public class NGUIButtonHandler : MonoBehaviour {
    public object data;
    public int index = -1;

    //Used when passing message data
    public delegate void MessgeCb(int index, object data);
    //Used when btn state changed
    public delegate void EventHandle(object[] data);
    public MessgeCb clickedHandle = null;
    public MessgeCb longPressedHandle = null;
    public EventHandle onStateChangedHandle = null;

    private float m_longPressTime = 0.0f;
    private float m_lastCbTime = 0.0f;
    private float m_longPressEventTime = 0.5f;
    private float m_longPressCbInterval = 0.1f;

    public void SetData(int index, object data) {
        this.index = index;
        this.data = data;
    }

    bool m_isEnable = true;
    public bool IsEnable {
        get { return m_isEnable; }
        set {
            m_isEnable = value;
        }
    }

    private bool m_pressed = false;
    public bool Pressed {
        get { return m_pressed; }
    }
    private bool m_pointerOn = false;
    public bool PointerOn {
        get { return m_pointerOn; }
    }

    void OnClick() {
        if (!m_isEnable) return;
        if (clickedHandle != null) {
            clickedHandle(index, data);
            /*  UIBlackBoard.Instance.EventPool.Emit(UIEvent.ON_BTN_CLICK, soundType);
            if (soundType == UISoundManager.ButtonSoundType.Setting_Off)
                soundType = UISoundManager.ButtonSoundType.Setting_On;
            else if (soundType == UISoundManager.ButtonSoundType.Setting_On)
                soundType = UISoundManager.ButtonSoundType.Setting_Off;*/
        }
    }
    void OnPress(bool press) {
        if (!m_isEnable)
            return;
        if (press) {
            m_pressed = true;
            if (onStateChangedHandle != null)
                onStateChangedHandle(new object[] {"PointerDown"});
        }
        else {
            m_pressed = false;
            m_longPressTime = 0f;
            if (onStateChangedHandle != null)
                onStateChangedHandle(new object[] { "PointerUp" });
        }
    }
    void OnHover(bool hover) {
        if (!m_isEnable)
            return;
        m_pointerOn = hover;
        if (hover) {
            if (onStateChangedHandle != null)
                onStateChangedHandle(new object[] {"PointerOn"});
        }
        else {
            if (onStateChangedHandle != null)
                onStateChangedHandle(new object[] { "PointerOff" });
        }
    }
    void OnLongPressed() {
        if (!m_isEnable) return;
        if (longPressedHandle != null)
            longPressedHandle(index, data);
    }

    void Update() {
        if (m_pressed) {
            m_longPressTime += Time.deltaTime;
            if (m_longPressTime > m_longPressEventTime) {
                if (m_lastCbTime > m_longPressCbInterval) {
                    OnLongPressed();
                    m_lastCbTime = 0.0f;
                }
                m_lastCbTime += Time.deltaTime;
            }
        }
    }
}
