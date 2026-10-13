class Player : Participant
{
    // Stats everybody can read but only Player can edit
    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int Pushes { get; private set; }

    // Totalt antal spelade händer
    public int GamesPlayed => Wins + Losses + Pushes;

    public Player(string name, int wins = 0, int losses = 0, int pushes = 0) : base(name)
    {
        Wins = wins;
        Losses = losses;
        Pushes = pushes;
    }

    // Anropas av Round när en hand är avgjord
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

    // T.ex. "Niclas  W: 5  L: 3  P: 1"
    public string StatsText()
    {
        return $"{Name}  W: {Wins}  L: {Losses}  P: {Pushes}";
    }
}
