using System.Collections.Generic;

namespace Checkers.Models.Board;

public class PositionTracker
{
    private readonly Dictionary<string, int> _history;

    public PositionTracker()
    {
        _history = new Dictionary<string, int>();
    }

    public PositionTracker(PositionTracker other)
    {
        _history = new Dictionary<string, int>(other._history);
    }

    public void Add(string hashString)
    {
        if (_history.ContainsKey(hashString))
            _history[hashString]++;
        else
            _history.Add(hashString, 1);
    }

    public int Get(string hashString)
    {
        if (_history.ContainsKey(hashString))
            return _history[hashString];
        return 0;
    }
}