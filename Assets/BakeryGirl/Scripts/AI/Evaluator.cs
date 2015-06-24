using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// To evaluate a game board and giva a mark to each side of player
/// </summary>
public class Evaluator
{
    #region Evaluation Constants
    public const double MaxValue = 1000000;
    public const double Infinite = MaxValue * 2;

    public static readonly double[,] PosEval = new double[BoardInfo.Row, BoardInfo.Col]{
        {1,1,1,1,1},
        {2,2,2,2,2},
        {3,3,3,3,3},
        {4,4,5,4,4},
        {5,6,7,6,5},
        {6,7,8,7,6},
        {7,8,10,8,7}
    };

    public static readonly double[] CardEval = new double[(int)Unit.TypeEnum.Void]{
        1.5,
        1,
        1.5,
        5,  // old 4
        1.3 // old 1.5, 1.2, 1.4
    };

    public const double PosFactor = 5;
    public const double CardFactor = 20;
    public const double DangerFactor = 10;
    public const double SpecialFactor = 20;
    #endregion

    #region Evaluate Interfaces
    public static double EvaluateResult(Ruler.GameResult result, Unit.OwnerEnum owner)
    {
        if (result == Ruler.GameResult.Black_Win)
            return owner == Unit.OwnerEnum.Black ? MaxValue : -MaxValue;
        else if (result == Ruler.GameResult.White_Win)
            return owner == Unit.OwnerEnum.White ? MaxValue : -MaxValue;
        else if (result == Ruler.GameResult.Draw)
            return 0;
        return 0;
    }

    public static double Evaluate(GameDescriptor descriptor)
    {
        Unit.OwnerEnum owner = descriptor.Turn;
        Ruler.GameResult result = Ruler.CheckGame(descriptor);

        if (result != Ruler.GameResult.NotYet)
            return EvaluateResult(result, owner);

        double value = 0;

        value += EvalPosition(descriptor, owner) * PosFactor;
        value += EvalCard(descriptor, owner) * CardFactor;
        value += EvalDanger(descriptor, owner) * DangerFactor;
        value += EvalSpecial(descriptor, owner) * SpecialFactor;

        return value;
    }
    #endregion

    #region Evaluate Aspects
    private static double EvalPosition(GameDescriptor descriptor, Unit.OwnerEnum owner)
    {
        double value = 0;
        for (int i = 0; i < BoardInfo.Row; i++)
            for (int j = 0; j < BoardInfo.Col; j++)
            {
                UnitInfo info = descriptor.GetInfo(new Position(i, j));
                if (info.owner != Unit.OwnerEnum.None)
                {
                    if (info.owner == Unit.OwnerEnum.Black)
                        value += (owner == info.owner) ? PosEval[i, j] : (-PosEval[i, j]);
                    else
                        value += (owner == info.owner) ? PosEval[BoardInfo.Row - i - 1, BoardInfo.Col - j - 1] : (-PosEval[BoardInfo.Row - i - 1, BoardInfo.Col - j - 1]);
                }
            }
        return value;
    }

    private static double EvalCard(GameDescriptor descriptor, Unit.OwnerEnum owner)
    {
        double value = 0;

        for (Unit.TypeEnum type = Unit.TypeEnum.Bread; type < Unit.TypeEnum.Void; type++)
        {
            int tmp = descriptor.GetPlayerInfo(type, owner);
            if (tmp <= 2)
                value += CardEval[(int)type] * tmp;
            else
                value += CardEval[(int)type] * (tmp - 2) / 2 + CardEval[(int)type] * 2;

            tmp = descriptor.GetPlayerInfo(type, Unit.Opposite(owner));
            if (tmp <= 2)
                value -= CardEval[(int)type] * tmp;
            else
                value -= CardEval[(int)type] * (tmp - 2) / 2 + CardEval[(int)type] * 2;
        }

        return value;
    }

    private static double EvalDanger(GameDescriptor descriptor, Unit.OwnerEnum owner)
    {
        double value = 0;
        List<double> equalList = new List<double>();

        for (int i = 0; i < BoardInfo.Row; i++)
            for (int j = 0; j < BoardInfo.Col; j++)
            {
                UnitInfo src_info = descriptor.GetInfo(new Position(i, j));
                if (src_info.owner == owner)
                    foreach (Position delta in Controller.MoveOffsetList)
                    {
                        Position tar = src_info.pos + delta;
                        if (tar.IsValid)
                        {
                            UnitInfo tar_info = descriptor.GetInfo(tar);
                            if (tar_info.owner == Unit.Opposite(src_info.owner))
                            {
                                if (src_info.type > tar_info.type)
                                {
                                    value += CardEval[(int)tar_info.type];
                                    if (src_info.type == Unit.TypeEnum.Bomb || tar_info.type == Unit.TypeEnum.Bomb)
                                        value -= CardEval[(int)src_info.type];
                                }
                                else if (src_info.type < tar_info.type)
                                {
                                    value -= CardEval[(int)src_info.type];
                                    if (src_info.type == Unit.TypeEnum.Bomb || tar_info.type == Unit.TypeEnum.Bomb)
                                        value += CardEval[(int)src_info.type];
                                }
                                else
                                {
                                    equalList.Add(CardEval[(int)(src_info.type)]);
                                }
                            }
                        }
                    }
            }
        if (equalList.Count > 0)
        {
            equalList.Sort();
            for (int i = 0; i < equalList.Count - 1; i++)
                value -= equalList[i];
            value += equalList[equalList.Count - 1];
        }

        return value;
    }

    private static double EvalSpecial(GameDescriptor descriptor, Unit.OwnerEnum owner)
    {
        double value = 0;
        if (descriptor.GetPlayerInfo(Unit.TypeEnum.Boss, Unit.Opposite(owner)) > 0)
            value += descriptor.GetPlayerInfo(Unit.TypeEnum.Bomb, owner) > 0 ? 1 : 0;
        if (descriptor.GetPlayerInfo(Unit.TypeEnum.Boss, owner) > 0)
            value -= descriptor.GetPlayerInfo(Unit.TypeEnum.Bomb, Unit.Opposite(owner)) > 0 ? 1 : 0;
        if (descriptor.RestResource > 0)
        {
            if (descriptor.GetPlayerInfo(Unit.TypeEnum.Scout, owner) == 0)
            {
                if (descriptor.GetPlayerInfo(Unit.TypeEnum.Bread, owner) == 0)
                    value -= 4;
                if (descriptor.RestResource >= 5)
                    value -= 3;
            }
            if (descriptor.GetPlayerInfo(Unit.TypeEnum.Scout, owner) >= 2 && descriptor.RestResource > 5)
                value += 7;
            if (descriptor.GetPlayerInfo(Unit.TypeEnum.Scout, Unit.Opposite(owner)) >= 2 && descriptor.RestResource > 5)
                value -= 7;

            if (descriptor.GetPlayerInfo(Unit.TypeEnum.Scout, Unit.Opposite(owner)) == 0)
            {
                if (descriptor.GetPlayerInfo(Unit.TypeEnum.Bread, Unit.Opposite(owner)) == 0)
                    value += 4;
                if (descriptor.RestResource >= 5)
                    value += 3;
            }
        }

        if (descriptor.RestResource == 0)
        {
            value -= descriptor.GetPlayerInfo(Unit.TypeEnum.Scout, owner) * 0.5;
            value += descriptor.GetPlayerInfo(Unit.TypeEnum.Scout, Unit.Opposite(owner)) * 0.5;
        }
        return value;
    }
    #endregion
}
