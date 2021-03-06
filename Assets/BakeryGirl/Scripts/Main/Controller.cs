﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using BakeryGirl.Chess;

/// <summary>
/// Drag Event to maintain drag action in controller
/// </summary>
public class DragEvent
{
    public enum StateEnum { IDLE, HOLD };
    private Unit unit;
    private Unit source;
    private StateEnum state = StateEnum.IDLE;

    public StateEnum State { get { return state; } }
    public Unit Source { get { return source; } }

    public void Set(Unit unit)
    {
        this.unit = unit;
        this.unit.gameObject.SetActive(false);
    }

    public void Start(Unit source, Vector3 point)
    {
        state = StateEnum.HOLD;

        this.source = source;
        source.Sprite.color = new Color(source.Sprite.color.r, source.Sprite.color.g, source.Sprite.color.b, 0.1f);

        unit.setSprite(ArtManager.Instance.GetBattleCardGraphics(source.Type, source.Owner));
        unit.transform.position = point;
        this.unit.gameObject.SetActive(true);
    }

    public void Update(Vector3 point)
    {
        unit.transform.position = point;
    }

    public void Stop()
    {
        state = StateEnum.IDLE;

        source.Sprite.color = new Color(source.Sprite.color.r, source.Sprite.color.g, source.Sprite.color.b, 1f);
        this.unit.gameObject.SetActive(false);
    }
};

/// <summary>
/// To show Hint grid in controller's move action
/// </summary>
public class Hint
{
    public static readonly float[] MoveHintRotationList = { 180, -90, 90, 0 };
    public enum HintType { Move, None };

    private bool isShow;
    private HintType type;
    private List<Unit> units = new List<Unit>();
    private Unit.OwnerEnum owner;
    private Unit source;

    public bool IsShow { get { return isShow; } }
    public HintType Type { get { return type; } }
    public Unit Source { get { return source; } }

    public void SetMoveHint(Unit source)
    {
        ClearHints();
        this.owner = source.Owner;
        this.source = source;
        source.Focus = true;

        for (int i = 0; i < 4; i++)
        {
            var offset = Controller.MoveOffsetList[i];
            Position newPos = source.Pos+offset;
            if (newPos.IsValid && GameInfo.Instance.board.GetUnitOwner(newPos) != owner)
            {
                Unit unit = GameInfo.Instance.board.InstantiateUnit(new UnitInfo(source.Pos + offset, Unit.TypeEnum.Tile));
                SetHintStyle(unit, MoveHintRotationList[i]);
                units.Add(unit);
            }
        }
        type = HintType.Move;

        isShow = true;
    }

    public void ClearHints()
    {
        if (source != null) {
            source.Focus = false;
        }

        source = null;
        isShow = false;
        foreach (Unit unit in units)
            GameObject.Destroy(unit.gameObject);
        units.Clear();

        type = HintType.None;
    }

    private bool SetHintStyle(Unit tile, float rotation)
    {
        Board board = GameInfo.Instance.board;
        if (GameInfo.Instance.ShouldUpsidedown) {
            rotation += 180.0f;
        }
        if (board.GetUnitOwner(tile.Pos) == owner) {
            return false;
        }
        else if (board.GetUnitOwner(tile.Pos) == Unit.Opposite(owner)) {
            tile.SetColor(1, 0, 0);
            tile.setSprite("attack_hint");
            tile.CardActive = true;
        }
        else {
            if (board.GetUnitType(tile.Pos) == Unit.TypeEnum.Bread) {
                tile.SetColor(0, 0, 1);
                
            }
            else if (board.GetGridState(tile.Pos) == Board.GridState.Base0 || board.GetGridState(tile.Pos) == Board.GridState.Base1) {
                tile.SetColor(1, 0.785f, 0);
            }
            else {
                tile.SetColor(0, 1, 0);
            }
            tile.setSprite("move_hint");
            tile.CardActive = true;
            tile.transform.Rotate(new Vector3(0, 0, rotation));
        }
        return true;
    }
}

public class GameCache {
    public GameDescriptor descriptor;
    public LastMoveInfo lastMove;
    public int turnNum;

    public GameCache(Board board, Unit.OwnerEnum turn, int turnNum, LastMoveInfo lastMove) {
        descriptor = new GameDescriptor(board, turn);
        this.turnNum = turnNum;
        this.lastMove = lastMove;
    }
}

public class LastMoveInfo {
    public Position from;
    public Position to;
    public LastMoveInfo(Position from, Position to) {
        this.from = from;
        this.to = to;
    }
    public Vector2 GetOffset() {
        var offset = to - from;
        return new Vector2(offset.C, offset.R);
    }
}

/// <summary>
/// Controller
/// To maintain the state of game & do all the action to controll game state
/// </summary>
public class Controller : MonoBehaviour
{
    #region Enums
    public enum MoveState { Idle, Pick, Occupy };
    public enum MainState { Uninitialized, Ready, Move, Wait, Over, AgentThinking, AgentRunning, AgentEffectWaiting };
    public enum PhaseState { Player, Agent, Other};
    public enum GameMode { Normal, Agent, Stay};
    public enum EffectType { Unknown, Move, CollectBread, Killout, MoveIn};
    #endregion

    #region Static or Constant Variables
    public static readonly Position[] MoveOffsetList = { new Position(-1, 0), new Position(0, 1), new Position(0, -1), new Position(1, 0) };
    #endregion

    #region Variables
    public GameMode initGameMode = GameMode.Normal;
    public UIGameResult resultUI;
    public UIStatus statusUI;
    public UIBattleStart battleStartUI;
    public UISwitchMode switchModeUI;
    public LastMoveHint lastMoveHint;

    private GameMode gameMode = GameMode.Normal;
    private MoveState moveState;
    private Unit.OwnerEnum turn;
    private int turnNum;
    private MainState state = MainState.Uninitialized;
    private int effectNum;
    private Board board;
    private UIStorage storage;
    private Hint hint = new Hint();
    private Ruler.GameResult result;
    private LastMoveInfo lastMove;

    // Game Cache
    private GameCache cache;

    private List<PlayerAction[]> _actionLogs = new List<PlayerAction[]>();
    private List<PlayerAction> _actionsCurrentTurn = new List<PlayerAction>();

    public GameMode Mode { get { return gameMode; } }
    public Unit.OwnerEnum Turn {
        get {
            return turn;
        }
    }
    public MainState State {
        get { return state; }
    }
    public bool IsEffecting
    {
        get { return effectNum > 0; }
    }
    public PhaseState Phase
    {
        get {
            if (state == MainState.Move || state == MainState.Wait)
                return PhaseState.Player;
            else if (state == MainState.AgentThinking || state == MainState.AgentRunning)
                return PhaseState.Agent;
            else
                return PhaseState.Other;
        }
    }
    public bool IsStart {
        get {
            return state != MainState.Uninitialized;
        }
    }
    public bool IsMoving {
        get {
            return moveState == MoveState.Pick;
        }
    }
    public PlayerAgent Agent {
        get; private set;
    }

    public GameClient Client {
        get {
            return GameClientAgent.ClientInstance;
        }
    }
    #endregion

    #region Public Interface Functions
    public void RestartGame(GameMode newMode)
    {
        NewGame(newMode == GameMode.Stay ? gameMode : newMode);
        StartGame();
    }
    public void NextTurn(bool lastTurnIsMe = false)
    {
        if (lastTurnIsMe) {
            DoAction(PlayerAction.CreateComplete());
        }
        _DoTurnOver();
        if (state == MainState.Wait || state == MainState.AgentRunning || state == MainState.AgentEffectWaiting)
        {
            Ruler.GameResult result = Ruler.CheckGame(board);
            if (result == Ruler.GameResult.NotYet)
            {
                turn = Unit.Opposite(turn);
                state = MainState.Move;
                NewTurn(false);
            }
            else
            {
                state = MainState.Over;
                this.result = result;
                OnGameOver();   
            }
        }
    }
    public void RollbackGame() {
        if (effectNum != 0 && !(state == MainState.Move || state == MainState.Wait)) {
            return;
        }
        hint.ClearHints();
        board.RollbackGame(cache);
        storage.RollbackGame(cache);
        lastMove = cache.lastMove;
        lastMoveHint.Focus(lastMove);
        turn = cache.descriptor.Turn;
        turnNum = cache.turnNum;
        state = MainState.Move;
        moveState = MoveState.Idle;
        _actionsCurrentTurn.Clear();
    }
    #endregion

    #region Effect Functions
    public void StartEffect(EffectType effect = EffectType.Unknown)
    {
        effectNum++;
        storage.IsEndTurnValid = false;
        storage.IsCancelValid = false;
    }

    public void StopEffect(EffectType effect = EffectType.Unknown)
    {
        effectNum--;

        if (effect == EffectType.Move)
        {
            if (lastMove != null)
            {
                lastMoveHint.Focus(lastMove);
                //lastMove.Focus = true;
            }
        }
        if (state == MainState.Wait && effectNum == 0) {
            storage.IsEndTurnValid = true;
            storage.IsCancelValid = true;
        }
        else if (state == MainState.Move && effectNum == 0) {
            storage.IsCancelValid = true;
        }
    }

    public void ClearEffect()
    {
        effectNum = 0;
    }

    private void MoveEffect(Unit src, Position pos)
    {
        var originPos = src.Pos;
        board.Pick(src.Pos);
        src.Pos = pos;
        Vector2 targetPos = Unit.PosToScreen(pos);
        iTween.MoveTo(src.gameObject, iTween.Hash("position", new Vector3(targetPos.x, targetPos.y, 0), "time", 0.2f, "oncomplete", "OnMoveComplete", "oncompletetarget", src.gameObject));
        StartEffect(EffectType.Move);

        if (lastMove != null) {
            lastMoveHint.UnFocus();
        }
        lastMove = new LastMoveInfo(originPos, pos);
    }

    private void CollectBreadEffect(Unit bread, Unit.OwnerEnum owner)
    {
        bread.Owner = owner;
        // set 
        bread.setSprite(ArtManager.Instance.GetBattleCardGraphicsByName("bread"));
        iTween.MoveAdd(bread.gameObject, iTween.Hash("amount", new Vector3(0, BoardInfo.GridHeight, 0), "time", 0.5f, "easetype", iTween.EaseType.easeOutCubic,"oncomplete", (iTween.CallbackDelegate)(tween => {
            iTween.MoveTo(bread.gameObject, iTween.Hash("position", storage.GetCollectPoint(owner), "time", 1f, "oncomplete", "OnDisappearComplete", "oncompletetarget", bread.gameObject));
        })));
        StartEffect(EffectType.CollectBread);
    }

    private void KillEnemyEffect(Unit enemy)
    {
        enemy.CardAlive = false;
        iTween.FadeTo(enemy.gameObject, 0, 0.5f);
        iTween.MoveTo(enemy.gameObject, iTween.Hash("position", enemy.gameObject.transform.position + UnitInfo.KilledEffectOffset, "time", 1f, "oncomplete", "OnDisappearComplete", "oncompletetarget", enemy.gameObject));
        StartEffect(EffectType.Killout);
    }

    public void BuyCardEffect(Unit card, Unit.OwnerEnum owner)
    {
        Vector3 position = Unit.PosToScreen(BoardInfo.Base[(int)owner]);
        iTween.FadeFrom(card.gameObject, 0, 1.0f);
        iTween.MoveTo(card.gameObject, iTween.Hash("position", position, "time", 1.5f, "oncomplete", "OnAppearComplete", "oncompletetarget", card.gameObject));
        StartEffect(EffectType.MoveIn);
    }
    #endregion

    #region Private Functions
    private void NewGame(GameMode gameMode)
    {
        this.gameMode = gameMode;
        GameInfo.Instance.NewGame();

        lastMove = null;
        lastMoveHint.UnFocus();
        moveState = MoveState.Idle;
        ClearEffect();
        hint.ClearHints();

        if (gameMode == GameMode.Agent) {
            InitAgent();
        }

        board.NewGame();
        storage.NewGame();
        switchModeUI.NewGame();
        state = MainState.Ready;
        turn = Unit.OwnerEnum.Black;
        turnNum = 0;
        if (resultUI != null) {
            resultUI.gameObject.SetActive(false);
        }
        storage.IsEndTurnValid = false;
        storage.IsCancelValid = false;

        _actionLogs.Clear();
        _actionsCurrentTurn.Clear();

        statusUI.ShowAction();
    }
    private void StartGame()
    {
        state = MainState.Move;
        NewTurn(true);
        if (battleStartUI != null) {
            battleStartUI.Show();
        }
    }
    private void NewTurn(bool initial)
    {
        turnNum++;
        board.SwitchTurn(turn);
        storage.SwitchTurn(turn, turnNum);
        if (gameMode == GameMode.Agent) {
            AgentSwitchTurn(initial);
        }
        else {
            statusUI.ShowAction();
        }
        cache = new GameCache(board, turn, turnNum, lastMove);
    }
    private void OnGameOver()
    {
        Agent.OnGameOver(result);
        if (resultUI != null) {
            resultUI.gameObject.SetActive(true);
            resultUI.OnGameOver(storage, turnNum, result);
        }
    }
    private bool CheckMoveOffset(Position offset)
    {
        foreach (Position pos in MoveOffsetList)
            if (pos == offset)
                return true;
        return false;
    }
    private void MouseEvent(Vector3 point)
    {
        Position pos = Unit.ScreenToPos(new Vector2(point.x, point.y));

        if (state == MainState.Move) {
            if (moveState == MoveState.Idle) {
                if (pos.IsValid && Input.GetMouseButtonUp(0)) {
                    if (board.GetUnitOwner(pos) == Turn) {
                        moveState = MoveState.Pick;
                        hint.SetMoveHint(board.GetUnit(pos));

                        SoundManager.Instance.PlaySound("button_click");
                    }
                }
            }
            else if (moveState == MoveState.Pick) {
                if (Input.GetMouseButtonUp(0)) {
                    if (pos.IsValid) {
                        if (CheckMoveOffset(pos - hint.Source.Pos)) {
                            if (DoAction(PlayerAction.CreateMove(hint.Source.Pos, pos))) {
                                state = MainState.Wait;
                            }
                        }

                        hint.ClearHints();
                        moveState = MoveState.Idle;
                    }
                }
                else if (Input.GetMouseButtonUp(1)) {
                    hint.ClearHints();
                    moveState = MoveState.Idle;
                }
            }
        }

        if (!IsMoving && pos.IsValid) {
            var unit = board.GetUnit(pos, false);
            if (unit != null) {
                GameInfo.Instance.characterImage.Show(unit.GetCardName());
            }
        }
    }
    private void CheckGameOver()
    {
        Ruler.GameResult result = Ruler.CheckGame(board);
        if (result != Ruler.GameResult.NotYet)
        {
            state = MainState.Over;
            this.result = result;
        }
    }
    private void OnBGMActive(bool active)
    {
        if (active)
        {
            if (!GetComponent<AudioSource>().isPlaying)
                GetComponent<AudioSource>().Play();
        }
        else
        {
            if (GetComponent<AudioSource>().isPlaying)
                GetComponent<AudioSource>().Stop();
        }
    }
    #endregion

    #region Actions
    private bool _DoTurnOver()
    {
        var actions = _actionsCurrentTurn.ToArray();
        _actionsCurrentTurn.Clear();

        _actionLogs.Add(actions);
        if (Agent != null)
        {
            Agent.OnMove(state == MainState.AgentThinking || state == MainState.AgentRunning || state == MainState.AgentEffectWaiting, actions);
        }

        return true;
    }
    private bool _DoMove(Unit src, Unit des, Position desPos)
    {
        if (des != null && src.Owner == des.Owner)
            return false;

        Ruler.ConflictResult result = des == null?Ruler.ConflictResult.Nothing:Ruler.CheckConflict(src.Type, des.Type);
        switch (result)
        {
            case Ruler.ConflictResult.Boom:
                KillEnemyEffect(board.Pick(src.Pos));
                KillEnemyEffect(board.Pick(des.Pos));
                SoundManager.Instance.PlaySound("bomb");
                break;
            case Ruler.ConflictResult.Src_Win:
                KillEnemyEffect(board.Pick(des.Pos));
                MoveEffect(src, desPos);
                SoundManager.Instance.PlaySound("gun_fire");
                break;
            case Ruler.ConflictResult.Des_Win:
                return false;
            case Ruler.ConflictResult.Eat_Bread:
                Unit bread = board.Pick(des.Pos);
                MoveEffect(src, desPos);
                CollectBreadEffect(bread, src.Owner);
                SoundManager.Instance.PlaySound("gain_resource");
                break;
            case Ruler.ConflictResult.Nothing:
                MoveEffect(src, desPos);
                SoundManager.Instance.PlaySound("move");
                break;
        }
        return true;
    }
    private bool _DoBuy(Unit.TypeEnum type)
    {
        SoundManager.Instance.PlaySound("build");
        return GameInfo.Instance.storage.BuyCard(type, turn);
    }
    #endregion

    #region Agent Functions
    private void InitAgent()
    {
        Agent.Initialize();
    }
    private void AgentSwitchTurn(bool initial)
    {
        if(Agent.SwitchTurn(turn, initial))
        {
            state = MainState.AgentThinking;
            Agent.Think(board);
            statusUI.ShowDot(Agent.waitingText);
        }
    }
    private bool DoAgentAction()
    {
        PlayerAction action = Agent.NextAction();
        DoAction(action);
        return action.type == PlayerAction.Type.Complete;
    }
    public bool DoAction(PlayerAction action) {
        bool flag = false;
        if (action.type == PlayerAction.Type.Complete)
            flag = true;
        else if (action.type == PlayerAction.Type.Move)
            flag = _DoMove(board.GetUnit(action.move.src), board.GetUnit(action.move.tar), action.move.tar);
        else
            flag = _DoBuy(action.buy.type);
        if (flag) {
            _actionsCurrentTurn.Add(action);
        }
        return flag;
    }

    public void DestroyAgent() {
        if (Agent != null) {
            Agent.OnAgentDestroy();
        }
    }
    #endregion

    #region Unity Callback Functions
    void Awake()
    {
        // If someone set nextMode to valid term, then use it, and after that we set nextMode to stay.
        gameMode = GameInfo.nextMode == GameMode.Stay ? initGameMode : GameInfo.nextMode;
        GameInfo.nextMode = GameMode.Stay;
    }

    void Start()
    {
        board = GameInfo.Instance.board;
        storage = GameInfo.Instance.storage;
        if (Agent == null) {
            Agent = GameObject.FindObjectOfType<PlayerAgent>();
        }

        NewGame(gameMode);
        StartGame();
    }

    void Update()
    {
        if (state == MainState.Over || IsEffecting)
            return;

        if (state == MainState.AgentThinking)
        {
            if (gameMode == GameMode.Agent) {
                if (Agent.State == PlayerAgent.StateEnum.Complete) {
                    statusUI.ShowTimecost(Agent.GetCostTime());
                    state = MainState.AgentRunning;
                }
            }
        }
        if (state == MainState.AgentRunning)
        {
            if (gameMode == GameMode.Agent) {
                if (DoAgentAction()) {
                    state = MainState.AgentEffectWaiting;
                }
            }
        }
        else if (state == MainState.AgentEffectWaiting) {
            NextTurn();
        }
        else
        {
            var point = GameInfo.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (board.gameObject.GetComponent<Collider2D>() == Physics2D.OverlapPoint(point)) {
                MouseEvent(point);
            }
        }
    }

    void OnGUI()
    {
        if (IsEffecting)
            return;

        //if (GUI.Button(new Rect(0, 0, 100, 50), "Restart"))
        //{
        //    NewGame(gameMode);
        //    StartGame();
        //    return;
        //}
        //if (GUI.Button(new Rect(120, 0, 100, 50), gameMode == GameMode.Normal ? "PvC" : "PvP"))
        //{
        //    NewGame(gameMode == GameMode.Normal ? GameMode.AI : GameMode.Normal);
        //    StartGame();
        //    return;
        //}

        //if (State == MainState.Wait && GUI.Button(new Rect((Screen.width-100)/2, (Screen.height-50)/2, 100, 50), "Over"))
        //{
        //    NextTurn();
        //}
    }

    void OnDestroy() {
        DestroyAgent();
    }
    #endregion
}
