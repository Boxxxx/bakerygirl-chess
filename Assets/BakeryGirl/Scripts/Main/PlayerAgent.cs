using UnityEngine;
using System;
using System.Collections.Generic;

namespace BakeryGirl.Chess {
    /// <summary>
    /// A action in an ai phase, must include move action, and also can have a alternative buy action
    /// can do or undo, to recover the last game descriptor
    /// </summary>
    public class PlayerAction : IPhotonNetworkMsg
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

        public static PlayerAction CreateMove(Position src, Position tar)
        {
            return new PlayerAction(new MoveAction(src, tar));
        }
        public static PlayerAction CreateBuy(Unit.TypeEnum type)
        {
            return new PlayerAction(new BuyAction(type));
        }
        public static PlayerAction CreateComplete()
        {
            return new PlayerAction();
        }

        public Dictionary<string, object> ToMsg()
        {
            var dict = new Dictionary<string, object>();
            dict["type"] = (int)type;
            if (type == Type.Move || type == Type.Move_And_Buy)
            {
                dict["move_src_c"] = move.src.C;
                dict["move_src_r"] = move.src.R;
                dict["move_dst_c"] = move.tar.C;
                dict["move_dst_r"] = move.tar.R;
            }
            if (type == Type.Buy || type == Type.Move_And_Buy)
            {
                dict["buy_type"] = (int)buy.type;
                dict["buy_status"] = (int)buy.status;
            }
            return dict;
        }

        public object ParseFromMsg(Dictionary<string, object> args)
        {
            type = Type.Move + Convert.ToInt32(args["type"]);
            move = null;
            buy = null;
            if (type == Type.Move || type == Type.Move_And_Buy)
            {
                move = new MoveAction(
                    new Position(Convert.ToInt32(args["move_src_r"]), Convert.ToInt32(args["move_src_c"])),
                    new Position(Convert.ToInt32(args["move_dst_r"]), Convert.ToInt32(args["move_dst_c"])));
            }
            if (type == Type.Buy || type == Type.Move_And_Buy)
            {
                buy = new BuyAction()
                {
                    type = Unit.TypeEnum.Bread + Convert.ToInt32(args["buy_type"]),
                    status = BuyAction.Status.Before_Move + Convert.ToInt32(args["buy_status"])
                };
            }
            return this;
        }
    }

    /// <summary>
    /// The Agent of player, you should implement these methods:
    ///     1. Initialize
    ///     2. Think
    ///     3. SwitchTurn
    /// </summary>
    public abstract class PlayerAgent : MonoBehaviour {
        public enum StateEnum { Idle, Thinking, Complete };
        public StateEnum State { get { return _state; } }
        public abstract Unit.OwnerEnum MyTurn { get; }

        public string waitingText = "思考中";

        private StateEnum _state = StateEnum.Idle;
        private List<PlayerAction> _actions = new List<PlayerAction>();
        private float _costTime = 0;
        private Action<PlayerAction[], float> _complete;

        public virtual void Initialize() {
            _state = StateEnum.Idle;
        }
        // Launch think logic
        public virtual void Think(Board board, Action<PlayerAction[], float> complete = null) {
            _state = StateEnum.Thinking;
            _costTime = Time.realtimeSinceStartup;
            _complete = complete;
        }
        // Switch turn, return true if it's agent's turn
        public abstract bool SwitchTurn(Unit.OwnerEnum nowTurn, bool initial);
        // Get next action, it will pull the action from action list
        public PlayerAction NextAction() {
            if (_actions.Count == 0)
                return new PlayerAction();
            PlayerAction action = _actions[0];
            _actions.RemoveAt(0);
            if (_actions.Count == 0)
                _state = StateEnum.Idle;

            return action;
        }
        // On action received
        public virtual void OnMove(bool isAgent, PlayerAction[] actions) {
            Debug.Log((isAgent ? "It is agent turn" : "It is player turn") + " and the action num is " + actions.Length);
        }
        // Get cost time in seconds
        public float GetCostTime()
        {
            return _costTime;
        }

        protected void TurnOver(ICollection<PlayerAction> actions) {
            _actions = new List<PlayerAction>(actions);
            _state = StateEnum.Complete;
            _costTime = Time.realtimeSinceStartup - _costTime;

            if (_complete != null) {
                _complete(_actions.ToArray(), _costTime);
            }
        }
    }
}