namespace Mai26ma_Grupp1_BlackJack.Tests;

public class PlayerTests
{
    private static Player PlayerWith(params Rank[] ranks)
    {
        Player player = new("Test");
        foreach (Rank rank in ranks)
            player.Hand.CardsOnHand.Add(new Card(Suit.Clubs, rank));
        return player;
    }

    [Fact]
    public void Constructor_TrimsNameAndRejectsEmpty()
    {
        Assert.Equal("Anna", new Player("  Anna ").Name);
        Assert.Throws<ArgumentException>(() => new Player("  "));
    }

    [Fact]
    public void Constructor_RejectsNegativeStatistics() =>
        Assert.Throws<ArgumentException>(() => new Player("A", wins: -1));

    [Theory]
    [InlineData(new[] { Rank.Ten, Rank.Six }, true)]
    [InlineData(new[] { Rank.Ten, Rank.Ace }, false)]
    [InlineData(new[] { Rank.Ten, Rank.Six, Rank.Seven }, false)]
    public void CanHit_OnlyBelowTwentyOne(Rank[] ranks, bool expected) =>
        Assert.Equal(expected, PlayerWith(ranks).CanHit);

    [Fact]
    public void HasBlackjack_OnlyWithTwoCards()
    {
        Assert.True(PlayerWith(Rank.Ace, Rank.King).HasBlackjack);
        Assert.False(PlayerWith(Rank.Five, Rank.Six, Rank.Ten).HasBlackjack);
    }

    [Fact]
    public void IsBust_WhenOverTwentyOne()
    {
        Assert.True(PlayerWith(Rank.Ten, Rank.Nine, Rank.Five).IsBust);
        Assert.False(PlayerWith(Rank.Ten, Rank.Ace).IsBust);
    }

    [Theory]
    [InlineData("Win", 1, 0, 0)]
    [InlineData("Blackjack", 1, 0, 0)]
    [InlineData("Loss", 0, 1, 0)]
    [InlineData("Push", 0, 0, 1)]
    public void SettleBet_UpdatesStatistics(string outcome, int wins, int losses, int pushes)
    {
        Player player = new("A");
        player.TryPlaceBet(10);

        player.SettleBet(Enum.Parse<Outcome>(outcome));

        Assert.Equal(wins, player.Wins);
        Assert.Equal(losses, player.Losses);
        Assert.Equal(pushes, player.Pushes);
        Assert.Equal(1, player.GamesPlayed);
    }

    [Fact]
    public void NewRound_MovesHandToDiscardPile()
    {
        Player player = PlayerWith(Rank.Two, Rank.Three);
        Deck deck = new(new RuleSet());

        player.NewRound(deck);

        Assert.Empty(player.Hand.CardsOnHand);
        Assert.Equal(2, deck.DiscardedCards.Count);
    }
}
