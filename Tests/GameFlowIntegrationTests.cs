namespace Mai26ma_Grupp1_BlackJack.Tests;

// Integration tests: real Round, Deck, Dealer, Player and Wallet working together.
public class GameFlowIntegrationTests
{
    private static Card C(Rank rank) => new(Suit.Spades, rank);

    private static Deck StackedDeck(RuleSet rules, params Rank[] topCards)
    {
        Deck deck = new(rules);
        foreach (Rank rank in topCards)
            deck.DeckCards.Add(C(rank));
        for (int i = 0; i < 60; i++)
            deck.DeckCards.Add(C(Rank.Two));
        return deck;
    }

    private static Round NewRound(int number, List<Player> players, Dealer dealer, Deck deck, RuleSet rules)
    {
        foreach (Player player in players)
            Assert.True(player.TryPlaceBet(10));
        Round round = new(number, players, dealer, deck, rules);
        round.StartRound();
        return round;
    }

    [Theory]
    [InlineData(2, false, 1)]
    [InlineData(2, true, 4)]
    [InlineData(6, false, 3)]
    [InlineData(6, true, 5)]
    [InlineData(8, false, 6)]
    [InlineData(8, true, 2)]
    public void LongGame_KeepsCardsChipsAndStatisticsConsistent(
        int deckCount, bool dealerHitsSoft17, int playerCount)
    {
        RuleSet rules = new() { DeckCount = deckCount, DealerHitsSoft17 = dealerHitsSoft17 };
        TableSimulation table = new(rules, playerCount, seed: deckCount * 100 + playerCount);
        int roundsPlayed = 0;

        for (int i = 0; i < 400; i++)
        {
            RoundRecord? record = table.PlayRound();
            if (record is null)
                break;
            roundsPlayed++;

            Assert.Equal(table.TotalCards, table.CardsAccountedFor());

            foreach (Player player in record.Players)
            {
                Outcome outcome = record.Round.Outcomes[player];
                int expected = record.ChipsBefore[player]
                    + TableSimulation.ExpectedChipChange(outcome, record.Bets[player]);
                Assert.Equal(expected, player.Chips);
                Assert.Equal(0, player.CurrentBet);
                Assert.Equal(0, player.InsuranceBet);

                if (player.IsBust)
                    Assert.Equal(Outcome.Loss, outcome);
                if (player.HasBlackjack && !record.Round.Dealer.HasBlackjack)
                    Assert.Equal(Outcome.Blackjack, outcome);
            }

            Dealer dealer = record.Round.Dealer;
            if (record.Players.Any(player => !player.IsBust))
            {
                Assert.True(dealer.IsBust || dealer.Score >= 17);
                if (!dealerHitsSoft17)
                    Assert.False(dealer.Score < 17);
                if (dealerHitsSoft17 && dealer.Score == 17)
                    Assert.False(dealer.Hand.IsSoft);
            }
        }

        Assert.True(roundsPlayed > 0);
        Assert.All(table.Seated, player =>
        {
            Assert.True(player.Chips >= 0);
            Assert.True(player.GamesPlayed <= roundsPlayed);
        });
    }

    [Fact]
    public void LongGame_TotalGamesMatchRoundsForAPlayerWhoNeverLeaves()
    {
        TableSimulation table = new(new RuleSet(), 1, seed: 7, chips: 1_000_000);

        for (int i = 0; i < 200; i++)
            Assert.NotNull(table.PlayRound());

        Player player = table.Seated[0];
        Assert.Equal(200, player.GamesPlayed);
        Assert.Equal(table.TotalCards, table.CardsAccountedFor());
    }

    [Fact]
    public void LongGame_ReshufflesAtTheCutCardAndAlwaysHasCardsForTheRound()
    {
        RuleSet rules = new() { DeckCount = 2 };
        TableSimulation table = new(rules, 4, seed: 11, chips: 1_000_000);
        int reshuffles = 0;
        const int cardsPerParticipant = 11;
        const int participants = 5;

        for (int i = 0; i < 300; i++)
        {
            RoundRecord record = table.PlayRound()!;
            if (record.Reshuffled)
                reshuffles++;

            // 2 cards per participant are already dealt when we measure.
            Assert.True(
                record.CardsLeftAfterDeal + Round.StartingHandSize * participants
                    >= cardsPerParticipant * participants);
        }

        Assert.True(reshuffles > 0, "The shoe should be reshuffled during a long game.");
        Assert.Equal(table.TotalCards, table.CardsAccountedFor());
    }

    [Fact]
    public void LongGame_ReshuffleNeverHappensInTheMiddleOfARound()
    {
        RuleSet rules = new() { DeckCount = 2 };
        TableSimulation table = new(rules, 5, seed: 3, chips: 1_000_000);

        for (int i = 0; i < 200; i++)
        {
            int discardedBefore = table.Deck.DiscardedCards.Count;
            RoundRecord record = table.PlayRound()!;

            // Discards only grow during a round, so a mid-round shuffle would empty them.
            if (!record.Reshuffled)
                Assert.True(table.Deck.DiscardedCards.Count >= discardedBefore);
            Assert.Equal(table.TotalCards, table.CardsAccountedFor());
        }
    }

    [Fact]
    public void LongGame_PlayersWhoRunOutOfChipsAreNotDealtIn()
    {
        TableSimulation table = new(new RuleSet(), 3, seed: 5, chips: 20);
        int safetyLimit = 5000;

        while (safetyLimit-- > 0 && table.PlayRound() is not null) { }

        Assert.True(safetyLimit > 0, "Everyone should eventually run out of chips.");
        Assert.All(table.Seated, player => Assert.True(player.IsOutOfChips));
        Assert.Equal(table.TotalCards, table.CardsAccountedFor());
    }

    [Fact]
    public void ThreePlayers_NaturalForFirstPlayerStillLetsEveryoneBePlayed()
    {
        RuleSet rules = new();
        Player first = new("First"), second = new("Second"), third = new("Third");
        List<Player> players = [first, second, third];
        // Deal: first, second, third, dealer, then first, second, third, dealer.
        Deck deck = StackedDeck(rules,
            Rank.Ace, Rank.Ten, Rank.Nine, Rank.Six,
            Rank.King, Rank.Eight, Rank.Eight, Rank.Ten);
        Dealer dealer = new(rules);

        Round round = NewRound(1, players, dealer, deck, rules);
        Assert.False(dealer.PeeksAndHasBlackjack);
        Assert.True(first.HasBlackjack);
        Assert.True(second.CanHit);
        Assert.True(third.CanHit);
        round.EndRound();

        // Dealer 16 draws a Two and ends on 18.
        Assert.Equal(18, dealer.Score);
        Assert.Equal(Outcome.Blackjack, round.Outcomes[first]);
        Assert.Equal(Outcome.Push, round.Outcomes[second]);
        Assert.Equal(Outcome.Loss, round.Outcomes[third]);
        Assert.Equal(1015, first.Chips);
        Assert.Equal(1000, second.Chips);
        Assert.Equal(990, third.Chips);
    }

    [Fact]
    public void TwoRoundsInARow_ChipsAndStatisticsCarryOver()
    {
        RuleSet rules = new();
        Player player = new("A");
        Dealer dealer = new(rules);
        Deck deck = StackedDeck(rules,
            Rank.Ten, Rank.Ten, Rank.Nine, Rank.Seven,   // round 1: 19 vs 17
            Rank.Ten, Rank.Ten, Rank.Seven, Rank.Nine);  // round 2: 17 vs 19

        Round first = NewRound(1, [player], dealer, deck, rules);
        first.EndRound();
        Assert.Equal(Outcome.Win, first.Outcomes[player]);
        Assert.Equal(1010, player.Chips);

        Round second = NewRound(2, [player], dealer, deck, rules);
        second.EndRound();
        Assert.Equal(Outcome.Loss, second.Outcomes[player]);
        Assert.Equal(1000, player.Chips);
        Assert.Equal((1, 1, 0), (player.Wins, player.Losses, player.Pushes));
        Assert.Equal(2, player.GamesPlayed);
    }

    [Fact]
    public void DealerNaturalWithAceUpCard_InsuranceBreaksEven()
    {
        RuleSet rules = new();
        Player player = new("A");
        Dealer dealer = new(rules);
        // Player Ten and Nine, dealer Ace up and King in the hole.
        Deck deck = StackedDeck(rules, Rank.Ten, Rank.Ace, Rank.Nine, Rank.King);

        Round round = NewRound(1, [player], dealer, deck, rules);
        Assert.True(dealer.ShowsAce);
        Assert.True(player.TryTakeInsurance());
        Assert.Equal(985, player.Chips);
        Assert.True(dealer.PeeksAndHasBlackjack);
        round.EndRound();

        Assert.Equal(Outcome.Loss, round.Outcomes[player]);
        Assert.Equal(1000, player.Chips);
        Assert.Equal(0, player.InsuranceBet);
    }

    [Fact]
    public void DealerNaturalWithAceUpCard_NoInsuranceLosesTheBet()
    {
        RuleSet rules = new();
        Player player = new("A");
        Dealer dealer = new(rules);
        Deck deck = StackedDeck(rules, Rank.Ten, Rank.Ace, Rank.Nine, Rank.King);

        Round round = NewRound(1, [player], dealer, deck, rules);
        round.EndRound();

        Assert.Equal(990, player.Chips);
    }

    [Fact]
    public void DealerWithoutNaturalAfterInsurance_InsuranceIsLost()
    {
        RuleSet rules = new();
        Player player = new("A");
        Dealer dealer = new(rules);
        // Dealer Ace up, Six in the hole (soft 17 stands), player 19.
        Deck deck = StackedDeck(rules, Rank.Ten, Rank.Ace, Rank.Nine, Rank.Six);

        Round round = NewRound(1, [player], dealer, deck, rules);
        Assert.True(player.TryTakeInsurance());
        Assert.False(dealer.PeeksAndHasBlackjack);
        round.EndRound();

        Assert.Equal(Outcome.Win, round.Outcomes[player]);
        // 1000 - 10 bet - 5 insurance + 20 win payout.
        Assert.Equal(1005, player.Chips);
    }

    [Theory]
    [InlineData(false, 17, "Win")]
    [InlineData(true, 19, "Loss")]
    public void DealerSoft17Rule_ChangesTheResult(bool dealerHitsSoft17, int dealerScore, string expected)
    {
        RuleSet rules = new() { DealerHitsSoft17 = dealerHitsSoft17 };
        Player player = new("A");
        Dealer dealer = new(rules);
        // Dealer Ace + Six = soft 17, player 18. A hitting dealer draws a Two for soft 19.
        Deck deck = StackedDeck(rules, Rank.Ten, Rank.Ace, Rank.Eight, Rank.Six);

        Round round = NewRound(1, [player], dealer, deck, rules);
        round.EndRound();

        Assert.Equal(dealerScore, dealer.Score);
        Assert.Equal(Enum.Parse<Outcome>(expected), round.Outcomes[player]);
    }

    [Fact]
    public void ThreeCard21_NaturalBeatsItOnlyWhenTheRuleIsOn()
    {
        foreach (bool naturalWins in new[] { true, false })
        {
            RuleSet rules = new() { NaturalBlackjackBeatsThreeCard21 = naturalWins };
            Player player = new("A");
            Dealer dealer = new(rules);
            // Player natural. Dealer Seven + Seven draws a Seven for 21 in three cards.
            Deck deck = StackedDeck(rules,
                Rank.Ace, Rank.Seven, Rank.King, Rank.Seven, Rank.Seven);

            Round round = NewRound(1, [player], dealer, deck, rules);
            round.EndRound();

            Assert.Equal(21, dealer.Score);
            Assert.Equal(naturalWins ? Outcome.Blackjack : Outcome.Push, round.Outcomes[player]);
        }
    }
}
