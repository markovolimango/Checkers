using Checkers.Models.Board;

namespace Tests;

[TestFixture]
public class BoardTests
{
    private static IEnumerable<TestCaseData> MaskIndexer_GetsCorrectPiece_TestData()
    {
        var board = new Board();
        yield return new TestCaseData(board, 0x0000000000000002UL, Piece.WhiteMan);
        yield return new TestCaseData(board, 0x0008000000000000UL, Piece.RedMan);
        yield return new TestCaseData(board, 0x0004000000000000UL, Piece.Empty);
        yield return new TestCaseData(board, 0x8008000000000000UL, Piece.Empty);
    }

    private static IEnumerable<TestCaseData> IndexIndexer_GetsCorrectPiece_Invalid_TestData()
    {
        var board = new Board();
        yield return new TestCaseData(board, (byte)1, Piece.WhiteMan);
        yield return new TestCaseData(board, (byte)51, Piece.RedMan);
        yield return new TestCaseData(board, (byte)50, Piece.Empty);
        yield return new TestCaseData(board, (byte)63, Piece.Empty);
    }

    private static IEnumerable<TestCaseData> PosIndexer_GetsCorrectPiece_Invalid_TestData()
    {
        var board = new Board();
        yield return new TestCaseData(board, 0, 1, Piece.WhiteMan);
        yield return new TestCaseData(board, 6, 3, Piece.RedMan);
        yield return new TestCaseData(board, 6, 2, Piece.Empty);
        yield return new TestCaseData(board, 7, 7, Piece.Empty);
    }

    private static IEnumerable<TestCaseData> FindAllLegalMoves_FindsCorrectMoves_TestData()
    {
        yield return new TestCaseData(new Board(
                "Row 0: ., ., ., ., ., w, ., w\n" +
                "Row 0: w, ., ., ., w, ., w, .\n" +
                "Row 0: ., w, ., w, ., w, ., w\n" +
                "Row 0: w, ., ., ., w, ., ., .\n" +
                "Row 0: ., r, ., r, ., ., ., .\n" +
                "Row 0: r, ., ., ., r, ., r, .\n" +
                "Row 0: ., r, ., r, ., r, ., r\n" +
                "Row 0: ., ., r, ., ., ., r, .\n",
                true),
            new List<Move>(),
            new List<Move>
            {
                new([(3, 0), (5, 2), (7, 0)]),
                new([(3, 0), (5, 2), (7, 4)]),
                new([(3, 4), (5, 2), (7, 0)]),
                new([(3, 4), (5, 2), (7, 4)])
            });

        yield return new TestCaseData(new Board(
                "Row 0: ., w, ., w, ., w, ., w\n" +
                "Row 1: w, ., w, ., w, ., w, .\n" +
                "Row 2: ., ., ., w, ., w, ., w\n" +
                "Row 3: ., ., ., ., ., ., ., .\n" +
                "Row 4: ., ., ., ., ., r, ., .\n" +
                "Row 5: r, ., ., ., w, ., r, .\n" +
                "Row 6: ., r, ., r, ., r, ., r\n" +
                "Row 7: r, ., r, ., r, ., r, .\n",
                false),
            new List<Move>(),
            new List<Move> { new([(6, 5), (4, 3)]) });

        yield return new TestCaseData(new Board(
                "Row 0: ., w, ., w, ., ., ., w\n" +
                "Row 1: r, ., w, ., ., ., w, .\n" +
                "Row 2: ., ., ., ., ., ., ., .\n" +
                "Row 3: W, ., ., ., ., ., w, .\n" +
                "Row 4: ., ., ., ., ., ., ., w\n" +
                "Row 5: ., ., ., ., ., ., ., .\n" +
                "Row 6: ., ., ., ., ., ., ., w\n" +
                "Row 7: r, ., ., ., ., ., r, .\n",
                false),
            new List<Move>(),
            new List<Move>
            {
                new([(7, 0), (6, 1)]),
                new([(7, 6), (6, 5)])
            });
    }

    [TestCaseSource(nameof(FindAllLegalMoves_FindsCorrectMoves_TestData))]
    public void FindAllLegalMoves_FindsCorrectMoves(Board board, List<Move> kingsMoves, List<Move> menMoves)
    {
        Assert.Multiple(() =>
        {
            Assert.That(kingsMoves.SequenceEqual(board.KingsMoves));
            Assert.That(menMoves.SequenceEqual(board.MenMoves));
        });
    }

    [TestCaseSource(nameof(IndexIndexer_GetsCorrectPiece_Invalid_TestData))]
    public void IndexIndexer_GetsCorrectPiece(Board board, byte index, Piece piece)
    {
        Assert.That(board[index], Is.EqualTo(piece));
    }

    [TestCaseSource(nameof(MaskIndexer_GetsCorrectPiece_TestData))]
    public void MaskIndexer_GetsCorrectPiece(Board board, ulong mask, Piece piece)
    {
        Assert.That(board[mask], Is.EqualTo(piece));
    }

    [TestCaseSource(nameof(PosIndexer_GetsCorrectPiece_Invalid_TestData))]
    public void PosIndexer_GetsCorrectPiece(Board board, int row, int col, Piece piece)
    {
        Assert.That(board[row, col], Is.EqualTo(piece));
    }
}