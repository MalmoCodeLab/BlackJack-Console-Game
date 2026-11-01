namespace Mai26ma_Grupp1_BlackJack.Tests;

public class RoundResultFormatterTests
{
    private static Card C(Rank rank) => new(Suit.Spades, rank);

    // Cards are dealt in the order given; filler keeps the shoe from running out.
    private static Deck StackedDeck(RuleSet rules, params Rank[] topCards)
    {
        Deck deck = new(rules);
        foreach (Rank rank in topCards)
            deck.DeckCards.Add(C(rank));
        for (int i = 0; i < 60; i++)
            deck.DeckCards.Add(C(Rank.Two));
        return deck;
    }

    private static Round PlayedRound(
        Player[] players, Rank[] cards, Action<Deck>? afterDeal = null)
    {
        RuleSet rules = new();
        Deck deck = StackedDeck(rules, cards);
        Round round = new(1, [.. players], new Dealer(rules), deck, rules);
        foreach (Player player in players)
            player.TryPlaceBet(10);
        round.StartRound();
        afterDeal?.Invoke(deck);
        round.EndRound();
        return round;
    }

    private static string SoloResult(string outcome, int player, int dealer) =>
        GameMessages.SoloResult(GameMessages.OutcomeHeadline(outcome, true), player, dealer);

    [Fact]
    public void Natural_ShowsBlackjackWin()
    {
        Player player = new("A");
        Round round = PlayedRound([player], [Rank.Ace, Rank.Ten, Rank.King, Rank.Seven]);

        Assert.Equal(SoloResult(GameMessages.PlayerBlackjackOutcome, 21, 17),
            RoundResultFormatter.Format(round));
    }

    [Fact]
    public void PlayerBust_ShowsLoss()
    {
        Player player = new("A");
        Round round = PlayedRound(
            [player], [Rank.Ten, Rank.Ten, Rank.Six, Rank.Seven, Rank.King],
            deck => player.ReceiveCard(deck));

        Assert.Equal(SoloResult(GameMessages.PlayerBustOutcome, 26, 17),
            RoundResultFormatter.Format(round));
    }

    [Fact]
    public void DealerBust_ShowsWin()
    {
        Player player = new("A");
        Round round = PlayedRound(
            [player], [Rank.Ten, Rank.Ten, Rank.Nine, Rank.Six, Rank.King]);

        Assert.Equal(SoloResult(GameMessages.DealerBustOutcome, 19, 26),
            RoundResultFormatter.Format(round));
    }

    [Fact]
    public void DealerNatural_ShowsLoss()
    {
        Player player = new("A");
        Round round = PlayedRound([player], [Rank.Ten, Rank.Ace, Rank.Nine, Rank.King]);

        Assert.Equal(SoloResult(GameMessages.DealerBlackjackOutcome, 19, 21),
            RoundResultFormatter.Format(round));
    }

    [Fact]
    public void HigherDealer_ShowsLoss()
    {
        Player player = new("A");
        Round round = PlayedRound([player], [Rank.Ten, Rank.Ten, Rank.Seven, Rank.King]);

        Assert.Equal(SoloResult(GameMessages.LossOutcome, 17, 20),
            RoundResultFormatter.Format(round));
    }

    [Fact]
    public void Tie_ShowsPush()
    {
        Player player = new("A");
        Round round = PlayedRound([player], [Rank.Ten, Rank.Ten, Rank.Eight, Rank.Eight]);

        Assert.Equal(SoloResult(GameMessages.PushOutcome, 18, 18),
            RoundResultFormatter.Format(round));
    }

    [Fact]
    public void SeveralPlayers_OneNamedLinePerPlayer()
    {
        Player anna = new("Anna");
        Player bo = new("Bo");
        // Anna 19, Bo 18, dealer 17.
        Round round = PlayedRound(
            [anna, bo], [Rank.Ten, Rank.Ten, Rank.Ten, Rank.Nine, Rank.Eight, Rank.Seven]);

        string[] lines = RoundResultFormatter.Format(round)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lines.Length);
        Assert.Equal(GameMessages.NamedResult(
            "Anna", GameMessages.OutcomeHeadline(GameMessages.WinOutcome, false), 19, 17), lines[0]);
        Assert.Equal(GameMessages.NamedResult(
            "Bo", GameMessages.OutcomeHeadline(GameMessages.WinOutcome, false), 18, 17), lines[1]);
    }

    [Fact]
    public void PlayerWhoLosesLastChips_GetsOutOfChipsLine()
    {
        Player player = new("Anna", chips: 10);
        Round round = PlayedRound([player], [Rank.Ten, Rank.Ten, Rank.Seven, Rank.King]);

        string[] lines = RoundResultFormatter.Format(round)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lines.Length);
        Assert.Equal(GameMessages.PlayerOutOfChips("Anna"), lines[1]);
    }
}
