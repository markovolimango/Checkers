using System;
using System.Collections.Generic;
using System.Diagnostics;
using Checkers.Models;

namespace Checkers.Bot;

public class Bot
{
    private static readonly float[] RowMultipliers = [0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f, 1.3f, 1.4f];

    public float Evaluate(Board board, int depth)
    {
        var sw = new Stopwatch();
        sw.Start();
        var res = Evaluate(board, depth, float.MinValue, float.MaxValue);
        sw.Stop();
        Console.WriteLine($"Time took: {sw.Elapsed.TotalMilliseconds}ms");
        return res;
    }

    private float Minimize(Board board, int depth, float alpha, float beta)
    {
        if (depth == 0)
            return EvaluateSimple(board);

        var res = ProcessMovesMin(board, depth, alpha, ref beta, board.KingsMoves);
        res = float.Min(res, ProcessMovesMin(board, depth, alpha, ref beta, board.MenMoves));

        return res;
    }

    private float ProcessMovesMin(Board board, int depth, float alpha, ref float beta, List<Move> moves)
    {
        var res = float.MaxValue;
        foreach (var move in moves)
        {
            var newBoard = new Board(board);
            newBoard.MakeMove(move);
            var eval = Maximize(newBoard, depth - 1, alpha, beta);
            if (eval < res)
                res = eval;
            if (eval < beta)
                beta = eval;
            if (beta <= alpha)
                break;
        }

        return res;
    }

    private float Maximize(Board board, int depth, float alpha, float beta)
    {
        if (depth == 0)
            return EvaluateSimple(board);

        var res = ProcessMovesMax(board, depth, ref alpha, beta, board.KingsMoves);
        res = float.Max(res, ProcessMovesMax(board, depth, ref alpha, beta, board.MenMoves));

        return res;
    }

    private float ProcessMovesMax(Board board, int depth, ref float alpha, float beta, List<Move> moves)
    {
        var res = float.MinValue;
        foreach (var move in moves)
        {
            var newBoard = new Board(board);
            newBoard.MakeMove(move);
            var eval = Minimize(newBoard, depth - 1, alpha, beta);
            if (eval > res)
                res = eval;
            if (eval > alpha)
                alpha = eval;
            if (beta <= alpha)
                break;
        }

        return res;
    }

    private float Evaluate(Board board, int depth, float alpha, float beta)
    {
        return board.IsBlackTurn ? Minimize(board, depth - 1, alpha, beta) : Maximize(board, depth - 1, alpha, beta);
    }

    private float EvaluateSimple(Board board)
    {
        if (board.IsRedWin)
            return 200;
        if (board.IsBlackWin)
            return -200;

        var res = 0f;
        foreach (var index in board.GetPieceIndexes(Piece.RedMan))
            res += 1 * RowMultipliers[index / 8];
        foreach (var index in board.GetPieceIndexes(Piece.RedKing))
            res += 2;
        foreach (var index in board.GetPieceIndexes(Piece.BlackMan))
            res -= 1 * RowMultipliers[7 - index / 8];
        foreach (var index in board.GetPieceIndexes(Piece.BlackKing))
            res -= 2;
        return res;
    }
}