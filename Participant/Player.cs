public class Player : Participant
{
    public string Name { get; set; }

    // Statisik som alla kan läsa men bara Player kan ändra.
    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int Pushes { get; private set; }

    public Player (string name, int wins = 0, int losses = 0, int pushes = 0)
    {
        Name = name;
        Wins = wins;
        Losses = losses;
        Pushes = pushes;
    }
}