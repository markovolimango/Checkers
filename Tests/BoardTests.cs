using Checkers.Models;

namespace Tests;

[TestFixture]
public class BoardTests
{
    private static IEnumerable<TestCaseData> MaskIndexer_GetsCorrectPiece_TestData()
    {
        var board = new Board();
        yield return new TestCaseData(board, 0x0000000000000002UL, Piece.BlackMan);
        yield return new TestCaseData(board, 0x0008000000000000UL, Piece.RedMan);
        yield return new TestCaseData(board, 0x0004000000000000UL, Piece.Empty);
        yield return new TestCaseData(board, 0x8008000000000000UL, Piece.Empty);
    }

    [TestCaseSource(nameof(MaskIndexer_GetsCorrectPiece_TestData))]
    public void MaskIndexer_GetsCorrectPiece(Board board, ulong mask, Piece piece)
    {
        Assert.That(board[mask], Is.EqualTo(piece));
    }

    private static IEnumerable<TestCaseData> IndexIndexer_GetsCorrectPiece_Invalid_TestData()
    {
        var board = new Board();
        yield return new TestCaseData(board, (byte)1, Piece.BlackMan);
        yield return new TestCaseData(board, (byte)51, Piece.RedMan);
        yield return new TestCaseData(board, (byte)50, Piece.Empty);
        yield return new TestCaseData(board, (byte)63, Piece.Empty);
    }

    [TestCaseSource(nameof(IndexIndexer_GetsCorrectPiece_Invalid_TestData))]
    public void IndexIndexer_GetsCorrectPiece(Board board, byte index, Piece piece)
    {
        Assert.That(board[index], Is.EqualTo(piece));
    }

    private static IEnumerable<TestCaseData> PosIndexer_GetsCorrectPiece_Invalid_TestData()
    {
        var board = new Board();
        yield return new TestCaseData(board, 0, 1, Piece.BlackMan);
        yield return new TestCaseData(board, 6, 3, Piece.RedMan);
        yield return new TestCaseData(board, 6, 2, Piece.Empty);
        yield return new TestCaseData(board, 7, 7, Piece.Empty);
    }

    [TestCaseSource(nameof(PosIndexer_GetsCorrectPiece_Invalid_TestData))]
    public void PosIndexer_GetsCorrectPiece(Board board, int row, int col, Piece piece)
    {
        Assert.That(board[row,col], Is.EqualTo(piece));
    }

    [Test]
    public void FindAllLegalMoves_FindsCorrectMoves()
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