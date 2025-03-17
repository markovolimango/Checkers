using System;
using System.Collections.Generic;
using System.Numerics;
using Console = System.Console;

namespace Checkers.Models;

public class Board
{
    private const ulong TopEdge = 0x00000000000000FF;
    private const ulong BottomEdge = 0xFF00000000000000;
    private const ulong LeftEdge = 0x0101010101010101;
    private const ulong RightEdge = 0x8080808080808080;
    private const ulong Edges = TopEdge | BottomEdge | LeftEdge | RightEdge;
    private readonly List<Move> _legalMoves = new(15);

    private ulong _blackKings;
    private ulong _blackMen;
    private ulong _emptySquares;

    private ulong _redKings;
    private ulong _redMen;

    public Board()
    {
        _emptySquares = 0xAA55AAFFFF55AA55;
        _blackMen = 0x0000000000AA55AA;
        _redMen = 0x55AA550000000000;
        _blackKings = 0x0000000000000000;
        _redKings = 0x0000000000000000;
        IsBlackTurn = false;
        FindAllLegalMoves();
    }

    public bool IsBlackTurn { get; private set; }
    private ulong BlackPieces => _blackMen & _blackKings;
    private ulong RedPieces => _redMen & _redKings;

    public Piece this[ulong mask]
    {
        get
        {
            if ((mask & (mask - 1)) != 0)
                throw new IndexOutOfRangeException("Invalid mask");
            if ((_emptySquares & mask) != 0) return Piece.Empty;
            if ((_blackMen & mask) != 0) return Piece.BlackMan;
            if ((_redMen & mask) != 0) return Piece.RedMan;
            if ((_blackKings & mask) != 0) return Piece.BlackKing;
            if ((_redKings & mask) != 0) return Piece.RedKing;
            throw new IndexOutOfRangeException("Invalid mask");
        }
        set
        {
            _emptySquares &= ~mask;
            _blackMen &= ~mask;
            _redMen &= ~mask;
            _blackKings &= ~mask;
            _redKings &= ~mask;

            switch (value)
            {
                case Piece.Empty:
                    _emptySquares |= mask;
                    break;
                case Piece.BlackMan:
                    _blackMen |= mask;
                    break;
                case Piece.RedMan:
                    _redMen |= mask;
                    break;
                case Piece.BlackKing:
                    _blackKings |= mask;
                    break;
                case Piece.RedKing:
                    _redKings |= mask;
                    break;
            }
        }
    }

    public Piece this[byte index]
    {
        get => this[1UL << index];
        set => this[1UL << index] = value;
    }

    public Piece this[int row, int col]
    {
        get => this[ToMask(row, col)];
        set => this[ToMask(row, col)] = value;
    }

    public static ulong ToMask(int row, int col)
    {
        return 1UL << (row * 8 + col);
    }

    public static byte ToIndex(ulong mask)
    {
        return (byte)BitOperations.TrailingZeroCount(mask);
    }

    public static (int row, int col) ToPos(ulong mask)
    {
        var index = BitOperations.TrailingZeroCount(mask);
        return (index / 8, index % 8);
    }

    public static (int row, int col) ToPos(byte index)
    {
        return (index / 8, index % 8);
    }

    private ulong FindTargetSquares(ulong mask, Piece piece)
    {
        if (piece == Piece.Empty)
            return 0;

        ulong res = 0;
        if ((mask & TopEdge) == 0 && piece != Piece.BlackMan)
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

        if ((sbyte)piece < 0)
            return res & ~_blackMen & ~_blackKings;
        return res & ~_redMen & ~_redKings;
    }

    private (ulong destinations, ulong captures) FindJumps(ulong mask, Piece piece, ulong targets)
    {
        ulong captures = 0, destinations = 0;
        if ((sbyte)piece > 0)
            targets &= _blackMen;
        else
            targets &= _redMen;

        foreach (var enemyTarget in GetPieceMasks(targets))
        {
            if ((enemyTarget & Edges) != 0)
                continue;
            var dir = BitOperations.TrailingZeroCount(enemyTarget) - BitOperations.TrailingZeroCount(mask);
            ulong t = 0;
            if (dir >= 0)
                t = (enemyTarget << dir) & _emptySquares;
            else
                t = (enemyTarget >> -dir) & _emptySquares;
            if (t != destinations)
            {
                destinations |= t;
                captures |= enemyTarget;
            }
        }

        return (destinations, captures);
    }

    private void DFS(List<Move> moves, List<byte> path, ulong captures, ulong mask, Piece piece)
    {
        var jumps = FindJumps(mask, piece, FindTargetSquares(mask, piece));

        var hasFoundCapture = false;
        foreach (var jump in GetPieceMasks(jumps))
        {
            if ((captures & jump.first) != 0)
                continue;
            hasFoundCapture = true;
            var myPath = new List<byte>(path);
            myPath.Add(ToIndex(jump.first));
            var myCaptures = captures | jump.second;
            DFS(moves, myPath, myCaptures, jump.first, piece);
        }

        if (!hasFoundCapture) moves.Add(new Move(path, captures));
    }

    private void FindAllLegalMoves()
    {
        _legalMoves.Clear();
        Console.WriteLine("All legal moves:");
        if (IsBlackTurn)
        {
            _legalMoves.AddRange(FindLegalMoves(_blackMen, Piece.BlackMan));
            _legalMoves.AddRange(FindLegalMoves(_blackKings, Piece.BlackKing));
        }
        else
        {
            _legalMoves.AddRange(FindLegalMoves(_redMen, Piece.RedMan));
            _legalMoves.AddRange(FindLegalMoves(_redKings, Piece.RedKing));
        }
    }

    private List<Move> FindLegalMoves(ulong pieces, Piece piece)
    {
        List<Move> res = new(10);
        var hasFoundCapture = false;
        foreach (var mask in GetPieceMasks(pieces))
        {
            var targets = FindTargetSquares(mask, piece);
            var jumps = FindJumps(mask, piece, targets);
            if (jumps.destinations == 0)
            {
                if (hasFoundCapture)
                    continue;
                foreach (var emptyTarget in GetPieceMasks(targets & _emptySquares))
                    res.Add(new Move([ToIndex(mask), ToIndex(emptyTarget)], 0));
            }
            else
            {
                if (!hasFoundCapture)
                {
                    res.Clear();
                    hasFoundCapture = true;
                }

                Console.WriteLine("DFS: ");
                DFS(res, [ToIndex(mask)], 0, mask, piece);
            }
        }

        Console.WriteLine("Moves:");
        foreach (var move in res)
            Console.WriteLine(move);
        return res;
    }

    public List<Move> FindMovesStartingWith(List<byte> path)
    {
        return FindMovesStartingWith(path, _legalMoves);
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
        IsBlackTurn = !IsBlackTurn;
        FindAllLegalMoves();
    }

    public static IEnumerable<ulong> GetPieceMasks(ulong pieces)
    {
        while (pieces != 0)
        {
            yield return pieces & ~(pieces - 1);
            ;
            pieces &= pieces - 1;
        }
    }

    public static IEnumerable<(ulong first, ulong second)> GetPieceMasks((ulong first, ulong second) pieces)
    {
        while (pieces.first != 0 && pieces.second != 0)
        {
            yield return (pieces.first & ~(pieces.first - 1), pieces.second & ~(pieces.second - 1));
            pieces.first &= pieces.first - 1;
            pieces.second &= pieces.second - 1;
        }
    }

    public static IEnumerable<byte> GetPieceIndexes(ulong pieces)
    {
        while (pieces != 0)
        {
            yield return (byte)BitOperations.TrailingZeroCount(pieces);
            pieces &= pieces - 1;
        }
    }

    private bool CheckIfValid()
    {
        if ((_emptySquares & _blackMen & _redMen & _blackKings & _redKings) != 0x0000000000000000) return false;
        if ((_emptySquares | _blackMen | _redMen | _blackKings | _redKings) != 0xFFFFFFFFFFFFFFFF) return false;
        return true;
    }
}