using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    public Sprite normalSprite;
    public Sprite activeSprite;
    public Sprite selectedSprite;

    public bool alwaysBreath = false;
    public bool breathFromOriginColor = true;
    public float breathTime = 1.0f;
    public iTween.EaseType breathEaseType = iTween.EaseType.linear;
    public Color breathColor = new Color(1.0f, 1.0f, 1.0f, 0.6f);

    private float m_halfBreathTime = 0;
    private float m_breathDelta = 0;
    private Color m_initColor;
    private bool m_isActive = false;
    private bool m_isSelected = false;

    public bool IsActive {
        get { return m_isActive; }
        set {
            if (m_isActive != value) {
                m_isActive = value;
                RefreshSprite();
                if (m_isActive) {
                    m_breathDelta = 0;
                }
                else {
                    spriteRenderer.color = m_initColor;
                }
            }
        }
    }

    public bool IsSelected {
        get { return m_isSelected; }
        set {
            if (m_isSelected != value) {
                m_isSelected = value;
                RefreshSprite();
            }
        }
    }

    void Awake() {
        spriteRenderer.sprite = normalSprite;
        m_initColor = spriteRenderer.color;
        m_halfBreathTime = breathTime * .5f;
    }

    void Update() {
        var color = m_initColor;
        if (IsActive || alwaysBreath) {
            m_breathDelta += Time.deltaTime;
            if (m_breathDelta >= breathTime) {
                m_breathDelta -= breathTime;
            }
            float ratio = m_breathDelta / breathTime;
            ratio = ratio > 0.5f ? (1 - ratio) * 2 : ratio * 2;
            ratio = iTween.GetEasingValue(0, 1, ratio, breathEaseType);
            if (breathFromOriginColor) {
                color = Color.Lerp(color, breathColor, ratio);
            }
            else {
                color = Color.Lerp(breathColor, color, ratio);
            }
            spriteRenderer.color = color;
        }
    }

    private void RefreshSprite() {
        if (!m_isActive) {
            spriteRenderer.sprite = normalSprite;
        }
        else if (!m_isSelected) {
            spriteRenderer.sprite = activeSprite == null ? normalSprite : activeSprite;
        }
        else {
            spriteRenderer.sprite = selectedSprite == null ? normalSprite : selectedSprite;
        }
    }
}
