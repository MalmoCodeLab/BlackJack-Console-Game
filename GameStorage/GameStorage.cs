using System.Text.Json;
using System.Text;

// Loads player statistics and saves both machine-readable and human-readable results.
public class GameStorage
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    
    // Creates storage that reads and writes player results at the specified path.
    public GameStorage(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(Environment.CurrentDirectory, "player-results.json");
    }
    
    // Loads player statistics from the JSON results file.
    // <exception cref="IOException">The results file cannot be read.</exception>
    // <exception cref="System.Text.Json.JsonException">The results file contains invalid JSON.</exception>
    public Dictionary<string, PlayerStatistics> LoadResults()
    {
        if (!File.Exists(_filePath))
        {
            return new Dictionary<string, PlayerStatistics>(StringComparer.OrdinalIgnoreCase);
        }

        using var stream = File.OpenRead(_filePath);
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
            .AppendLine($"{"Spelare",-20} {"Vinster",8} {"Förluster",10} {"Lika",8} {"Rundor",8}")
            .AppendLine(new string('-', 58));

        foreach (var (name, statistics) in results.OrderBy(
            result => result.Key,
            StringComparer.OrdinalIgnoreCase))
        {
            int roundsPlayed = statistics.Wins + statistics.Losses + statistics.Pushes;
            report.AppendLine(
                $"{name,-20} {statistics.Wins,8} {statistics.Losses,10} {statistics.Pushes,8} {roundsPlayed,8}");
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

// Tracks a player's total wins, losses, and pushes across rounds.
public class PlayerStatistics
{
    // Gets or sets the number of rounds the player has won.
    public int Wins { get; set; }

    // Gets or sets the number of rounds the player has lost.
    public int Losses { get; set; }

    // Gets or sets the number of tied rounds.
    public int Pushes { get; set; }
}