using Checkers.Models;

namespace Checkers.Bot;

public class Bot
{
    private static readonly float[] RowMultipliers = [0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f, 1.3f, 1.4f];

    public float Evaluate(Board board, int depth)
    {
        if (depth == 0)
            return Evaluate(board);

        float res;
        if (board.IsBlackTurn)
        {
            res = float.MaxValue;
            foreach (var move in board.KingsMoves)
            {
                var newBoard = new Board(board);
                newBoard.MakeMove(move);
                var eval = Evaluate(newBoard, depth - 1);
                if (eval < res)
                    res = eval;
            }

            foreach (var move in board.MenMoves)
            {
                var newBoard = new Board(board);
                newBoard.MakeMove(move);
                var eval = Evaluate(newBoard, depth - 1);
                if (eval < res)
                    res = eval;
            }
        }
        else
        {
            res = float.MinValue;
            foreach (var move in board.KingsMoves)
            {
                var newBoard = new Board(board);
                newBoard.MakeMove(move);
                var eval = Evaluate(newBoard, depth - 1);
                if (eval > res)
                    res = eval;
            }

            foreach (var move in board.MenMoves)
            {
                var newBoard = new Board(board);
                newBoard.MakeMove(move);
                var eval = Evaluate(newBoard, depth - 1);
                if (eval > res)
                    res = eval;
            }
        }

        return res;
    }

    private float Evaluate(Board board)
    {
        if (board.IsRedWin)
            return 200;
        if (board.IsBlackWin)
            return -200;

        var res = 0f;
        foreach (var index in board.GetPieceIndexes(Piece.RedMan))
            res += 1 * RowMultipliers[index / 8];
        foreach (var index in board.GetPieceIndexes(Piece.RedKing))
            res += 2;
        foreach (var index in board.GetPieceIndexes(Piece.BlackMan))
            res -= 1 * RowMultipliers[7 - index / 8];
        foreach (var index in board.GetPieceIndexes(Piece.BlackKing))
            res -= 2;
        return res;
    }
}