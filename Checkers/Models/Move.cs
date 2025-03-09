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

    public Move(List<Pos> path, List<Pos> captures)
    {
        Path = path;
        Captures = captures;
    }

    public List<Pos> Path { get; set; }
    public Pos First => Path[0];
    public Pos Last => Path[Path.Count - 1];
    public List<Pos> Captures { get; set; }

    public override string ToString()
    {
        var res = "";
        foreach (var pos in Path)
            res += $"{pos} ->";
        res = res.Remove(res.Length - 2);
        if (Captures.Count > 0)
        {
            res += ", capturing: ";
            foreach (var pos in Captures)
                res += $"{pos} ";
        }

        return res;
    }
}