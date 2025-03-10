using System;
using System.Collections.Generic;

namespace Checkers.Models;

public class Move
{
    public Move(Pos from, Pos to)
    {
        if ((to - from).Magnitude2 != 2)
            throw new Exception("Invalid move");
        Path = [from, to];
        Captures = [];
    }

    public Move(Pos from, Pos to, Pos capture)
    {
        if ((to - from).Magnitude2 != 8)
            throw new Exception("Invalid move");
        Path = [from, to];
        Captures = [capture];
    }

    public Move(List<Pos> path)
    {
        if (path.Count != 2)
            throw new Exception($"Invalid move with path size {path[0]}, {path[1]}, {path[2]}.");
        if ((path[1] - path[0]).Magnitude2 != 2)
            throw new Exception("Invalid move");
        Path = path;
        Captures = [];
    }

    public Move(List<Pos> path, List<Pos> captures)
    {
        Path = path;
        Captures = captures;
    }

    public Move(IReadOnlyList<Pos> path, IReadOnlyList<Pos> captures)
    {
        Path = path;
        Captures = captures;
    }

    public IReadOnlyList<Pos> Path { get; }
    public Pos Start => Path[0];
    public Pos End => Path[Path.Count - 1];
    public IReadOnlyList<Pos> Captures { get; }

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