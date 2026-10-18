class Player : Participant
{
    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int Pushes { get; private set; }

    public const int StartingChips = 1000;

    public int Chips { get; private set; }
    public int CurrentBet { get; private set; }
    public bool IsOutOfChips => Chips <= 0;

    public int GamesPlayed => Wins + Losses + Pushes;

    public bool CanHit => !IsBust && Score < BlackjackValue;

    public Player(string name, int wins = 0, int losses = 0, int pushes = 0, int chips = StartingChips) : base(name)
    {
        if (wins < 0 || losses < 0 || pushes < 0)
            throw new ArgumentException("Statistiken kan inte vara negativ.");
        if (chips < 0)
            throw new ArgumentOutOfRangeException(nameof(chips), "Marker kan inte vara negativa.");

        Wins = wins;
        Losses = losses;
        Pushes = pushes;
        Chips = chips;
    }

    public void RecordResult(Outcome outcome)
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

    // Places a bet before the deal. Returns false if the amount is not allowed.
    public bool TryPlaceBet(int amount)
    {
        if (amount <= 0 || amount > Chips)
            return false;

        Chips -= amount;
        CurrentBet += amount;
        return true;
    }

    // Gives the whole bet back to the player.
    public void CancelBet()
    {
        Chips += CurrentBet;
        CurrentBet = 0;
    }

    // Pays out the bet. The bet was already taken from Chips when it was placed.
    public void SettleBet(Outcome outcome)
    {
        switch (outcome)
        {
            case Outcome.Win:
                Chips += CurrentBet * 2;                  // bet back + 1:1
                break;
            case Outcome.Blackjack:
                Chips += CurrentBet + CurrentBet * 3 / 2; // bet back + 3:2
                break;
            case Outcome.Push:
                Chips += CurrentBet;                      // bet back
                break;
            case Outcome.Loss:
                break;                                    // bet is lost
        }
        RecordResult(outcome);
        CurrentBet = 0;
    }
}