namespace Checkers.Models;

public enum Piece : byte
{
    Empty = 0,
    BlackMan = 3,
    BlackKing = 4,
    RedMan = 1,
    RedKing = 2
}

public static class PieceExtensions
{
    public static bool IsBlack(this Piece piece)
    {
        return (byte)piece > 2;
    }
}