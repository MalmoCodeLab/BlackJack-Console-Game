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
            case Outcome.Lose:
                Losses++;
                break;
            case Outcome.Push:
                Pushes++;
                break;
        }
    }

    public void RecordWin()
    {
        Wins++;
    }

    public void RecordLoss()
    {
        Losses++;
    }

    public void RecordPush()
    {
        Pushes++;
    }

    // Places a bet before the deal. Returns false if the amount is not allowed.
    public bool TryPlaceBet(int amount)
    {
        if (amount <= 0 || amount > Chips)
            return false;

        Chips -= amount;
        CurrentBet = amount;
        return true;
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
            case Outcome.Lose:
                break;                                    // bet is lost
        }
        RecordResult(outcome);
        CurrentBet = 0;
    }

}
