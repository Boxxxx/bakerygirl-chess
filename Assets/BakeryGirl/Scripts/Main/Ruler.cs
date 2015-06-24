using UnityEngine;
using System.Collections;

/// <summary>
/// To check & describe the rule of game 
/// Or in a word, check game conflict & game over rule
/// </summary>
public class Ruler
{
    #region Enums
    public enum GameResult { White_Win, Black_Win, Draw, NotYet};
    public enum ConflictResult { Src_Win, Des_Win, Eat_Bread, Nothing, Boom, Deny};
    #endregion

    #region Game Rule Function
    /// <summary>
    /// To Check whether game is over & return the game result
    /// </summary>
    /// <param name="board">the board info</param>
    /// <returns>the game result</returns>
    static public GameResult CheckGame(Board board)
    {
        if (board.GetPlayerTotalCount(Unit.OwnerEnum.Black) == 0 && board.GetPlayerTotalCount(Unit.OwnerEnum.White) == 0)
            return GameResult.Draw;
        if ((board.GetUnitOwner(BoardInfo.Base[0]) == Unit.OwnerEnum.White && board.GetUnitType(BoardInfo.Base[0]) != Unit.TypeEnum.Bomb) || board.GetPlayerTotalCount(Unit.OwnerEnum.Black) == 0)
            return GameResult.White_Win;
        else if ((board.GetUnitOwner(BoardInfo.Base[1]) == Unit.OwnerEnum.Black && board.GetUnitType(BoardInfo.Base[1]) != Unit.TypeEnum.Bomb) || board.GetPlayerTotalCount(Unit.OwnerEnum.White) == 0)
            return GameResult.Black_Win;
        else
        {
            int blackRestUseful = board.GetPlayerTotalCount(Unit.OwnerEnum.Black) - board.GetPlayerInfo(Unit.TypeEnum.Bomb, Unit.OwnerEnum.Black);
            int whiteRestUseful = board.GetPlayerTotalCount(Unit.OwnerEnum.White) - board.GetPlayerInfo(Unit.TypeEnum.Bomb, Unit.OwnerEnum.White);
            if (blackRestUseful == 0 && whiteRestUseful == 0)
                return GameResult.Draw;
            else if (whiteRestUseful == 0)
                return GameResult.Black_Win;
            else if (blackRestUseful == 0)
                return GameResult.White_Win;
            return GameResult.NotYet;
        }
    }
    /// <summary>
    /// Check Game Over for Game Descriptor
    /// </summary>
    /// <param name="descriptor">the descriptor to check</param>
    /// <returns></returns>
    static public GameResult CheckGame(GameDescriptor descriptor)
    {
        int blackTotal = descriptor.GetPlayerTotalCount(Unit.OwnerEnum.Black);
        int whiteTotal = descriptor.GetPlayerTotalCount(Unit.OwnerEnum.White);

        if (blackTotal == 0 && whiteTotal == 0)
                return GameResult.Draw;
        if ((descriptor.GetOwner(BoardInfo.Base[0]) == Unit.OwnerEnum.White && descriptor.GetType(BoardInfo.Base[0]) != Unit.TypeEnum.Bomb) || blackTotal == 0)
            return GameResult.White_Win;
        else if ((descriptor.GetOwner(BoardInfo.Base[1]) == Unit.OwnerEnum.Black && descriptor.GetType(BoardInfo.Base[1]) != Unit.TypeEnum.Bomb) || whiteTotal == 0)
            return GameResult.Black_Win;
        else
        {
            int blackRestUseful = blackTotal - descriptor.GetPlayerInfo(Unit.TypeEnum.Bomb, Unit.OwnerEnum.Black);
            int whiteRestUseful = whiteTotal - descriptor.GetPlayerInfo(Unit.TypeEnum.Bomb, Unit.OwnerEnum.White);

            if (blackRestUseful == 0 && whiteRestUseful == 0)
                return GameResult.Draw;
            else if (whiteRestUseful == 0)
                return GameResult.Black_Win;
            else if (blackRestUseful == 0)
                return GameResult.White_Win;

            return GameResult.NotYet;
        }
    }
    /// <summary>
    /// To Check the result of a conflict move(or simple move when one of side is null)
    /// </summary>
    /// <param name="src">Source Unit's Type</param>
    /// <param name="des">Destination Unit's Type</param>
    /// <returns>The result of the conflict</returns>
    static public ConflictResult CheckConflict(Unit.TypeEnum src, Unit.TypeEnum des)
    {
        if (!Unit.IsSoldier(src) || des == Unit.TypeEnum.Void)
            return ConflictResult.Nothing;

        if (des == Unit.TypeEnum.Bread)
        {
            if (src == Unit.TypeEnum.Scout)
                return ConflictResult.Eat_Bread;
            else
                return ConflictResult.Nothing;
        }
        else if (src == Unit.TypeEnum.Bomb || des == Unit.TypeEnum.Bomb)
            return ConflictResult.Boom;
        else if (src >= des)
            return ConflictResult.Src_Win;
        else
            return ConflictResult.Des_Win;
    }
    #endregion
}
