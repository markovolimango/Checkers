using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Checkers.Models;

namespace Checkers.Engine;

public class Engine
{
    private const int MaxDepth = 20;
    private static readonly float[] RowMultipliers = [0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f, 1.3f, 1.4f];

    public Move? FindBestMoveWithTimeLimit(Board board, int timeLimitMs)
    {
        Console.WriteLine($"FindBestMoveWithTimeLimit: {timeLimitMs}");
        var sw = new Stopwatch();
        sw.Start();
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(timeLimitMs);

        Move? bestMove = null;

        try
        {
            for (var depth = 1; depth <= MaxDepth; depth++)
            {
                var move = FindBestMoveWithDepth(board, depth, cts.Token);
                bestMove = move;

                Console.WriteLine(
                    $"Depth {depth} completed. Time elapsed: {sw.Elapsed.TotalMilliseconds}ms");

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
        return bestMove;
    }

    private Move? FindBestMoveWithDepth(Board board, int depth,
        CancellationToken cancellationToken)
    {
        var allMoves = new List<Move>();
        allMoves.AddRange(board.KingsMoves);
        allMoves.AddRange(board.MenMoves);
        var foundMoves = new ConcurrentBag<(float Score, Move Move)>();

        Parallel.ForEach(allMoves, new ParallelOptions
            { CancellationToken = cancellationToken, MaxDegreeOfParallelism = Environment.ProcessorCount }, move =>
        {
            var newBoard = new Board(board);
            newBoard.MakeMove(move);
            var score = Evaluate(newBoard, depth - 1, float.MinValue, float.MaxValue, cancellationToken);

            foundMoves.Add((score, move));
        });

        Move? res = null;
        var bestScore = board.IsWhiteTurn ? 200f : -200f;

        if (board.IsWhiteTurn)
        {
            foreach (var (score, move) in foundMoves)
                if (score < bestScore)
                {
                    bestScore = score;
                    res = move;
                }
        }
        else
        {
            foreach (var (score, move) in foundMoves)
                if (score > bestScore)
                {
                    bestScore = score;
                    res = move;
                }
        }

        return res;
    }

    private float Evaluate(Board board, int depth, float alpha, float beta, CancellationToken cancellationToken)
    {
        return board.IsWhiteTurn
            ? Minimize(board, depth, alpha, beta, cancellationToken)
            : Maximize(board, depth, alpha, beta, cancellationToken);
    }

    private float Minimize(Board board, int depth, float alpha, float beta, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (depth == 0)
            return EvaluateSimple(board);
        var moves = new List<Move>();
        moves.AddRange(board.KingsMoves);
        moves.AddRange(board.MenMoves);

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

    private float Maximize(Board board, int depth, float alpha, float beta, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (depth == 0)
            return EvaluateSimple(board);

        var moves = new List<Move>();
        moves.AddRange(board.KingsMoves);
        moves.AddRange(board.MenMoves);

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

    private float EvaluateSimple(Board board)
    {
        if (board.KingsMoves.Count == 0 && board.MenMoves.Count == 0)
        {
            if (board.IsWhiteTurn)
                return 200;
            return -200;
        }

        var res = 0f;
        foreach (var index in board.GetPieceIndexes(Piece.RedMan))
            res += 1 * RowMultipliers[index / 8];
        foreach (var unused in board.GetPieceIndexes(Piece.RedKing))
            res += 2;
        foreach (var index in board.GetPieceIndexes(Piece.WhiteMan))
            res -= 1 * RowMultipliers[7 - index / 8];
        foreach (var unused in board.GetPieceIndexes(Piece.WhiteKing))
            res -= 2;
        return res;
    }
}