using UnityEngine;
using System.Collections.Generic;
using FullInspector;

public class SlotInfo {
    public Unit.TypeEnum type = Unit.TypeEnum.Void;
    public Unit.OwnerEnum owner = Unit.OwnerEnum.None;
    public bool hasBread = false;

    public override string ToString() {
        return string.Format("({0}, {1}, {2})", type, owner, hasBread);
    }
}

/// <summary>
/// Maintain Chessboard and game turn
/// </summary>
public class Board : BaseBehavior
{
    #region Enums
    public enum GridState {Base0, Base1, Void, Bread};
    #endregion

    public GameObject unitPrefab;

    #region Variables
    // board data
	private GridState[,] boardState;
    private Unit[,] boardUnit;
    private Unit[,] boardBread;
    private int restBreadNum;
    private Dictionary<Unit.TypeEnum, int>[] playerInfo;
    #endregion

    #region Properties
    public int RestBread
    {
        get { return restBreadNum; }
    }
    public GridState[,] GridInfo { get { return boardState.Clone() as GridState[,]; } }
    #endregion

    #region Public Interface Function
    /// <summary>
    /// Start a new game, refresh all the data
    /// </summary>
	public void NewGame()
	{
        ClearGame();
        boardUnit = new Unit[BoardInfo.Row, BoardInfo.Col];
        boardBread = new Unit[BoardInfo.Row, BoardInfo.Col];
		boardState = new GridState[BoardInfo.Row, BoardInfo.Col];
        playerInfo = new Dictionary<Unit.TypeEnum, int>[2]{new Dictionary<Unit.TypeEnum, int>(), new Dictionary<Unit.TypeEnum, int>()};

        for (int r = 0; r < BoardInfo.Row; r++)
            for (int c = 0; c < BoardInfo.Col; c++)
                boardState[r, c] = GridState.Void;

        boardState[BoardInfo.Base[0].R, BoardInfo.Base[0].C] = GridState.Base0;
        boardState[BoardInfo.Base[1].R, BoardInfo.Base[1].C] = GridState.Base1;

		foreach(Position pos in BoardInfo.BreadList)
            CreateUnit(new UnitInfo(pos, Unit.TypeEnum.Bread));
        restBreadNum = BoardInfo.BreadList.Length;

        foreach (UnitInfo info in BoardInfo.InitUnitList)
            CreateUnit(info);
	}
    /// <summary>
    /// Clear the gameobjects
    /// </summary>
    public void ClearGame()
    {
        if(boardUnit != null)
            foreach (Unit unit in boardUnit)
                if(unit != null)
                    GameObject.Destroy(unit.gameObject);
        if(boardBread != null)
            foreach (Unit unit in boardBread)
                if (unit != null)   
                    GameObject.Destroy(unit.gameObject);
    }
    /// <summary>
    /// Rollback the board to cached state.
    /// </summary>
    public void RollbackGame(GameCache cache) {
        ClearGame();
        boardUnit = new Unit[BoardInfo.Row, BoardInfo.Col];
        boardBread = new Unit[BoardInfo.Row, BoardInfo.Col];
        boardState = new GridState[BoardInfo.Row, BoardInfo.Col];
        playerInfo = new Dictionary<Unit.TypeEnum, int>[2] { new Dictionary<Unit.TypeEnum, int>(), new Dictionary<Unit.TypeEnum, int>() };
        restBreadNum = 0;

        for (int i = 0; i < BoardInfo.Row; i++) {
            for (int j = 0; j < BoardInfo.Col; j++) {
                boardState[i, j] = cache.descriptor.GetGridState(i, j);
                if (boardState[i, j] == GridState.Bread) {
                    CreateUnit(new UnitInfo(new Position(i, j), Unit.TypeEnum.Bread));
                }
                var unitInfo = cache.descriptor.GetUnitInfo(i, j);
                if (unitInfo.type >= Unit.TypeEnum.Scout && unitInfo.type <= Unit.TypeEnum.Bomb) {
                    CreateUnit(unitInfo);
                }
            }
        }
        for (Unit.TypeEnum type = Unit.TypeEnum.Bread; type < Unit.TypeEnum.Void; type++) {
            playerInfo[(int)Unit.OwnerEnum.Black][type] = cache.descriptor.GetPlayerInfo(type, Unit.OwnerEnum.Black);
            playerInfo[(int)Unit.OwnerEnum.White][type] = cache.descriptor.GetPlayerInfo(type, Unit.OwnerEnum.White);
        }
        restBreadNum = cache.descriptor.RestResource;
        SwitchTurn(cache.descriptor.Turn);
    }

    /// <summary>
    /// Pick a unit from board (not delete)
    /// </summary>
    /// <param name="pos">the position to pick</param>
    /// <returns></returns>
    public Unit Pick(Position pos)
    {
        if (boardUnit[pos.R, pos.C] != null)
        {
            Unit tmp = boardUnit[pos.R, pos.C];
            boardUnit[pos.R, pos.C] = null;
            ModifyPlayerInfo(tmp.Type, tmp.Owner, -1);
            return tmp;
        }
        else if (GetUnitType(pos) == Unit.TypeEnum.Bread)
        {
            restBreadNum--;
            Unit tmp = boardBread[pos.R, pos.C];
            boardState[pos.R, pos.C] = GridState.Void;
            boardBread[pos.R, pos.C] = null;
            return tmp;
        }
        return null;
    }
    /// <summary>
    /// Put a unit into board
    /// </summary>
    /// <param name="pos">the position to put</param>
    /// <param name="unit">the unit to put</param>
    /// <returns></returns>
    public bool Put(Position pos, Unit unit)
    {
        if (GetUnitOwner(pos) == Unit.OwnerEnum.None)
        {
            unit.setPosition(pos);
            if (Unit.IsSoldier(unit.Type))
            {
                boardUnit[pos.R, pos.C] = unit;
                ModifyPlayerInfo(unit.Type, unit.Owner, 1);
            }
            else if (unit.Type == Unit.TypeEnum.Bread)
            {
                boardState[pos.R, pos.C] = GridState.Bread;
                boardBread[pos.R, pos.C] = unit;
                restBreadNum++;
            }
            return true;
        }
        return false;
    }
    /// <summary>
    /// Create specific unit & put it into board
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private bool CreateUnit(UnitInfo info)
    {
        if (!info.pos.IsValid)
            return false;
        if (info.owner != Unit.OwnerEnum.None && GetUnitOwner(info.pos) != Unit.OwnerEnum.None)
            return false;

        Unit unit = InstantiateUnit(info);

        Put(info.pos, unit);

        if (unit.Type == Unit.TypeEnum.Bread)
            AddBreadBounceEffect(unit);

        return true;
    }
    private void AddBreadBounceEffect(Unit unit)
    {
    }

    /// <summary>
    /// Get the count of specific type&owner card in this game
    /// </summary>
    /// <param name="type">the type to query</param>
    /// <param name="owner">the owner to query</param>
    /// <returns></returns>
    public int GetPlayerInfo(Unit.TypeEnum type, Unit.OwnerEnum owner)
    {
        if(playerInfo[(int)owner].ContainsKey(type))
            return playerInfo[(int)owner][type];
        else
            return 0;
    }
    /// <summary>
    /// Modify specific type&owner info with delta
    /// </summary>
    /// <param name="type">the type to modify</param>
    /// <param name="owner">the owner to modify</param>
    /// <param name="delta">the delta to modify</param>
    /// <returns></returns>
    public int ModifyPlayerInfo(Unit.TypeEnum type, Unit.OwnerEnum owner, int delta)
    {
        if (!playerInfo[(int)owner].ContainsKey(type))
            playerInfo[(int)owner][type] = delta;
        else
            playerInfo[(int)owner][type] += delta;

        if(type == Unit.TypeEnum.Bread)
            GameInfo.Instance.storage.UpdateResourceNum(GameInfo.Instance.board.GetPlayerInfo(Unit.TypeEnum.Bread, Unit.OwnerEnum.Black),
                                      GameInfo.Instance.board.GetPlayerInfo(Unit.TypeEnum.Bread, Unit.OwnerEnum.White));

        return playerInfo[(int)owner][type];
    }
    /// <summary>
    /// Sum the total count of all kinds of unit of owner
    /// </summary>
    /// <param name="owner">the owner to query</param>
    /// <returns></returns>
    public int GetPlayerTotalCount(Unit.OwnerEnum owner)
    {
        int sum = 0;
        foreach (KeyValuePair<Unit.TypeEnum,int> single in playerInfo[(int)owner])
            sum += single.Value;
        return sum;
    }
    /// <summary>
    /// Get the total count of soldiers of specific owner(it's, remove bread count)
    /// </summary>
    /// <param name="owner">the owner to query</param>
    /// <returns></returns>
    public int GetPlayerSolider(Unit.OwnerEnum owner)
    {
        int sum = GetPlayerTotalCount(owner);
        return sum - GetPlayerInfo(Unit.TypeEnum.Bread, owner);
    }

    /// <summary>
    /// Create a specific unit with UnitInfo
    /// </summary>
    /// <param name="info">the info to create</param>
    /// <returns></returns>
    public Unit InstantiateUnit(UnitInfo info) {
        Unit unit = (Instantiate(unitPrefab) as GameObject).GetComponent<Unit>();
        unit.Initialize(info);
        return unit;
    }

    public void SwitchTurn(Unit.OwnerEnum owner) {
        foreach (var unit in boardUnit) {
            if (unit == null) {
                continue;
            }
            if (unit.Owner == owner) {
                unit.CardActive = true;
            }
            else {
                unit.CardActive = false;
            }
        }
    }
    #endregion

    #region Get & Set Functions
    /// <summary>
    /// Get unit type of specific position
    /// </summary>
    /// <param name="position">the position to query</param>
    /// <returns></returns>
    public Unit.TypeEnum GetUnitType(Position position)
    {
        if (boardUnit[position.R, position.C] == null)
        {
            if (boardState[position.R, position.C] == GridState.Bread)
                return Unit.TypeEnum.Bread;
            return Unit.TypeEnum.Void;
        }

        return boardUnit[position.R, position.C].Type;
    }
    /// <summary>
    /// Get unit owner of specific position
    /// </summary>
    /// <param name="position">the position to query</param>
    /// <returns></returns>
    public Unit.OwnerEnum GetUnitOwner(Position position)
    {
        if (boardUnit[position.R, position.C] == null)
            return Unit.OwnerEnum.None;

        return boardUnit[position.R, position.C].Owner;
    }
    /// <summary>
    /// Get the unit of specific position
    /// </summary>
    /// <param name="position">the position to query</param>
    /// <returns></returns>
    public Unit GetUnit(Position position, bool includeBread = true)
    {
        if (position.R < 0)
            return null;
        else if (boardUnit[position.R, position.C] != null)
            return boardUnit[position.R, position.C];
        else if (includeBread)
            return boardBread[position.R, position.C];
        else
            return null;
    }
    /// <summary>
    /// Get the unit infoof specific position
    /// </summary>
    /// <param name="position">the position to query</param>
    /// <returns></returns>
    public UnitInfo GetUnitInfo(Position position)
    {
        Unit unit = GetUnit(position);
        if (unit != null)
            return new UnitInfo(unit.Pos, unit.Type, unit.Owner);
        else
            return new UnitInfo(position, Unit.TypeEnum.Void);
    }
    /// <summary>
    /// Get the Grid State of specific position (Base,Bread or Void)
    /// </summary>
    /// <param name="position">the position to query</param>
    /// <returns></returns>
    public GridState GetGridState(Position position)
    {
        return boardState[position.R, position.C];
    }
    /// <summary>
    /// Generate the summary info of board
    /// </summary>
    public SlotInfo[,] GenerateBoardSummary() {
        if (boardUnit == null) {
            return null;
        }

        var boardSummary = new SlotInfo[BoardInfo.Row, BoardInfo.Col];
        for (int i = 0; i < BoardInfo.Row; i++) {
            for (int j = 0; j < BoardInfo.Col; j++) {
                if (boardUnit[i, j] == null) {
                    boardSummary[i, j] = new SlotInfo() {
                        type = Unit.TypeEnum.Void,
                        owner = Unit.OwnerEnum.None,
                        hasBread = boardBread[i, j] == null ? false : boardBread[i, j].Type == Unit.TypeEnum.Bread
                    };
                }
                else {
                    boardSummary[i, j] = new SlotInfo() {
                        type = boardUnit[i, j].Type,
                        owner = boardUnit[i, j].Owner,
                        hasBread = boardBread[i, j] == null ? false : boardBread[i, j].Type == Unit.TypeEnum.Bread
                    };
                }
            }
        }
        return boardSummary;
    }

    public static string GetCardName(Unit.TypeEnum type, Unit.OwnerEnum owner) {
        string spriteName;
        if (type == Unit.TypeEnum.Bread)
            spriteName = "bread";
        else if (type == Unit.TypeEnum.Void)
            spriteName = "void";
        else if (type == Unit.TypeEnum.Tile)
            spriteName = "tile";
        else
            spriteName = type.ToString().ToLower() + ((int)owner).ToString();
        return spriteName;
    }
    #endregion

    #region Unity Callback Function
    protected override void Awake() {
        base.Awake();
	}
	
	void Start () {
	}
	
	void Update () {
    }
    #endregion
}
