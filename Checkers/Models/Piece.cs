using System;
using System.Collections.Generic;

namespace Checkers.Models;

public enum Piece
{
    Empty = 0,
    BlackMan = -1,
    BlackKing = -2,
    RedMan = 1,
    RedKing = 2
}

public static class PieceExtensions
{
    public static Team GetTeam(this Piece piece)
    {
        if (Math.Sign((int)piece) == -1)
            return Team.Black;
        if (Math.Sign((int)piece) == 1)
            return Team.Red;
        return Team.Empty;
    }

    public static bool IsSameTeam(Piece piece1, Piece piece2)
    {
        return piece1.GetTeam() == piece2.GetTeam();
    }

    public static bool IsMan(this Piece piece)
    {
        return piece == Piece.BlackMan || piece == Piece.RedMan;
    }

    public static bool IsKing(this Piece piece)
    {
        return piece == Piece.BlackKing || piece == Piece.RedKing;
    }

    public static List<Pos> GetMoveDirections(this Piece piece)
    {
        var moveDirections = new List<Pos>();
        if (piece == Piece.Empty)
            return moveDirections;

        var moveDir = (int)piece.GetTeam();
        moveDirections.Add((moveDir, -1));
        moveDirections.Add((moveDir, 1));
        if (piece.IsMan())
            return moveDirections;

        moveDirections.Add((-moveDir, -1));
        moveDirections.Add((-moveDir, 1));
        return moveDirections;
    }
}