namespace Checkers.Models;

public enum Piece : sbyte
{
    Empty = 0,
    BlackMan = -1,
    BlackKing = -2,
    RedMan = 1,
    RedKing = 2
}

public static class PieceExtensions
{
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

    public static float GetValue(this Piece piece)
    {
        return (int)piece;
    }
}