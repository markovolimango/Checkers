using System;
using System.Collections.Generic;
using static Checkers.Models.PieceExtensions;

namespace Checkers.Models;

public class Board
{
    private readonly List<Pos> _capturesBuffer = new(4);
    private List<Pos> _adjacentBuffer = new(4);

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
        LegalMoves = new List<Move>(15);
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
        LegalMoves = new List<Move>(15);
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

    public List<Move> LegalMoves { get; }
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
        _adjacentBuffer.Clear();
        foreach (var dir in piece.GetMoveDirections())
        {
            var targetPos = pos + dir;
            if (IsInBoard(targetPos))
                _adjacentBuffer.Add(targetPos);
        }

        return _adjacentBuffer;
    }

    private List<Pos> FindAdjacent(Pos pos)
    {
        return FindAdjacent(pos, GetPiece(pos));
    }

    private List<Pos> FindPossibleCaptures(Pos pos, Piece piece)
    {
        _adjacentBuffer = FindAdjacent(pos, piece);
        _capturesBuffer.Clear();

        foreach (var adj in _adjacentBuffer)
            if (AreOppositeTeam(piece, GetPiece(adj)))
            {
                var targetPos = adj + (adj - pos);

                if (!IsInBoard(targetPos)) continue;
                if (GetPiece(targetPos) != Piece.Empty) continue;

                _capturesBuffer.Add(adj);
            }

        return _capturesBuffer;
    }

    private List<Pos> FindPossibleCaptures(Pos pos)
    {
        return FindPossibleCaptures(pos, GetPiece(pos));
    }

    private void NonRecursiveDFS(List<Move> moves, Pos initialPos, Piece piece)
    {
        // Stack to store the state of our search
        var stack = new Stack<DFSState>();

        // Initialize with starting position
        stack.Push(new DFSState(
            new List<Pos> { initialPos }, // Initial path with just starting position
            new List<Pos>(), // Empty captures list
            initialPos, // Current position
            0)); // Index to track which capture we're processing

        while (stack.Count > 0)
        {
            var state = stack.Pop();

            // Get possible captures from current position
            var possibleCaptures = FindPossibleCaptures(state.CurrentPos, piece);

            // If no captures possible, add the move and continue
            if (possibleCaptures.Count == 0)
            {
                moves.Add(new Move(state.Path, state.Captures));
                continue;
            }

            // If we've processed all captures for this position, continue to next state
            if (state.CaptureIndex >= possibleCaptures.Count)
                continue;

            // Push the current state back with incremented index
            stack.Push(new DFSState(
                state.Path,
                state.Captures,
                state.CurrentPos,
                state.CaptureIndex + 1));

            // Get the current capture to process
            var capture = possibleCaptures[state.CaptureIndex];

            // Skip if we've already captured this position
            if (state.Captures.Contains(capture))
                continue;

            // Create new lists with the current capture
            var newPath = new List<Pos>(state.Path);
            var newCaptures = new List<Pos>(state.Captures);

            // Calculate the new position after the capture
            var targetPos = capture + (capture - state.CurrentPos);
            newPath.Add(targetPos);
            newCaptures.Add(capture);
            
            stack.Push(new DFSState(
                newPath,
                newCaptures,
                targetPos,
                0)); // Start with first capture at new position
        }
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

            NonRecursiveDFS(LegalMoves, start, piece);
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
    
    private class DFSState
    {
        public DFSState(List<Pos> path, List<Pos> captures, Pos currentPos, int captureIndex)
        {
            Path = path;
            Captures = captures;
            CurrentPos = currentPos;
            CaptureIndex = captureIndex;
        }

        public List<Pos> Path { get; }
        public List<Pos> Captures { get; }
        public Pos CurrentPos { get; }
        public int CaptureIndex { get; }
    }
}