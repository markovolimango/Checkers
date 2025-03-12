using System;

namespace Checkers.Models;

public struct Pos : IEquatable<Pos>
{
    public readonly int Row;
    public readonly int Col;

    public Pos(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public bool IsInKingsRow => Row == 0 || Row == 7;

    public static implicit operator Pos((int row, int col) pos)
    {
        return new Pos(pos.row, pos.col);
    }


    public override string ToString()
    {
        return $"({Row}, {Col})";
    }

    public static Pos operator -(Pos a, Pos b)
    {
        return new Pos(a.Row - b.Row, a.Col - b.Col);
    }

    public static Pos operator +(Pos a, Pos b)
    {
        return new Pos(a.Row + b.Row, a.Col + b.Col);
    }

    public static Pos operator /(Pos a, int b)
    {
        return new Pos(a.Row / b, a.Col / b);
    }

    public static bool operator ==(Pos a, Pos b)
    {
        return a.Row == b.Row && a.Col == b.Col;
    }

    public static bool operator !=(Pos a, Pos b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Col);
    }

    public bool Equals(Pos other)
    {
        return Row == other.Row && Col == other.Col;
    }

    public override bool Equals(object? obj)
    {
        return obj is Pos other && Equals(other);
    }
}