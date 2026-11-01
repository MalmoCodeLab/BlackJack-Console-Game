namespace Mai26ma_Grupp1_BlackJack.Tests;

// Plays rounds, saves the results, and loads them into a new session.
public class SaveAndLoadIntegrationTests : IDisposable
{
    private readonly string directory = Path.Combine(
        Path.GetTempPath(), "bj-integration-tests-" + Guid.NewGuid().ToString("N"));

    private string JsonPath => Path.Combine(directory, "results.json");

    public void Dispose()
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
    }

    private static PlayerStatistics ToStatistics(Player player) => new()
    {
        Wins = player.Wins,
        Losses = player.Losses,
        Pushes = player.Pushes,
        Chips = player.Chips
    };

    private static Player FromStatistics(string name, PlayerStatistics stats) =>
        new(name, stats.Wins, stats.Losses, stats.Pushes, stats.Chips);

    [Fact]
    public void PlayedSession_SurvivesSaveAndLoad()
    {
        TableSimulation table = new(new RuleSet(), 3, seed: 21);
        for (int i = 0; i < 50; i++)
            table.PlayRound();

        GameStorage storage = new(JsonPath);
        storage.SaveResults(table.Seated.ToDictionary(p => p.Name, ToStatistics));
        Dictionary<string, PlayerStatistics> loaded = new GameStorage(JsonPath).LoadResults();

        Assert.Equal(table.Seated.Count, loaded.Count);
        foreach (Player player in table.Seated)
        {
            Player restored = FromStatistics(player.Name, loaded[player.Name]);
            Assert.Equal(player.Wins, restored.Wins);
            Assert.Equal(player.Losses, restored.Losses);
            Assert.Equal(player.Pushes, restored.Pushes);
            Assert.Equal(player.Chips, restored.Chips);
        }
    }

    [Fact]
    public void LoadedPlayers_CanKeepPlayingAndSaveAgain()
    {
        TableSimulation first = new(new RuleSet(), 2, seed: 1);
        for (int i = 0; i < 20; i++)
            first.PlayRound();
        new GameStorage(JsonPath).SaveResults(first.Seated.ToDictionary(p => p.Name, ToStatistics));

        Dictionary<string, PlayerStatistics> loaded = new GameStorage(JsonPath).LoadResults();
        TableSimulation second = new(new RuleSet(), 2, seed: 2);
        second.Seated.Clear();
        second.Seated.AddRange(loaded.Select(pair => FromStatistics(pair.Key, pair.Value)));
        int gamesBefore = second.Seated.Sum(p => p.GamesPlayed);

        RoundRecord? record = second.PlayRound();
        new GameStorage(JsonPath).SaveResults(second.Seated.ToDictionary(p => p.Name, ToStatistics));
        Dictionary<string, PlayerStatistics> reloaded = new GameStorage(JsonPath).LoadResults();

        Assert.True(record is not null || second.Seated.All(p => p.IsOutOfChips));
        int expectedGames = gamesBefore + (record?.Players.Count ?? 0);
        Assert.Equal(expectedGames, reloaded.Values.Sum(s => s.Wins + s.Losses + s.Pushes));
        foreach (Player player in second.Seated)
            Assert.Equal(player.Chips, reloaded[player.Name].Chips);
    }

    [Fact]
    public void OldSaveWithoutChips_StartsWithDefaultChipsAndCanPlay()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(JsonPath, """{ "Anna": { "Wins": 4, "Losses": 2, "Pushes": 1 } }""");

        PlayerStatistics stats = new GameStorage(JsonPath).LoadResults()["Anna"];
        Player anna = FromStatistics("Anna", stats);
        RuleSet rules = new();
        Deck deck = new(rules);
        deck.CreateDeck();
        deck.ShuffleDeck();
        Round round = new(1, [anna], new Dealer(rules), deck, rules);

        Assert.True(anna.TryPlaceBet(10));
        round.StartRound();
        round.EndRound();

        Assert.Equal(8, anna.GamesPlayed);
        Assert.Equal(7, stats.Wins + stats.Losses + stats.Pushes);
    }

    [Fact]
    public void PlayerWhoWentBroke_IsStillSavedAndLoadedWithZeroChips()
    {
        Player broke = new("Broke", chips: 0);

        new GameStorage(JsonPath).SaveResults(
            new Dictionary<string, PlayerStatistics> { [broke.Name] = ToStatistics(broke) });
        Player restored = FromStatistics("Broke", new GameStorage(JsonPath).LoadResults()["Broke"]);

        Assert.Equal(0, restored.Chips);
        Assert.True(restored.IsOutOfChips);
    }
}
