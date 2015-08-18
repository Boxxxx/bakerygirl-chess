using UnityEngine;
using UnityEngine.UI;

public class UIImageToggle : MonoBehaviour {
    public RectTransform on;
    public RectTransform off;

	public void SetSelected(bool selected) {
        on.gameObject.SetActive(selected);
        off.gameObject.SetActive(!selected);
    }
}
