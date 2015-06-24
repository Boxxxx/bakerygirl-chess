using UnityEngine;
using System;
using System.Collections;

public class GlobalInfo {
	
	// Singleton implement
	private static GlobalInfo instance;
	protected GlobalInfo() {}
	public static GlobalInfo Instance {
	    get {
		    if(instance == null)
			    instance = new GlobalInfo();
		
		    return instance;
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
    public Storage storage;
    public Camera mainCamera;
    public Controller controller;
}

public class BoardInfo
{
	public const int Row = 7;
	public const int Col = 5;
    public const float GridWidth = 103;
    public const float GridHeight = 102;
    public const float GridHalfWidth = 103 / 2.0f;
    public const float GridHalfHeight = 102 / 2.0f;
    public const float UnitSpriteWidth = 63;
    public const float UnitSpriteHeight = 84;
    public const float UnitSpriteHalfWidth = 63 / 2f;
    public const float UnitSpriteHalfHeight = 84 / 2f;

    public readonly static Vector2 GridZeroPosition = new Vector2(379 + UnitSpriteHalfWidth, 974 - (753 + UnitSpriteHalfHeight));
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
    public static readonly Vector3 collectPointOffset = new Vector3(52, 132, 0);

    public static readonly Vector3[] CardPosOffset = { new Vector3(-58, -101.5f, 0),
                                                        new Vector3(50, -101.5f, 0),
                                                        new Vector3(-58, 0.5f, 0),
                                                        new Vector3(50, 0.5f, 0) };
    public static readonly Unit.TypeEnum[] CardTypeList = { Unit.TypeEnum.Boss, Unit.TypeEnum.Bomb, Unit.TypeEnum.Pioneer, Unit.TypeEnum.Scout };
    public static readonly int[] CardCost = { 2, 1, 1, 1 };
}

public class UnitInfo : ICloneable  
{
    public Position pos = new Position();
    public Unit.TypeEnum type = Unit.TypeEnum.Void;
    public Unit.OwnerEnum owner = Unit.OwnerEnum.None;

    public static readonly Vector3 KilledEffectOffset = new Vector3(-50, 50, 0);

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