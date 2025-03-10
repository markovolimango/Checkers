using Checkers.Models;

namespace Tests.Models;

[TestFixture]
public class BoardTests
{
    [Test]
    public void NewBoard_StartState_HasCorrectPieces()
    {
        var expected = new Piece[8, 8]
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
        Assert.That(expected, Is.EqualTo(board.Pieces));
    }

    [Test]
    public void NewBoard_CustomState_HasCorrectPieces()
    {
        var expected = new Piece[8, 8]
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
        var board = new Board(expected, Team.Red);
        Assert.That(board.Pieces, Is.EqualTo(expected));
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
        var board = new Board(new Piece[8, 8]
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
}