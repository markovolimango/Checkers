using Checkers.Models;

namespace Tests;

[TestFixture]
public class BoardTests
{
    [Test]
    public void FindAllLegalMoves_MultipleMultipleChoiceLongMoves_FindsCorrectMoves()
    {
        var board = new Board(new byte[,]
        {
            { 0, 0, 0, 0, 0, 3, 0, 3 },
            { 3, 0, 0, 0, 3, 0, 3, 0 },
            { 0, 3, 0, 3, 0, 3, 0, 3 },
            { 3, 0, 0, 0, 3, 0, 0, 0 },
            { 0, 1, 0, 1, 0, 0, 0, 0 },
            { 1, 0, 0, 0, 1, 0, 1, 0 },
            { 0, 1, 0, 1, 0, 1, 0, 1 },
            { 0, 0, 1, 0, 0, 0, 1, 0 }
        }, true);
        List<Move> menMoves =
        [
            new([(3, 0), (5, 2), (7, 0)]),
            new([(3, 0), (5, 2), (7, 4)]),
            new([(3, 4), (5, 2), (7, 0)]),
            new([(3, 4), (5, 2), (7, 4)])
        ];
        Assert.That(menMoves.SequenceEqual(board.MenMoves));
    }
}