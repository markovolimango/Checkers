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
    private static readonly Dictionary<Piece, Pos[]> MoveDirections = new()
    {
        { Piece.Empty, [] },
        { Piece.BlackMan, [(-1, -1), (-1, 1)] },
        { Piece.BlackKing, [(-1, -1), (-1, 1), (1, -1), (1, 1)] },
        { Piece.RedMan, [(1, -1), (1, 1)] },
        { Piece.RedKing, [(1, -1), (1, 1), (-1, -1), (-1, 1)] }
    };

    public static Team GetTeam(this Piece piece)
    {
        if (piece == Piece.BlackMan || piece == Piece.BlackKing)
            return Team.Black;
        if (piece == Piece.RedMan || piece == Piece.RedKing)
            return Team.Red;
        return Team.Empty;
    }

    public static bool AreSameTeam(Piece piece1, Piece piece2)
    {
        return piece1.GetTeam() == piece2.GetTeam();
    }

    public static bool AreOppositeTeam(Piece piece1, Piece piece2)
    {
        return (int)piece1.GetTeam() == -(int)piece2.GetTeam();
    }

    public static bool IsMan(this Piece piece)
    {
        return piece == Piece.BlackMan || piece == Piece.RedMan;
    }

    public static bool IsKing(this Piece piece)
    {
        return piece == Piece.BlackKing || piece == Piece.RedKing;
    }

    public static Piece Promote(this Piece piece)
    {
        if (piece == Piece.BlackMan)
            return Piece.BlackKing;
        if (piece == Piece.RedMan)
            return Piece.RedKing;
        return piece;
    }

    public static Pos[] GetMoveDirections(this Piece piece)
    {
        return MoveDirections[piece];
    }

    public static float GetValue(this Piece piece)
    {
        return (int)piece;
    }
}