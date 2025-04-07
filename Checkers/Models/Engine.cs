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
    private const float WinScore = 1000;
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
                var best = FindBestMoveAndScoreWithDepth(board, depth, cts.Token);
                bestMove = best.move;
                if (best.score >= WinScore || best.score <= -WinScore)
                    break;

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
    ///     Finds the best move and score with a set depth, used only internally
    /// </summary>
    private (Move? move, float score) FindBestMoveAndScoreWithDepth(Board.Board board, int depth,
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
        var bestScore = board.IsWhiteTurn ? WinScore : -WinScore;

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

        if (board.IsDraw)
            bestScore = 0;
        return (res, bestScore);
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

        if (board.IsDraw)
            return 0f;
        var res = WinScore;
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

        if (board.IsDraw)
            return 0f;
        var res = -WinScore;
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
        if (board.IsDraw)
            return 0f;
        if (board.KingsMoves.Count == 0 && board.MenMoves.Count == 0)
        {
            if (board.IsWhiteTurn)
                return WinScore;
            return -WinScore;
        }

        var res = 0f;
        var numberOfPieces = new int[5];

        foreach (var index in board.GetPieceIndexes(Piece.RedMan))
        {
            res += 1 * RowMultipliers[index / 8];
            numberOfPieces[(byte)Piece.RedMan]++;
        }

        foreach (var index in board.GetPieceIndexes(Piece.RedKing))
        {
            res += 3;
            numberOfPieces[(byte)Piece.RedKing]++;
        }

        foreach (var index in board.GetPieceIndexes(Piece.WhiteMan))
        {
            res -= 1 * RowMultipliers[7 - index / 8];
            numberOfPieces[(byte)Piece.WhiteMan]++;
        }

        foreach (var index in board.GetPieceIndexes(Piece.WhiteKing))
        {
            res -= 3;
            numberOfPieces[(byte)Piece.WhiteKing]++;
        }

        if (numberOfPieces[(byte)Piece.WhiteKing] + numberOfPieces[(byte)Piece.WhiteMan] <= 4 ||
            numberOfPieces[(byte)Piece.RedKing] + numberOfPieces[(byte)Piece.RedMan] <= 4)
        {
            if (numberOfPieces[(byte)Piece.WhiteKing] > numberOfPieces[(byte)Piece.RedKing])
            {
                res -= CalculateWhiteDistanceBonus(board, numberOfPieces);
            }
            else if (numberOfPieces[(byte)Piece.WhiteKing] < numberOfPieces[(byte)Piece.RedKing])
            {
                res += CalculateRedDistanceBonus(board, numberOfPieces);
            }
            else
            {
                if (numberOfPieces[(byte)Piece.WhiteMan] > numberOfPieces[(byte)Piece.RedMan])
                    res -= CalculateWhiteDistanceBonus(board, numberOfPieces);
                if (numberOfPieces[(byte)Piece.WhiteMan] < numberOfPieces[(byte)Piece.RedMan])
                    res += CalculateRedDistanceBonus(board, numberOfPieces);
            }
        }

        return res;
    }

    /// <summary>
    ///     Rewards aggression if red has better pieces
    /// </summary>
    private static float CalculateRedDistanceBonus(Board.Board board, int[] numberOfPieces)
    {
        var totalAvgDistance = 0f;
        foreach (var from in board.GetPiecePositions(Piece.WhiteKing))
        {
            var avgDistance = 0f;
            foreach (var to in board.GetPiecePositions(Piece.RedKing))
                avgDistance += Math.Abs(from.row - to.row) + Math.Abs(from.col - to.col);
            foreach (var to in board.GetPiecePositions(Piece.RedMan))
                avgDistance += Math.Abs(from.row - to.row) + Math.Abs(from.col - to.col);
            avgDistance /= numberOfPieces[(byte)Piece.WhiteKing] + numberOfPieces[(byte)Piece.WhiteMan];
            totalAvgDistance += avgDistance;
        }

        totalAvgDistance /= numberOfPieces[(byte)Piece.RedKing];
        if (totalAvgDistance >= 4)
            return (8 - totalAvgDistance) / 2;
        return 0f;
    }

    /// <summary>
    ///     Rewards aggression if white has better pieces
    /// </summary>
    private static float CalculateWhiteDistanceBonus(Board.Board board, int[] numberOfPieces)
    {
        var totalAvgDistance = 0f;
        foreach (var from in board.GetPiecePositions(Piece.RedKing))
        {
            var avgDistance = 0f;
            foreach (var to in board.GetPiecePositions(Piece.WhiteKing))
                avgDistance += Math.Abs(from.row - to.row) + Math.Abs(from.col - to.col);
            foreach (var to in board.GetPiecePositions(Piece.WhiteMan))
                avgDistance += Math.Abs(from.row - to.row) + Math.Abs(from.col - to.col);
            avgDistance /= numberOfPieces[(byte)Piece.RedKing] + numberOfPieces[(byte)Piece.RedMan];
            totalAvgDistance += avgDistance;
        }

        totalAvgDistance /= numberOfPieces[(byte)Piece.WhiteKing];
        if (totalAvgDistance >= 4)
            return (8 - totalAvgDistance) / 2;
        return 0f;
    }
}