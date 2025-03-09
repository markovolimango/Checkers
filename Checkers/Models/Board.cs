using System;
using System.Collections.Generic;

namespace Checkers.Models;

public class Board
{
    private readonly Piece[,] _pieces;
    private List<Move> _legalMoves;
    private int _turn = -1;

    public Board()
    {
        _pieces = new Piece[8, 8];
        for (var row = 0; row < 3; row++)
        for (var col = 1 - row % 2; col < 8; col += 2)
            _pieces[row, col] = Piece.Red;

        for (var row = 5; row < 8; row++)
        for (var col = 1 - row % 2; col < 8; col += 2)
            _pieces[row, col] = Piece.Black;

        _legalMoves = FindAllLegalMoves();
    }

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

    public bool MakeMove(Move? move)
    {
        if (move is null)
            return false;

        Console.WriteLine($"Now making move {move}");
        var piece = GetPiece(move.First);
        RemovePiece(move.First);
        PutPiece(move.Last, piece);
        foreach (var pos in move.Captures)
            RemovePiece(pos);
        _legalMoves = FindAllLegalMoves();
        return true;
    }

    public bool IsInBoard(Pos pos)
    {
        return pos.Row >= 0 && pos.Row < 8 && pos.Col >= 0 && pos.Col < 8;
    }

    private List<Move> FindAllLegalMoves()
    {
        var legalMoves = new List<Move>();
        var captureMoves = new List<Move>();
        var nonCaptureMoves = new List<Move>();

        for (var row = 0; row < 8; row++)
        for (var col = 0; col < 8; col++)
        {
            var from = new Pos(row, col);
            var piece = GetPiece(from);
            var pieceMoveDir = Math.Sign((int)piece);
            if (pieceMoveDir != _turn)
                continue;
            var to = new Pos(row + pieceMoveDir, col - 1);
            if (IsInBoard(to))
            {
                var targetPiece = GetPiece(to);
                if (targetPiece == Piece.Empty)
                {
                    nonCaptureMoves.Add(new Move(from, to));
                }
                else if (Math.Sign((int)targetPiece) != pieceMoveDir)
                {
                    var capture = to;
                    to += new Pos(pieceMoveDir, -1);
                    if (IsInBoard(to) && GetPiece(to) == Piece.Empty)
                        captureMoves.Add(new Move(from, to, capture));
                }
            }

            to = new Pos(row + pieceMoveDir, col + 1);
            if (IsInBoard(to))
            {
                var targetPiece = GetPiece(to);
                if (targetPiece == Piece.Empty)
                {
                    nonCaptureMoves.Add(new Move(from, to));
                }
                else if (Math.Sign((int)targetPiece) != pieceMoveDir)
                {
                    var capture = to;
                    to += new Pos(pieceMoveDir, +1);
                    if (IsInBoard(to) && GetPiece(to) == Piece.Empty)
                        captureMoves.Add(new Move(from, to, capture));
                }
            }
        }

        if (captureMoves.Count > 0)
            legalMoves = captureMoves;
        else legalMoves = nonCaptureMoves;

        Console.WriteLine("Legal Moves:");
        foreach (var move in legalMoves) Console.WriteLine(move.ToString());
        _turn *= -1;
        return legalMoves;
    }
}