using System;
using System.Collections.Generic;
using System.Linq;
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
        if (pieces.GetLength(0) != 8 || pieces.GetLength(1) != 8)
            throw new ArgumentException("Invalid board size.");

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

    public List<Move> FindMovesStartingWith(List<Pos> path)
    {
        if (!IsInBoard(path))
            throw new IndexOutOfRangeException();

        var res = new List<Move>();
        var len = path.Count;

        foreach (var move in _legalMoves)
        {
            if (move.Path.Count < len) continue;

            var i = 0;
            while (i < len)
            {
                if (move.Path[i] != path[i])
                    break;
                i++;
            }

            if (i == len)
                res.Add(move);
        }

        return res;
    }

    private bool IsLegalMove(Move move)
    {
        return _legalMoves.Contains(move);
    }

    public void MakeMove(Move move)
    {
        if (!IsLegalMove(move))
            throw new ArgumentException("Invalid move");

        Console.WriteLine($"Now making move {move}");
        var piece = GetPiece(move.Start);
        RemovePiece(move.Start);
        PutPiece(move.End, piece);
        if (move.End.Row == 7 || move.End.Row == 0)
            PutPiece(move.End, piece.Promote());
        foreach (var pos in move.Captures)
            RemovePiece(pos);
        CurrentTurn = (Team)(-(int)CurrentTurn);
        _legalMoves = FindAllLegalMoves();
    }

    private List<Pos> FindAdjacent(Pos pos, Piece piece)
    {
        var adjacent = new List<Pos>();
        foreach (var dir in piece.GetMoveDirections())
        {
            var targetPos = pos + dir;
            if (IsInBoard(targetPos))
                adjacent.Add(targetPos);
        }

        return adjacent;
    }

    private List<Pos> FindAdjacent(Pos pos)
    {
        return FindAdjacent(pos, GetPiece(pos));
    }

    private List<Pos> FindPossibleCaptures(Pos pos, Piece piece)
    {
        var adjacent = FindAdjacent(pos, piece);
        var possibleCaptures = new List<Pos>();

        foreach (var adj in adjacent)
            if (AreOppositeTeam(piece, GetPiece(adj)))
            {
                var targetPos = adj + (adj - pos);

                if (!IsInBoard(targetPos)) continue;
                if (GetPiece(targetPos) != Piece.Empty) continue;

                possibleCaptures.Add(adj);
            }

        return possibleCaptures;
    }

    private List<Pos> FindPossibleCaptures(Pos pos)
    {
        return FindPossibleCaptures(pos, GetPiece(pos));
    }

    private bool IsInBoard(Pos pos)
    {
        return pos.Row >= 0 && pos.Row < 8 && pos.Col >= 0 && pos.Col < 8;
    }

    private bool IsInBoard(List<Pos> path)
    {
        foreach (var pos in path)
            if (!IsInBoard(pos))
                return false;
        return true;
    }

    private void AddCaptureMoves(List<Move> moves, IReadOnlyList<Pos> path, IReadOnlyList<Pos> captures, Pos pos,
        Piece piece)
    {
        var possibleCaptures = FindPossibleCaptures(pos, piece);
        if (possibleCaptures.Count == 0)
        {
            moves.Add(new Move(path, captures));
            return;
        }

        foreach (var capture in possibleCaptures)
        {
            if (captures.Contains(capture)) continue;

            var myPath = new List<Pos>(path);
            var myCaptures = new List<Pos>(captures);

            var targetPos = capture + (capture - pos);
            myPath.Add(targetPos);
            myCaptures.Add(capture);
            AddCaptureMoves(moves, myPath, myCaptures, targetPos, piece);
        }
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

            var possibleCaptures = FindPossibleCaptures(start);
            if (possibleCaptures.Count == 0)
            {
                if (hasFoundCaptureMove) continue;

                foreach (var end in FindAdjacent(start))
                    if (GetPiece(end) == Piece.Empty)
                        moves.Add(new Move(start, end));
                continue;
            }

            if (!hasFoundCaptureMove)
            {
                moves.Clear();
                hasFoundCaptureMove = true;
            }

            AddCaptureMoves(moves, [start], [], start, piece);
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