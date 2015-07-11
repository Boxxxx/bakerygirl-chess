using UnityEngine;
using System.Collections;

/// <summary>
/// Unit
/// To describe a unit in game board or storage
/// </summary>
public class Unit : MonoBehaviour
{
    #region Enums
    public enum TypeEnum { Bread, Scout, Pioneer, Boss, Bomb, Void, Tile };
    public enum OwnerEnum { Black, White, None };
    public enum MoveWay { Direct, Transition };
    #endregion

    #region Variables
    private TypeEnum type;
    private OwnerEnum owner;
    private Position pos;
    private tk2dAnimatedSprite sprite;
    private bool isFocus;
    private tk2dAnimatedSprite highlight;
    #endregion

    #region Properties
    public TypeEnum Type { get { return type; } }
    public OwnerEnum Owner { get { return owner; } set { owner = value; } }
    public Position Pos { get { return pos; } set { pos = value; } }
    public tk2dAnimatedSprite Sprite { get { return sprite; } }
    public bool Focus { 
        get { return isFocus; }
        set
        {
            isFocus = value;
            if (isFocus == false)
                highlight.gameObject.SetActive(false);
            else
                highlight.gameObject.SetActive(true);
        }
    }
    #endregion

    #region Constuctor
    public Unit(UnitInfo info) {
        Initialize(info);
    }
    public Unit(Position position) {
        Initialize(new UnitInfo(position, TypeEnum.Void, OwnerEnum.None));
    }
    public void Initialize(UnitInfo info)
    {
        this.sprite = GetComponent<tk2dAnimatedSprite>();
        this.type = info.type;
        this.owner = info.owner;
        this.pos = info.pos;

        setTransform(pos);
        setSpriteId(GetSpriteId(type, owner));

        if (IsSoldier(type))
            transform.parent = GameObject.Find("soldier").transform;
        else if (type == TypeEnum.Bread)
            transform.parent = GameObject.Find("res").transform;
        else if (type == TypeEnum.Tile)
            transform.parent = GameObject.Find("tile").transform;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
    }
    #endregion

    #region Unity Callback Function
    void Awake() {
        highlight = gameObject.transform.Find("highlight").GetComponent<tk2dAnimatedSprite>();
    }

	void Start () {
	}
	
	void Update () {
	}
    #endregion

    #region Set & Get Functions
    private void setTransform(Position position)
    {
        Vector2 coor = PosToScreen(position);
        transform.position = new Vector3(coor.x, coor.y, 0);
    }
    public void setSpriteId(int id)
    {
        sprite.Stop();
        sprite.clipId = id;
        sprite.Start();
    }
    public void setPosition(Position pos)
    {
        this.pos = pos;
        setTransform(pos);
    }
    #endregion

    #region Public Static Utility Functions
    public static Vector2 PosToScreen(Position position)
    {
        return new Vector2(position.C * BoardInfo.GridWidth + BoardInfo.GridZeroPosition.x, position.R * BoardInfo.GridWidth + BoardInfo.GridZeroPosition.y);
    }
    public static Position ScreenToPos(Vector2 position)
    {
        return new Position((int)Mathf.Floor((position.y - BoardInfo.GridZeroPosition.y + BoardInfo.UnitSpriteHalfHeight) / BoardInfo.GridHeight),
                        (int)Mathf.Floor((position.x - BoardInfo.GridZeroPosition.x + BoardInfo.UnitSpriteHalfWidth) / BoardInfo.GridWidth));
    }
    public static Unit.OwnerEnum Opposite(Unit.OwnerEnum owner)
    {
        if (owner == Unit.OwnerEnum.Black)
            return Unit.OwnerEnum.White;
        else if (owner == Unit.OwnerEnum.White)
            return Unit.OwnerEnum.Black;
        else
            return Unit.OwnerEnum.None;
    }
    public static int GetSpriteIdByName(string name)
    {
        return GlobalInfo.Instance.storage.unitPrefab.GetComponent<tk2dAnimatedSprite>().GetClipIdByName(name);
    }
    public static int GetSpriteId(TypeEnum type, OwnerEnum owner)
    {
        string spriteName;
        if (type == TypeEnum.Bread)
            spriteName = "bread_bounce";
        else if (type == TypeEnum.Void)
            spriteName = "void";
        else if (type == TypeEnum.Tile)
            spriteName = "tile";
        else
            spriteName = type.ToString().ToLower() + ((int)owner).ToString();
        return GlobalInfo.Instance.storage.unitPrefab.GetComponent<tk2dAnimatedSprite>().GetClipIdByName(spriteName);
    }
    public static bool IsSoldier(TypeEnum type)
    {
        return (type > TypeEnum.Bread && type < TypeEnum.Void);
    }
    #endregion

    #region Private Functions
    private void OnDisappearComplete()
    {
        if (type == TypeEnum.Bread)
        {
            GlobalInfo.Instance.board.ModifyPlayerInfo(type, owner, 1);
            GlobalInfo.Instance.controller.StopEffect(Controller.EffectType.Killout);
            GameObject.Destroy(gameObject);
        }
        else if (IsSoldier(type))
        {
            GlobalInfo.Instance.controller.StopEffect(Controller.EffectType.Killout);
            GameObject.Destroy(gameObject);
        }
    }
    private void OnAppearComplete()
    {
        if (IsSoldier(type))
        {
            GlobalInfo.Instance.board.Put(Pos, this);
            GlobalInfo.Instance.controller.StopEffect(Controller.EffectType.MoveIn);
        }
    }
    private void OnMoveComplete()
    {
        GlobalInfo.Instance.board.Put(Pos, this);
        GlobalInfo.Instance.controller.StopEffect(Controller.EffectType.Move);
    }
    #endregion
}
