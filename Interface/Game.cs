using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Spectre.Console;

public class Game
{
    private readonly ConsoleUi ui = new ConsoleUi();
    private readonly GameStorage gameStorage = new GameStorage();

    // Runs the main menu.
    public void Run()
    {
        ui.HardClear();
        bool running = true;
        while (running)
        {
            string choice = ui.ShowMenu();
            switch (choice)
            {
                case GameMessages.NewGame:
                    StartNewGame();
                    break;
                case GameMessages.LoadGame:
                    LoadGame();
                    break;
                case GameMessages.ShowRules:
                    ui.ShowRules();
                    break;
                case GameMessages.ShowCredits:
                    ui.ShowCredits();
                    break;
                case GameMessages.Exit:
                    running = false;
                    break;
            }
        }
        Console.Clear();
    }

    // Creates a new Blackjack game.
    private void StartNewGame()
    {
        Deck deck = new Deck();
        deck.CreateDeck();
        deck.ShuffleDeck();
        int playerCount = ui.AskPlayerCountCentered();
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
            ShowMessage(GameMessages.LoadPlayerStatisticsError);
            return;
        }
    
    // Ask every player for a name. Only Player 1 has saved statistics.
        List<Player> players = new();
        HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);
        for (int i = 1; i <= playerCount; i++)
        {
            string name;
            while (true)
            {
                name = ui.AskPlayerNameCentered(i);
                if (string.IsNullOrWhiteSpace(name))
                    name = GameMessages.PlayerName(i);
                if (usedNames.Add(name))
                    break;
                ShowMessage("Namnet används redan. Välj ett annat namn.");
            }
            if (i == 1)
            {
                if (!results.ContainsKey(name) && results.Count >= 3)
                {
                    ShowMessage("Du har redan tre sparade spelare. Ta bort en under Ladda spel först.");
                    return;
                }
                if (results.TryGetValue(name, out PlayerStatistics? stats))
                    players.Add(new Player(name, stats.Wins, stats.Losses, stats.Pushes));
                else
                    players.Add(new Player(name));
            }
            else
                players.Add(new Player(name));
        }
        BettingUI.Reset();
        DealNewHandAndShow(players, deck);
    }

    // Starts fresh rounds with the same players and their saved statistics.
    private void DealNewHandAndShow(List<Player> players, Deck deck)
    {
        bool newRound;
        int roundNumber = 1;
        Dealer dealer = new Dealer();
        do
        {
            Round round = new Round(roundNumber++, players, dealer, deck);
            round.StartRound();
            newRound = ShowGame(round);
        }
        while (newRound);
    }

    // Displays the Blackjack table and handles input.
    private bool ShowGame(Round round)
    {
        List<Player> players = round.Players;
        Dealer dealer = round.Dealer;
        Deck deck = round.Deck;
    
    // The third action activates the chips already drawn on the table.
        string[] gameOptions = GameMessages.GameOptions;
        string[] menuOptions = GameMessages.InGameMenuOptions;
        int selectedGameOption = 0;
        int selectedMenuOption = 0;
        bool sideMenuActive = false;
        bool bettingMode = false;
        int currentPlayerIndex = 0;
        string[] playerStatuses = new string[players.Count];
        for (int i = 0; i < players.Count; i++) playerStatuses[i] = "VÄNTAR";
        playerStatuses[0] = "DIN TUR";
        bool playing = true;
        bool roundFinished = false;
        bool startNewRound = false;
        string? tableNotice = players[0].HasBlackjack ? $"{players[0].Name.ToUpperInvariant()} – NATURAL BLACKJACK!" : null;
        DateTime noticeUntil = tableNotice == null ? DateTime.MinValue : DateTime.UtcNow.AddSeconds(2.4);
        void AdvanceTurn()
        {
            if (currentPlayerIndex < players.Count - 1)
            {
                currentPlayerIndex++;
                selectedGameOption = 0;
                playerStatuses[currentPlayerIndex] = "DIN TUR";
                if (players[currentPlayerIndex].HasBlackjack)
                {
                    tableNotice = $"{players[currentPlayerIndex].Name.ToUpperInvariant()} – NATURAL BLACKJACK!";
                    noticeUntil = DateTime.UtcNow.AddSeconds(2.4);
                }
            }
            else
            {
                round.EndRound();
                // Display the final outcome for every player; Round owns the game rules.
                for (int i = 0; i < players.Count; i++)
                {
                    playerStatuses[i] = round.Outcomes[players[i]] switch
                    {
                        Outcome.Blackjack => "VANN",
                        Outcome.Win => "VANN",
                        Outcome.Lose => "FÖRLORADE",
                        Outcome.Push => "OAVGJORT",
                        _ => "KLAR"
                    };
                }
                roundFinished = true;
                sideMenuActive = true;
                selectedMenuOption = 0;
                Player mainPlayer = players[0];
                tableNotice = round.Outcomes[mainPlayer] switch
                {
                    Outcome.Blackjack => "NATURAL BLACKJACK – DU VANN!",
                    Outcome.Win => "DU VANN!",
                    Outcome.Lose when mainPlayer.IsBust => "TJOCK – DU FÖRLORADE!",
                    Outcome.Lose when dealer.HasBlackjack => "DEALERN FICK BLACKJACK – DU FÖRLORADE!",
                    Outcome.Lose => "DU FÖRLORADE!",
                    Outcome.Push => "OAVGJORT!",
                    _ => "RESULTAT SAKNAS"
                };
                noticeUntil = DateTime.MinValue;
                SaveResultsOnly(players);
            }
        }
        ui.HardClear();
        try
        {
            Console.CursorVisible = false;
            AnsiConsole.Live(new Text(""))
                .AutoClear(false)
                .Start(context =>
                {
                    while (playing)
                    {
                int totalHeight = 27; // Same compact table size for 1–5 players.
                ui.CheckWidth();
                        // Keep the table compact and update it without clearing the console.
                int tableHeight = totalHeight - 3;
                var layout = new Layout("Root")
                    .Size(totalHeight)
                    .SplitColumns(
                        new Layout("Table").Ratio(4),
                        new Layout("Menu").Ratio(1));
                layout["Table"].SplitRows(
                    new Layout("BlackjackTable").Size(tableHeight),
                    new Layout("PlayerActions").Size(3));
                string? visibleNotice = tableNotice;
                layout["BlackjackTable"].Update(
                    ui.BuildBlackjackTable(players, dealer, tableHeight, visibleNotice, playerStatuses));
                layout["PlayerActions"].Update(
                    ui.BuildPlayerActions(
                        gameOptions, selectedGameOption,
                        sideMenuActive, roundFinished, bettingMode));
            
    // Extra room for Balance and Current Bet.
                int menuHeight = totalHeight - 3;
                layout["Menu"].SplitRows(
                    new Layout("MenuContent").Size(menuHeight),
                    new Layout("MenuSpace").Ratio(1));
                layout["MenuContent"].Update(
                    ui.BuildGameMenu(menuOptions, selectedMenuOption,
                        players, sideMenuActive, roundFinished, playerStatuses));
                layout["MenuSpace"].Update(new Text(""));
            
    // Batch the redraw so VS Code does not display partially painted frames.
                Console.Write("\u001b[?2026h");
                try
                {
                    context.UpdateTarget(layout);
                    context.Refresh();
                }
                finally
                {
                    Console.Write("\u001b[?2026l");
                }
                ConsoleKey key = ReadTableKey(totalHeight, tableNotice != null && noticeUntil > DateTime.UtcNow
                    ? Math.Max(1, (int)(noticeUntil - DateTime.UtcNow).TotalMilliseconds) : -1);
            
    // Finish the notification regardless of whether a key was pressed.
                if (tableNotice != null && noticeUntil != DateTime.MinValue)
                {
                    if (DateTime.UtcNow >= noticeUntil)
                    {
                        tableNotice = null;
                        noticeUntil = DateTime.MinValue;
                        if (!roundFinished && players[currentPlayerIndex].HasBlackjack)
                            {
                            playerStatuses[currentPlayerIndex] = "BLACKJACK";
                            AdvanceTurn();
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                if (key == ConsoleKey.NoName) continue;
            
    // When betting, arrows control the chips rather than Hit/Stand.
                if (bettingMode)
                {
                    if (key == ConsoleKey.Escape)
                    {
                        bettingMode = false;
                        continue;
                    }
                    if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow)
                    {
                    
    // Leave chip selection and handle the arrow in the game menu.
                        bettingMode = false;
                    }
                    else
                    {
                        if (key == ConsoleKey.LeftArrow ||
                            key == ConsoleKey.RightArrow || key == ConsoleKey.Enter)
                            BettingUI.HandleKey(key);
                        continue;
                    }
                }
            
    // Once the round is over, only the right-hand menu is interactive.
                if (roundFinished)
                {
                    sideMenuActive = true;
                    bettingMode = false;
                }
                switch (key)
                {
                    case ConsoleKey.Escape:
                    
    // After the round, keep focus on the only available menu.
                        if (!roundFinished)
                        {
                            sideMenuActive = false;
                            selectedGameOption = 0;
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (roundFinished) break;
                        sideMenuActive = false;
                        selectedGameOption = Math.Max(0, selectedGameOption - 1);
                        break;
                    case ConsoleKey.RightArrow:
                        if (roundFinished) break;
                        sideMenuActive = false;
                        selectedGameOption = Math.Min(
                            gameOptions.Length - 1, selectedGameOption + 1);
                        break;
                    case ConsoleKey.UpArrow:
                        if (roundFinished) sideMenuActive = true;
                        if (!sideMenuActive)
                        {
                            sideMenuActive = true;
                            selectedMenuOption = 0;
                        }
                        else
                        {
                            selectedMenuOption =
                                (selectedMenuOption - 1 + menuOptions.Length)
                                % menuOptions.Length;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (roundFinished) sideMenuActive = true;
                        if (sideMenuActive)
                        {
                            if (selectedMenuOption < menuOptions.Length - 1)
                            {
                                selectedMenuOption++;
                            }
                            else
                            {
                                if (!roundFinished)
                                {
                                    sideMenuActive = false;
                                    selectedGameOption = 0;
                                }
                                else selectedMenuOption = 0;
                            }
                        }
                        break;
                    case ConsoleKey.Enter:
                        if (roundFinished) sideMenuActive = true;
                        if (sideMenuActive)
                        {
                            if (selectedMenuOption == 0)
                            {
                            
    // Start a new deal with the same players and statistics.
                                startNewRound = true;
                                playing = false;
                            }
                            else if (selectedMenuOption == 1)
                            {
                            
    // Leave the table and return to the main menu.
                                playing = false;
                            }
                            else if (selectedMenuOption == 2)
                            {
                            
    // Save only Player 1 before leaving the table.
                                if (SaveGame(players))
                                    playing = false;
                            }
                        }
                        else if (!roundFinished)
                        {
                        
    // Bet is a temporary UI test, not a Round/Player change.
                            if (selectedGameOption == 2)
                            {
                                bettingMode = true;
                                break;
                            }
                            Player currentPlayer = players[currentPlayerIndex];
                            if (currentPlayer.HasBlackjack)
                            {
                                playerStatuses[currentPlayerIndex] = "BLACKJACK";
                                AdvanceTurn();
                                break;
                            }
                            if (selectedGameOption == 0)
                            {
                                currentPlayer.ReceiveCard(deck);
                            }
                        
    // Automatically finish this player’s turn at 21 or after a bust.
                        
    // The dealer still plays before the final result is decided.
                            if (selectedGameOption == 1 || currentPlayer.IsBust || currentPlayer.Score == 21)
                            {
                                playerStatuses[currentPlayerIndex] = currentPlayer.IsBust ? "TJOCK" :
                                    currentPlayer.HasBlackjack ? "BLACKJACK" :
                                    currentPlayer.Score == 21 ? "21 POÄNG" : "STANNAT";
                                AdvanceTurn();
                            }
                        }
                        break;
                }
                    }
                });
        }
        finally
        {
            Console.CursorVisible = true;
            AnsiConsole.Clear();
        }
    
    // Keep the final cards on the table until the player chooses an option.
        return startNewRound;
    }

    // Watches terminal width and height while waiting for a key without redrawing the table.
    private ConsoleKey ReadTableKey(int requiredHeight, int timeoutMs = -1)
    {
        DateTime until = timeoutMs < 0 ? DateTime.MaxValue : DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (true)
        {
            if (Console.WindowWidth < ConsoleUi.MinWidth ||
                Console.WindowHeight < requiredHeight)
            {
            
    // Restore the cursor before displaying the resize warning.
                Console.CursorVisible = true;
                ui.CheckWidth(requiredHeight);
                ui.HardClear(false);
                Console.CursorVisible = false;
                return ConsoleKey.NoName; // Redraw the table at its new size.
            }
            if (Console.KeyAvailable)
                return Console.ReadKey(true).Key;
            if (DateTime.UtcNow >= until) return ConsoleKey.NoName;
            System.Threading.Thread.Sleep(35);
        }
    }

    // Saves player statistics without interrupting the game UI.
    private bool SaveResultsOnly(List<Player> players)
    {
        try
        {
        
    // Store only Player 1. Other players are temporary.
            Player player = players[0];
            Dictionary<string, PlayerStatistics> results = new()
            {
                [player.Name] = new PlayerStatistics
                {
                    Wins = player.Wins,
                    Losses = player.Losses,
                    Pushes = player.Pushes
                }
            };
            Dictionary<string, PlayerStatistics> existing = gameStorage.LoadResults();
            if (!existing.ContainsKey(player.Name) && existing.Count >= 3)
                return false;
            foreach (var entry in results) existing[entry.Key] = entry.Value;
            gameStorage.SaveResults(existing);
            return true;
        }
        catch (Exception ex) when (
            ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            return false;
        }
    }

    // Saves statistics when the player chooses Save and Quit.
    private bool SaveGame(List<Player> players)
    {
        bool saved = SaveResultsOnly(players);
        ShowMessage(saved ? GameMessages.SaveStatisticsSuccess :
                            GameMessages.SaveStatisticsError);
        return saved;
    }

    // Shows up to three saved profiles. Delete removes a profile after confirmation.

    // The storage class itself remains unchanged.
    private void LoadGame()
    {
        Dictionary<string, PlayerStatistics> results;
        try { results = gameStorage.LoadResults(); }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            ShowMessage(GameMessages.LoadSavedResultsError);
            return;
        }
        if (results.Count == 0)
        {
            ShowMessage(GameMessages.NoSavedGames);
            return;
        }
        List<string> names = new(results.Keys);
        names.Sort(StringComparer.OrdinalIgnoreCase);
        int selected = 0;
        bool cancelled = false, confirmingDelete = false;
        string? error = null;
        AnsiConsole.Clear();
        AnsiConsole.Live(ui.BuildSavedPlayersMenu(names, results, selected, confirmingDelete)).Start(context =>
        {
            context.Refresh();
            while (names.Count > 0)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                if (confirmingDelete)
                {
                    if (key == ConsoleKey.Enter || key == ConsoleKey.Y || key == ConsoleKey.J)
                    {
                        string name = names[selected];
                    
    // Save a copy so the UI never claims deletion if writing fails.
                        var updated = new Dictionary<string, PlayerStatistics>(results, StringComparer.OrdinalIgnoreCase);
                        updated.Remove(name);
                        try { gameStorage.SaveResults(updated); }
                        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
                        {
                            error = "Kunde inte ta bort sparfilen.";
                            cancelled = true;
                            break;
                        }
                        results = updated;
                        names.RemoveAt(selected);
                        if (names.Count == 0) { cancelled = true; break; }
                        selected = Math.Min(selected, names.Count - 1);
                    }
                    confirmingDelete = false;
                }
                else if (key == ConsoleKey.UpArrow)
                    selected = (selected - 1 + names.Count) % names.Count;
                else if (key == ConsoleKey.DownArrow)
                    selected = (selected + 1) % names.Count;
                else if (key == ConsoleKey.Delete)
                    confirmingDelete = true;
                else if (key == ConsoleKey.Escape)
                {
                    cancelled = true;
                    break;
                }
                else if (key == ConsoleKey.Enter)
                    break;
                else
                    continue;
                context.UpdateTarget(ui.BuildSavedPlayersMenu(names, results, selected, confirmingDelete));
                context.Refresh();
            }
        });
        if (error != null) ShowMessage(error);
        if (cancelled || names.Count == 0) return;
        string selectedName = names[selected];
        PlayerStatistics stats = results[selectedName];
        List<Player> players = new() { new Player(selectedName, stats.Wins, stats.Losses, stats.Pushes) };
        BettingUI.Reset();
        Deck deck = new Deck();
        deck.CreateDeck();
        deck.ShuffleDeck();
        DealNewHandAndShow(players, deck);
    }

    // Displays a centered message and waits for input.
    private void ShowMessage(string message)
    {
        ui.ShowCenteredMessage(message);
    }
}
