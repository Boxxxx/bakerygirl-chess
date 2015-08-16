using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using BakeryGirl.Chess;
using System;

/// <summary>
/// Implement move action, do and redo, but do not ensure safety
/// </summary>
public class MoveAction
{
    public Position src;
    public Position tar;

    private bool restore_hasMove;
    private int restore_Resource;
    private int restore_Rest;
    private UnitInfo restore_srcInfo;
    private UnitInfo restore_tarInfo;

    public MoveAction(Position src, Position tar)
    {
        this.src = src;
        this.tar = tar;
    }
    public void Do(GameDescriptor descriptor)
    {
        restore_Rest = descriptor.RestResource;
        restore_Resource = descriptor.GetPlayerInfo(Unit.TypeEnum.Bread, descriptor.Turn);
        restore_hasMove = descriptor.HasMove;
        restore_srcInfo = descriptor.GetInfo(src).Clone() as UnitInfo;
        restore_tarInfo = descriptor.GetInfo(tar).Clone() as UnitInfo;

        descriptor.Move(src, tar);
    }
    public void UnDo(GameDescriptor descriptor)
    {
        if(descriptor.GetType(src) != Unit.TypeEnum.Bread)
            descriptor.Pick(src);
        if (descriptor.GetType(tar) != Unit.TypeEnum.Bread)
            descriptor.Pick(tar);

        descriptor.Put(src, restore_srcInfo);
        descriptor.Put(tar, restore_tarInfo);

        descriptor.HasMove = restore_hasMove;
        descriptor.SetPlayerInfo(Unit.TypeEnum.Bread, descriptor.Turn, restore_Resource);
        descriptor.RestResource = restore_Rest;
    }
};

/// <summary>
/// Implement buy action, but do not ensure safety
/// </summary>
public class BuyAction
{
    public enum Status { Before_Move, After_Move, None};
    public Unit.TypeEnum type = Unit.TypeEnum.Void;
    public Status status = Status.None;
   
    // restore for undo
    private int restore_Resource;
    private bool restore_hasBuy;

    public BuyAction(Unit.TypeEnum type = Unit.TypeEnum.Void)
    {
        this.type = type;
    }
    public void Do(GameDescriptor descriptor)
    {
        restore_hasBuy = descriptor.HasBuy;
        restore_Resource = descriptor.GetPlayerInfo(Unit.TypeEnum.Bread, descriptor.Turn);

        descriptor.Buy(type, descriptor.Turn);
    }
    public void UnDo(GameDescriptor descriptor)
    {
        descriptor.SetPlayerInfo(Unit.TypeEnum.Bread, descriptor.Turn, restore_Resource);
        if(type != Unit.TypeEnum.Void)
            descriptor.Pick(BoardInfo.Base[(int)descriptor.Turn]);

        descriptor.HasBuy = restore_hasBuy;
    }
};

/// <summary>
/// The abstract logic interface of an ai player
/// </summary>
public abstract class AIPlayer : PlayerAgent
{
    #region Enums
    public Unit.OwnerEnum myTurn = Unit.OwnerEnum.White;
    #endregion

    #region Variables
    protected GameDescriptor descriptor;
    protected PlayerAction action;
    protected int nodeCount;
    private List<PlayerAction> actions = new List<PlayerAction>();
    private Thread aiTask;
    #endregion

    #region Properties
    public MoveAction MoveResult
    {
        get { return action.move; }
    }
    public BuyAction BuyResult
    {
        get { return action.buy; }
    }
    public PlayerAction ActionResult
    {
        get { return action; }
    }
    public override Unit.OwnerEnum PlayerTurn { get { return Unit.Opposite(myTurn); } }
    public int Node
    {
        get { return nodeCount; }
    }
    #endregion

    #region Public Interface
    public override void Think(Board board, Action<PlayerAction[], float> complete = null)
    {
        base.Think(board, complete);

        descriptor = new GameDescriptor(board, myTurn);
        action = new PlayerAction();
        nodeCount = 0;

        aiTask = new Thread(DoCalculate);
        aiTask.Start();
        //DoCalculate();
    }
    public override void Initialize()
    {
        base.Initialize();
        if (aiTask != null)
            aiTask.Interrupt();
    }
    public override bool SwitchTurn(Unit.OwnerEnum nowTurn, bool initial)
    {
        return myTurn == nowTurn;
    }
    #endregion

    #region Private or Protected Functions
    private List<PlayerAction> UnpackAction()
    {
        var actions = new List<PlayerAction>();
        if (action.buy != null && action.buy.status == BuyAction.Status.Before_Move)
            actions.Add(new PlayerAction(action.buy));
        actions.Add(new PlayerAction(action.move));
        if (action.buy != null && action.buy.status == BuyAction.Status.After_Move)
            actions.Add(new PlayerAction(action.buy));
        return actions;
    }
    protected abstract void DoCalculate();
    #endregion

    #region Unity Callback Functions
    void Awake() {
        if (FindObjectOfType<PlayerAgent>() != null) {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (State == StateEnum.Thinking && !aiTask.IsAlive)
        {
            TurnOver(UnpackAction());
        }
    }
    #endregion
}
