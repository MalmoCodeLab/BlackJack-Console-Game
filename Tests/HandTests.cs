namespace Mai26ma_Grupp1_BlackJack.Tests;

public class HandTests
{
    private static Hand HandOf(params Rank[] ranks)
    {
        Hand hand = new();
        foreach (Rank rank in ranks)
            hand.CardsOnHand.Add(new Card(Suit.Spades, rank));
        return hand;
    }

    [Theory]
    [InlineData(new[] { Rank.Two, Rank.Three }, 5)]
    [InlineData(new[] { Rank.King, Rank.Queen }, 20)]
    [InlineData(new[] { Rank.Nine, Rank.Jack, Rank.Ten }, 29)]
    [InlineData(new[] { Rank.Ace, Rank.King }, 21)]
    [InlineData(new[] { Rank.Ace, Rank.Ace }, 12)]
    [InlineData(new[] { Rank.Ace, Rank.Ace, Rank.Nine }, 21)]
    [InlineData(new[] { Rank.Ace, Rank.Six, Rank.King }, 17)]
    [InlineData(new[] { Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace }, 14)]
    public void Score_CountsAcesAsOneOrEleven(Rank[] ranks, int expected) =>
        Assert.Equal(expected, HandOf(ranks).Score);

    [Fact]
    public void Score_EmptyHand_IsZero() => Assert.Equal(0, HandOf().Score);

    [Theory]
    [InlineData(new[] { Rank.Ace, Rank.Six }, true)]
    [InlineData(new[] { Rank.Ace, Rank.Six, Rank.King }, false)]
    [InlineData(new[] { Rank.Ten, Rank.Seven }, false)]
    [InlineData(new[] { Rank.Ace, Rank.Ace, Rank.Five }, true)]
    [InlineData(new[] { Rank.Ace, Rank.King }, true)]
    public void IsSoft_TrueOnlyWhenAceStillCountsAsEleven(Rank[] ranks, bool expected) =>
        Assert.Equal(expected, HandOf(ranks).IsSoft);

    [Fact]
    public void DiscardCards_EmptiesHandAndReturnsAllCards()
    {
        Hand hand = HandOf(Rank.Two, Rank.Three, Rank.Four);

        List<Card> discarded = hand.DiscardCards();

        Assert.Equal(3, discarded.Count);
        Assert.Empty(hand.CardsOnHand);
    }

    [Fact]
    public void ReceiveCard_TakesTopCardFromDeck()
    {
        Deck deck = new(new RuleSet { DeckCount = 2 });
        deck.CreateDeck();
        Card top = deck.DeckCards[0];
        Hand hand = new();

        hand.ReceiveCard(deck);

        Assert.Same(top, hand.CardsOnHand[0]);
        Assert.Equal(103, deck.DeckCards.Count);
    }
}
