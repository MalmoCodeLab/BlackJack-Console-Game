namespace Mai26ma_Grupp1_BlackJack.Tests;

// RoundFlow is the UI-free turn flow: betting, insurance and player turns.
public class RoundFlowTests
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

    // Deal order: every player, the dealer, every player, the dealer.
    private static RoundFlow NewFlow(Player[] players, params Rank[] cards)
    {
        RuleSet rules = new();
        Round round = new(1, [.. players], new Dealer(rules), StackedDeck(rules, cards), rules);
        round.StartRound();
        return new RoundFlow(round);
    }

    private static RoundFlow BettingDone(Player[] players, params Rank[] cards)
    {
        RoundFlow flow = NewFlow(players, cards);
        foreach (Player player in players)
        {
            Assert.True(player.TryPlaceBet(10));
            Assert.True(flow.ConfirmBet());
        }
        return flow;
    }

    private static RoundFlow Playing(Player[] players, params Rank[] cards)
    {
        RoundFlow flow = BettingDone(players, cards);
        flow.BeginPlay();
        return flow;
    }

    [Fact]
    public void NewFlow_StartsInBettingWithTheFirstPlayerBetting()
    {
        Player a = new("Anna"), b = new("Bo");
        RoundFlow flow = NewFlow([a, b], Rank.Ten, Rank.Nine, Rank.Ten, Rank.Eight, Rank.Seven, Rank.Six);

        Assert.Equal(RoundPhase.Betting, flow.Phase);
        Assert.Equal(0, flow.CurrentPlayerIndex);
        Assert.Equal(["SATSAR", "VÄNTAR"], flow.Statuses);
        Assert.Contains("ANNA", flow.Notice);
        Assert.Same(a, flow.CurrentPlayer);
    }

    [Fact]
    public void ConfirmBet_WithoutABet_IsRefusedWithANotice()
    {
        RoundFlow flow = NewFlow([new Player("A")], Rank.Ten, Rank.Ten, Rank.Nine, Rank.Seven);

        Assert.False(flow.ConfirmBet());

        Assert.Equal(RoundPhase.Betting, flow.Phase);
        Assert.Contains("SATSA", flow.Notice);
    }

    [Fact]
    public void ConfirmBet_MovesToTheNextPlayerThenToDealing()
    {
        Player a = new("Anna"), b = new("Bo");
        RoundFlow flow = NewFlow([a, b], Rank.Ten, Rank.Nine, Rank.Ten, Rank.Eight, Rank.Seven, Rank.Six);

        a.TryPlaceBet(10);
        Assert.True(flow.ConfirmBet());
        Assert.Equal(1, flow.CurrentPlayerIndex);
        Assert.Equal(["SATSAT", "SATSAR"], flow.Statuses);
        Assert.Equal(RoundPhase.Betting, flow.Phase);

        b.TryPlaceBet(10);
        Assert.True(flow.ConfirmBet());
        Assert.Equal(RoundPhase.Dealing, flow.Phase);
        Assert.Equal(0, flow.CurrentPlayerIndex);
    }

    [Fact]
    public void BeginPlay_StartsTheFirstPlayersTurn()
    {
        Player a = new("Anna"), b = new("Bo");
        RoundFlow flow = Playing([a, b], Rank.Ten, Rank.Nine, Rank.Ten, Rank.Eight, Rank.Seven, Rank.Six);

        Assert.Equal(RoundPhase.PlayerTurns, flow.Phase);
        Assert.Equal(["DIN TUR", "VÄNTAR"], flow.Statuses);
        Assert.Contains("TA KORT ELLER STANNA", flow.Notice);
        Assert.False(flow.AwaitingNaturalPause);
        Assert.Empty(flow.TakeCelebrations());
    }

    [Fact]
    public void Actions_InTheWrongPhase_Throw()
    {
        RoundFlow flow = NewFlow([new Player("A")], Rank.Ten, Rank.Ten, Rank.Nine, Rank.Seven);

        Assert.Throws<InvalidOperationException>(() => flow.Hit());
        Assert.Throws<InvalidOperationException>(() => flow.Stand());
        Assert.Throws<InvalidOperationException>(() => flow.BeginPlay());
        Assert.Throws<InvalidOperationException>(() => flow.AnswerInsurance(true));
        Assert.Throws<InvalidOperationException>(() => flow.FinishNaturalPause());
    }

    [Fact]
    public void Stand_MovesThroughEveryPlayerAndCompletesTheRound()
    {
        Player a = new("Anna"), b = new("Bo");
        RoundFlow flow = Playing([a, b], Rank.Ten, Rank.Nine, Rank.Ten, Rank.Eight, Rank.Seven, Rank.Six);

        Assert.True(flow.Stand());
        Assert.Equal(1, flow.CurrentPlayerIndex);
        Assert.Equal(["STANNAT", "DIN TUR"], flow.Statuses);

        Assert.True(flow.Stand());
        Assert.Equal(RoundPhase.Completed, flow.Phase);
        Assert.Equal(["STANNAT", "STANNAT"], flow.Statuses);

        flow.Round.EndRound();
        Assert.Equal(2, flow.Round.Outcomes.Count);
    }

    [Fact]
    public void Hit_TakesACardAndKeepsTheTurnWhileThePlayerCanHit()
    {
        Player a = new("Anna");
        RoundFlow flow = Playing([a], Rank.Ten, Rank.Ten, Rank.Three, Rank.Seven, Rank.Two);

        Assert.True(flow.Hit());

        Assert.Equal(3, a.Hand.CardsOnHand.Count);
        Assert.Equal(RoundPhase.PlayerTurns, flow.Phase);
        Assert.Equal(0, flow.CurrentPlayerIndex);
    }

    [Fact]
    public void Hit_ToBust_EndsTheTurnWithTjockStatus()
    {
        Player a = new("Anna");
        RoundFlow flow = Playing([a], Rank.Ten, Rank.Ten, Rank.Six, Rank.Seven, Rank.King);

        flow.Hit();

        Assert.True(a.IsBust);
        Assert.Equal("TJOCK", flow.Statuses[0]);
        Assert.Equal(RoundPhase.Completed, flow.Phase);
        Assert.Empty(flow.TakeCelebrations());
    }

    [Fact]
    public void Hit_To21_CelebratesAndMovesOn()
    {
        Player a = new("Anna"), b = new("Bo");
        RoundFlow flow = Playing([a, b],
            Rank.Ten, Rank.Nine, Rank.Ten, Rank.Five, Rank.Eight, Rank.Seven, Rank.Six);

        flow.Hit();

        Assert.Equal(21, a.Score);
        Assert.Equal("21 POÄNG", flow.Statuses[0]);
        Assert.Equal(1, flow.CurrentPlayerIndex);
        Celebration celebration = Assert.Single(flow.TakeCelebrations());
        Assert.False(celebration.NaturalBlackjack);
        Assert.Equal(0, celebration.PlayerIndex);
        Assert.Contains("21 POÄNG", celebration.Message);
        Assert.Equal("21 POÄNG", celebration.Statuses[0]);
        Assert.Empty(flow.TakeCelebrations());
    }

    [Fact]
    public void Hit_WithoutABet_IsRefused()
    {
        Player a = new("Anna");
        RoundFlow flow = Playing([a], Rank.Ten, Rank.Ten, Rank.Six, Rank.Seven, Rank.Two);
        a.CancelBet();

        Assert.False(flow.Hit());
        Assert.False(flow.Stand());

        Assert.Equal(2, a.Hand.CardsOnHand.Count);
        Assert.Contains("SATSA", flow.Notice);
        Assert.Equal(RoundPhase.PlayerTurns, flow.Phase);
    }

    [Fact]
    public void NaturalForFirstPlayer_DoesNotEndTheRound_SecondPlayerStillPlays()
    {
        Player a = new("Anna"), b = new("Bo");
        RoundFlow flow = Playing([a, b], Rank.Ace, Rank.Nine, Rank.Ten, Rank.King, Rank.Eight, Rank.Seven);

        Assert.True(a.HasBlackjack);
        Assert.True(flow.AwaitingNaturalPause);
        Assert.Equal("BLACKJACK", flow.Statuses[0]);
        Celebration celebration = Assert.Single(flow.TakeCelebrations());
        Assert.True(celebration.NaturalBlackjack);
        Assert.Equal(0, celebration.PlayerIndex);

        // Keys cannot act for anyone while the message is shown.
        Assert.Throws<InvalidOperationException>(() => flow.Hit());
        Assert.Throws<InvalidOperationException>(() => flow.Stand());

        flow.FinishNaturalPause();

        Assert.Equal(RoundPhase.PlayerTurns, flow.Phase);
        Assert.Equal(1, flow.CurrentPlayerIndex);
        Assert.Equal("DIN TUR", flow.Statuses[1]);
        Assert.False(flow.AwaitingNaturalPause);

        Assert.True(flow.Hit());
        Assert.Equal(3, b.Hand.CardsOnHand.Count);
        Assert.True(flow.Stand());
        Assert.Equal(RoundPhase.Completed, flow.Phase);
    }

    [Fact]
    public void NaturalForSecondPlayer_IsShownAfterTheFirstPlayerIsDone()
    {
        Player a = new("Anna"), b = new("Bo");
        RoundFlow flow = Playing([a, b], Rank.Nine, Rank.Ace, Rank.Ten, Rank.Eight, Rank.King, Rank.Seven);

        Assert.False(flow.AwaitingNaturalPause);
        flow.Stand();

        Assert.True(flow.AwaitingNaturalPause);
        Assert.Equal(1, flow.CurrentPlayerIndex);
        Assert.Equal("BLACKJACK", flow.Statuses[1]);
        Assert.Equal(1, Assert.Single(flow.TakeCelebrations()).PlayerIndex);

        flow.FinishNaturalPause();
        Assert.Equal(RoundPhase.Completed, flow.Phase);
    }

    [Fact]
    public void NaturalForEveryPlayer_StillStepsThroughAllOfThem()
    {
        Player a = new("Anna"), b = new("Bo"), c = new("Cy");
        RoundFlow flow = Playing([a, b, c],
            Rank.Ace, Rank.Ace, Rank.Ace, Rank.Nine,
            Rank.King, Rank.King, Rank.King, Rank.Eight);

        List<int> celebrated = [];
        while (flow.Phase == RoundPhase.PlayerTurns)
        {
            Assert.True(flow.AwaitingNaturalPause);
            celebrated.AddRange(flow.TakeCelebrations().Select(x => x.PlayerIndex));
            flow.FinishNaturalPause();
        }
        celebrated.AddRange(flow.TakeCelebrations().Select(x => x.PlayerIndex));

        Assert.Equal([0, 1, 2], celebrated);
        Assert.Equal(RoundPhase.Completed, flow.Phase);
    }

    [Fact]
    public void Twenty21AfterAHit_FollowedByANaturalNextPlayer_QueuesBothInOrder()
    {
        Player a = new("Anna"), b = new("Bo");
        RoundFlow flow = Playing([a, b],
            Rank.Ten, Rank.Ace, Rank.Ten, Rank.Five, Rank.King, Rank.Seven, Rank.Six);
        Assert.True(b.HasBlackjack);

        flow.Hit();

        Assert.Equal(21, a.Score);
        Assert.True(flow.AwaitingNaturalPause);
        Assert.Equal(1, flow.CurrentPlayerIndex);
        IReadOnlyList<Celebration> celebrations = flow.TakeCelebrations();
        Assert.Equal([false, true], celebrations.Select(x => x.NaturalBlackjack));
        Assert.Equal([0, 1], celebrations.Select(x => x.PlayerIndex));
        // Each celebration keeps the statuses from the moment it happened.
        Assert.NotEqual("BLACKJACK", celebrations[0].Statuses[1]);
        Assert.Equal("BLACKJACK", celebrations[1].Statuses[1]);
    }

    [Fact]
    public void DealerAceUp_OffersInsuranceToEachPlayerInTurn()
    {
        Player a = new("Anna"), b = new("Bo");
        RoundFlow flow = BettingDone([a, b],
            Rank.Ten, Rank.Nine, Rank.Ace, Rank.Nine, Rank.Eight, Rank.Six);

        flow.BeginPlay();
        Assert.Equal(RoundPhase.Insurance, flow.Phase);
        Assert.Equal("FÖRSÄKRING?", flow.Statuses[0]);
        Assert.Contains("FÖRSÄKRING FÖR 5", flow.Notice);

        flow.AnswerInsurance(true);
        Assert.Equal("FÖRSÄKRAD", flow.Statuses[0]);
        Assert.Equal(5, a.InsuranceBet);
        Assert.Equal(1, flow.CurrentPlayerIndex);
        Assert.Equal("FÖRSÄKRING?", flow.Statuses[1]);

        flow.AnswerInsurance(false);
        Assert.Equal("EJ FÖRSÄKRAD", flow.Statuses[1]);
        Assert.Equal(0, b.InsuranceBet);

        Assert.Equal(RoundPhase.PlayerTurns, flow.Phase);
        Assert.Equal(0, flow.CurrentPlayerIndex);
    }

    [Fact]
    public void Insurance_PlayerWithTooFewChips_IsToldSo()
    {
        Player a = new("Anna", chips: 5), b = new("Bo");
        RoundFlow flow = NewFlow([a, b], Rank.Ten, Rank.Nine, Rank.Ace, Rank.Nine, Rank.Eight, Rank.Six);
        a.TryPlaceBet(5);
        flow.ConfirmBet();
        b.TryPlaceBet(10);
        flow.ConfirmBet();
        flow.BeginPlay();

        flow.AnswerInsurance(true);

        Assert.Equal("FÖR LITE MARKER", flow.Statuses[0]);
        Assert.Equal(0, a.InsuranceBet);
    }

    [Fact]
    public void DealerNaturalWithAceUp_EndsTheRoundAfterInsuranceAndPaysIt()
    {
        Player a = new("Anna");
        RoundFlow flow = BettingDone([a], Rank.Ten, Rank.Ace, Rank.Nine, Rank.King);
        flow.BeginPlay();
        flow.AnswerInsurance(true);

        Assert.Equal(RoundPhase.Completed, flow.Phase);
        Assert.Equal(["KLAR"], flow.Statuses);

        flow.Round.EndRound();
        Assert.Equal(Outcome.Loss, flow.Round.Outcomes[a]);
        Assert.Equal(1000, a.Chips);
    }

    [Fact]
    public void DealerNaturalWithTenUp_EndsTheRoundImmediatelyWithoutInsurance()
    {
        Player a = new("Anna"), b = new("Bo");
        RoundFlow flow = BettingDone([a, b],
            Rank.Nine, Rank.Eight, Rank.King, Rank.Ten, Rank.Nine, Rank.Ace);

        flow.BeginPlay();

        Assert.Equal(RoundPhase.Completed, flow.Phase);
        Assert.Equal(["KLAR", "KLAR"], flow.Statuses);
        flow.Round.EndRound();
        Assert.All(flow.Round.Outcomes.Values, outcome => Assert.Equal(Outcome.Loss, outcome));
    }

    [Fact]
    public void NaturalPlayerWithDealerNatural_PushesWithoutAnyTurns()
    {
        Player a = new("Anna");
        RoundFlow flow = BettingDone([a], Rank.Ace, Rank.King, Rank.King, Rank.Ace);

        flow.BeginPlay();

        Assert.Equal(RoundPhase.Completed, flow.Phase);
        Assert.False(flow.AwaitingNaturalPause);
        flow.Round.EndRound();
        Assert.Equal(Outcome.Push, flow.Round.Outcomes[a]);
    }

    [Theory]
    [InlineData(1, 11)]
    [InlineData(2, 22)]
    [InlineData(4, 44)]
    public void RandomPlay_AlwaysFinishesAndSettlesEveryPlayer(int playerCount, int seed)
    {
        Random random = new(seed);
        RuleSet rules = new();
        Deck deck = new(rules);
        deck.CreateDeck();
        deck.ShuffleDeck();
        Dealer dealer = new(rules);
        Player[] players = Enumerable.Range(1, playerCount)
            .Select(i => new Player($"P{i}", chips: 1_000_000)).ToArray();

        for (int number = 1; number <= 150; number++)
        {
            Round round = new(number, [.. players], dealer, deck, rules);
            round.StartRound();
            RoundFlow flow = new(round);
            int steps = 0;

            while (flow.Phase != RoundPhase.Completed)
            {
                Assert.True(steps++ < 500, "The flow must always reach the end of the round.");
                switch (flow.Phase)
                {
                    case RoundPhase.Betting:
                        flow.CurrentPlayer.TryPlaceBet(random.Next(5, 101));
                        flow.ConfirmBet();
                        break;
                    case RoundPhase.Dealing:
                        flow.BeginPlay();
                        break;
                    case RoundPhase.Insurance:
                        flow.AnswerInsurance(random.Next(2) == 0);
                        break;
                    default:
                        if (flow.AwaitingNaturalPause)
                            flow.FinishNaturalPause();
                        else if (random.Next(2) == 0)
                            flow.Hit();
                        else
                            flow.Stand();
                        break;
                }
                flow.TakeCelebrations();
            }

            round.EndRound();
            Assert.Equal(playerCount, round.Outcomes.Count);
            Assert.All(players, player => Assert.Equal(0, player.CurrentBet));
            Assert.Equal(
                rules.DeckCount * Deck.CardsPerDeck,
                deck.DeckCards.Count + deck.DiscardedCards.Count
                    + dealer.Hand.CardsOnHand.Count
                    + players.Sum(player => player.Hand.CardsOnHand.Count));
        }
    }
}
