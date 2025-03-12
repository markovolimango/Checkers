using Checkers.Models;

namespace Tests.Models;

[TestFixture]
public class BoardTests
{
    [Test]
    public void NewBoard_StartState_HasCorrectPieces()
    {
        var start = new[,]
        {
            //@formatter:off
            { Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan },
            { Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty },
            { Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan },
            { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty },
            { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty },
            { Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty },
            { Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan },
            { Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty }
            //@formatter:on
        };
        var board = new Board();
        Assert.That(start, Is.EqualTo(board.Pieces));
    }

    [Test]
    public void NewBoard_CustomState_HasCorrectPieces()
    {
        var custom = new[,]
        {
            //@formatter:off
            { Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan },
            { Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.Empty, Piece.Empty, Piece.RedMan, Piece.Empty },
            { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.RedMan },
            { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty },
            { Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.Empty },
            { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.BlackMan, Piece.Empty },
            { Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.Empty, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.Empty },
            { Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty }
            //@formatter:on
        };
        var board = new Board(custom, Team.Red);
        Assert.That(board.Pieces, Is.EqualTo(custom));
    }

    [Test]
    public void NewBoard_InvalidState_ThrowsException()
    {
        var invalid = new[,]
        {
            { Piece.Empty, Piece.Empty, Piece.Empty },
            { Piece.Empty, Piece.Empty, Piece.Empty }
        };
        Assert.Throws<ArgumentException>(() => new Board(invalid, Team.Red));
    }

    [Test]
    public void GetPiece_StartState_ReturnsCorrectPieces()
    {
        var board = new Board();
        Assert.Multiple(() =>
        {
            Assert.That(board.GetPiece(0, 0), Is.EqualTo(Piece.Empty));
            Assert.That(board.GetPiece(1, 1), Is.EqualTo(Piece.Empty));
            Assert.That(board.GetPiece(1, 2), Is.EqualTo(Piece.RedMan));
            Assert.That(board.GetPiece(7, 3), Is.EqualTo(Piece.Empty));
            Assert.That(board.GetPiece(7, 4), Is.EqualTo(Piece.BlackMan));
            Assert.That(board.GetPiece(5, 5), Is.EqualTo(Piece.Empty));
            Assert.That(board.GetPiece(5, 6), Is.EqualTo(Piece.BlackMan));
            Assert.That(board.GetPiece(0, 7), Is.EqualTo(Piece.RedMan));
        });
    }

    [Test]
    public void GetPiece_CustomState_ReturnsCorrectPiece()
    {
        var board = new Board(new[,]
            {
                //@formatter:off
                { Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan },
                { Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.Empty, Piece.Empty, Piece.RedMan, Piece.Empty },
                { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.RedMan },
                { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty },
                { Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.Empty },
                { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.BlackMan, Piece.Empty },
                { Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.Empty, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.Empty },
                { Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty }
                //@formatter:on
            },
            Team.Red);
        Assert.Multiple(() =>
        {
            Assert.That(board.GetPiece(0, 0), Is.EqualTo(Piece.Empty));
            Assert.That(board.GetPiece(1, 1), Is.EqualTo(Piece.Empty));
            Assert.That(board.GetPiece(1, 2), Is.EqualTo(Piece.RedMan));
            Assert.That(board.GetPiece(7, 3), Is.EqualTo(Piece.Empty));
            Assert.That(board.GetPiece(7, 4), Is.EqualTo(Piece.BlackMan));
            Assert.That(board.GetPiece(5, 5), Is.EqualTo(Piece.Empty));
            Assert.That(board.GetPiece(5, 6), Is.EqualTo(Piece.BlackMan));
            Assert.That(board.GetPiece(0, 7), Is.EqualTo(Piece.RedMan));
        });
    }

    [Test]
    public void GetPiece_OutsideOfBoard_ThrowsException()
    {
        var board = new Board();
        Assert.Multiple(() =>
        {
            Assert.That(() => board.GetPiece(-1, 0), Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(() => board.GetPiece(8, 8), Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(() => board.GetPiece((-5, 6)), Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(() => board.GetPiece((200, 0)), Throws.TypeOf<IndexOutOfRangeException>());
        });
    }

    [Test]
    public void FindMovesStartingWith_OneMove_ReturnsCorrectMove()
    {
        var board = new Board();
        List<Move> expected = [new((5, 0), (4, 1))];
        Assert.Multiple(() =>
        {
            Assert.That(board.FindMovesStartingWith([(5, 0)]), Is.EqualTo(expected));
            Assert.That(board.FindMovesStartingWith([(5, 0), (4, 1)]), Is.EqualTo(expected));
        });
    }

    [Test]
    public void FindMovesStartingWith_MultipleMoves_ReturnsCorrectMoves()
    {
        var board = new Board(new[,]
        {
            //@formatter:off
            { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
            { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
            { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
            { Piece.Empty ,Piece.RedMan ,Piece.Empty ,Piece.RedMan ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
            { Piece.Empty ,Piece.Empty ,Piece.BlackMan ,Piece.Empty ,Piece.BlackMan ,Piece.Empty ,Piece.Empty ,Piece.Empty},
            { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
            { Piece.Empty ,Piece.Empty ,Piece.BlackMan ,Piece.Empty ,Piece.BlackMan ,Piece.Empty ,Piece.Empty ,Piece.Empty},
            { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty}
            //@formatter:on
        }, Team.Red);
        List<Move> expected =
            [new([(3, 1), (5, 3), (7, 1)], [(4, 2), (6, 2)]), new([(3, 1), (5, 3), (7, 5)], [(4, 2), (6, 4)])];
        Assert.Multiple(() =>
        {
            Assert.That(board.FindMovesStartingWith([(3, 1)]), Is.EqualTo(expected));
            Assert.That(board.FindMovesStartingWith([(3, 1), (5, 3)]), Is.EqualTo(expected));
        });
    }

    [Test]
    public void FindMovesStartingWith_NoMoves_ReturnsEmpty()
    {
        var board = new Board();
        List<Move> expected = [];
        Assert.Multiple(() =>
        {
            Assert.That(board.FindMovesStartingWith([(0, 0)]), Is.EqualTo(expected));
            Assert.That(board.FindMovesStartingWith([(1, 2)]), Is.EqualTo(expected));
            Assert.That(board.FindMovesStartingWith([(7, 6)]), Is.EqualTo(expected));
        });
    }

    public static IEnumerable<TestCaseData> FindMovesStartingWith_OutsideOfBoard_TestCases()
    {
        yield return new TestCaseData(new List<Pos> { new(-1, 0) });
        yield return new TestCaseData(new List<Pos> { new(8, 11) });
        yield return new TestCaseData(new List<Pos> { new(0, 0), new(20, 3) });
        yield return new TestCaseData(new List<Pos>
            { new(4, 5), new(6, 7), new(2, 2), new(3, 16), new(1, 1) });
    }

    [TestCaseSource(nameof(FindMovesStartingWith_OutsideOfBoard_TestCases))]
    public void FindMovesStartingWith_OutsideOfBoard_ThrowsException(List<Pos> path)
    {
        var board = new Board();
        Assert.Throws<IndexOutOfRangeException>(() => board.FindMovesStartingWith(path));
    }

    public static IEnumerable<TestCaseData> MakeMove_LegalMove_TestCases()
    {
        yield return new TestCaseData(new Board(new[,]
            {
                //@formatter:off
                { Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.Empty },
                { Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty },
                { Piece.Empty, Piece.RedMan, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.RedMan },
                { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty },
                { Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty },
                { Piece.Empty, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty },
                { Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan },
                { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty }
                //@formatter:on
            }, Team.Black),
            new Move([(4, 3), (2, 5), (0, 7)], [(3, 4), (1, 6)]),
            new Board(new[,]
            {
                //@formatter:off
                { Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.BlackKing },
                { Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.Empty, Piece.Empty },
                { Piece.Empty, Piece.RedMan, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.RedMan },
                { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.RedMan, Piece.Empty },
                { Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty },
                { Piece.Empty, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty },
                { Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan },
                { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty }
                //@formatter:on
            }, Team.Red));
        yield return new TestCaseData(new Board(new[,]
            {
                //@formatter:off
                { Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan },
                { Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty },
                { Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan },
                { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty },
                { Piece.Empty, Piece.Empty, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty },
                { Piece.BlackMan, Piece.Empty, Piece.Empty, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty },
                { Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan },
                { Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty }

                //@formatter:on
            }, Team.Red),
            new Move([(2, 3), (3, 4)], []),
            new Board(new[,]
            {
                //@formatter:off
                { Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan },
                { Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan, Piece.Empty },
                { Piece.Empty, Piece.RedMan, Piece.Empty, Piece.Empty, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.RedMan },
                { Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty, Piece.RedMan, Piece.Empty, Piece.Empty, Piece.Empty },
                { Piece.Empty, Piece.Empty, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.Empty, Piece.Empty, Piece.Empty },
                { Piece.BlackMan, Piece.Empty, Piece.Empty, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty },
                { Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan },
                { Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty, Piece.BlackMan, Piece.Empty }
                //@formatter:on
            }, Team.Black));
    }

    [TestCaseSource(nameof(MakeMove_LegalMove_TestCases))]
    public void MakeMove_LegalMove_CorrectlyModifiesBoard(Board startState, Move move, Board expectedState)
    {
        startState.MakeMove(move);
        Assert.That(startState.Pieces, Is.EqualTo(expectedState.Pieces));
    }
}