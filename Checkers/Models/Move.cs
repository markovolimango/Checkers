using System.Collections.Generic;

namespace Checkers.Models;

public class Move
{
    public Move(Pos from, Pos to)
    {
        Path = [from, to];
        Captures = [];
    }

    public Move(List<Pos> path, List<Pos> captures)
    {
        Path = path;
        Captures = captures;
    }

    public Move(IReadOnlyList<Pos> path, IReadOnlyList<Pos> captures)
    {
        Path = new List<Pos>(path);
        Captures = new List<Pos>(captures);
    }

    public List<Pos> Path { get; }
    public Pos Start => Path[0];
    public Pos End => Path[Path.Count - 1];
    public List<Pos> Captures { get; }

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