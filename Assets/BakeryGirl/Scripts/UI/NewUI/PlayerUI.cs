using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerUI : MonoBehaviour {
    public Text numRestResource;
    public RectTransform pointCollectResource;
    public Button endTurn;
    public Button cancel;
    public Image flag;
    public Image waitingMask;
    public Button[] cards;

    private int m_resource = 0;
    private bool m_isMyTurn = false;

    public int Resource {
        get { return m_resource; }
        set {
            m_resource = value;
            numRestResource.text = string.Format("{0}", m_resource.ToString("D2"));
        }
    }

    public bool IsMyTurn {
        get {
            return m_isMyTurn;
        }
        set {
            m_isMyTurn = value;
            endTurn.interactable = value;
            cancel.interactable = value;
            flag.gameObject.SetActive(value);
            //waitingMask.gameObject.SetActive(!value);
            foreach (var card in cards) {
                card.interactable = value;
            }
        }
    }
}
