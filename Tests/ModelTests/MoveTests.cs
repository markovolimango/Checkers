using Checkers.Models.Board;

namespace Tests.ModelTests;

[TestFixture]
public class MoveTests
{
    private static IEnumerable<TestCaseData> EqualityOperators_ReturnCorrectValue_TestData()
    {
        yield return new TestCaseData(new Move([(1, 2), (2, 3)]), new Move([(1, 2), (2, 3)]), true);
        yield return new TestCaseData(new Move([(1, 2), (2, 3)]), new Move([(1, 2), (3, 4)]), false);
        yield return new TestCaseData(new Move([(1, 2), (3, 4), (5, 6)]), new Move([(1, 2), (3, 4), (5, 6)]), true);
    }

    [Test]
    [TestCaseSource(nameof(EqualityOperators_ReturnCorrectValue_TestData))]
    public void EqualityOperators_ReturnCorrectValue(Move a, Move b, bool areEqual)
    {
        Assert.Multiple(() =>
        {
            Assert.That(a == b, Is.EqualTo(areEqual));
            Assert.That(a != b, Is.EqualTo(!areEqual));
        });
    }
}