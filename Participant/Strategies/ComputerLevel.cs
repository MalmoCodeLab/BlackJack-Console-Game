// The difficulty levels you can choose for a computer player.
public enum ComputerLevel
{
    Easy,
    Medium,
    Hard,
    Expert
}

static class ComputerLevels
{
    public static readonly ComputerLevel[] All =
        { ComputerLevel.Easy, ComputerLevel.Medium, ComputerLevel.Hard, ComputerLevel.Expert };

    public static IPlayerStrategy CreateStrategy(ComputerLevel level) => level switch
    {
        ComputerLevel.Easy => new BeginnerStrategy(),
        ComputerLevel.Medium => new SimpleStrategy(),
        ComputerLevel.Hard => new BasicStrategy(),
        _ => new CountingStrategy()
    };

    public static string Label(ComputerLevel level) => level switch
    {
        ComputerLevel.Easy => "Lätt",
        ComputerLevel.Medium => "Medel",
        ComputerLevel.Hard => "Svår",
        _ => "Expert"
    };

    public static string Description(ComputerLevel level) => level switch
    {
        ComputerLevel.Easy => "Rädd för att bli tjock, stannar på 12",
        ComputerLevel.Medium => "Tar kort under 17, som givaren",
        ComputerLevel.Hard => "Spelar efter grundstrategin",
        _ => "Grundstrategi + kortränkning"
    };
}
