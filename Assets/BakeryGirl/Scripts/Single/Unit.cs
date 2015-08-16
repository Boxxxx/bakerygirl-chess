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
    private static readonly Vector3 kMinimumOffset = new Vector3(-0.2f, -0.18f, 0);
    private static readonly Vector3 kMinimumScale = new Vector3(0.5f, 0.5f, 1.0f);
    private const float kMiminumTime = 0.25f;

    private TypeEnum type;
    private OwnerEnum owner;
    private Position pos;
    private SpriteRenderer sprite;
    private bool isFocus;
    private bool isMinimum;
    private Card m_card;
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
            if (m_card != null) {
                m_card.IsSelected = value;
            }
        }
    }
    public bool CardActive {
        set {
            if (m_card != null) {
                m_card.IsActive = value;
            }
        }
    }
    public bool Minimum {
        get { return isMinimum; }
        set {
            if (isMinimum != value) {
                isMinimum = value;
                if (m_card != null) {
                    if (isMinimum) {
                        iTween.MoveTo(m_card.gameObject, iTween.Hash("position", kMinimumOffset, "time", kMiminumTime, "islocal", true));
                        iTween.ScaleTo(m_card.gameObject, iTween.Hash("scale", kMinimumScale, "time", kMiminumTime, "islocal", true));
                    }
                    else {
                        iTween.MoveTo(m_card.gameObject, iTween.Hash("position", Vector3.zero, "time", kMiminumTime, "islocal", true));
                        iTween.ScaleTo(m_card.gameObject, iTween.Hash("scale", new Vector3(1, 1, 1), "time", kMiminumTime, "islocal", true));
                    }
                }
            }
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
        setSprite(ArtManager.Instance.GetBattleCardGraphics(type, owner));

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
    }

	void Start () {
	}
	
	void Update () {
        if (type == TypeEnum.Bread) {
            var unit = GameInfo.Instance.board.GetUnit(pos, false);
            if (unit != null && unit.type != TypeEnum.Scout) {
                Minimum = true;
            }
            else {
                Minimum = false;
            }
        }
	}
    #endregion

    #region Set & Get Functions
    private void setTransform(Position position)
    {
        Vector2 coor = PosToScreen(position);
        transform.position = new Vector3(coor.x, coor.y, 0);
    }
    public void setSprite(Card graphics)
    {
        if (m_card != null) {
            Destroy(m_card.gameObject);
            m_card = null;
        }
        if (graphics != null) {
            m_card = (Instantiate(graphics.gameObject, transform.position, Quaternion.identity) as GameObject).GetComponent<Card>();
            m_card.transform.parent = transform;
            m_card.spriteRenderer.sortingOrder = -pos.R;
            sprite = m_card.GetComponentInChildren<SpriteRenderer>();
        }
    }
    public void setSprite(string name) {
        setSprite(ArtManager.Instance.GetBattleCardGraphicsByName(name));
    }
    public void setPosition(Position pos)
    {
        this.pos = pos;
        setTransform(pos);
        if (m_card != null) {
            m_card.spriteRenderer.sortingOrder = -pos.R;
        }
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
    public string GetCardName() {
        return ArtManager.GetCardName(type, owner);
    }
    #endregion

    #region Public Static Utility Functions
    public static Vector2 PosToScreen(Position position)
    {
        if (GameInfo.Instance.ShouldUpsidedown) {
            position = position.Upsidedown;
        }
        return new Vector2(position.C * BoardInfo.GridWidth + BoardInfo.GridZeroPosition.x, position.R * BoardInfo.GridWidth + BoardInfo.GridZeroPosition.y);
    }
    public static Position ScreenToPos(Vector2 position)
    {
        var pos = new Position(
            (int)Mathf.Floor((position.y - BoardInfo.GridZeroPosition.y + BoardInfo.GridHalfWidth) / BoardInfo.GridHeight),
            (int)Mathf.Floor((position.x - BoardInfo.GridZeroPosition.x + BoardInfo.GridHalfHeight) / BoardInfo.GridWidth));
        if (GameInfo.Instance.ShouldUpsidedown) {
            pos = pos.Upsidedown;
        }
        return pos;
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
            GameInfo.Instance.board.ModifyPlayerInfo(type, owner, 1);
            GameInfo.Instance.controller.StopEffect(Controller.EffectType.Killout);
            GameObject.Destroy(gameObject);
        }
        else if (IsSoldier(type))
        {
            GameInfo.Instance.controller.StopEffect(Controller.EffectType.Killout);
            GameObject.Destroy(gameObject);
        }
    }
    private void OnAppearComplete()
    {
        if (IsSoldier(type))
        {
            GameInfo.Instance.board.Put(Pos, this);
            GameInfo.Instance.controller.StopEffect(Controller.EffectType.MoveIn);
        }
    }
    private void OnMoveComplete()
    {
        GameInfo.Instance.board.Put(Pos, this);
        GameInfo.Instance.controller.StopEffect(Controller.EffectType.Move);
    }
    #endregion
}
