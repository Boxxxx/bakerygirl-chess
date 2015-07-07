using UnityEngine;
using System;

namespace BakeryGirl.Chess {
    /// <summary>
    /// A action in an ai phase, must include move action, and also can have a alternative buy action
    /// can do or undo, to recover the last game descriptor
    /// </summary>
    public class PlayerAction
    {
        public enum Type { Move, Buy, Move_And_Buy, Complete };

        public Type type = Type.Complete;
        public BuyAction buy;
        public MoveAction move;

        private Unit.OwnerEnum restore_Turn;

        public PlayerAction()
        {
            type = Type.Complete;
        }
        public PlayerAction(MoveAction move)
        {
            type = Type.Move;
            this.move = move;
        }
        public PlayerAction(BuyAction buy)
        {
            type = Type.Buy;
            this.buy = buy;
        }
        public PlayerAction(MoveAction move, BuyAction buy)
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

    public abstract class PlayerAgent : MonoBehaviour {
        public enum StateEnum { Idle, Thinking, Complete };
        public abstract StateEnum State { get; }

        public string waitingText = "思考中";

        public abstract void Initialize();
        // Launch think logic
        public abstract void Think(Board board, Action<PlayerAction[], float> complete = null);
        // Switch turn, return true if it's agent's turn
        public abstract bool SwitchTurn(Unit.OwnerEnum nowTurn);
        // Get next action, it will pull the action from action list
        public abstract PlayerAction NextAction();
        // Get cost time in seconds
        public abstract float GetCostTime();
    }
}