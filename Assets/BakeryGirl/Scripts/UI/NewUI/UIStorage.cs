using UnityEngine;
using UnityEngine.UI;
using BakeryGirl.Chess;

public class UIStorage : MonoBehaviour {
    public UIPlayer playerDown;
    public UIPlayer playerUp;

    public UIStatus status;

    public UIPlayer PlayerBlack {
        get;
        private set;
    }
    public UIPlayer PlayerWhite {
        get;
        private set;
    }

    public UIPlayer NowPlayer {
        get {
            if (m_turn == Unit.OwnerEnum.Black)
                return PlayerBlack;
            else
                return PlayerWhite;
        }
    }

    public bool IsEndTurnValid {
        set {
            NowPlayer.endTurn.interactable = value;
        }
    }
    public bool IsCancelValid {
        set {
            NowPlayer.cancel.interactable = value;
        }
    }

    private Unit.OwnerEnum m_turn;
    private bool m_hasbuy = false;

    #region Public Interface Function
    /// <summary>
    /// Call by Controller, to initialize and start new game
    /// </summary>
    public void NewGame() {
        PlayerBlack = GameInfo.Instance.ShouldUpsidedown ? playerUp : playerDown;
        PlayerWhite = GameInfo.Instance.ShouldUpsidedown ? playerDown : playerUp;

        PlayerBlack.NewGame(Unit.OwnerEnum.Black);
        PlayerWhite.NewGame(Unit.OwnerEnum.White);
    }

    /// <summary>
    /// Rollback all info about storage back to cache state.
    /// </summary>
    /// <param name="cache"></param>
    public void RollbackGame(GameCache cache) {
        UpdateResourceNum(
            cache.descriptor.GetPlayerInfo(Unit.TypeEnum.Bread, Unit.OwnerEnum.Black),
            cache.descriptor.GetPlayerInfo(Unit.TypeEnum.Bread, Unit.OwnerEnum.White));
        SwitchTurn(cache.descriptor.Turn, cache.turnNum);
    }

    /// <summary>
    /// Update storage resource num, to buy cards
    /// </summary>
    /// <param name="white">white's resource num</param>
    /// <param name="black">black's resource num</param>
    public void UpdateResourceNum(int black, int white) {
        PlayerWhite.Resource = white;
        PlayerBlack.Resource = black;
    }

    /// <summary>
    /// switch turn (black | white) to show speific shop info
    /// </summary>
    /// <param name="turn"></param>
    public void SwitchTurn(Unit.OwnerEnum turn, int turnNum) {
        m_hasbuy = false;
        m_turn = turn;
        PlayerBlack.IsMyTurn = turn == Unit.OwnerEnum.Black;
        PlayerWhite.IsMyTurn = turn != Unit.OwnerEnum.Black;
        NowPlayer.endTurn.interactable = false;
        NowPlayer.cancel.interactable = false;
        status.SetTurn(turnNum);
    }

    /// <summary>
    /// Do buy card action
    /// </summary>
    /// <param name="type"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public bool BuyCard(Unit.TypeEnum type, Unit.OwnerEnum owner) {
        Unit newCard = GameInfo.Instance.board.InstantiateUnit(new UnitInfo(BoardInfo.Base[(int)m_turn], type, owner));
        Transform card = NowPlayer.cards[TypeToIndex(type)].transform;
        newCard.transform.position = UI2WorldPosition(card.transform.position);

        m_hasbuy = true;

        newCard.Owner = owner;
        GameInfo.Instance.board.ModifyPlayerInfo(Unit.TypeEnum.Bread, m_turn, -StorageInfo.CardCost[TypeToIndex(newCard.Type)]);
        GameInfo.Instance.controller.BuyCardEffect(newCard, owner);

        return true;
    }

    /// <summary>
    /// Get the position that the bread will fly into to show resource addition
    /// </summary>
    /// <param name="owner">to specific which board to fly into</param>
    /// <returns></returns>
    public Vector3 GetCollectPoint(Unit.OwnerEnum owner) {
        return UI2WorldPosition(NowPlayer.pointCollectResource.position);
    }
    
    /// <summary>
    /// Get the resource num of both players.
    /// </summary>
    public int[] GetResourceNum() {
        return new int[] { PlayerBlack.Resource, PlayerWhite.Resource };
    }
    public int GetResourceNum(Unit.OwnerEnum owner) {
        if (owner == Unit.OwnerEnum.Black) {
            return PlayerBlack.Resource;
        }
        else {
            return PlayerWhite.Resource;
        }
    }

    public void ClickCard(int num) {
        if (GameInfo.Instance.controller.Phase == Controller.PhaseState.Player) {
            Unit.TypeEnum type = StorageInfo.CardTypeList[num];
            if (CanBuy(type)) {
                GameInfo.Instance.controller.DoAction(PlayerAction.CreateBuy(type));
            }
        }
    }

    public void EnterUICard(int index) {
        if (GameInfo.Instance.controller.Phase == Controller.PhaseState.Player) {
            Unit.TypeEnum type = StorageInfo.CardTypeList[index];
            GameInfo.Instance.characterImage.Show(ArtManager.GetCardName(type, m_turn));
        }
    }
    #endregion

    #region Private Function
    /// <summary>
    /// Check whether now player can buy specific card or not
    /// </summary>
    /// <param name="type">the card's type</param>
    /// <returns></returns>
    private bool CanBuy(Unit.TypeEnum type) {
        if (m_hasbuy)
            return false;
        if (NowPlayer.Resource < StorageInfo.CardCost[TypeToIndex(type)])
            return false;
        if (GameInfo.Instance.board.GetUnitOwner(BoardInfo.Base[(int)m_turn]) != Unit.OwnerEnum.None)
            return false;
        if (type == Unit.TypeEnum.Boss && GameInfo.Instance.board.GetPlayerInfo(Unit.TypeEnum.Boss, m_turn) != 0)
            return false;
        if (GameInfo.Instance.board.GetPlayerTotalCount(m_turn) - GameInfo.Instance.board.GetPlayerInfo(Unit.TypeEnum.Bread, m_turn) >= 5)
            return false;

        return true;
    }
    #endregion

    #region Public Static Funcions
    /// <summary>
    /// transform card type into index of Storage card list
    /// </summary>
    /// <param name="type">the card's type</param>
    /// <returns></returns>
    static public int TypeToIndex(Unit.TypeEnum type) {
        for (int i = 0; i < StorageInfo.CardTypeList.Length; i++)
            if (type == StorageInfo.CardTypeList[i])
                return i;
        return -1;
    }
    #endregion

    #region Unity Callback Functions
    void Update() {
        if (GameInfo.Instance.controller.Phase == Controller.PhaseState.Player) {
            for (var i = 0; i < NowPlayer.cards.Length; i++) {
                var card = NowPlayer.cards[i];
                Unit.TypeEnum type = StorageInfo.CardTypeList[i];
                if (!CanBuy(type)) {
                    card.interactable = false;
                }
                else {
                    card.interactable = true;
                }
            }
        }
        else {
            foreach (var card in NowPlayer.cards) {
                card.interactable = false;
            }
        }
    }

    Vector3 UI2WorldPosition(Vector3 pos) {
        var screenPos = RectTransformUtility.WorldToScreenPoint(null, pos);
        var worldPos = GameInfo.Instance.mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;
        return worldPos;
    }
    #endregion
}
