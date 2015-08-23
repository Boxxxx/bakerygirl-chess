using System;
using System.Collections.Generic;
using BakeryGirl.Chess;

/// <summary>
/// A simulator of game play, can do any movement in raw data
/// </summary>
public class GameDescriptor : ICloneable
{
    #region GameData
    int restResource;
    int[][] playerInfo;
    UnitInfo[,] units;
    Board.GridState[,] grids;
    Unit.OwnerEnum turn;

    bool hasBuy = false;
    bool hasMove = false;
    #endregion

    #region Properties
    public Unit.OwnerEnum Turn { get { return turn; } set { turn = value; } }
    public bool HasBuy { get { return hasBuy; } set { hasBuy = value; } }
    public bool HasMove { get { return hasMove; } set { hasMove = value; } }
    public int RestResource { get { return restResource; } set{restResource = value;}}
    private GameDescriptor(Unit.OwnerEnum turn) { 
        this.turn = turn;
        units = new UnitInfo[BoardInfo.Row, BoardInfo.Col];
        grids = new Board.GridState[BoardInfo.Row, BoardInfo.Col];
        playerInfo = new int[2][] { new int[(int)(Unit.TypeEnum.Void)], new int[(int)(Unit.TypeEnum.Void)] };
    }
    public GameDescriptor(Board board, Unit.OwnerEnum turn)
    {
        units = new UnitInfo[BoardInfo.Row, BoardInfo.Col];
        grids = new Board.GridState[BoardInfo.Row, BoardInfo.Col];
        playerInfo = new int[2][] { new int[(int)(Unit.TypeEnum.Void)], new int[(int)(Unit.TypeEnum.Void)]};

        for (int i = 0; i < BoardInfo.Row; i++)
            for (int j = 0; j < BoardInfo.Col; j++)
            {
                units[i, j] = board.GetUnitInfo(new Position(i, j));
                if (units[i, j].type == Unit.TypeEnum.Bread)
                    units[i, j].type = Unit.TypeEnum.Void;
                grids[i, j] = board.GetGridState(new Position(i, j));
            }
        
        for(Unit.TypeEnum type = Unit.TypeEnum.Bread; type < Unit.TypeEnum.Void; type++) {
           playerInfo[(int)Unit.OwnerEnum.Black][(int)type] = board.GetPlayerInfo(type,Unit.OwnerEnum.Black);
           playerInfo[(int)Unit.OwnerEnum.White][(int)type] = board.GetPlayerInfo(type,Unit.OwnerEnum.White);
        }

        restResource = board.RestBread;

        hasMove = false;
        hasBuy = false;
        this.turn = turn;
    }
    #endregion

    #region Game Operations
    public bool NewTurn()
    {
        if (!hasMove)
            return false;
        hasMove = false;
        hasBuy = false;
        turn = Unit.Opposite(turn);
        return true;
    }

    public void DoAction(PlayerAction action)
    {
        action.Do(this);
        NewTurn();
    }
    public bool CanMove(Position src, Position tar)
    {
        if (GetOwner(src) == GetOwner(tar))
            return false;

        return !hasMove && Ruler.CheckConflict(GetType(src), GetType(tar)) != Ruler.ConflictResult.Des_Win;
    }
    public bool Move(Position src, Position tar)
    {
        if (GetOwner(src) == GetOwner(tar))
            return false;

        Ruler.ConflictResult result = Ruler.CheckConflict(GetType(src), GetType(tar));
        switch (result)
        {
            case Ruler.ConflictResult.Boom:
                Pick(src);
                Pick(tar);
                break;
            case Ruler.ConflictResult.Src_Win:
                Pick(tar);
                Put(tar, Pick(src));
                break;
            case Ruler.ConflictResult.Des_Win:
                return false;
            case Ruler.ConflictResult.Eat_Bread:
                Pick(tar);
                UnitInfo info = Put(tar, Pick(src));
                playerInfo[(int)info.owner][(int)Unit.TypeEnum.Bread] += StorageInfo.ResourcePerBread;
                break;
            case Ruler.ConflictResult.Nothing:
                Put(tar, Pick(src));
                break;
        }
        hasMove = true;
        return true;
    }
    public bool CanBuy(Unit.TypeEnum type, Unit.OwnerEnum owner)
    {
        if (hasBuy)
            return false;
        if (!Unit.IsSoldier(type))
            return false;
        if (playerInfo[(int)owner][(int)Unit.TypeEnum.Bread] < StorageInfo.CardCost[UIStorage.TypeToIndex(type)])
            return false;
        if (GetInfo(BoardInfo.Base[(int)owner]).owner != Unit.OwnerEnum.None)
            return false;
        if (type == Unit.TypeEnum.Bomb && GameInfo.Instance.board.GetPlayerInfo(Unit.TypeEnum.Bomb, turn) >= 2)
            return false;
        if (type == Unit.TypeEnum.Boss && GetPlayerInfo(Unit.TypeEnum.Boss, turn) != 0)
            return false;
        if (GetPlayerTotalCount(turn) - GetPlayerInfo(Unit.TypeEnum.Bread, turn) >= 5)
            return false;

        return true;
    }
    public bool Buy(Unit.TypeEnum type, Unit.OwnerEnum owner)
    {
        if (!CanBuy(type, owner))
            return false;

        Put(BoardInfo.Base[(int)owner], new UnitInfo(type, owner));
        playerInfo[(int)owner][(int)Unit.TypeEnum.Bread] -= StorageInfo.CardCost[UIStorage.TypeToIndex(type)];
        hasBuy = true;

        return true;
    }

    public Unit.TypeEnum GetType(Position pos)
    {
        if (units[pos.R, pos.C].type == Unit.TypeEnum.Void && grids[pos.R, pos.C] == Board.GridState.Bread)
            return Unit.TypeEnum.Bread;
        return units[pos.R, pos.C].type;
    }
    public UnitInfo GetInfo(Position pos)
    {
        if (units[pos.R, pos.C].type == Unit.TypeEnum.Void && grids[pos.R, pos.C] == Board.GridState.Bread)
            return new UnitInfo(pos, Unit.TypeEnum.Bread);
        return units[pos.R, pos.C].Clone() as UnitInfo;
    }
    public Unit.OwnerEnum GetOwner(Position pos)
    {
        return GetInfo(pos).owner;
    }

    public int GetPlayerInfo(Unit.TypeEnum type, Unit.OwnerEnum owner)
    {
        return playerInfo[(int)owner][(int)type];
    }
    public void SetPlayerInfo(Unit.TypeEnum type, Unit.OwnerEnum owner, int value)
    {
        playerInfo[(int)owner][(int)type] = value;
    }
    public int GetPlayerTotalCount(Unit.OwnerEnum owner)
    {
        int sum = 0;
        for (Unit.TypeEnum type = Unit.TypeEnum.Bread; type < Unit.TypeEnum.Void; type++)
            sum += playerInfo[(int)owner][(int)type];
        return sum;
    }

    public UnitInfo Pick(Position pos)
    {
        UnitInfo info = GetInfo(pos);
        if (GetType(pos) == Unit.TypeEnum.Bread)
        {
            grids[pos.R, pos.C] = Board.GridState.Void;
            restResource--;
        }
        else
        {
            if (info.owner != Unit.OwnerEnum.None)
                playerInfo[(int)info.owner][(int)info.type]--;
            units[pos.R, pos.C].type = Unit.TypeEnum.Void;
            units[pos.R, pos.C].owner = Unit.OwnerEnum.None;
        }

        return info;
    }
    public UnitInfo Put(Position pos, UnitInfo unit)
    {
        if (unit.type == Unit.TypeEnum.Bread)
        {
            grids[unit.pos.R, unit.pos.C] = Board.GridState.Bread;
            restResource++;
        }
        else
        {
            if (unit.owner != Unit.OwnerEnum.None)
                playerInfo[(int)unit.owner][(int)unit.type]++;
            units[pos.R, pos.C] = new UnitInfo(pos, unit.type, unit.owner);
        }
        return GetInfo(pos);
    }
    #endregion

    #region Query Legal Actions for next step
    public List<BuyAction> QueryBuyActions()
    {
        List<BuyAction> actionList = new List<BuyAction>();
        for (Unit.TypeEnum type = Unit.TypeEnum.Bread + 1; type < Unit.TypeEnum.Void; type++)
            if (CanBuy(type, turn))
                actionList.Add(new BuyAction(type));
        return actionList;
    }
    public List<MoveAction> QueryMoveActions()
    {
        List<MoveAction> actionList = new List<MoveAction>();
        foreach (UnitInfo info in units)
            if (info.owner == turn)
            {
                Position src = info.pos;
                foreach (Position delta in Controller.MoveOffsetList)
                {
                    Position tar = src + delta;
                    if (tar.IsValid && CanMove(src, tar))
                        actionList.Add(new MoveAction(src, tar));
                }
            }
        return actionList;
    }
 
    public List<PlayerAction> QueryAllActions_Slow()
    {
        List<PlayerAction> actions = new List<PlayerAction>();
        GameDescriptor stage1, stage2;
        List<BuyAction> buyList1 = QueryBuyActions(), buyList2;
        List<MoveAction> moveList;
        buyList1.Add(new BuyAction());

        foreach (BuyAction buy1 in buyList1)
        {
            stage1 = Clone() as GameDescriptor;
            if (buy1.type != Unit.TypeEnum.Void)
                buy1.Do(stage1);
            moveList = stage1.QueryMoveActions();
            foreach (MoveAction move in moveList)
            {
                stage2 = stage1.Clone() as GameDescriptor;
                move.Do(stage2);
                if (buy1.type == Unit.TypeEnum.Void)
                {
                    buyList2 = stage2.QueryBuyActions(); 
                    foreach (BuyAction buy2 in buyList2)
                    {
                        if (buy2.type != Unit.TypeEnum.Void)
                        {
                            buy2.status = BuyAction.Status.After_Move;
                            actions.Add(new PlayerAction(move, buy2));
                        }
                    }
                    actions.Add(new PlayerAction(move));
                }
                else
                {
                    buy1.status = BuyAction.Status.Before_Move;
                    actions.Add(new PlayerAction(move, buy1));
                }
            }
        }
        
        return actions;
    }
    public List<PlayerAction> QueryAllActions()
    {
        List<PlayerAction> actions = new List<PlayerAction>();

        List<BuyAction> buyList1 = QueryBuyActions(), buyList2;
        List<MoveAction> moveList;
        buyList1.Add(new BuyAction());

        foreach (BuyAction buy1 in buyList1)
        {
            //stage1 = Clone() as GameDescriptor;
            if (buy1.type != Unit.TypeEnum.Void)
                buy1.Do(this);
            moveList = this.QueryMoveActions();
            foreach (MoveAction move in moveList)
            {
                move.Do(this);
                if (buy1.type == Unit.TypeEnum.Void)
                {
                    buyList2 = this.QueryBuyActions();
                    foreach (BuyAction buy2 in buyList2)
                    {
                        if (buy2.type != Unit.TypeEnum.Void)
                        {
                            buy2.status = BuyAction.Status.After_Move;
                            actions.Add(new PlayerAction(move, buy2));
                        }
                    }
                    actions.Add(new PlayerAction(move));
                }
                else
                {
                    buy1.status = BuyAction.Status.Before_Move;
                    actions.Add(new PlayerAction(move, buy1));
                }
                move.UnDo(this);
            }
            if (buy1.type != Unit.TypeEnum.Void)
                buy1.UnDo(this);
        }

        return actions;
    }
    #endregion

    #region Misc
    public object Clone()
    {
        GameDescriptor clone = new GameDescriptor(turn);
        clone.restResource = restResource;
        for (int i = 0; i < playerInfo[0].Length; i++)
            clone.playerInfo[0][i] = playerInfo[0][i];
        for (int i = 0; i < playerInfo[1].Length; i++)
            clone.playerInfo[1][i] = playerInfo[1][i];
        for (int i = 0; i < BoardInfo.Row; i++)
            for (int j = 0; j < BoardInfo.Col; j++)
            {
                clone.units[i, j] = units[i, j].Clone() as UnitInfo;
                clone.grids[i, j] = grids[i, j];
            }
        clone.turn = turn;
        clone.hasBuy = hasBuy;
        clone.hasMove = hasMove;

        return clone;
    }
    public bool Compare(GameDescriptor rhs)
    {
        if (rhs.restResource != restResource)
            return false;
        for (int i = 0; i < playerInfo[0].Length; i++)
            if (rhs.playerInfo[0][i] != playerInfo[0][i])
                return false;
        for (int i = 0; i < playerInfo[1].Length; i++)
            if (rhs.playerInfo[1][i] != playerInfo[1][i])
                return false;
        for (int i = 0; i < BoardInfo.Row; i++)
            for (int j = 0; j < BoardInfo.Col; j++)
            {
                if (!rhs.units[i, j].Compare(units[i, j]))
                    return false;
                if (rhs.grids[i, j] != grids[i, j])
                    return false;
            }
        if (rhs.turn != turn)
            return false;
        if (rhs.hasBuy != hasBuy)
            return false;
        if (rhs.hasMove != hasMove)
            return false;
        return true;
    }
    #endregion

    #region Gets
    public UnitInfo GetUnitInfo(int r, int c) {
        return units[r, c];
    }
    public Board.GridState GetGridState(int r, int c) {
        return grids[r, c];
    }
    #endregion
}
