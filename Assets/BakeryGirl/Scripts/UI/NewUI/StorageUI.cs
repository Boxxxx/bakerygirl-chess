using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BakeryGirl.Chess;

public class StorageUI : MonoBehaviour {
    public PlayerUI player0;
    public PlayerUI player1;

    public PlayerUI NowPlayer {
        get {
            if (m_turn == Unit.OwnerEnum.Black)
                return player0;
            else
                return player1;
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
        player0.Resource = 0;
        player1.Resource = 0;
    }

    /// <summary>
    /// Rollback all info about storage back to cache state.
    /// </summary>
    /// <param name="cache"></param>
    public void RollbackGame(GameCache cache) {
        UpdateResourceNum(
            cache.descriptor.GetPlayerInfo(Unit.TypeEnum.Bread, Unit.OwnerEnum.Black),
            cache.descriptor.GetPlayerInfo(Unit.TypeEnum.Bread, Unit.OwnerEnum.White));
        SwitchTurn(cache.descriptor.Turn);
    }

    /// <summary>
    /// Update storage resource num, to buy cards
    /// </summary>
    /// <param name="white">white's resource num</param>
    /// <param name="black">black's resource num</param>
    public void UpdateResourceNum(int white, int black) {
        player0.Resource = white;
        player1.Resource = black;
    }

    /// <summary>
    /// switch turn (black | white) to show speific shop info
    /// </summary>
    /// <param name="turn"></param>
    public void SwitchTurn(Unit.OwnerEnum turn) {
        m_hasbuy = false;
        m_turn = turn;
        player0.IsMyTurn = turn == Unit.OwnerEnum.Black;
        player1.IsMyTurn = turn != Unit.OwnerEnum.Black;
        NowPlayer.endTurn.interactable = false;
        NowPlayer.cancel.interactable = false;
    }

    /// <summary>
    /// Do buy card action
    /// </summary>
    /// <param name="type"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public bool BuyCard(Unit.TypeEnum type, Unit.OwnerEnum owner) {
        Unit newCard = GlobalInfo.Instance.board.InstantiateUnit(new UnitInfo(BoardInfo.Base[(int)m_turn], type, owner));
        Transform card = NowPlayer.cards[TypeToIndex(type)].transform;
        newCard.transform.position = UI2WorldPosition(card.transform.position);

        m_hasbuy = true;

        newCard.Owner = owner;
        GlobalInfo.Instance.board.ModifyPlayerInfo(Unit.TypeEnum.Bread, m_turn, -StorageInfo.CardCost[TypeToIndex(newCard.Type)]);
        GlobalInfo.Instance.controller.BuyCardEffect(newCard, owner);

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
        return new int[] { player0.Resource, player1.Resource };
    }
    public int GetResourceNum(Unit.OwnerEnum owner) {
        if (owner == Unit.OwnerEnum.Black) {
            return player0.Resource;
        }
        else {
            return player1.Resource;
        }
    }

    public void ClickCard(int num) {
        if (GlobalInfo.Instance.controller.Phase == Controller.PhaseState.Player) {
            Unit.TypeEnum type = StorageInfo.CardTypeList[num];
            if (CanBuy(type)) {
                GlobalInfo.Instance.controller.DoAction(PlayerAction.CreateBuy(type));
            }
        }
    }

    public void EnterUICard(int index) {
        if (GlobalInfo.Instance.controller.Phase == Controller.PhaseState.Player) {
            Unit.TypeEnum type = StorageInfo.CardTypeList[index];
            GlobalInfo.Instance.characterImage.Show(Board.GetCardName(type, m_turn));
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
        if (GlobalInfo.Instance.board.GetUnitOwner(BoardInfo.Base[(int)m_turn]) != Unit.OwnerEnum.None)
            return false;
        if (type == Unit.TypeEnum.Boss && GlobalInfo.Instance.board.GetPlayerInfo(Unit.TypeEnum.Boss, m_turn) != 0)
            return false;
        if (GlobalInfo.Instance.board.GetPlayerTotalCount(m_turn) - GlobalInfo.Instance.board.GetPlayerInfo(Unit.TypeEnum.Bread, m_turn) >= 5)
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
    void Awake() {
        GlobalInfo.Instance.storage = this;
    }

    void Update() {
        if (GlobalInfo.Instance.controller.Phase == Controller.PhaseState.Player) {
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
        var worldPos = GlobalInfo.Instance.mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;
        return worldPos;
    }
    #endregion
}
