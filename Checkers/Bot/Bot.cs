using System;
using Checkers.Models;

namespace Checkers.Bot;

public class Bot
{
    private const float WinValue = 200f;
    private static readonly float[] Multipliers = [0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f, 1.3f, 1.4f];

    public float Evaluate(Board board, int depth)
    {
        if (board.Winner != Team.Empty)
            return board.Winner == Team.Red ? WinValue : -WinValue;

        if (depth == 0)
            return Evaluate(board);

        var res = board.CurrentTurn == Team.Red ? float.MinValue : float.MaxValue;

        foreach (var move in board.LegalMoves)
        {
            var newBoard = new Board(board);
            newBoard.MakeMove(move);
            var newRes = Evaluate(newBoard, depth - 1);
            if (newBoard.CurrentTurn == Team.Red)
            {
                if (newRes < res) res = newRes;
            }
            else
            {
                if (newRes > res) res = newRes;
            }
        }

        return res;
    }

    public float Evaluate(Board board)
    {
        float res = 0;
        for (var row = 0; row < 8; row++)
        for (var col = 0; col < 8; col++)
        {
            var piece = board[row, col];
            if (piece == Piece.Empty) continue;
            var team = piece.GetTeam();

            var value = piece.GetValue();
            if (piece.IsMan())
            {
                var multiplier = team == Team.Red ? Multipliers[row] : Multipliers[7 - row];
                value *= multiplier;
            }

            res += value;
        }

        return MathF.Round(res, 4);
    }
}