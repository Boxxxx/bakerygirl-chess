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
    private SpriteRenderer sprite;
    private bool isFocus;
    private tk2dAnimatedSprite highlight;
    private GameObject m_graphics;
    #endregion

    #region Properties
    public TypeEnum Type { get { return type; } }
    public OwnerEnum Owner { get { return owner; } set { owner = value; } }
    public Position Pos { get { return pos; } set { pos = value; } }
    public SpriteRenderer Sprite { get { return sprite; } }
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
        this.sprite = null;
        this.type = info.type;
        this.owner = info.owner;
        this.pos = info.pos;

        setTransform(pos);
        setSprite(GetCardGraphics(type, owner));

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
    public void setSprite(GameObject graphics)
    {
        if (m_graphics != null) {
            Destroy(m_graphics);
            m_graphics = null;
        }
        if (graphics != null) {
            m_graphics = Instantiate(graphics, transform.position, Quaternion.identity) as GameObject;
            m_graphics.transform.parent = transform;
            sprite = m_graphics.GetComponentInChildren<SpriteRenderer>();
        }
    }
    public void setPosition(Position pos)
    {
        this.pos = pos;
        setTransform(pos);
    }
    public void SetAlpha(float alpha) {
        if (sprite != null) {
            var color = sprite.color;
            color.a = alpha;
            sprite.color = color;
        }
    }
    public void SetColor(float r, float g, float b) {
        if (sprite != null) {
            sprite.color = new Color(r, g, b, sprite.color.a);
        }
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
    public static OwnerEnum Opposite(OwnerEnum owner)
    {
        if (owner == OwnerEnum.Black)
            return OwnerEnum.White;
        else if (owner == OwnerEnum.White)
            return OwnerEnum.Black;
        else
            return OwnerEnum.None;
    }
    public static GameObject GetCardGraphics(TypeEnum type, OwnerEnum owner) {
        return GlobalInfo.Instance.storage.GetCardGraphics(type, owner);
    }
    public static GameObject GetCardGraphicsByName(string name) {
        return GlobalInfo.Instance.storage.GetCardGraphicsByName(name);
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
