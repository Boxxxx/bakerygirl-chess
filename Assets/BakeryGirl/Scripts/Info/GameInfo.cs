using UnityEngine;
using System;

public class GlobalInfo : MonoBehaviour {
	// Singleton implement
	private static GlobalInfo m_instance;
	protected GlobalInfo() {}
	public static GlobalInfo Instance {
	    get {
            if (m_instance == null) {
                var obj = new GameObject("GlobalInfo", typeof(GlobalInfo));
                m_instance = obj.GetComponent<GlobalInfo>();
            }
		
		    return m_instance;
	    }
    }
	
	// Global Info
	public int GameWidth
	{
		get {return 1240;}
	}
	public int GameHeight
	{
		get {return 974;}
	}

    public Board board;
    public UIStorage storage;
    public Camera mainCamera;
    public Controller controller;
    public CharacterImageShowup characterImage;

    void Awake() {
        m_instance = this;
    }
}

public class BoardInfo
{
	public const int Row = 7;
	public const int Col = 5;
    public const float GridWidth = 0.745f;
    public const float GridHeight = 0.745f;
    public const float GridHalfWidth = GridWidth / 2.0f;
    public const float GridHalfHeight = GridHeight / 2.0f;
    public const float UnitSpriteWidth = 0.83f;
    public const float UnitSpriteHeight = 1.01f;
    public const float UnitSpriteHalfWidth = UnitSpriteWidth / 2f;
    public const float UnitSpriteHalfHeight = UnitSpriteHeight / 2f;

    public readonly static Vector2 GridZeroPosition = new Vector2(-1.08f, -2.28f);
	public readonly static Position[] Base = new Position[]{new Position(0, Col/2), new Position(Row-1, Col/2)};
	public readonly static Position[] BreadList = new Position[]{new Position(0, 0), new Position(0, 4), new Position(2, 1), new Position(2, 3),
										 new Position(3, 0), new Position(3, 2), new Position(3, 4),
										 new Position(6, 0), new Position(6, 4), new Position(4, 1), new Position(4, 3)};
    public readonly static UnitInfo[] InitUnitList = new UnitInfo[]{
           new UnitInfo(new Position(0, 1), Unit.TypeEnum.Pioneer, Unit.OwnerEnum.Black),
           new UnitInfo(new Position(0, 3), Unit.TypeEnum.Scout, Unit.OwnerEnum.Black),
           new UnitInfo(new Position(6, 1), Unit.TypeEnum.Scout, Unit.OwnerEnum.White),
           new UnitInfo(new Position(6, 3), Unit.TypeEnum.Pioneer, Unit.OwnerEnum.White)
    };
}

public class StorageInfo
{
    public static readonly Vector3[] CardPosOffset = { new Vector3(-0.58f, -1.015f, 0),
                                                        new Vector3(0.50f, -1.015f, 0),
                                                        new Vector3(-0.58f, 0.005f, 0),
                                                        new Vector3(0.50f, 0.005f, 0) };
    public static readonly Unit.TypeEnum[] CardTypeList = { Unit.TypeEnum.Scout, Unit.TypeEnum.Pioneer, Unit.TypeEnum.Boss, Unit.TypeEnum.Bomb };
    public static readonly int[] CardCost = { 1, 1, 2, 1 };

    public static readonly Color Orange = new Color(1.0f, 0.4f, 0f);
}

public class UnitInfo : ICloneable  
{
    public Position pos = new Position();
    public Unit.TypeEnum type = Unit.TypeEnum.Void;
    public Unit.OwnerEnum owner = Unit.OwnerEnum.None;

    public static readonly Vector3 KilledEffectOffset = new Vector3(-0.50f, 0.50f, 0);

    public UnitInfo() { }
    public UnitInfo(Position pos, Unit.TypeEnum type, Unit.OwnerEnum owner = Unit.OwnerEnum.None)
    {
        this.pos = pos;
        this.type = type;
        this.owner = owner;
    }
    public UnitInfo(Unit.TypeEnum type, Unit.OwnerEnum owner = Unit.OwnerEnum.None)
    {
        this.pos = new Position();
        this.type = type;
        this.owner = owner;
    }
    public bool Compare(UnitInfo rhs)
    {
        return pos == rhs.pos && type == rhs.type && owner == rhs.owner;
    }
    public object Clone()
    {
        return new UnitInfo(pos, type, owner);
    }
}

public class Position : ICloneable
{
	private int _r = 0;
	private int _c = 0;
	
	public int R
	{
		get{return _r;}
		set{_r = value;}
	}
	
	public int C
	{
		get{return _c;}
		set{_c = value;}
	}
	
    /// <summary>
    /// To Check whether the position is in valid grid
    /// </summary>
	public bool IsValid
	{
		get {return (_r >= 0 && _r < BoardInfo.Row && _c >= 0 && _c < BoardInfo.Col);}
	}
	
	public Position() {}
	public Position(int r, int c) 
	{
		_r = r;
		_c = c;
	}

    public static Position operator+ (Position a, Position b)
    {
        return new Position(a.R + b.R, a.C + b.C);
    }

    public static Position operator- (Position a, Position b)
    {
        return new Position(a.R - b.R, a.C - b.C);
    }

    public static bool operator== (Position a, Position b)
    {
        return a.R == b.R && a.C == b.C;
    }

    public static bool operator !=(Position a, Position b)
    {
        return !(a == b);
    }

    public override bool Equals(object other)
    {
        return (this == (other as Position));
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public object Clone()
    {
        return new Position(R, C);
    }
}