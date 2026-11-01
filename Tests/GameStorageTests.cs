using System.Text.Json;

namespace Mai26ma_Grupp1_BlackJack.Tests;

public class GameStorageTests : IDisposable
{
    private readonly string directory = Path.Combine(
        Path.GetTempPath(), "bj-storage-tests-" + Guid.NewGuid().ToString("N"));

    private string JsonPath => Path.Combine(directory, "results.json");

    public void Dispose()
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
    }

    private static Dictionary<string, PlayerStatistics> Sample() => new()
    {
        ["Anna"] = new PlayerStatistics { Wins = 3, Losses = 2, Pushes = 1, Chips = 1250 },
        ["Bo"] = new PlayerStatistics { Wins = 0, Losses = 5, Pushes = 0, Chips = 400 }
    };

    [Fact]
    public void LoadResults_MissingFile_ReturnsEmpty()
    {
        GameStorage storage = new(JsonPath);

        Assert.Empty(storage.LoadResults());
    }

    [Fact]
    public void SaveThenLoad_RoundTripsAllFields()
    {
        GameStorage storage = new(JsonPath);

        storage.SaveResults(Sample());
        Dictionary<string, PlayerStatistics> loaded = storage.LoadResults();

        Assert.Equal(2, loaded.Count);
        Assert.Equal(3, loaded["Anna"].Wins);
        Assert.Equal(2, loaded["Anna"].Losses);
        Assert.Equal(1, loaded["Anna"].Pushes);
        Assert.Equal(1250, loaded["Anna"].Chips);
        Assert.Equal(400, loaded["Bo"].Chips);
    }

    [Fact]
    public void LoadResults_NamesAreCaseInsensitive()
    {
        GameStorage storage = new(JsonPath);
        storage.SaveResults(Sample());

        Assert.True(storage.LoadResults().ContainsKey("anna"));
    }

    [Fact]
    public void SaveResults_CreatesMissingDirectoryAndReadableReport()
    {
        GameStorage storage = new(JsonPath);

        storage.SaveResults(Sample());

        Assert.True(File.Exists(JsonPath));
        string report = File.ReadAllText(Path.ChangeExtension(JsonPath, ".txt"));
        Assert.Contains("Anna", report);
        Assert.Contains("Bo", report);
        Assert.Contains("1250", report);
    }

    [Fact]
    public void SaveResults_ReportIsSortedByName()
    {
        GameStorage storage = new(JsonPath);
        storage.SaveResults(new Dictionary<string, PlayerStatistics>
        {
            ["Zed"] = new(),
            ["adam"] = new()
        });

        string report = File.ReadAllText(Path.ChangeExtension(JsonPath, ".txt"));

        Assert.True(report.IndexOf("adam", StringComparison.Ordinal) <
                    report.IndexOf("Zed", StringComparison.Ordinal));
    }

    [Fact]
    public void SaveResults_LeavesNoTemporaryFiles()
    {
        GameStorage storage = new(JsonPath);

        storage.SaveResults(Sample());
        storage.SaveResults(Sample());

        Assert.Empty(Directory.GetFiles(directory, "*.tmp"));
    }

    [Fact]
    public void SaveResults_Null_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new GameStorage(JsonPath).SaveResults(null!));

    [Fact]
    public void LoadResults_OldSaveWithoutChips_GetsStartingChips()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(JsonPath, """{ "Anna": { "Wins": 1, "Losses": 0, "Pushes": 0 } }""");

        Dictionary<string, PlayerStatistics> loaded = new GameStorage(JsonPath).LoadResults();

        Assert.Equal(Wallet.StartingChips, loaded["Anna"].Chips);
    }

    [Fact]
    public void LoadResults_InvalidJson_Throws()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(JsonPath, "not json");

        Assert.ThrowsAny<JsonException>(() => new GameStorage(JsonPath).LoadResults());
    }

    [Fact]
    public void LoadResults_JsonNull_ReturnsEmpty()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(JsonPath, "null");

        Assert.Empty(new GameStorage(JsonPath).LoadResults());
    }

    [Fact]
    public void DeletePlayer_RemovesOnlyThatPlayer()
    {
        GameStorage storage = new(JsonPath);
        storage.SaveResults(Sample());

        Assert.True(storage.DeletePlayer("anna"));

        Dictionary<string, PlayerStatistics> loaded = storage.LoadResults();
        Assert.False(loaded.ContainsKey("Anna"));
        Assert.True(loaded.ContainsKey("Bo"));
        Assert.DoesNotContain("Anna", File.ReadAllText(Path.ChangeExtension(JsonPath, ".txt")));
    }

    [Fact]
    public void DeletePlayer_UnknownName_ReturnsFalse()
    {
        GameStorage storage = new(JsonPath);
        storage.SaveResults(Sample());

        Assert.False(storage.DeletePlayer("Nobody"));
        Assert.Equal(2, storage.LoadResults().Count);
    }
}
