public class Player : Participant
{

    // Stats everybody can read but only Player can edit
    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int Pushes { get; private set; }
    public Player(string name, int wins = 0, int losses = 0, int pushes = 0) : base(name)
    {
        Wins = wins;
        Losses = losses;
        Pushes = pushes;
    }
}