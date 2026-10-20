using System.Text.Json;

internal sealed class PlayerSessionService
{
    private readonly GameStorage gameStorage;
    private readonly ConsoleUi ui;

    public PlayerSessionService(GameStorage gameStorage, ConsoleUi ui)
    {
        this.gameStorage = gameStorage;
        this.ui = ui;
    }

    public List<Player>? CreateNewPlayers(int playerCount)
    {
        Dictionary<string, PlayerStatistics> results;
        try
        {
            results = gameStorage.LoadResults();
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException ||
            ex is JsonException)
        {
            ui.ShowCenteredMessage(GameMessages.LoadPlayerStatisticsError);
            return null;
        }

        List<Player> players = new();
        HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);
        for (int i = 1; i <= playerCount; i++)
        {
            string playerName;
            while (true)
            {
                string? enteredName = ui.AskPlayerNameCentered(i);
                if (enteredName is null)
                    return null; // Esc: cancel and return to the main menu.

                playerName = enteredName.Trim();
                if (string.IsNullOrWhiteSpace(playerName))
                    continue; // The input screen displays the Swedish error.

                if (usedNames.Add(playerName))
                    break;

                ui.ShowCenteredMessage(GameMessages.DuplicatePlayerName);
            }

            if (results.TryGetValue(playerName, out PlayerStatistics? stats))
            {
                // A saved player without chips is not allowed to play.
                if (stats.Chips <= 0)
                {
                    ui.ShowCenteredMessage(GameMessages.SavedPlayerOutOfChips(playerName));
                    return null;
                }
                players.Add(CreateSavedPlayer(playerName, stats));
            }
            else
                players.Add(new Player(playerName));
        }

        return players;
    }

    public List<Player>? LoadPlayers()
    {
        Dictionary<string, PlayerStatistics> results;
        try
        {
            results = gameStorage.LoadResults();
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException ||
            ex is JsonException)
        {
            ui.ShowCenteredMessage(GameMessages.LoadSavedResultsError);
            return null;
        }

        if (results.Count == 0)
        {
            ui.ShowCenteredMessage(GameMessages.NoSavedGames);
            return null;
        }

        List<string>? selectedNames = ui.SelectSavedPlayers(results, DeletePlayer);
        if (selectedNames is null || selectedNames.Count == 0)
            return null;

        List<Player> players = new();
        foreach (string selectedName in selectedNames)
        {
            PlayerStatistics stats = results[selectedName];

            // A saved player without chips is not allowed to play.
            if (stats.Chips <= 0)
            {
                ui.ShowCenteredMessage(GameMessages.SavedPlayerOutOfChips(selectedName));
                return null;
            }
            players.Add(CreateSavedPlayer(selectedName, stats));
        }
        return players;
    }

    // Creates a player from saved statistics, with the saved chips.
    private static Player CreateSavedPlayer(string name, PlayerStatistics stats)
    {
        return new Player(name, stats.Wins, stats.Losses, stats.Pushes, stats.Chips);
    }

    public bool SaveResults(List<Player> players)
    {
        try
        {
            Dictionary<string, PlayerStatistics> results = gameStorage.LoadResults();
            foreach (Player player in players)
            {
                results[player.Name] = new PlayerStatistics
                {
                    Wins = player.Wins,
                    Losses = player.Losses,
                    Pushes = player.Pushes,
                    Chips = player.Chips
                };
            }
            gameStorage.SaveResults(results);
            return true;
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException ||
            ex is JsonException)
        {
            return false;
        }
    }

    private bool DeletePlayer(string playerName)
    {
        try
        {
            return gameStorage.DeletePlayer(playerName);
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException ||
            ex is JsonException)
        {
            return false;
        }
    }

    public bool SaveGame(List<Player> players)
    {
        bool saved = SaveResults(players);
        ui.ShowCenteredMessage(saved
            ? GameMessages.SaveStatisticsSuccess
            : GameMessages.SaveStatisticsError);
        return saved;
    }
}
