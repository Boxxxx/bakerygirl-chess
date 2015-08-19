using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class ButtonEnhance : MonoBehaviour {
    public Material disableMaterial;

    private Button m_button;
    private Image m_image;
    private bool m_interactable = false;

	public bool Interactable {
        get {
            return m_interactable;
        }
        set {
            m_interactable = value;
            m_button.interactable = value;
            m_image.material = m_interactable ? null : disableMaterial;
        }
    }

    public void SetInteractable(bool value) {
        Interactable = value;
    }

    void Awake() {
        m_button = GetComponent<Button>();
        m_image = GetComponent<Image>();
    }
}
