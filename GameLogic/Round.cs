class Round
{
    private bool started;
    private bool ended;
    private readonly Dictionary<Player, Outcome> outcomes = new();

    public int RoundNumber { get; }
    public List<Player> Players { get; }
    public Dealer Dealer { get; }
    public Deck Deck { get; }
    public IReadOnlyDictionary<Player, Outcome> Outcomes => outcomes;

    public Round(int roundNumber, List<Player> players, Dealer dealer, Deck deck)
    {
        if (roundNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(roundNumber));
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(dealer);
        ArgumentNullException.ThrowIfNull(deck);
        if (players.Count == 0)
            throw new ArgumentException("A round must have at least one player.", nameof(players));
        if (players.Exists(player => player is null))
            throw new ArgumentException("Players cannot contain null entries.", nameof(players));

        RoundNumber = roundNumber;
        Players = players;
        Dealer = dealer;
        Deck = deck;
    }

    public void StartRound()
    {
        if (started)
            throw new InvalidOperationException("This round has already started.");

        foreach (Player player in Players)
            player.NewRound(Deck);
        Dealer.NewRound(Deck);
        Deck.CheckIfNewShuffleIsNeeded();

        for (int cardNumber = 0; cardNumber < 2; cardNumber++)
        {
            foreach (Player player in Players)
                player.ReceiveCard(Deck);
            Dealer.ReceiveCard(Deck);
        }

        started = true;
    }

    public void EndRound()
    {
        if (!started)
            throw new InvalidOperationException("Start the round before ending it.");
        if (ended)
            throw new InvalidOperationException("This round has already ended.");

        Dealer.ShowHoleCard();
        bool anyPlayerStillIn = Players.Exists(player => !player.IsBust);
        if (anyPlayerStillIn)
        {
            while (Dealer.ShouldHit())
                Dealer.ReceiveCard(Deck);
        }

        DetermineWinners();
        ended = true;
    }

    private void DetermineWinners()
    {
        foreach (Player player in Players)
        {
            Outcome outcome;
            if (player.IsBust || (Dealer.HasBlackjack && !player.HasBlackjack))
                outcome = Outcome.Lose;
            else if (Dealer.IsBust)
                outcome = Outcome.Win;
            else if (player.HasBlackjack && !Dealer.HasBlackjack)
                outcome = Outcome.Blackjack;
            else if (player.Score > Dealer.Score)
                outcome = Outcome.Win;
            else if (player.Score < Dealer.Score)
                outcome = Outcome.Lose;
            else
                outcome = Outcome.Push;

            player.RecordResult(outcome);
            outcomes[player] = outcome;
        }
    }
}
