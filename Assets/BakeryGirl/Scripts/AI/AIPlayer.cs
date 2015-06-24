using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

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
/// A action in an ai phase, must include move action, and also can have a alternative buy action
/// can do or undo, to recover the last game descriptor
/// </summary>
public class AI_Action
{
    public enum Type {Move, Buy, Move_And_Buy, Complete};

    public Type type = Type.Complete;
    public BuyAction buy;
    public MoveAction move;

    private Unit.OwnerEnum restore_Turn;

    public AI_Action()
    {
        type = Type.Complete;
    }
    public AI_Action(MoveAction move)
    {
        type = Type.Move;
        this.move = move;
    }
    public AI_Action(BuyAction buy)
    {
        type = Type.Buy;
        this.buy = buy;
    }
    public AI_Action(MoveAction move, BuyAction buy)
    {
        type = Type.Move_And_Buy;
        this.move = move;
        this.buy = buy;
    }
    public void Do(GameDescriptor descriptor)
    {
        restore_Turn = descriptor.Turn;

        if (buy != null && buy.status == BuyAction.Status.Before_Move)
            buy.Do(descriptor);
        move.Do(descriptor);
        if (buy != null && buy.status == BuyAction.Status.After_Move)
            buy.Do(descriptor);
    }
    public void UnDo(GameDescriptor descriptor)
    {
        descriptor.Turn = restore_Turn;

        // should undo in the reversed order
        if (buy != null && buy.status == BuyAction.Status.After_Move)
            buy.UnDo(descriptor);
        move.UnDo(descriptor);
        if (buy != null && buy.status == BuyAction.Status.Before_Move)
            buy.UnDo(descriptor);
    }
}

/// <summary>
/// The abstract logic interface of an ai player
/// </summary>
public abstract class AIPlayer : MonoBehaviour
{
    #region Enums
    public enum StateEnum { Idle, Thinking, Complete };
    public Unit.OwnerEnum myTurn = Unit.OwnerEnum.White;
    #endregion

    #region Variables
    protected GameDescriptor descriptor;
    protected AI_Action action;
    protected int nodeCount;
    private StateEnum state = StateEnum.Idle;
    private float costTime;
    private List<AI_Action> actions = new List<AI_Action>();
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
    public AI_Action ActionResult
    {
        get { return action; }
    }
    public Unit.OwnerEnum MyTurn { get { return myTurn; } }
    public StateEnum State { get { return state; } }
    public float CostTime
    {
        get { return costTime*1000; }
    }
    public int Node
    {
        get { return nodeCount; }
    }
    #endregion

    #region Public Interface
    public void Think(Board board)
    {
        state = StateEnum.Thinking;

        costTime = Time.realtimeSinceStartup;

        descriptor = new GameDescriptor(board, myTurn);
        action = new AI_Action();
        nodeCount = 0;

        aiTask = new Thread(DoCalculate);
        aiTask.Start();
        //DoCalculate();
    }
    public AI_Action GetAcition()
    {
        if (actions.Count == 0)
            return new AI_Action();
        AI_Action action = actions[0];
        actions.RemoveAt(0);
        if (actions.Count == 0)
            state = StateEnum.Idle;

        return action;
    }
    public virtual void Initialize()
    {
        if (aiTask != null)
            aiTask.Interrupt();
        state = StateEnum.Idle;
    }
    #endregion

    #region Private or Protected Functions
    private void UnpackAction()
    {
        if (action.buy != null && action.buy.status == BuyAction.Status.Before_Move)
            actions.Add(new AI_Action(action.buy));
        actions.Add(new AI_Action(action.move));
        if (action.buy != null && action.buy.status == BuyAction.Status.After_Move)
            actions.Add(new AI_Action(action.buy));
    }
    protected abstract void DoCalculate();
    protected void Add(AI_Action action)
    {
        actions.Add(action);
    }
    #endregion

    #region Unity Callback Functions
    void Update()
    {
        if (state == StateEnum.Thinking && !aiTask.IsAlive)
        {
            state = StateEnum.Complete;

            actions.Clear();
            UnpackAction();

            costTime = Time.realtimeSinceStartup - costTime;
        }
    }
    #endregion
}
