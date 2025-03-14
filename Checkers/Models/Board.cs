using System;
using System.Collections.Generic;
using System.Linq;
using static Checkers.Models.PieceExtensions;

namespace Checkers.Models;

public class Board
{
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
        NumberOfPieces[Team.Black] = 12;
        NumberOfPieces[Team.Red] = 12;
        FindAllLegalMoves();
    }

    public Board(Piece[,] pieces, Team currentTurn)
    {
        if (pieces.GetLength(0) != 8 || pieces.GetLength(1) != 8)
            throw new ArgumentException("Invalid board size.");

        Pieces = new Piece[8, 8];
        for (var row = 0; row < 8; row++)
        for (var col = 0; col < 8; col++)
        {
            var piece = pieces[row, col];
            Pieces[row, col] = piece;
            if (piece != Piece.Empty)
                NumberOfPieces[piece.GetTeam()]++;
        }

        CurrentTurn = currentTurn;
        FindAllLegalMoves();
    }

    public Board(Board other)
    {
        Pieces = new Piece[8, 8];
        for (var row = 0; row < 8; row++)
        for (var col = 0; col < 8; col++)
            Pieces[row, col] = other[row, col];
        CurrentTurn = other.CurrentTurn;
        NumberOfPieces[Team.Black] = other.NumberOfPieces[Team.Black];
        NumberOfPieces[Team.Red] = other.NumberOfPieces[Team.Red];
        LegalMoves = new List<Move>(other.LegalMoves);
    }

    public List<Move> LegalMoves { get; } = new List<Move>(15);
    public Piece[,] Pieces { get; }
    public Piece this[int row, int col] => Pieces[row, col];
    public Piece this[Pos pos] => Pieces[pos.Row, pos.Col];
    public Team CurrentTurn { get; private set; }

    private Dictionary<Team, int> NumberOfPieces { get; } =
        new() { { Team.Black, 0 }, { Team.Red, 0 }, { Team.Empty, 0 } };

    public Team Winner { get; private set; }

    private static bool IsInBoard(Pos pos)
    {
        return pos.Row >= 0 && pos.Row < 8 && pos.Col >= 0 && pos.Col < 8;
    }

    private static bool IsInBoard(List<Pos> path)
    {
        foreach (var pos in path)
            if (!IsInBoard(pos))
                return false;
        return true;
    }

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

    private void AddCaptureMoves(List<Move> moves, IReadOnlyList<Pos> path, IReadOnlyList<Pos> captures, Pos pos,
        Piece piece)
    {
        var possibleCaptures = FindPossibleCaptures(pos, piece);

        var i = 0;
        foreach (var capture in possibleCaptures)
        {
            if (captures.Contains(capture)) continue;

            i++;
            var myPath = new List<Pos>(path);
            var myCaptures = new List<Pos>(captures);

            var targetPos = capture + (capture - pos);
            myPath.Add(targetPos);
            myCaptures.Add(capture);
            AddCaptureMoves(moves, myPath, myCaptures, targetPos, piece);
        }

        if (i == 0)
            moves.Add(new Move(path, captures));
    }

    private void FindAllLegalMoves()
    {
        LegalMoves.Clear();
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
                        LegalMoves.Add(new Move(start, end));
                continue;
            }

            if (!hasFoundCaptureMove)
            {
                LegalMoves.Clear();
                hasFoundCaptureMove = true;
            }

            AddCaptureMoves(LegalMoves, [start], [], start, piece);
        }
    }

    public List<Move> FindMovesStartingWith(List<Pos> path, List<Move> moves)
    {
        if (!IsInBoard(path)) throw new IndexOutOfRangeException();

        var res = new List<Move>();
        var len = path.Count;

        foreach (var move in moves)
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

    public List<Move> FindMovesStartingWith(List<Pos> path)
    {
        return FindMovesStartingWith(path, LegalMoves);
    }

    public void MakeMove(Move move)
    {
        var piece = GetPiece(move.Start);
        RemovePiece(move.Start);
        PutPiece(move.End, piece);
        if (move.End.Row == 7 || move.End.Row == 0)
            PutPiece(move.End, piece.Promote());
        foreach (var pos in move.Captures)
            RemovePiece(pos);
        CurrentTurn = (Team)(-(int)CurrentTurn);
        FindAllLegalMoves();
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