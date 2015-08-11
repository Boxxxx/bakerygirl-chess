using UnityEngine;
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

        unit.setSprite(GlobalInfo.Instance.board.GetCardGraphics(source.Type, source.Owner));
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
            if (newPos.IsValid && GlobalInfo.Instance.board.GetUnitOwner(newPos) != owner)
            {
                Unit unit = GlobalInfo.Instance.board.InstantiateUnit(new UnitInfo(source.Pos + offset, Unit.TypeEnum.Tile));
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
        Board board = GlobalInfo.Instance.board;
        if (board.GetUnitOwner(tile.Pos) == owner) {
            return false;
        }
        else if (board.GetUnitOwner(tile.Pos) == Unit.Opposite(owner)) {
            tile.SetColor(1, 0, 0);
            tile.CardActive = true;
            tile.setSprite("attack_hint");
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
            tile.CardActive = true;
            tile.setSprite("move_hint");
            tile.transform.Rotate(new Vector3(0, 0, rotation));
        }
        return true;
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
    public Button turnOverButton;
    public ResultSprite resultSprite;
    public UILogger logger;
    private GameMode gameMode = GameMode.Normal;
    public PlayerAgent agent;
    public LastMoveHint lastMoveHint;
    //public string aiClassName = "";

    private MoveState moveState;
    private Unit.OwnerEnum turn;
    private MainState state = MainState.Uninitialized;
    private int effectNum;
    private Board board;
    private StorageUI storage;
    private Hint hint = new Hint();
    private Ruler.GameResult result;
    private Unit lastMoveUnit;

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
    #endregion

    #region Effect Functions
    public void StartEffect(EffectType effect = EffectType.Unknown)
    {
        effectNum++;
        storage.IsEndTurnValid = false;
    }

    public void StopEffect(EffectType effect = EffectType.Unknown)
    {
        effectNum--;

        if (effect == EffectType.Move)
        {
            if (lastMoveUnit != null)
            {
                lastMoveHint.Focus(lastMoveUnit);
                //lastMove.Focus = true;
            }
        }
        if (state == MainState.Wait && effectNum == 0) {
            storage.IsEndTurnValid = true;
        }
    }

    public void ClearEffect()
    {
        effectNum = 0;
    }

    private void MoveEffect(Unit src, Position pos)
    {
        board.Pick(src.Pos);
        src.Pos = pos;
        Vector2 targetPos = Unit.PosToScreen(pos);
        iTween.MoveTo(src.gameObject, iTween.Hash("position", new Vector3(targetPos.x, targetPos.y, 0), "time", 0.2f, "oncomplete", "OnMoveComplete", "oncompletetarget", src.gameObject));
        StartEffect(EffectType.Move);

        if (lastMoveUnit != null) {
            lastMoveHint.UnFocus();
        }
        lastMoveUnit = src;
    }

    private void CollectBreadEffect(Unit bread, Unit.OwnerEnum owner)
    {
        bread.Owner = owner;
        // set 
        bread.setSprite(GlobalInfo.Instance.board.GetCardGraphicsByName("bread"));
        iTween.MoveAdd(bread.gameObject, iTween.Hash("amount", new Vector3(0, BoardInfo.GridHeight, 0), "time", 0.5f, "easetype", iTween.EaseType.easeOutCubic,"oncomplete", (iTween.CallbackDelegate)(tween => {
            iTween.MoveTo(bread.gameObject, iTween.Hash("position", storage.GetCollectPoint(owner), "time", 1f, "oncomplete", "OnDisappearComplete", "oncompletetarget", bread.gameObject));
        })));
        StartEffect(EffectType.CollectBread);
    }

    private void KillEnemyEffect(Unit enemy)
    {
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

        lastMoveUnit = null;
        moveState = MoveState.Idle;
        ClearEffect();
        hint.ClearHints();
        board.NewGame();
        storage.NewGame();
        state = MainState.Ready;
        turn = Unit.OwnerEnum.Black;
        if (resultSprite != null) {
            resultSprite.gameObject.SetActive(false);
        }
        storage.IsEndTurnValid = false;

        _actionLogs.Clear();
        _actionsCurrentTurn.Clear();

        if (gameMode == GameMode.Agent) {
            InitAgent();
        }
        if (logger != null) {
            logger.Text = "";
            logger.State = UILogger.StateEnum.Normal;
        }
    }
    private void StartGame()
    {
        state = MainState.Move;
    }
    private void NewTurn(bool initial)
    {
        board.SwitchTurn(turn);
        storage.SwitchTurn(turn);
        if (gameMode == GameMode.Agent) {
            AgentSwitchTurn(initial);
        }
    }
    private void OnGameOver()
    {
        agent.OnGameOver(result);
        if (resultSprite != null) {
            resultSprite.gameObject.SetActive(true);
            resultSprite.OnGameOver(result);
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
                GlobalInfo.Instance.characterImage.Show(unit.GetCardName());
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
            if (!audio.isPlaying)
                audio.Play();
        }
        else
        {
            if (audio.isPlaying)
                audio.Stop();
        }
    }
    #endregion

    #region Actions
    private bool _DoTurnOver()
    {
        var actions = _actionsCurrentTurn.ToArray();
        _actionsCurrentTurn.Clear();

        _actionLogs.Add(actions);
        if (agent != null)
        {
            agent.OnMove(state == MainState.AgentThinking || state == MainState.AgentRunning || state == MainState.AgentEffectWaiting, actions);
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
                break;
            case Ruler.ConflictResult.Src_Win:
                KillEnemyEffect(board.Pick(des.Pos));
                MoveEffect(src, desPos);
                break;
            case Ruler.ConflictResult.Des_Win:
                return false;
            case Ruler.ConflictResult.Eat_Bread:
                Unit bread = board.Pick(des.Pos);
                MoveEffect(src, desPos);
                CollectBreadEffect(bread, src.Owner);
                break;
            case Ruler.ConflictResult.Nothing:
                MoveEffect(src, desPos);
                break;
        }
        return true;
    }
    private bool _DoBuy(Unit.TypeEnum type)
    {
        return GlobalInfo.Instance.storage.BuyCard(type, turn);
    }
    #endregion

    #region Agent Functions
    private void InitAgent()
    {
        agent.Initialize();
    }
    private void AgentSwitchTurn(bool initial)
    {
        if(agent.SwitchTurn(turn, initial))
        {
            state = MainState.AgentThinking;
            agent.Think(board);
            if (logger != null) {
                logger.Text = agent.waitingText;
                logger.State = UILogger.StateEnum.Dot;
            }
        }
    }
    private bool DoAgentAction()
    {
        PlayerAction action = agent.NextAction();
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
    #endregion

    #region Unity Callback Functions
    void Awake()
    {
        GlobalInfo.Instance.controller = this;
        gameMode = initGameMode;
    }

    void Start()
    {
        board = GlobalInfo.Instance.board;
        storage = GlobalInfo.Instance.storage;

        NewGame(initGameMode);
        StartGame();

        NewTurn(true);
    }

    void Update()
    {
        if (state == MainState.Over || IsEffecting)
            return;

        if (state == MainState.AgentThinking)
        {
            if (gameMode == GameMode.Agent) {
                if (agent.State == PlayerAgent.StateEnum.Complete) {
                    if (logger != null) {
                        logger.Text = string.Format("花费时间 : {0}ms", agent.GetCostTime());
                        logger.State = UILogger.StateEnum.Normal;
                    }
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
            RaycastHit hit;
            if (board.collider.Raycast(GlobalInfo.Instance.mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 1000f))
            {
                Vector3 point = hit.point;
                
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
    #endregion
}
