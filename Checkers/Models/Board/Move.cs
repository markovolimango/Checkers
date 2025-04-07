using System;
using System.Collections.Generic;

namespace Checkers.Models.Board;

public class Move : IEquatable<Move>, IComparable<Move>
{
    /// <summary>
    ///     Creates a move from a list of jumped-to pieces and a bitmask of captures pieces.
    /// </summary>
    public Move(List<byte> path, ulong captures)
    {
        Path = new List<byte>(path);
        Captures = captures;
    }

    /// <summary>
    ///     Used in tests to create the expected move from an easier to read path
    /// </summary>
    /// <param name="path"></param>
    public Move(List<(int row, int col)> path)
    {
        Path = new List<byte>(path.Count) { Board.ToIndex(path[0].row, path[0].col) };
        for (var i = 1; i < path.Count; i++)
        {
            Path.Add(Board.ToIndex(path[i].row, path[i].col));
            Captures |= Board.ToMask((path[i].row + path[i - 1].row) / 2, (path[i].col + path[i - 1].col) / 2);
        }
    }

    public List<byte> Path { get; }
    public ulong Captures { get; }
    public byte Start => Path[0];
    public byte End => Path[^1];

    public int CompareTo(Move? other)
    {
        if (other is null)
            return 1;
        return Path.Count.CompareTo(other.Path.Count);
    }

    public bool Equals(Move? other)
    {
        if (other is null || Path.Count != other.Path.Count)
            return false;

        for (var i = 0; i < Path.Count; i++)
            if (Path[i] != other.Path[i])
                return false;

        return true;
    }

    public override string ToString()
    {
        var res = "";
        foreach (var index in Path)
            res += $"{Board.ToPos(index)} -> ";
        res = res.Remove(res.Length - 4, 4);
        if (Captures == 0)
            return res;
        res += " capturing ";
        foreach (var capture in Board.GetPieceMasks(Captures))
            res += $"{Board.ToPos(capture)}, ";
        return res.Remove(res.Length - 2, 2);
    }

    public override bool Equals(object? obj)
    {
        return obj is Move other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Path, Captures);
    }

    public static bool operator ==(Move a, Move b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Move a, Move b)
    {
        return !a.Equals(b);
    }
}