class Player : Participant
{
    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int Pushes { get; private set; }

    private Wallet Wallet { get; }

    public int Chips => Wallet.Chips;
    public int CurrentBet => Wallet.CurrentBet;
    public int InsuranceBet => Wallet.InsuranceBet;
    public int InsuranceCost => Wallet.InsuranceCost;
    public bool IsOutOfChips => Wallet.IsOutOfChips;

    public int GamesPlayed => Wins + Losses + Pushes;

    public bool CanHit => !IsBust && Score < Hand.BlackjackValue;

    // A computer player has a strategy; a human player does not.
    public IPlayerStrategy? Strategy { get; }
    public bool IsComputer => Strategy is not null;

    public Player(string name, int wins = 0, int losses = 0, int pushes = 0, int chips = Wallet.StartingChips,
        IPlayerStrategy? strategy = null) : base(name)
    {
        if (wins < 0 || losses < 0 || pushes < 0)
            throw new ArgumentException("Statistiken kan inte vara negativ.");

        Wins = wins;
        Losses = losses;
        Pushes = pushes;
        Wallet = new Wallet(chips);
        Strategy = strategy;
    }

    private void RecordResult(Outcome outcome)
    {
        switch (outcome)
        {
            case Outcome.Win:
            case Outcome.Blackjack:
                Wins++;
                break;
            case Outcome.Loss:
                Losses++;
                break;
            case Outcome.Push:
                Pushes++;
                break;
        }
    }

    public bool TryPlaceBet(int amount) => Wallet.TryPlaceBet(amount);
    public void CancelBet() => Wallet.CancelBet();
    public bool TryTakeInsurance() => Wallet.TryTakeInsurance();
    public void SettleInsurance(bool dealerHasBlackjack) => Wallet.SettleInsurance(dealerHasBlackjack);

    public void SettleBet(Outcome outcome)
    {
        Wallet.SettleBet(outcome);
        RecordResult(outcome);   // statistics are not money, so they stay in Player
    }
}