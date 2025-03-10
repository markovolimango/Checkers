using System;
using System.Collections.Generic;
using static Checkers.Models.PieceExtensions;

namespace Checkers.Models;

public class Board
{
    private List<Move> _legalMoves;

    public Board()
    {
        Pieces = new Piece[8, 8];
        for (var row = 0; row < 3; row++)
        for (var col = 1 - row % 2; col < 8; col += 2)
            Pieces[row, col] = Piece.RedMan;

        for (var row = 5; row < 8; row++)
        for (var col = 1 - row % 2; col < 8; col += 2)
            Pieces[row, col] = Piece.BlackMan;

        CurrentTurn = Team.Black;
        _legalMoves = FindAllLegalMoves();
    }

    public Board(Piece[,] pieces, Team currentTurn)
    {
        Pieces = new Piece[8, 8];
        for (var row = 0; row < 8; row++)
        for (var col = 0; col < 8; col++)
            Pieces[row, col] = pieces[row, col];

        CurrentTurn = currentTurn;
        _legalMoves = FindAllLegalMoves();
    }

    public Piece[,] Pieces { get; }

    public Team CurrentTurn { get; private set; }

    public Piece GetPiece(Pos pos)
    {
        if (!IsInBoard(pos))
            throw new IndexOutOfRangeException($"Position {pos} is outside of an 8x8 board.");
        return Pieces[pos.Row, pos.Col];
    }

    public Piece GetPiece(int row, int col)
    {
        return Pieces[row, col];
    }

    private void PutPiece(Pos pos, Piece piece)
    {
        Pieces[pos.Row, pos.Col] = piece;
    }

    private void RemovePiece(Pos pos)
    {
        Pieces[pos.Row, pos.Col] = Piece.Empty;
    }

    public Move? FindMove(Pos from, Pos to)
    {
        foreach (var move in _legalMoves)
            if (move.Path[0] == from && move.Path[1] == to)
                return move;

        return null;
    }

    public void MakeMove(Move? move)
    {
        if (move is null)
            return;

        Console.WriteLine($"Now making move {move}");
        var piece = GetPiece(move.Start);
        RemovePiece(move.Start);
        PutPiece(move.End, piece);
        foreach (var pos in move.Captures)
            RemovePiece(pos);
        CurrentTurn = (Team)(-(int)CurrentTurn);
        _legalMoves = FindAllLegalMoves();
        //Console.WriteLine($"{this}\n\n");
    }

    private List<Pos> FindAdjacent(Pos pos)
    {
        var piece = GetPiece(pos);
        var adjacent = new List<Pos>();
        foreach (var dir in piece.GetMoveDirections())
        {
            var targetPos = pos + dir;
            if (IsInBoard(targetPos))
                adjacent.Add(targetPos);
        }

        return adjacent;
    }

    private List<Pos> FindAllCaptures(Pos pos)
    {
        var piece = GetPiece(pos);
        var adjacent = FindAdjacent(pos);

        var captures = new List<Pos>();
        foreach (var adj in adjacent)
            if (AreOppositeTeam(piece, GetPiece(adj)))
            {
                var targetPos = pos + (adj - pos);

                if (!IsInBoard(targetPos)) continue;
                if (GetPiece(targetPos) != Piece.Empty) continue;

                captures.Add(adj);
            }

        return captures;
    }

    private static bool IsInBoard(Pos pos)
    {
        return pos.Row >= 0 && pos.Row < 8 && pos.Col >= 0 && pos.Col < 8;
    }

    private List<Move> FindAllLegalMoves()
    {
        var moves = new List<Move>();
        var hasFoundCaptureMove = false;

        for (var row = 0; row < 8; row++)
        for (var col = 0; col < 8; col++)
        {
            Pos start = (row, col);
            var piece = GetPiece(start);
            if (piece.GetTeam() != CurrentTurn)
                continue;

            foreach (var dir in piece.GetMoveDirections())
            {
                List<Pos> path = [start];
                var second = path[0] + dir;
                if (!IsInBoard(second))
                    continue;

                var targetPiece = GetPiece(second);
                if (targetPiece.GetTeam() == piece.GetTeam())
                    continue;

                if (targetPiece == Piece.Empty)
                {
                    if (hasFoundCaptureMove)
                        continue;

                    path.Add(second);
                    moves.Add(new Move(path));
                }
                else
                {
                    List<Pos> captures = [second];
                    second += dir;
                    if (!IsInBoard(second))
                        continue;

                    targetPiece = GetPiece(second);
                    if (targetPiece != Piece.Empty)
                        continue;

                    if (!hasFoundCaptureMove)
                    {
                        moves.Clear();
                        hasFoundCaptureMove = true;
                    }

                    path.Add(second);
                    moves.Add(new Move(path, captures));
                }
            }
        }

        Console.WriteLine("Legal Moves:");
        foreach (var move in moves) Console.WriteLine(move.ToString());
        return moves;
    }

    public override string ToString()
    {
        var res = "";
        for (var row = 0; row < 8; row++)
        {
            res += "{ ";
            for (var col = 0; col < 8; col++)
                res += $"Piece.{GetPiece(row, col)}, ";
            res = res.Remove(res.Length - 2);
            res += " },\n";
        }

        res = res.Remove(res.Length - 2);
        return res;
    }
}