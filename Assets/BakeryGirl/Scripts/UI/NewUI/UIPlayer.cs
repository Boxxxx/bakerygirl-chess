using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIPlayer : MonoBehaviour {
    public Text numRestResource;
    public RectTransform pointCollectResource;
    public Button endTurn;
    public Button cancel;
    public Image flag;
    public Image waitingMask;
    public Text playerName;
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

    public string Username {
        get { return playerName.text; }
        set {
            playerName.text = value;
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
            waitingMask.gameObject.SetActive(!value);
            flag.color = value ? StorageInfo.Orange : Color.black;
            playerName.color = value ? StorageInfo.Orange : Color.black;
            foreach (var card in cards) {
                card.interactable = value;
            }
        }
    }
}
