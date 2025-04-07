using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Checkers.Models.Board;

namespace Checkers.Models;

public class Engine
{
    private const int MaxDepth = 20;
    private static readonly float[] RowMultipliers = [0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f, 1.3f, 1.4f];

    /// <summary>
    ///     Finds the best move with a set time limit
    /// </summary>
    public Move? FindBestMoveWithTimeLimit(Board.Board board, int timeLimitMs)
    {
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

                if (sw.ElapsedMilliseconds > timeLimitMs)
                    break;
            }
        }
        catch (OperationCanceledException)
        {
        }

        sw.Stop();
        return bestMove;
    }

    /// <summary>
    ///     Finds the best move with a set depth, used only internaly
    /// </summary>
    private Move? FindBestMoveWithDepth(Board.Board board, int depth,
        CancellationToken cancellationToken)
    {
        var allMoves = new List<Move>();
        allMoves.AddRange(board.KingsMoves);
        allMoves.AddRange(board.MenMoves);
        var foundMoves = new ConcurrentBag<(float Score, Move Move)>();

        Parallel.ForEach(allMoves, new ParallelOptions
            { CancellationToken = cancellationToken, MaxDegreeOfParallelism = Environment.ProcessorCount }, move =>
        {
            var newBoard = new Board.Board(board);
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

    /// <summary>
    ///     Evaluates a position
    /// </summary>
    private float Evaluate(Board.Board board, int depth, float alpha, float beta, CancellationToken cancellationToken)
    {
        return board.IsWhiteTurn
            ? Minimize(board, depth, alpha, beta, cancellationToken)
            : Maximize(board, depth, alpha, beta, cancellationToken);
    }

    /// <summary>
    ///     Minimizing part of the minimax algotithm
    /// </summary>
    private float Minimize(Board.Board board, int depth, float alpha, float beta, CancellationToken cancellationToken)
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

            var newBoard = new Board.Board(board);
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

    /// <summary>
    ///     Maximizing part of the minimax algorithm
    /// </summary>
    private float Maximize(Board.Board board, int depth, float alpha, float beta, CancellationToken cancellationToken)
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

            var newBoard = new Board.Board(board);
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

    /// <summary>
    ///     Evaluates a position with zero depth, just counting pieces and how far along are they
    /// </summary>
    private static float EvaluateSimple(Board.Board board)
    {
        if (board.KingsMoves.Count == 0 && board.MenMoves.Count == 0)
        {
            if (board.IsWhiteTurn)
                return 1000;
            return -1000;
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