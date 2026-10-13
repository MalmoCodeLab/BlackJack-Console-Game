using System.Text.Json;

// Provides functionality for storing and loading player statistics.
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

    // Saves player statistics as indented JSON, replacing any existing results file.
    // <param name="results">The player statistics to save, keyed by player name.</param>
    // <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
    // <exception cref="IOException">The results file cannot be written.</exception>
    public void SaveResults(IReadOnlyDictionary<string, PlayerStatistics> results)
    {
        ArgumentNullException.ThrowIfNull(results);

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