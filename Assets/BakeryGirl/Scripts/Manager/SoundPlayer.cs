using UnityEngine;

public class SoundPlayer : MonoBehaviour {
    public void OnButtonClick() {
        SoundManager.Instance.PlaySound("button_click");
    }
}
