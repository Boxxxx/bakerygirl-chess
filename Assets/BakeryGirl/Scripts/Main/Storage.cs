using UnityEngine;
using System;
using System.Collections.Generic;
using BakeryGirl.Chess;
using FullInspector;

/// <summary>
/// To maintain shopping card list of both players and do buy-card-action
/// </summary>
public class Storage : BaseBehavior
{
    #region Variables
    public GameObject unitPrefab;
    public GameObject board0;
    public GameObject board1;
    public Dictionary<string, GameObject> cardGraphics;
    private Unit.OwnerEnum turn;
    private int[] resourceNum;
    private bool hasbuy;
    #endregion

    #region Public Interface Function
    /// <summary>
    /// Call by Controller, to initialize and start new game
    /// </summary>
    public void NewGame()
    {
        resourceNum = new int[2] { 0, 0 };
    }

    /// <summary>
    /// Create a specific unit with UnitInfo
    /// </summary>
    /// <param name="info">the info to create</param>
    /// <returns></returns>
    public Unit CreateUnit(UnitInfo info)
    {
        Unit unit = (Instantiate(unitPrefab) as GameObject).GetComponent<Unit>();
        unit.Initialize(info);
        return unit;
    }

    /// <summary>
    /// Update storage resource num, to buy cards
    /// </summary>
    /// <param name="white">white's resource num</param>
    /// <param name="black">black's resource num</param>
    public void UpdateResourceNum(int white, int black)
    {
        resourceNum[0] = white;
        resourceNum[1] = black;
        board0.gameObject.transform.Find("Number").GetComponent<NumberBar>().Number = white;
        board1.gameObject.transform.Find("Number").GetComponent<NumberBar>().Number = black;
    }

    /// <summary>
    /// switch turn (black | white) to show speific shop info
    /// </summary>
    /// <param name="turn"></param>
    public void SwitchTurn(Unit.OwnerEnum turn)
    {
        hasbuy = false;
        this.turn = turn;
        if (turn == Unit.OwnerEnum.Black)
        {
            board0.transform.Find("Unit").gameObject.SetActive(true);
            board1.transform.Find("Unit").gameObject.SetActive(false);
        }
        else
        {
            board0.transform.Find("Unit").gameObject.SetActive(false);
            board1.transform.Find("Unit").gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Do buy card action
    /// </summary>
    /// <param name="type"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public bool BuyCard(Unit.TypeEnum type, Unit.OwnerEnum owner)
    {
        Unit newCard = CreateUnit(new UnitInfo(BoardInfo.Base[(int)turn], type, owner));
        Transform card = NowBoard.transform.Find("Unit").transform.Find(TypeToIndex(type).ToString());
        newCard.transform.position = card.transform.position;

        hasbuy = true;

        newCard.Owner = owner;
        GlobalInfo.Instance.board.ModifyPlayerInfo(Unit.TypeEnum.Bread, turn, -StorageInfo.CardCost[TypeToIndex(newCard.Type)]);
        GlobalInfo.Instance.controller.BuyCardEffect(newCard, owner);

        return true;
    }

    /// <summary>
    /// Get the position that the bread will fly into to show resource addition
    /// </summary>
    /// <param name="owner">to specific which board to fly into</param>
    /// <returns></returns>
    public Vector3 GetCollectPoint(Unit.OwnerEnum owner)
    {
        if (owner == Unit.OwnerEnum.Black)
            return board0.transform.position + StorageInfo.collectPointOffset;
        else
            return board1.transform.position + StorageInfo.collectPointOffset;
    }

    public GameObject GetCardGraphics(Unit.TypeEnum type, Unit.OwnerEnum owner) {
        string spriteName;
        if (type == Unit.TypeEnum.Bread)
            spriteName = "bread";
        else if (type == Unit.TypeEnum.Void)
            spriteName = "void";
        else if (type == Unit.TypeEnum.Tile)
            spriteName = "tile";
        else
            spriteName = type.ToString().ToLower() + ((int)owner).ToString();
        return cardGraphics.ContainsKey(spriteName) ? cardGraphics[spriteName] : null;
    }

    public GameObject GetCardGraphicsByName(string name) {
        return cardGraphics.ContainsKey(name) ? cardGraphics[name] : null;
    }

    /// <summary>
    /// Get the resource num of both players.
    /// </summary>
    public int[] GetResourceNum() {
        return resourceNum;
    }
    public int GetResourceNum(Unit.OwnerEnum owner) {
        return resourceNum[(int)owner];
    }
    #endregion

    #region Private Function
    /// <summary>
    /// Get the shop board of matched turn
    /// </summary>
    private GameObject NowBoard
    {
        get
        {
            if (turn == Unit.OwnerEnum.Black)
                return board0;
            else
                return board1;
        }
    }

    /// <summary>
    /// Check whether now player can buy specific card or not
    /// </summary>
    /// <param name="type">the card's type</param>
    /// <returns></returns>
    private bool CanBuy(Unit.TypeEnum type)
    {
        if (hasbuy)
            return false;
        if (resourceNum[(int)turn] < StorageInfo.CardCost[TypeToIndex(type)])
            return false;
        if (GlobalInfo.Instance.board.GetUnitOwner(BoardInfo.Base[(int)turn]) != Unit.OwnerEnum.None)
            return false;
        if (type == Unit.TypeEnum.Boss && GlobalInfo.Instance.board.GetPlayerInfo(Unit.TypeEnum.Boss, turn) != 0)
            return false;
        if (GlobalInfo.Instance.board.GetPlayerTotalCount(turn) - GlobalInfo.Instance.board.GetPlayerInfo(Unit.TypeEnum.Bread, turn) >= 5)
            return false;

        return true;
    }

    /// <summary>
    /// Create the shopping card list, to click & buy
    /// </summary>
    /// <param name="board">which board to create</param>
    /// <param name="owner">owner type</param>
    private void CreateCardList(GameObject board, Unit.OwnerEnum owner)
    {
        for (int i = 0; i < StorageInfo.CardTypeList.Length; i++)
        {
            Unit unit = CreateUnit(new UnitInfo(StorageInfo.CardTypeList[i], owner));
            unit.name = i.ToString();
            unit.transform.parent = board.transform.Find("Unit");
            unit.transform.localPosition = StorageInfo.CardPosOffset[i];
        }
    }
    #endregion

    #region Public Static Funcions
    /// <summary>
    /// transform card type into index of Storage card list
    /// </summary>
    /// <param name="type">the card's type</param>
    /// <returns></returns>
    static public int TypeToIndex(Unit.TypeEnum type)
    {
        for (int i = 0; i < StorageInfo.CardTypeList.Length; i++)
            if (type == StorageInfo.CardTypeList[i])
                return i;
        return -1;
    }
    #endregion

    #region Unity Callback Functions
    protected override void Awake() {
        base.Awake();
        GlobalInfo.Instance.storage = this;
        resourceNum = new int[2] { 0, 0 };
    }

	void Start () {
        CreateCardList(board0, Unit.OwnerEnum.Black);
        CreateCardList(board1, Unit.OwnerEnum.White);
    }
	
	void Update () {
        if (GlobalInfo.Instance.controller.Phase != Controller.PhaseState.Other)
        {
            Transform board = NowBoard.transform.Find("Unit");
            foreach (Transform card in board)
            {
                RaycastHit hit;
                Unit.TypeEnum type = StorageInfo.CardTypeList[Convert.ToInt32(card.name)];
                if (!CanBuy(type))
                {
                    card.gameObject.SetActive(false);
                    card.GetComponent<Unit>().Focus = false;
                }
                else
                {
                    card.gameObject.SetActive(true);

                    if (GlobalInfo.Instance.controller.Phase == Controller.PhaseState.Player
                        && card.collider.Raycast(GlobalInfo.Instance.mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 1000f))
                    {
                        card.GetComponent<Unit>().Focus = true;
                        if (Input.GetMouseButtonUp(0))
                        {
                            GlobalInfo.Instance.controller.DoAction(PlayerAction.CreateBuy(type));
                        }
                    }
                    else
                        card.GetComponent<Unit>().Focus = false;
                }
            }
        }
    }
    #endregion
}
