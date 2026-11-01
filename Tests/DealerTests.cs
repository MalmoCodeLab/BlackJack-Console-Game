namespace Mai26ma_Grupp1_BlackJack.Tests;

public class DealerTests
{
    private static Dealer DealerWith(RuleSet rules, params Rank[] ranks)
    {
        Dealer dealer = new(rules);
        foreach (Rank rank in ranks)
            dealer.Hand.CardsOnHand.Add(new Card(Suit.Spades, rank));
        return dealer;
    }

    [Theory]
    [InlineData(new[] { Rank.Ten, Rank.Six }, true)]
    [InlineData(new[] { Rank.Ten, Rank.Seven }, false)]
    [InlineData(new[] { Rank.Ten, Rank.Nine }, false)]
    [InlineData(new[] { Rank.Two, Rank.Three, Rank.Four }, true)]
    public void ShouldHit_HardHands(Rank[] ranks, bool expected) =>
        Assert.Equal(expected, DealerWith(new RuleSet(), ranks).ShouldHit());

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void ShouldHit_SoftSeventeen_FollowsRule(bool hitsSoft17, bool expected)
    {
        Dealer dealer = DealerWith(
            new RuleSet { DealerHitsSoft17 = hitsSoft17 }, Rank.Ace, Rank.Six);

        Assert.Equal(expected, dealer.ShouldHit());
    }

    [Fact]
    public void ShouldHit_HardSeventeenWithAce_Stands()
    {
        Dealer dealer = DealerWith(
            new RuleSet { DealerHitsSoft17 = true }, Rank.Ace, Rank.Six, Rank.Ten);

        Assert.False(dealer.ShouldHit());
    }

    [Fact]
    public void ShowsAce_ChecksOnlyTheUpCard()
    {
        Assert.True(DealerWith(new RuleSet(), Rank.Ace, Rank.Five).ShowsAce);
        Assert.False(DealerWith(new RuleSet(), Rank.Five, Rank.Ace).ShowsAce);
        Assert.False(new Dealer(new RuleSet()).ShowsAce);
    }

    [Fact]
    public void PeeksAndHasBlackjack_FalseForEmptyHand() =>
        Assert.False(new Dealer(new RuleSet()).PeeksAndHasBlackjack);

    [Fact]
    public void NewRound_HidesHoleCardAndDiscardsHand()
    {
        RuleSet rules = new();
        Deck deck = new(rules);
        Dealer dealer = DealerWith(rules, Rank.Ten, Rank.Seven);
        dealer.ShowHoleCard();

        dealer.NewRound(deck);

        Assert.False(dealer.HoleCardVisible);
        Assert.Empty(dealer.Hand.CardsOnHand);
        Assert.Equal(2, deck.DiscardedCards.Count);
    }
}
