using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A simple AI implemented by alpha-beta search of game theory
/// </summary>
public class SimpleAI : AIPlayer
{
    // max depth to search
    public int Max_Depth = 5;
    // max node in an search
    public float Max_Node = 300000;
    // the number of random swap in an step enum
    private const int Disturb_Num = 100;

    override protected void DoCalculate()
    {
        AlphaBeta(descriptor, Max_Depth, -Evaluator.Infinite, Evaluator.Infinite);
    }

    double AlphaBeta(GameDescriptor descriptor, int depth, double alpha, double beta)
    {
        Ruler.GameResult result = Ruler.CheckGame(descriptor);

        if (result != Ruler.GameResult.NotYet)
            return Evaluator.EvaluateResult(result,descriptor.Turn)-depth;
        if (depth == 0 || nodeCount >= Max_Node)
        {
            nodeCount++;
            return Evaluator.Evaluate(descriptor) - depth; // the quicker, the better
        }

        List<AI_Action> actions = descriptor.QueryAllActions();
        Disturb(actions);

        foreach (AI_Action tryAction in actions)
        {
            descriptor.DoAction(tryAction);
            var tmp = -AlphaBeta(descriptor, depth - 1, -beta, -alpha);
            tryAction.UnDo(descriptor);

            if (tmp >= beta)
                return beta;
            if (tmp > alpha)
            {
                alpha = tmp;
                if (depth == Max_Depth)
                    action = tryAction;
            }
        }
        return alpha;
    }

    double AlphaBeta_Slow(GameDescriptor descriptor, int depth, double alpha, double beta)
    {
        Ruler.GameResult result = Ruler.CheckGame(descriptor);
        if (result != Ruler.GameResult.NotYet)
            return Evaluator.EvaluateResult(result, descriptor.Turn) - depth;
        if (depth == 0)
        {
            nodeCount++;
            return Evaluator.Evaluate(descriptor) - depth;
        }

        List<AI_Action> actions = descriptor.QueryAllActions_Slow();
        Disturb(actions);

        foreach (AI_Action tryAction in actions)
        {
            GameDescriptor clone = descriptor.Clone() as GameDescriptor;
            clone.DoAction(tryAction);
            var tmp = -AlphaBeta_Slow(clone, depth - 1, -beta, -alpha);

            if (tmp >= beta)
                return beta;
            if (tmp > alpha)
            {
                alpha = tmp;
                if (depth == Max_Depth)
                    action = tryAction;
            }
        }
        return alpha;
    }

    private void Disturb(List<AI_Action> actions)
    {
        long tick = DateTime.Now.Ticks;
        System.Random random = new System.Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
      
        for (int i = 0; i < Disturb_Num; i++)
        {
            int indexA = random.Next(0, actions.Count),
                indexB = random.Next(0, actions.Count);

            AI_Action tmp = actions[indexA];
            actions[indexA] = actions[indexB];
            actions[indexB] = tmp;
        }
    }
}
