using Checkers.Models;

namespace Checkers.Bot;

public class Bot
{
    public static float Evaluate(Board board, int depth)
    {
        if(depth == 0)
            return Evaluate(board);
        return 0;
    }

    private static float Evaluate(Board board)
    {
        if (board.IsRedWin)
            return 200;
        if (board.IsBlackWin)
            return -200;
        
        var res = 0f;
        foreach (var index in board.GetPieceIndexes(Piece.RedMan))
            res += 1;
        foreach (var index in board.GetPieceIndexes(Piece.RedKing))
            res += 2;
        foreach (var index in board.GetPieceIndexes(Piece.BlackMan))
            res -= 1;
        foreach (var index in board.GetPieceIndexes(Piece.BlackKing))
            res -= 2;
        return res;
    }
}