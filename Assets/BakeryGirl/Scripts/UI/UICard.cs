using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UICard : MonoBehaviour {
    public int index = 0;

    private Button m_button;

	public void OnHover() {
        if (m_button.interactable) {
            GameInfo.Instance.storage.EnterUICard(index);
        }
    }

    public void OnClick() {
        if (m_button.interactable) {
            GameInfo.Instance.storage.ClickCard(index);
        }
    }

    void Awake() {
        m_button = GetComponent<Button>();
    }
}
