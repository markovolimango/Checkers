using System;
using System.Collections.Generic;
using static Checkers.Models.PieceExtensions;

namespace Checkers.Models;

public class Board
{
    private readonly Piece[,] _pieces;
    private List<Move> _legalMoves;

    public Board()
    {
        _pieces = new Piece[8, 8];
        for (var row = 0; row < 3; row++)
        for (var col = 1 - row % 2; col < 8; col += 2)
            _pieces[row, col] = Piece.RedMan;

        for (var row = 5; row < 8; row++)
        for (var col = 1 - row % 2; col < 8; col += 2)
            _pieces[row, col] = Piece.BlackMan;

        CurrentTurn = Team.Black;
        _legalMoves = FindAllLegalMoves();
    }

    public Team CurrentTurn { get; private set; }

    public Piece GetPiece(Pos pos)
    {
        return _pieces[pos.Row, pos.Col];
    }

    private void PutPiece(Pos pos, Piece piece)
    {
        _pieces[pos.Row, pos.Col] = piece;
    }

    private void RemovePiece(Pos pos)
    {
        _pieces[pos.Row, pos.Col] = Piece.Empty;
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
    }

    private List<Pos> FindAllCaptures(Pos pos)
    {
        var piece = GetPiece(pos);
        var pieceMoveDir = (int)piece;
        List<Pos> adjacent = [pos + (pieceMoveDir, -1), pos + (pieceMoveDir, 1)];
        var captures = new List<Pos>();
        foreach (var adj in adjacent)
            if (!IsSameTeam(GetPiece(adj), piece))
                captures.Add(adj);
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
            Pos first = (row, col);
            var piece = GetPiece(first);
            if (piece.GetTeam() != CurrentTurn)
                continue;

            foreach (var dir in piece.GetMoveDirections())
            {
                List<Pos> path = [first];
                var second = path[0] + dir;
                if (!IsInBoard(second))
                    continue;

                var targetPiece = GetPiece(second);
                if (targetPiece.GetTeam() == piece.GetTeam())
                    continue;

                if (!hasFoundCaptureMove)
                {
                    if (targetPiece == Piece.Empty)
                    {
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

                        moves.Clear();
                        path.Add(second);
                        moves.Add(new Move(path, captures));
                        hasFoundCaptureMove = true;
                    }
                }
                else
                {
                    if (targetPiece == Piece.Empty)
                        continue;

                    List<Pos> captures = [second];
                    second += dir;
                    if (!IsInBoard(second))
                        continue;

                    targetPiece = GetPiece(second);
                    if (targetPiece != Piece.Empty)
                        continue;

                    path.Add(second);
                    moves.Add(new Move(path, captures));
                }
            }
        }

        Console.WriteLine("Legal Moves:");
        foreach (var move in moves) Console.WriteLine(move.ToString());
        return moves;
    }
}