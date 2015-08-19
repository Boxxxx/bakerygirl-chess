using UnityEngine;
using System.Collections;

public class UIRulePanel : MonoBehaviour {
    public const float kFadeTime = 0.2f;
    public void Show() {
        gameObject.SetActive(true);
        iTween.FadeFrom(gameObject, 0, kFadeTime);
    }
    public void Back() {
        gameObject.SetActive(false);
    }
}
