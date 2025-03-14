using System;
using System.Collections.Generic;

namespace Checkers.Models;

public class Move : IEquatable<Move>
{
    /// <summary>
    ///     Constructs a simple move with no captures.
    /// </summary>
    /// <param name="from">The position from which the piece starts</param>
    /// <param name="to">THe position the piece moves to.</param>
    public Move(Pos from, Pos to)
    {
        Path = new List<Pos>(2) { from, to };
        Captures = new List<Pos>(0);
    }

    /// <summary>
    ///     Constructs either a simple or jump move.
    /// </summary>
    /// <param name="path">A list of positions the piece stepped on</param>
    /// <param name="captures">A list of the pieces that were jumped over</param>
    public Move(List<Pos> path, List<Pos> captures)
    {
        if (path.Count != captures.Count + 1)
            if (path.Count != 2 || captures.Count != 0)
                throw new ArgumentException("Path list must have exactly one more element than captures.");
        Path = path;
        Captures = captures;
    }

    /// <summary>
    ///     Constructs either a simple or jump move.
    /// </summary>
    /// <param name="path">A list of positions the piece stepped on</param>
    /// <param name="captures">A list of the pieces that were jumped over</param>
    public Move(IReadOnlyList<Pos> path, IReadOnlyList<Pos> captures)
    {
        if (path.Count != captures.Count + 1)
            throw new ArgumentException("Path list must have exactly one more element than captures.");
        Path = new List<Pos>(path);
        Captures = new List<Pos>(captures);
    }

    public List<Pos> Path { get; }
    public Pos Start => Path[0];
    public Pos End => Path[^1];
    public List<Pos> Captures { get; }

    public bool Equals(Move? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Path.Count != other.Path.Count || Captures.Count != other.Captures.Count)
            return false;
        for (var i = 0; i < Captures.Count; i++)
            if (Path[i] != other.Path[i] || Captures[i] != other.Captures[i])
                return false;
        return Path[^1] == other.Path[^1];
    }

    public static bool operator ==(Move move1, Move move2)
    {
        return move1.Equals(move2);
    }

    public static bool operator !=(Move move1, Move move2)
    {
        return !(move1 == move2);
    }

    public override bool Equals(object? obj)
    {
        return obj is Move move && Equals(move);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            foreach (var pos in Path)
                hash = hash * 31 + pos.GetHashCode();
            foreach (var pos in Captures)
                hash = hash * 31 + pos.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        var res = "";
        foreach (var pos in Path)
            res += $"{pos} -> ";
        res = res.Remove(res.Length - 3);
        if (Captures.Count > 0)
        {
            res += ", capturing: ";
            foreach (var pos in Captures)
                res += $"{pos} ";
        }

        return res;
    }
}