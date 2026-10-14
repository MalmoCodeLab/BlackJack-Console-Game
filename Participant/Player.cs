class Player : Participant
{
    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int Pushes { get; private set; }

    public int GamesPlayed => Wins + Losses + Pushes;

    public bool CanHit => !IsBust && Score < BlackjackValue;

    public Player(string name, int wins = 0, int losses = 0, int pushes = 0) : base(name)
    {
        Wins = wins;
        Losses = losses;
        Pushes = pushes;
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

    public string StatsText()
    {
        return $"{Name}  W: {Wins}  L: {Losses}  P: {Pushes}";
    }
}
