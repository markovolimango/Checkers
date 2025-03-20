using System;
using System.Collections.Generic;

namespace Checkers.Models;

public class Move : IEquatable<Move>
{
    public Move(List<byte> path, ulong captures)
    {
        Path = new List<byte>(path);
        Captures = captures;
    }

    public Move(List<(int row, int col)> path)
    {
        Path = new List<byte>(path.Count);
        foreach (var pos in path)
            Path.Add(Board.ToIndex(pos.row, pos.col));
        Captures = 0;
    }

    public List<byte> Path { get; }
    public ulong Captures { get; }
    public byte Start => Path[0];
    public byte End => Path[^1];


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