class Round
{
    private bool started;
    private bool ended;
    private readonly Dictionary<Player, Outcome> outcomes = new();

    public int RoundNumber { get; }
    public List<Player> Players { get; }
    public Dealer Dealer { get; }
    public Deck Deck { get; private set; } = new Deck();
    public IReadOnlyDictionary<Player, Outcome> Outcomes => outcomes;

    public Round(int roundNumber, List<Player> players, Dealer dealer)
    {
        if (roundNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(roundNumber));
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(dealer);
        if (players.Count == 0)
            throw new ArgumentException("A round must have at least one player.", nameof(players));
        if (players.Exists(player => player is null))
            throw new ArgumentException("Players cannot contain null entries.", nameof(players));

        RoundNumber = roundNumber;
        Players = players;
        Dealer = dealer;
    }

    public void StartRound()
    {
        if (started)
            throw new InvalidOperationException("This round has already started.");

        foreach (Player player in Players)
            player.NewRound();
        Dealer.NewRound();

        Deck = new Deck();
        Deck.CreateDeck();
        Deck.ShuffleDeck();

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
            if (player.IsBust || (Dealer.HasBlackjack && !player.HasBlackjack))
            {
                player.Lose();
                outcomes[player] = Outcome.Loss;
            }
            else if (Dealer.IsBust || (player.HasBlackjack && !Dealer.HasBlackjack))
            {
                player.Win();
                outcomes[player] = Outcome.Win;
            }
            else if (player.Score > Dealer.Score)
            {
                player.Win();
                outcomes[player] = Outcome.Win;
            }
            else if (player.Score < Dealer.Score)
            {
                player.Lose();
                outcomes[player] = Outcome.Loss;
            }
            else
            {
                player.Push();
                outcomes[player] = Outcome.Push;
            }
        }
    }
}
