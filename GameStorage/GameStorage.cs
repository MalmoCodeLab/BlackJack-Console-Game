using System.Text.Json;
using System.Text;

// Loads player statistics and saves both machine-readable and human-readable results.
public class GameStorage
{
    private readonly string _filePath;
    private readonly bool _migrateLegacyResults;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    // Creates storage that reads and writes player results at the specified path.
    public GameStorage(string? filePath = null)
    {
        _migrateLegacyResults = filePath is null;
        _filePath = filePath ?? GetDefaultFilePath();
    }

    // Loads player statistics from the JSON results file.
    // <exception cref="IOException">The results file cannot be read.</exception>
    // <exception cref="System.Text.Json.JsonException">The results file contains invalid JSON.</exception>
    public Dictionary<string, PlayerStatistics> LoadResults()
    {
        if (File.Exists(_filePath))
            return ReadResults(_filePath);

        string legacyFilePath = Path.Combine(
            Environment.CurrentDirectory, "player-results.json");
        if (_migrateLegacyResults &&
            !PathsEqual(_filePath, legacyFilePath) &&
            File.Exists(legacyFilePath))
        {
            Dictionary<string, PlayerStatistics> legacyResults =
                ReadResults(legacyFilePath);
            SaveResults(legacyResults);
            return legacyResults;
        }

        return new Dictionary<string, PlayerStatistics>(StringComparer.OrdinalIgnoreCase);
    }

    private static string GetDefaultFilePath()
    {
        string localApplicationData =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localApplicationData))
            throw new InvalidOperationException(
                "The local application data folder could not be determined.");

        return Path.Combine(
            localApplicationData, "Mai26ma-Grupp1-BlackJack", "player-results.json");
    }

    private static bool PathsEqual(string firstPath, string secondPath)
    {
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return string.Equals(
            Path.GetFullPath(firstPath), Path.GetFullPath(secondPath), comparison);
    }

    private static Dictionary<string, PlayerStatistics> ReadResults(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var results = JsonSerializer.Deserialize<Dictionary<string, PlayerStatistics>>(stream);
        return results is null
            ? new Dictionary<string, PlayerStatistics>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, PlayerStatistics>(results, StringComparer.OrdinalIgnoreCase);
    }

    // Saves player statistics as JSON and a readable text report.
    // <param name="results">The player statistics to save, keyed by player name.</param>
    // <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
    // <exception cref="IOException">The results file cannot be written.</exception>
    public void SaveResults(IReadOnlyDictionary<string, PlayerStatistics> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        SaveJsonResults(results);
        SaveReadableResults(results);
    }

    // Deletes a player's statistics from the saved results and readable report.
    // Returns false when the player does not exist.
    public bool DeletePlayer(string playerName)
    {
        Dictionary<string, PlayerStatistics> results = LoadResults();
        if (!results.Remove(playerName))
            return false;

        SaveResults(results);
        return true;
    }

    private void SaveJsonResults(IReadOnlyDictionary<string, PlayerStatistics> results)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(_filePath))!;
        Directory.CreateDirectory(directory);

        var temporaryFilePath = Path.Combine(directory, $".{Path.GetFileName(_filePath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            using (var stream = File.Create(temporaryFilePath))
            {
                JsonSerializer.Serialize(stream, results, _jsonOptions);
            }

            File.Move(temporaryFilePath, _filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryFilePath))
            {
                File.Delete(temporaryFilePath);
            }
        }
    }

    private void SaveReadableResults(IReadOnlyDictionary<string, PlayerStatistics> results)
    {
        string textFilePath = Path.ChangeExtension(_filePath, ".txt");
        string directory = Path.GetDirectoryName(Path.GetFullPath(textFilePath))!;
        Directory.CreateDirectory(directory);

        var report = new StringBuilder()
            .AppendLine("BLACKJACK 21 - SPARAD SPELARSTATISTIK")
            .AppendLine("Sammanlagda resultat från tidigare spelsessioner.")
            .AppendLine()
            .AppendLine($"{"Spelare",-20} {"Vinster",8} {"Förluster",10} {"Lika",8} {"Rundor",8} {"Marker",8}")
            .AppendLine(new string('-', 67));

        foreach (var (name, statistics) in results.OrderBy(
            result => result.Key,
            StringComparer.OrdinalIgnoreCase))
        {
            int roundsPlayed = statistics.Wins + statistics.Losses + statistics.Pushes;
            report.AppendLine(
                $"{name,-20} {statistics.Wins,8} {statistics.Losses,10} {statistics.Pushes,8} {roundsPlayed,8} {statistics.Chips,8}");
        }

        string temporaryFilePath = Path.Combine(
            directory,
            $".{Path.GetFileName(textFilePath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            File.WriteAllText(temporaryFilePath, report.ToString(), Encoding.UTF8);
            File.Move(temporaryFilePath, textFilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryFilePath))
            {
                File.Delete(temporaryFilePath);
            }
        }
    }
}

// Tracks a player's total wins, losses, pushes and chips across rounds.
public class PlayerStatistics
{
    // Gets or sets the number of rounds the player has won.
    public int Wins { get; set; }

    // Gets or sets the number of rounds the player has lost.
    public int Losses { get; set; }

    // Gets or sets the number of tied rounds.
    public int Pushes { get; set; }

    // Gets or sets the player's chips. Old saves without chips get the starting amount.
    public int Chips { get; set; } = Player.StartingChips;
}