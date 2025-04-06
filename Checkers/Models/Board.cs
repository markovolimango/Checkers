using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Checkers.Models;

public class Board
{
    private const ulong TopEdge = 0x00000000000000FF;
    private const ulong BottomEdge = 0xFF00000000000000;
    private const ulong LeftEdge = 0x0101010101010101;
    private const ulong RightEdge = 0x8080808080808080;
    private const ulong Edges = TopEdge | BottomEdge | LeftEdge | RightEdge;

    private readonly ulong[] _pieces = new ulong[5];

    public readonly List<Move> KingsMoves;
    public readonly List<Move> MenMoves;
    private bool _hasFoundCapture;

    /// <summary>
    ///     Creates a board with the default starting state.
    /// </summary>
    public Board()
    {
        _pieces[(byte)Piece.Empty] = 0xAA55AAFFFF55AA55;
        _pieces[(byte)Piece.WhiteMan] = 0x0000000000AA55AA;
        _pieces[(byte)Piece.RedMan] = 0x55AA550000000000;
        _pieces[(byte)Piece.WhiteKing] = 0x0000000000000000;
        _pieces[(byte)Piece.RedKing] = 0x0000000000000000;
        IsWhiteTurn = false;
        MenMoves = new List<Move>(10);
        KingsMoves = new List<Move>(10);
        FindAllLegalMoves();
    }

    /// <summary>
    ///     Creates a copy af another board.
    /// </summary>
    public Board(Board other)
    {
        for (var i = 0; i < 5; i++)
            _pieces[i] = other._pieces[i];
        IsWhiteTurn = other.IsWhiteTurn;
        MenMoves = new List<Move>(other.MenMoves);
        KingsMoves = new List<Move>(other.KingsMoves);
    }

    /// <summary>
    ///     Creates a board from a predefined state (used for testing)
    /// </summary>
    /// <param name="pieces">Matrix representing the board state, each number corresponds to the piece enum's values</param>
    /// <param name="isWhiteTurn">Bool describing whose turn it is</param>
    public Board(byte[,] pieces, bool isWhiteTurn)
    {
        for (var row = 0; row < 8; row++)
        for (var col = 0; col < 8; col++)
            this[row, col] = (Piece)pieces[row, col];
        IsWhiteTurn = isWhiteTurn;
        MenMoves = new List<Move>(10);
        KingsMoves = new List<Move>(10);
        FindAllLegalMoves();
    }

    public bool IsWhiteTurn { get; private set; }
    private ulong WhitePieces => _pieces[(byte)Piece.WhiteMan] | _pieces[(byte)Piece.WhiteKing];
    private ulong RedPieces => _pieces[(byte)Piece.RedMan] | _pieces[(byte)Piece.RedKing];

    /// <summary>
    /// The fastest indexer, returns a piece from its bitmask directly, use this one when possible
    /// </summary>
    public Piece this[ulong mask]
    {
        get
        {
            for (byte i = 0; i < 5; i++)
                if ((_pieces[i] & mask) != 0)
                    return (Piece)i;
            throw new IndexOutOfRangeException("Invalid mask");
        }
        private set
        {
            var piece = (byte)value;
            if (((mask & TopEdge) != 0 && value == Piece.RedMan) ||
                ((mask & BottomEdge) != 0 && value == Piece.WhiteMan))
                piece++;
            var inverse = ~mask;
            for (byte i = 0; i < 5; i++)
                _pieces[i] &= inverse;
            _pieces[piece] |= mask;
        }
    }

    /// <summary>
    /// The index represents where the set bit should be in the bitmask.
    /// </summary>
    public Piece this[byte index]
    {
        get => this[1UL << index];
        private set => this[1UL << index] = value;
    }

    /// <summary>
    /// Slowest indexer because of conversion. Indexing starts at the top left with (0, 0)
    /// </summary>
    public Piece this[int row, int col]
    {
        get => this[ToMask(row, col)];
        private set => this[ToMask(row, col)] = value;
    }

    public static ulong ToMask(int row, int col)
    {
        return 1UL << (row * 8 + col);
    }

    public static byte ToIndex(ulong mask)
    {
        return (byte)BitOperations.TrailingZeroCount(mask);
    }

    public static byte ToIndex(int row, int col)
    {
        return (byte)(row * 8 + col);
    }

    public static (int row, int col) ToPos(ulong mask)
    {
        var index = ToIndex(mask);
        return (index / 8, index % 8);
    }

    public static (int row, int col) ToPos(byte index)
    {
        return (index / 8, index % 8);
    }

    /// <summary>
    /// Finds all squares that a piece looks at when trying to find a move.
    /// </summary>
    /// <returns>Bitmask, with each set bit being one square to look at</returns>
    private ulong FindTargetSquares(ulong mask, Piece piece)
    {
        ulong res = 0;
        if ((mask & TopEdge) == 0 && piece != Piece.WhiteMan)
        {
            if ((mask & LeftEdge) == 0)
                res |= mask >> 9;
            if ((mask & RightEdge) == 0)
                res |= mask >> 7;
        }

        if ((mask & BottomEdge) == 0 && piece != Piece.RedMan)
        {
            if ((mask & LeftEdge) == 0)
                res |= mask << 7;
            if ((mask & RightEdge) == 0)
                res |= mask << 9;
        }

        if (piece is Piece.WhiteMan or Piece.WhiteKing)
            return res & ~WhitePieces;
        return res & ~RedPieces;
    }

    /// <summary>
    /// Finds all (could be none) possible jumps for a piece from a destination.
    /// </summary>
    /// <param name="targets">The targets found with FindTargetSquares, passed as a parameter to avoid redundant calls</param>
    /// <returns>Pair of destinations (where the piece ends up) and captures (what the piece jumps over)</returns>
    private (ulong destinations, ulong captures) FindJumps(ulong mask, Piece piece, ulong targets)
    {
        ulong captures = 0, destinations = 0;
        if (!(piece == Piece.WhiteMan || piece == Piece.WhiteKing))
            targets &= WhitePieces;
        else
            targets &= RedPieces;

        foreach (var enemyTarget in GetPieceMasks(targets))
        {
            if ((enemyTarget & Edges) != 0)
                continue;
            var dir = ToIndex(enemyTarget) - ToIndex(mask);
            ulong dest = 0;
            if (dir >= 0)
                dest = (enemyTarget << dir) & _pieces[(byte)Piece.Empty];
            else
                dest = (enemyTarget >> -dir) & _pieces[(byte)Piece.Empty];
            if (dest != 0)
            {
                destinations |= dest;
                captures |= enemyTarget;
            }
        }

        return (destinations, captures);
    }

    /// <summary>
    /// DFS like function that finds and adds every full capture move to the moves list.
    /// </summary>
    /// <param name="moves">The list to which the moves get added</param>
    /// <param name="path">Where the piece has jumped so far</param>
    /// <param name="captures">What the piece has captured so far</param>
    /// <param name="mask">Starting location of the piece</param>
    /// <param name="piece">The piece that's finding moves</param>
    private void AddCaptureMoves(List<Move> moves, List<byte> path, ulong captures, ulong mask, Piece piece)
    {
        AddCaptureMoves(FindJumps(mask, piece, FindTargetSquares(mask, piece)), moves, path, captures, piece);
    }

    /// <summary>
    /// DFS like function that finds and adds every full capture move to the moves list.
    /// </summary>
    /// <param name="jumps">Possible jumps found with the FindJumps function</param>
    /// <param name="moves">The list to which the moves get added</param>
    /// <param name="path">Where the piece has jumped so far</param>
    /// <param name="captures">What the piece has captured so far</param>
    /// <param name="piece">The piece that's finding moves</param>
    private void AddCaptureMoves((ulong destinations, ulong captures) jumps, List<Move> moves, List<byte> path, ulong captures,
        Piece piece)
    {
        var hasFoundCapture = false;
        foreach (var jump in GetPieceMasks(jumps))
        {
            if ((captures & jump.captures) != 0)
                continue;
            hasFoundCapture = true;
            path.Add(ToIndex(jump.destinations));
            var myCaptures = captures | jump.captures;
            AddCaptureMoves(moves, path, myCaptures, jump.destinations, piece);
            path.RemoveAt(path.Count - 1);
        }

        if (!hasFoundCapture)
            moves.Add(new Move(path, captures));
    }

    /// <summary>
    /// Finds every legal move in the position and adds them to the MenMoves and KingsMoves lists
    /// </summary>
    private void FindAllLegalMoves()
    {
        _hasFoundCapture = false;
        if (IsWhiteTurn)
        {
            FindLegalMoves(_pieces[(byte)Piece.WhiteKing], Piece.WhiteKing, KingsMoves);
            FindLegalMoves(_pieces[(byte)Piece.WhiteMan], Piece.WhiteMan, MenMoves);
        }
        else
        {
            FindLegalMoves(_pieces[(byte)Piece.RedKing], Piece.RedKing, KingsMoves);
            FindLegalMoves(_pieces[(byte)Piece.RedMan], Piece.RedMan, MenMoves);
        }
    }

    /// <summary>
    /// Finds all legal moves for a piece type
    /// </summary>
    private void FindLegalMoves(ulong pieces, Piece piece, List<Move> moves)
    {
        moves.Clear();
        foreach (var mask in GetPieceMasks(pieces))
        {
            var targets = FindTargetSquares(mask, piece);
            var jumps = FindJumps(mask, piece, targets);
            if (jumps.destinations == 0)
            {
                if (_hasFoundCapture)
                    continue;
                foreach (var emptyTarget in GetPieceMasks(targets & _pieces[(byte)Piece.Empty]))
                    moves.Add(new Move([ToIndex(mask), ToIndex(emptyTarget)], 0));
            }
            else
            {
                if (!_hasFoundCapture)
                {
                    MenMoves.Clear();
                    KingsMoves.Clear();
                    _hasFoundCapture = true;
                }

                AddCaptureMoves(jumps, moves, [ToIndex(mask)], 0, piece);
            }
        }

        moves.Sort((a, b) => b.CompareTo(a));
    }

    
    public List<Move> FindMovesStartingWith(List<byte> path)
    {
        return FindMovesStartingWith(path, KingsMoves).Concat(FindMovesStartingWith(path, MenMoves)).ToList();
    }

    public List<Move> FindMovesStartingWith(List<byte> path, List<Move> moves)
    {
        var res = new List<Move>(3);
        var len = path.Count;
        foreach (var move in moves)
        {
            if (move.Path.Count < path.Count)
                continue;
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

    public void MakeMove(Move move)
    {
        var piece = this[move.Start];
        this[move.Start] = Piece.Empty;
        this[move.End] = piece;
        this[move.Captures] = Piece.Empty;
        IsWhiteTurn = !IsWhiteTurn;
        FindAllLegalMoves();
    }

    public static IEnumerable<ulong> GetPieceMasks(ulong pieces)
    {
        while (pieces != 0)
        {
            yield return pieces & ~(pieces - 1);
            pieces &= pieces - 1;
        }
    }

    public static IEnumerable<(ulong destinations, ulong captures)> GetPieceMasks((ulong destinations, ulong captures) pieces)
    {
        while (pieces.destinations != 0 && pieces.captures != 0)
        {
            yield return (pieces.destinations & ~(pieces.destinations - 1), pieces.captures & ~(pieces.captures - 1));
            pieces.destinations &= pieces.destinations - 1;
            pieces.captures &= pieces.captures - 1;
        }
    }

    public static IEnumerable<byte> GetPieceIndexes(ulong pieces)
    {
        while (pieces != 0)
        {
            yield return ToIndex(pieces);
            pieces &= pieces - 1;
        }
    }

    public IEnumerable<byte> GetPieceIndexes(Piece piece)
    {
        var pieces = _pieces[(byte)piece];
        while (pieces != 0)
        {
            yield return ToIndex(pieces & ~(pieces - 1));
            pieces &= pieces - 1;
        }
    }

    public override string ToString()
    {
        var res = "";
        for (var row = 0; row < 8; row++)
        {
            res += $"Row {row}: {ToChar(this[row, 0])}";
            for (var col = 1; col < 8; col++) res += $", {ToChar(this[row, col])}";

            res += "\n";
        }

        return res;
    }

    private char ToChar(Piece piece)
    {
        switch (piece)
        {
            case Piece.Empty:
                return '.';
            case Piece.RedMan:
                return 'r';
            case Piece.RedKing:
                return 'R';
            case Piece.WhiteMan:
                return 'w';
            case Piece.WhiteKing:
                return 'W';
        }

        throw new ArgumentException();
    }
}