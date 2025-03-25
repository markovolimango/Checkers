using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Checkers.Models;

namespace Checkers.Engine;

public static class Engine
{
    private static readonly float[] RowMultipliers = [0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f, 1.3f, 1.4f];

    public static (float Score, Move? BestMove) EvaluateWithTimeLimit(Board board, int maxDepth, int timeLimitMs)
    {
        var sw = new Stopwatch();
        sw.Start();
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(timeLimitMs);

        Move? bestMove = null;
        float bestScore = 0;

        try
        {
            for (var depth = 1; depth <= maxDepth; depth++)
            {
                var (score, move) = EvaluateWithDepth(board, depth, cts.Token);
                bestScore = score;
                bestMove = move;

                Console.WriteLine(
                    $"Depth {depth} completed. Score: {score}. Time elapsed: {sw.Elapsed.TotalMilliseconds}ms");

                if (sw.ElapsedMilliseconds > timeLimitMs)
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Time limit reached!");
        }

        sw.Stop();
        Console.WriteLine($"Total analysis time: {sw.Elapsed.TotalMilliseconds}ms");
        return (bestScore, bestMove);
    }

    private static (float Score, Move? BestMove) EvaluateWithDepth(Board board, int depth,
        CancellationToken cancellationToken)
    {
        var allMoves = new List<Move>();
        allMoves.AddRange(board.KingsMoves);
        allMoves.AddRange(board.MenMoves);

        if (allMoves.Count == 1)
            return (Evaluate(board, depth, float.MinValue, float.MaxValue, cancellationToken), allMoves[0]);

        var results = new ConcurrentBag<(float Score, Move Move)>();

        Parallel.ForEach(allMoves, new ParallelOptions
            { CancellationToken = cancellationToken, MaxDegreeOfParallelism = Environment.ProcessorCount }, move =>
        {
            var newBoard = new Board(board);
            newBoard.MakeMove(move);
            var score = Evaluate(newBoard, depth - 1, float.MinValue, float.MaxValue, cancellationToken);

            results.Add((score, move));
        });

        Move? bestMove = null;
        var bestScore = board.IsBlackTurn ? 200f : -200f;

        foreach (var (score, move) in results)
            if (board.IsBlackTurn)
            {
                if (score < bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
            else
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

        return (bestScore, bestMove);
    }

    private static float Evaluate(Board board, int depth, float alpha, float beta, CancellationToken cancellationToken)
    {
        return board.IsBlackTurn
            ? Minimize(board, depth, alpha, beta, cancellationToken)
            : Maximize(board, depth, alpha, beta, cancellationToken);
    }

    private static float Minimize(Board board, int depth, float alpha, float beta, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (depth == 0)
            return EvaluateSimple(board);

        var res = ProcessMovesMin(board, depth, alpha, ref beta, board.KingsMoves, cancellationToken);
        res = float.Min(res, ProcessMovesMin(board, depth, alpha, ref beta, board.MenMoves, cancellationToken));

        return res;
    }

    private static float ProcessMovesMin(Board board, int depth, float alpha, ref float beta, List<Move> moves,
        CancellationToken cancellationToken)
    {
        var res = 200f;
        foreach (var move in moves)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var newBoard = new Board(board);
            newBoard.MakeMove(move);
            var eval = Maximize(newBoard, depth - 1, alpha, beta, cancellationToken);
            if (eval < res)
                res = eval;
            if (eval < beta)
                beta = eval;
            if (beta <= alpha)
                break;
        }

        return res;
    }

    private static float Maximize(Board board, int depth, float alpha, float beta, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (depth == 0)
            return EvaluateSimple(board);

        var res = ProcessMovesMax(board, depth, ref alpha, beta, board.KingsMoves, cancellationToken);
        res = float.Max(res, ProcessMovesMax(board, depth, ref alpha, beta, board.MenMoves, cancellationToken));

        return res;
    }

    private static float ProcessMovesMax(Board board, int depth, ref float alpha, float beta, List<Move> moves,
        CancellationToken cancellationToken)
    {
        var res = -200f;
        foreach (var move in moves)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var newBoard = new Board(board);
            newBoard.MakeMove(move);
            var eval = Minimize(newBoard, depth - 1, alpha, beta, cancellationToken);
            if (eval > res)
                res = eval;
            if (eval > alpha)
                alpha = eval;
            if (beta <= alpha)
                break;
        }

        return res;
    }

    private static float EvaluateSimple(Board board)
    {
        if (board.IsRedWin) 
            return 200f;
        if (board.IsBlackWin)
            return -200f;

        var res = 0f;
        foreach (var index in board.GetPieceIndexes(Piece.RedMan))
            res += 1 * RowMultipliers[index / 8];
        foreach (var unused in board.GetPieceIndexes(Piece.RedKing))
            res += 2;
        foreach (var index in board.GetPieceIndexes(Piece.BlackMan))
            res -= 1 * RowMultipliers[7 - index / 8];
        foreach (var unused in board.GetPieceIndexes(Piece.BlackKing))
            res -= 2;
        return res;
    }
}