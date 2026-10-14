class Player : Participant
{

    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int Pushes { get; private set; }


    public int GamesPlayed => Wins + Losses + Pushes;

    public Player(string name, int wins = 0, int losses = 0, int pushes = 0) : base(name)
    {
        Wins = wins;
        Losses = losses;
        Pushes = pushes;
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

    public string StatsText()
    {
        return $"{Name}  W: {Wins}  L: {Losses}  P: {Pushes}";
    }

    internal void Win()
    {
        RecordWin();
    }

    internal void Lose()
    {
        RecordLoss();
    }

    internal void Push()
    {
        RecordPush();
    }
}
