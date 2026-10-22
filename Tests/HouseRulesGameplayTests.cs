namespace Mai26ma_Grupp1_BlackJack.Tests;

// Checklist items: "Givaren på mjuk 17", "Antal kortlekar", "Blandningsgräns"
// and "Naturlig tjugoett".
public class HouseRulesGameplayTests
{
    private static Card C(Rank rank) => new(Suit.Spades, rank);

    private static Dealer DealerWith(RuleSet rules, params Rank[] ranks)
    {
        Dealer dealer = new(rules);
        foreach (Rank rank in ranks)
            dealer.Hand.CardsOnHand.Add(C(rank));
        return dealer;
    }

    // Builds a deck whose first cards are dealt in exactly the given order,
    // followed by filler so no reshuffle happens during the round.
    private static Deck StackedDeck(RuleSet rules, params Rank[] topCards)
    {
        Deck deck = new(rules);
        foreach (Rank rank in topCards)
            deck.DeckCards.Add(C(rank));
        for (int i = 0; i < 60; i++)
            deck.DeckCards.Add(C(Rank.Two));
        return deck;
    }

    // ---- Givaren på mjuk 17 ----

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void SoftSeventeen_AcePlusSix_FollowsDealerHitsSoft17(bool dealerHitsSoft17, bool expectedHit)
    {
        RuleSet rules = new() { DealerHitsSoft17 = dealerHitsSoft17 };
        Dealer dealer = DealerWith(rules, Rank.Ace, Rank.Six);

        Assert.Equal(17, dealer.Score);
        Assert.Equal(expectedHit, dealer.ShouldHit());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HardSeventeen_AlwaysStands(bool dealerHitsSoft17)
    {
        Dealer dealer = DealerWith(
            new RuleSet { DealerHitsSoft17 = dealerHitsSoft17 }, Rank.Ten, Rank.Seven);

        Assert.False(dealer.ShouldHit());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SoftSeventeenBuiltFromThreeCards_FollowsTheRule(bool dealerHitsSoft17)
    {
        // A + 2 + 4 = soft 17.
        Dealer dealer = DealerWith(
            new RuleSet { DealerHitsSoft17 = dealerHitsSoft17 }, Rank.Ace, Rank.Two, Rank.Four);

        Assert.Equal(17, dealer.Score);
        Assert.Equal(dealerHitsSoft17, dealer.ShouldHit());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BelowSeventeen_AlwaysHits_AndEighteenOrMoreAlwaysStands(bool dealerHitsSoft17)
    {
        RuleSet rules = new() { DealerHitsSoft17 = dealerHitsSoft17 };

        Assert.True(DealerWith(rules, Rank.Ten, Rank.Six).ShouldHit());
        Assert.True(DealerWith(rules, Rank.Ace, Rank.Five).ShouldHit()); // soft 16
        Assert.False(DealerWith(rules, Rank.Ace, Rank.Seven).ShouldHit()); // soft 18
        Assert.False(DealerWith(rules, Rank.Ten, Rank.Nine).ShouldHit());
    }

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 3)]
    public void Round_DealerDrawsOnSoftSeventeenOnlyWhenRuleIsOn(bool dealerHitsSoft17, int expectedDealerCards)
    {
        RuleSet rules = new() { DealerHitsSoft17 = dealerHitsSoft17 };
        // Deal order: player, dealer, player, dealer. Dealer gets A + 6, then a 4.
        Deck deck = StackedDeck(rules, Rank.Ten, Rank.Ace, Rank.Nine, Rank.Six, Rank.Four);
        Dealer dealer = new(rules);
        Player player = new("Test");
        Round round = new(1, [player], dealer, deck, rules);

        round.StartRound();
        round.EndRound();

        Assert.Equal(expectedDealerCards, dealer.Hand.CardsOnHand.Count);
    }

    // ---- Antal kortlekar ----

    [Theory]
    [InlineData(1, 52)]
    [InlineData(2, 104)]
    [InlineData(6, 312)]
    [InlineData(8, 416)]
    public void CreateDeck_ProducesFiftyTwoCardsPerDeck(int deckCount, int expectedCards)
    {
        Deck deck = new(new RuleSet { DeckCount = deckCount, ReshuffleThreshold = 15 });

        deck.CreateDeck();

        Assert.Equal(expectedCards, deck.DeckCards.Count);
    }

    [Fact]
    public void CreateDeck_OneDeckHasEveryCardExactlyOnce()
    {
        Deck deck = new(new RuleSet { DeckCount = 1, ReshuffleThreshold = 15 });

        deck.CreateDeck();

        Assert.Equal(52, deck.DeckCards.Select(card => (card.Suit, card.Rank)).Distinct().Count());
    }

    [Fact]
    public void CreateDeck_SixDecksHasSixCopiesOfEveryCard()
    {
        Deck deck = new(RuleSet.Default);

        deck.CreateDeck();

        Assert.All(
            deck.DeckCards.GroupBy(card => (card.Suit, card.Rank)),
            group => Assert.Equal(6, group.Count()));
    }

    [Fact]
    public void Deck_WithNoRules_UsesDefaultSixDecks()
    {
        Deck deck = new();

        deck.CreateDeck();

        Assert.Equal(312, deck.DeckCards.Count);
    }

    // ---- Blandningsgräns ----

    private static Deck ShuffledDeck(RuleSet rules)
    {
        Deck deck = new(rules);
        deck.CreateDeck();
        deck.ShuffleDeck();
        return deck;
    }

    private static void DealDownToCutCard(Deck deck)
    {
        while (!deck.CutCardReached)
            deck.DiscardedCards.Add(deck.DealCard());
    }

    // Leaves exactly `remaining` cards in the shoe and the rest in the discard pile.
    private static Deck DeckWithRemaining(RuleSet rules, int remaining)
    {
        Deck deck = new(rules);
        deck.CreateDeck();
        while (deck.DeckCards.Count > remaining)
        {
            deck.DiscardedCards.Add(deck.DeckCards[0]);
            deck.DeckCards.RemoveAt(0);
        }
        return deck;
    }

    // The cut card sits somewhere between the configured limit and a quarter of the shoe.
    [Theory]
    [InlineData(1, 15, 15, 15)]
    [InlineData(1, 30, 30, 30)]
    [InlineData(6, 15, 15, 78)]
    [InlineData(6, 100, 100, 100)]
    public void ShuffleDeck_PlacesCutCardWithinConfiguredRange(
        int deckCount, int threshold, int lowest, int highest)
    {
        RuleSet rules = new() { DeckCount = deckCount, ReshuffleThreshold = threshold };

        for (int i = 0; i < 50; i++)
        {
            Deck deck = ShuffledDeck(rules);
            Assert.InRange(deck.CutCardPosition, lowest, highest);
            Assert.False(deck.CutCardReached);
        }
    }

    [Fact]
    public void CutCard_IsReachedOnlyWhenShoeShrinksToItsPosition()
    {
        RuleSet rules = new() { DeckCount = 1, ReshuffleThreshold = 15 };
        Deck deck = ShuffledDeck(rules);

        while (deck.DeckCards.Count > deck.CutCardPosition + 1)
            deck.DiscardedCards.Add(deck.DealCard());
        Assert.False(deck.CutCardReached);

        deck.DiscardedCards.Add(deck.DealCard());
        Assert.True(deck.CutCardReached);
    }

    [Fact]
    public void CheckIfNewShuffleIsNeeded_ReshufflesOnlyAfterCutCardIsReached()
    {
        RuleSet rules = new() { DeckCount = 1, ReshuffleThreshold = 15 };
        Deck deck = ShuffledDeck(rules);

        deck.DiscardedCards.Add(deck.DealCard());
        deck.CheckIfNewShuffleIsNeeded();
        Assert.Equal(51, deck.DeckCards.Count);

        DealDownToCutCard(deck);
        deck.CheckIfNewShuffleIsNeeded();

        Assert.Equal(52, deck.DeckCards.Count);
        Assert.Empty(deck.DiscardedCards);
        Assert.False(deck.CutCardReached);
    }

    [Fact]
    public void Reshuffle_KeepsEveryCard()
    {
        RuleSet rules = new() { DeckCount = 1, ReshuffleThreshold = 20 };
        Deck deck = ShuffledDeck(rules);
        DealDownToCutCard(deck);

        deck.CheckIfNewShuffleIsNeeded();

        Assert.Equal(52, deck.DeckCards.Select(card => (card.Suit, card.Rank)).Distinct().Count());
    }

    [Fact]
    public void Round_ReshufflesAtStartWhenCutCardWasReached()
    {
        RuleSet rules = new() { DeckCount = 1, ReshuffleThreshold = 40 };
        Deck deck = ShuffledDeck(rules);
        DealDownToCutCard(deck);
        Dealer dealer = new(rules);
        Round round = new(1, [new Player("Test")], dealer, deck, rules);

        round.StartRound();

        // 52 cards back in the shoe, minus the 4 dealt to one player and the dealer.
        Assert.Equal(48, deck.DeckCards.Count);
    }

    [Fact]
    public void Round_DoesNotReshuffleBeforeCutCardIsReached()
    {
        RuleSet rules = new() { DeckCount = 1, ReshuffleThreshold = 30 };
        Deck deck = ShuffledDeck(rules);
        Dealer dealer = new(rules);
        Round round = new(1, [new Player("Test")], dealer, deck, rules);

        round.StartRound();

        Assert.Equal(48, deck.DeckCards.Count);
        Assert.Empty(deck.DiscardedCards);
    }

    [Fact]
    public void CutCardReachedMidRound_DoesNotReshuffleUntilNextRound()
    {
        RuleSet rules = new() { DeckCount = 1, ReshuffleThreshold = 30 };
        Deck deck = ShuffledDeck(rules);
        while (deck.DeckCards.Count > deck.CutCardPosition + 3)
            deck.DiscardedCards.Add(deck.DealCard());
        Round round = new(1, [new Player("Test")], new Dealer(rules), deck, rules);

        round.StartRound();
        Assert.True(deck.CutCardReached);
        int remaining = deck.DeckCards.Count;
        round.Players[0].ReceiveCard(deck);

        Assert.Equal(remaining - 1, deck.DeckCards.Count);
    }

    [Fact]
    public void DealCard_EmptyShoe_ReusesDiscardPile()
    {
        RuleSet rules = new() { DeckCount = 1, ReshuffleThreshold = 15 };
        Deck deck = DeckWithRemaining(rules, 0);

        Card card = deck.DealCard();

        Assert.NotNull(card);
        Assert.Equal(51, deck.DeckCards.Count);
    }

    [Fact]
    public void DealCard_AllCardsInPlay_AddsFreshDecksInsteadOfCrashing()
    {
        RuleSet rules = new() { DeckCount = 1, ReshuffleThreshold = 15 };
        Deck deck = new(rules);

        Card card = deck.DealCard();

        Assert.NotNull(card);
        Assert.Equal(51, deck.DeckCards.Count);
    }

    [Fact]
    public void Round_ManyPlayersAndLowThreshold_NeverRunsOutOfCards()
    {
        RuleSet rules = new() { DeckCount = 1, ReshuffleThreshold = 1 };
        Deck deck = DeckWithRemaining(rules, 10);
        List<Player> players = [new("A"), new("B"), new("C"), new("D"), new("E")];
        Round round = new(1, players, new Dealer(rules), deck, rules);

        round.StartRound();
        foreach (Player player in players)
            for (int i = 0; i < 10; i++)
                player.ReceiveCard(deck);

        Assert.All(players, player => Assert.NotEmpty(player.Hand.CardsOnHand));
    }

    // ---- Naturlig tjugoett ----

    // Player natural (A + K) against a dealer 21 built from three cards (5 + 9 + 7).
    private static Outcome NaturalPlayerAgainstDealerThreeCard21(bool naturalBeatsThreeCard21)
    {
        RuleSet rules = new() { NaturalBlackjackBeatsThreeCard21 = naturalBeatsThreeCard21 };
        Deck deck = StackedDeck(rules, Rank.Ace, Rank.Five, Rank.King, Rank.Nine, Rank.Seven);
        Dealer dealer = new(rules);
        Player player = new("Test");
        Round round = new(1, [player], dealer, deck, rules);

        round.StartRound();
        round.EndRound();

        Assert.True(player.HasBlackjack);
        Assert.Equal(3, dealer.Hand.CardsOnHand.Count);
        Assert.Equal(21, dealer.Score);
        return round.Outcomes[player];
    }

    // Dealer natural (A + K) against a player 21 built from three cards (5 + 9 + 7).
    private static Outcome ThreeCard21PlayerAgainstDealerNatural(bool naturalBeatsThreeCard21)
    {
        RuleSet rules = new() { NaturalBlackjackBeatsThreeCard21 = naturalBeatsThreeCard21 };
        Deck deck = StackedDeck(rules, Rank.Five, Rank.Ace, Rank.Nine, Rank.King, Rank.Seven);
        Dealer dealer = new(rules);
        Player player = new("Test");
        Round round = new(1, [player], dealer, deck, rules);

        round.StartRound();
        player.ReceiveCard(deck);
        round.EndRound();

        Assert.True(dealer.HasBlackjack);
        Assert.Equal(3, player.Hand.CardsOnHand.Count);
        Assert.Equal(21, player.Score);
        return round.Outcomes[player];
    }

    [Fact]
    public void PlayerNatural_BeatsDealerThreeCard21_WhenRuleIsOn() =>
        Assert.Equal(Outcome.Blackjack, NaturalPlayerAgainstDealerThreeCard21(true));

    [Fact]
    public void PlayerNatural_PushesDealerThreeCard21_WhenRuleIsOff() =>
        Assert.Equal(Outcome.Push, NaturalPlayerAgainstDealerThreeCard21(false));

    [Fact]
    public void DealerNatural_BeatsPlayerThreeCard21_WhenRuleIsOn() =>
        Assert.Equal(Outcome.Loss, ThreeCard21PlayerAgainstDealerNatural(true));

    [Fact]
    public void DealerNatural_PushesPlayerThreeCard21_WhenRuleIsOff() =>
        Assert.Equal(Outcome.Push, ThreeCard21PlayerAgainstDealerNatural(false));

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TwoNaturals_AlwaysPush(bool naturalBeatsThreeCard21)
    {
        RuleSet rules = new() { NaturalBlackjackBeatsThreeCard21 = naturalBeatsThreeCard21 };
        Deck deck = StackedDeck(rules, Rank.Ace, Rank.Ace, Rank.King, Rank.King);
        Dealer dealer = new(rules);
        Player player = new("Test");
        Round round = new(1, [player], dealer, deck, rules);

        round.StartRound();
        round.EndRound();

        Assert.Equal(Outcome.Push, round.Outcomes[player]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NaturalAgainstNonTwentyOne_AlwaysPaysBlackjack(bool naturalBeatsThreeCard21)
    {
        RuleSet rules = new() { NaturalBlackjackBeatsThreeCard21 = naturalBeatsThreeCard21 };
        // Dealer gets 10 + 8 = 18 and stands.
        Deck deck = StackedDeck(rules, Rank.Ace, Rank.Ten, Rank.King, Rank.Eight);
        Dealer dealer = new(rules);
        Player player = new("Test");
        Round round = new(1, [player], dealer, deck, rules);

        round.StartRound();
        round.EndRound();

        Assert.Equal(Outcome.Blackjack, round.Outcomes[player]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ThreeCard21AgainstThreeCard21_AlwaysPushes(bool naturalBeatsThreeCard21)
    {
        RuleSet rules = new() { NaturalBlackjackBeatsThreeCard21 = naturalBeatsThreeCard21 };
        // Player 5 + 9, dealer 5 + 9, then both draw a 7.
        Deck deck = StackedDeck(rules, Rank.Five, Rank.Five, Rank.Nine, Rank.Nine, Rank.Seven, Rank.Seven);
        Dealer dealer = new(rules);
        Player player = new("Test");
        Round round = new(1, [player], dealer, deck, rules);

        round.StartRound();
        player.ReceiveCard(deck);
        round.EndRound();

        Assert.Equal(Outcome.Push, round.Outcomes[player]);
    }

    [Fact]
    public void Round_RejectsDealerAndDeckWithDifferentRules()
    {
        RuleSet rules = new();
        RuleSet other = new() { DealerHitsSoft17 = true };

        Assert.Throws<ArgumentException>(() =>
            new Round(1, [new Player("Test")], new Dealer(other), new Deck(rules), rules));
    }
}
