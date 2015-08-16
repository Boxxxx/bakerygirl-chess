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
    private Unit.OwnerEnum m_ownerType = Unit.OwnerEnum.None;
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

    public void NewGame(Unit.OwnerEnum owner) {
        m_ownerType = owner;

        Resource = 0;
        playerName.text = 
            owner == Unit.OwnerEnum.Black 
            ? GameInfo.Instance.blackPlayerName 
            : GameInfo.Instance.whitePlayerName;

        flag.sprite = ArtManager.Instance.GetFlagSprite(owner);
        flag.SetNativeSize();

        for (int i = 0; i < cards.Length; i++) {
            var image = cards[i].GetComponent<Image>();
            image.sprite = ArtManager.Instance.GetStorageCardSprite(StorageInfo.CardTypeList[i], owner);
            image.SetNativeSize();
        }
    }
}
