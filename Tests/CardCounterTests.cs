namespace Mai26ma_Grupp1_BlackJack.Tests;

public class CardCounterTests
{
    private static Card C(Rank rank) => new(Suit.Hearts, rank);

    [Theory]
    [InlineData(Rank.Two, 1)]
    [InlineData(Rank.Six, 1)]
    [InlineData(Rank.Seven, 0)]
    [InlineData(Rank.Nine, 0)]
    [InlineData(Rank.Ten, -1)]
    [InlineData(Rank.King, -1)]
    [InlineData(Rank.Ace, -1)]
    public void Count_UsesHiLoValues(Rank rank, int expectedRunning)
    {
        CardCounter counter = new();

        counter.Count(C(rank), 104);

        Assert.Equal(expectedRunning, counter.RunningCount);
    }

    [Fact]
    public void TrueCount_DividesByDecksRemaining()
    {
        CardCounter counter = new();

        counter.Count(C(Rank.Two), 104);
        counter.Count(C(Rank.Three), 104);
        counter.Count(C(Rank.Four), 104);
        counter.Count(C(Rank.Five), 104);

        // Running count 4 with two decks left.
        Assert.Equal(2.0, counter.TrueCount);
    }

    [Fact]
    public void TrueCount_IsZeroWhenShoeIsEmpty()
    {
        CardCounter counter = new();

        counter.Count(C(Rank.Two), 0);

        Assert.Equal(0, counter.TrueCount);
    }

    [Fact]
    public void Reset_ClearsCounts()
    {
        CardCounter counter = new();
        counter.Count(C(Rank.Two), 52);

        counter.Reset();

        Assert.Equal(0, counter.RunningCount);
        Assert.Equal(0, counter.TrueCount);
    }
}
