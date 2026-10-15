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
        // Only Player 1 gets a custom name and persistent statistics.
        string playerOneName = AskPlayerOneNameCentered();
        if (string.IsNullOrWhiteSpace(playerOneName))
            playerOneName = GameMessages.DefaultPlayerName;
        List<Player> players = new();
        if (results.TryGetValue(playerOneName, out PlayerStatistics? savedStats))
            players.Add(new Player(playerOneName, savedStats.Wins, savedStats.Losses, savedStats.Pushes));
        else
            players.Add(new Player(playerOneName));
        for (int i = 2; i <= playerCount; i++)
            players.Add(new Player(GameMessages.PlayerName(i)));
        DealNewHandAndShow(players, deck);
    }
    // Centers the name prompt horizontally and vertically in the console.
    private string AskPlayerOneNameCentered()
    {
        AnsiConsole.Clear();
        const string prompt = GameMessages.EnterPlayerNamePrompt;
        int width = Math.Max(1, Console.WindowWidth);
        int height = Math.Max(1, Console.WindowHeight);
        int left = Math.Max(0, (width - prompt.Length - 16) / 2);
        int top = Math.Max(0, height / 2);
        try
        {
            Console.SetCursorPosition(left, top);
        }
        catch (ArgumentOutOfRangeException)
        {
            // Use the current cursor position if the terminal is resized.
        }
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(prompt);
        Console.ResetColor();
        return (Console.ReadLine() ?? "").Trim();
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
        bool playing = true;
        bool roundFinished = false;
        string? roundSummary = null;
        bool startNewRound = false;
        ui.HardClear();
        try
        {
            Console.CursorVisible = false;
            while (playing)
            {
                int totalHeight = 27; // Same compact table size for 1–5 players.
                ui.CheckWidth();
                // Reuse the original layout, without clearing on every key press.
                Console.SetCursorPosition(0, 0);
                // Keep the table compact while keeping the action buttons directly below it.
                // A fixed compact table avoids unpainted layout rows (black stripes).
                int tableHeight = totalHeight - 3;
                var layout = new Layout("Root")
                    .Size(totalHeight)
                    .SplitColumns(
                        new Layout("Table").Ratio(4),
                        new Layout("Menu").Ratio(1));
                layout["Table"].SplitRows(
                    new Layout("BlackjackTable").Size(tableHeight),
                    new Layout("PlayerActions").Size(3));
                layout["BlackjackTable"].Update(
                    ui.BuildBlackjackTable(players, dealer, tableHeight));
                layout["PlayerActions"].Update(
                    BuildPlayerActions(
                        gameOptions, selectedGameOption,
                        sideMenuActive, roundFinished, bettingMode));
                // Extra room for Balance and Current Bet.
                int menuHeight = Math.Min(totalHeight - 3, 15 + players.Count);
                layout["Menu"].SplitRows(
                    new Layout("MenuContent").Size(menuHeight),
                    new Layout("MenuSpace").Ratio(1));
                layout["MenuContent"].Update(
                    BuildGameMenu(menuOptions, selectedMenuOption,
                        players, sideMenuActive));
                layout["MenuSpace"].Update(new Text(""));
                AnsiConsole.Write(layout);
                Console.Write("\u001b[J");
                ConsoleKey key = ReadTableKey(totalHeight);
                if (key == ConsoleKey.NoName)
                    continue;
                // When betting, arrows control the chips rather than Hit/Stand.
                if (bettingMode)
                {
                    if (key == ConsoleKey.Escape)
                    {
                        bettingMode = false;
                    }
                    else if (key == ConsoleKey.LeftArrow ||
                             key == ConsoleKey.RightArrow ||
                             key == ConsoleKey.Enter)
                    {
                        BettingUI.HandleKey(key);
                    }
                    continue;
                }
                switch (key)
                {
                    case ConsoleKey.LeftArrow:
                        sideMenuActive = false;
                        selectedGameOption = Math.Max(0, selectedGameOption - 1);
                        break;
                    case ConsoleKey.RightArrow:
                        sideMenuActive = false;
                        selectedGameOption = Math.Min(
                            gameOptions.Length - 1, selectedGameOption + 1);
                        break;
                    case ConsoleKey.UpArrow:
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
                        if (sideMenuActive)
                        {
                            if (selectedMenuOption < menuOptions.Length - 1)
                            {
                                selectedMenuOption++;
                            }
                            else
                            {
                                sideMenuActive = false;
                                selectedGameOption = 0;
                            }
                        }
                        break;
                    case ConsoleKey.Enter:
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
                            if (selectedGameOption == 0)
                            {
                                currentPlayer.ReceiveCard(deck);
                            }
                            // Automatically finish this player’s turn at 21 or after a bust.
                            // The dealer still plays before the final result is decided.
                            if (selectedGameOption == 1 || currentPlayer.IsBust || currentPlayer.Score == 21)
                            {
                                if (currentPlayerIndex < players.Count - 1)
                                {
                                    currentPlayerIndex++;
                                    selectedGameOption = 0;
                                }
                                else
                                {
                                    roundFinished = true;
                                    round.EndRound();
                                    roundSummary = ResolveRound(round);
                                    // Save the updated statistics as soon as the round ends.
                                    if (!SaveResultsOnly(players))
                                        roundSummary = GameMessages.AppendSaveResultsError(roundSummary);
                                    playing = false;
                                }
                            }
                        }
                        break;
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;
            AnsiConsole.Clear();
        }
        // Keep the player at the table and offer a fresh deal after the result.
        if (roundSummary != null)
            return ShowRoundEndMenu(roundSummary, players);
        return startNewRound;
    }
    // Watches terminal width and height while waiting for a key without redrawing the table.
    private ConsoleKey ReadTableKey(int requiredHeight)
    {
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
            System.Threading.Thread.Sleep(75);
        }
    }
    // Formats the outcomes that the round has already resolved and recorded.
    private string ResolveRound(Round round)
    {
        List<string> messages = new();
        foreach (Player player in round.Players)
        {
            string outcome = round.Outcomes[player] switch
            {
                Outcome.Blackjack => GameMessages.PlayerBlackjackOutcome,
                Outcome.Win when round.Dealer.IsBust => GameMessages.DealerBustOutcome,
                Outcome.Win => GameMessages.WinOutcome,
                Outcome.Lose when player.IsBust => GameMessages.PlayerBustOutcome,
                Outcome.Lose when round.Dealer.HasBlackjack => GameMessages.DealerBlackjackOutcome,
                Outcome.Lose => GameMessages.LossOutcome,
                Outcome.Push => GameMessages.PushOutcome,
                _ => throw new InvalidOperationException(
                    $"Round {round.RoundNumber} did not resolve player '{player.Name}'.")
            };
            string headline = GameMessages.OutcomeHeadline(
                outcome, soloPlayer: round.Players.Count == 1);
            if (round.Players.Count == 1)
            {
                messages.Add(GameMessages.SoloResult(
                    headline, player.Score, round.Dealer.Score));
            }
            else
                messages.Add(GameMessages.NamedResult(
                    player.Name, headline, player.Score, round.Dealer.Score));
        }
        return string.Join(Environment.NewLine, messages);
    }
    // Shows the outcome and lets the player start another round without leaving.
    private bool ShowRoundEndMenu(string summary, List<Player> players)
    {
        string[] options = GameMessages.RoundEndMenuOptions;
        int selected = 0;
        while (true)
        {
            AnsiConsole.Clear();
            bool confirmed = false;
            AnsiConsole.Live(BuildRoundEndPanel(summary, options, selected))
                .Start(context =>
                {
                    context.Refresh();
                    while (!confirmed)
                    {
                        ConsoleKey key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.UpArrow)
                            selected = (selected - 1 + options.Length) % options.Length;
                        else if (key == ConsoleKey.DownArrow)
                            selected = (selected + 1) % options.Length;
                        else if (key == ConsoleKey.Enter)
                        {
                            confirmed = true;
                            break;
                        }
                        else
                            continue;
                        context.UpdateTarget(BuildRoundEndPanel(summary, options, selected));
                        context.Refresh();
                    }
                });
            if (selected == 0)
                return true;
            if (selected == 1)
                return false;
            if (SaveGame(players))
                return false;
        }
    }
    // Builds a centered result panel in the same style as the main menu.
    private Spectre.Console.Rendering.IRenderable BuildRoundEndPanel(
        string summary, string[] options, int selected)
    {
        var rows = new List<Spectre.Console.Rendering.IRenderable>
        {
            new Align(new Markup(GameMessages.GameTitleMarkup), HorizontalAlignment.Center),
            new Align(new Markup(GameMessages.GameSubtitleMarkup), HorizontalAlignment.Center),
            new Rule(GameMessages.ResultTitleMarkup).RuleStyle(Color.Gold1),
            new Text("")
        };
        foreach (string result in summary.Split('\n'))
        {
            string color = GameMessages.IsLossOutcome(result) ? "red" :
                           GameMessages.IsWinOutcome(result) ? "green1" : "gold1";
            rows.Add(new Align(new Markup($"[{color}]{Markup.Escape(result)}[/]"),
                HorizontalAlignment.Center));
        }
        rows.Add(new Text(""));
        rows.Add(new Align(new Markup($"[bold gold1]{GameMessages.NewRoundPrompt}[/]"),
            HorizontalAlignment.Center));
        rows.Add(new Text(""));
        for (int i = 0; i < options.Length; i++)
        {
            string label = Markup.Escape(options[i]);
            string line = i == selected
                ? $"[bold white on #961923] ❯ {label} [/]"
                : $"[white]   {label} [/]";
            rows.Add(new Align(new Markup(line), HorizontalAlignment.Center));
            rows.Add(new Text(""));
        }
        rows.Add(new Rule().RuleStyle(Color.Gold1));
        rows.Add(new Align(new Markup(GameMessages.RoundEndNavigationMarkup),
            HorizontalAlignment.Center));
        var panel = new Panel(new Rows(rows))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1)
            .Expand();
        int terminalWidth = Math.Max(1, Console.WindowWidth);
        int panelWidth = Math.Min(68, terminalWidth);
        int left = (terminalWidth - panelWidth) / 2;
        return new Padder(panel).PadLeft(left).PadRight(terminalWidth - panelWidth - left);
    }
    // Builds the horizontal Hit, Stand and Bet buttons.
    private Panel BuildPlayerActions(
        string[] options,
        int selectedOption,
        bool sideMenuActive,
        bool roundFinished,
        bool bettingMode)
    {
        var actions = new Grid();
        for (int i = 0; i < options.Length; i++)
            actions.AddColumn(new GridColumn().NoWrap().PadRight(2));
        var buttons = new Spectre.Console.Rendering.IRenderable[options.Length];
        for (int i = 0; i < options.Length; i++)
        {
            string label = Markup.Escape(options[i]);
            bool selected = !sideMenuActive &&
                            !roundFinished && i == selectedOption;
            string highlight = bettingMode && i == 2
                ? "[bold black on gold1]"
                : "[bold white on #961923]";
            buttons[i] = new Markup(selected
                ? $"{highlight} ❯ {label} [/]"
                : $"[white]   {label}[/]");
        }
        actions.AddRow(buttons);
        return new Panel(new Align(actions, HorizontalAlignment.Center))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1)
            .Expand();
    }
    // Builds the right menu with scores and temporary betting values.
    private Panel BuildGameMenu(
        string[] menuOptions,
        int selectedOption,
        List<Player> players,
        bool sideMenuActive)
    {
        List<Spectre.Console.Rendering.IRenderable> content = new();
        content.Add(new Align(
            new Markup(GameMessages.GameMenuTitleMarkup),
            HorizontalAlignment.Center));
        content.Add(new Text(""));
        string[] shortOptions = GameMessages.InGameMenuOptions;
        for (int i = 0; i < menuOptions.Length; i++)
        {
            string label = Markup.Escape(
                i < shortOptions.Length ? shortOptions[i] : menuOptions[i]);
            content.Add(new Markup(sideMenuActive && i == selectedOption
                ? $"[bold white on #961923] ❯ {label} [/]"
                : $"[white]   {label}[/]"));
            content.Add(new Text(""));
        }
        // Two columns keep the labels left and the values right aligned.
        content.Add(new Rule(GameMessages.PointsTitleMarkup).RuleStyle(Color.Gold1));
        var scoreTable = new Table()
            .NoBorder()
            .HideHeaders()
            .Expand();
        scoreTable.AddColumn(new TableColumn("").LeftAligned());
        scoreTable.AddColumn(new TableColumn("").RightAligned());
        foreach (Player player in players)
        {
            scoreTable.AddRow(
                GameMessages.ScoreMarkup(Markup.Escape(player.Name)),
                GameMessages.PlayerScoreMarkup(player.Score));
        }
        content.Add(scoreTable);
        content.Add(new Text(""));
        content.Add(new Rule(GameMessages.ChipsTitleMarkup).RuleStyle(Color.Gold1));
        var moneyTable = new Table()
            .NoBorder()
            .HideHeaders()
            .Expand();
        moneyTable.AddColumn(new TableColumn("").LeftAligned());
        moneyTable.AddColumn(new TableColumn("").RightAligned());
        moneyTable.AddRow(
            GameMessages.BalanceMarkup,
            GameMessages.Balance(BettingUI.Balance));
        moneyTable.AddRow(
            GameMessages.CurrentBetMarkup,
            GameMessages.CurrentBet(BettingUI.CurrentBet));
        content.Add(moneyTable);
        return new Panel(new Rows(content))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1)
            .Expand();
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
            gameStorage.SaveResults(results);
            Dictionary<string, PlayerStatistics> savedResults =
                gameStorage.LoadResults();
            ui.ShowStatistics(savedResults);
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
    // Selects a saved player and starts a new deal with their saved statistics.
    // Cards and the current round are intentionally not saved.
    private void LoadGame()
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
            ShowMessage(GameMessages.LoadSavedResultsError);
            return;
        }
        if (results.Count == 0)
        {
            ShowMessage(GameMessages.NoSavedGames);
            return;
        }
        ui.ShowStatistics(results);
        List<string> names = new(results.Keys);
        names.Sort(StringComparer.OrdinalIgnoreCase);
        int selected = 0;
        bool cancelled = false;
        AnsiConsole.Clear();
        AnsiConsole.Live(BuildSavedPlayersMenu(names, results, selected))
            .Start(context =>
            {
                context.Refresh();
                while (true)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.UpArrow)
                        selected = (selected - 1 + names.Count) % names.Count;
                    else if (key == ConsoleKey.DownArrow)
                        selected = (selected + 1) % names.Count;
                    else if (key == ConsoleKey.Escape)
                    {
                        cancelled = true;
                        break;
                    }
                    else if (key == ConsoleKey.Enter)
                        break;
                    else
                        continue;
                    context.UpdateTarget(BuildSavedPlayersMenu(names, results, selected));
                    context.Refresh();
                }
            });
        if (cancelled)
            return;
        string name = names[selected];
        PlayerStatistics stats = results[name];
        List<Player> players = new()
        {
            new Player(name, stats.Wins, stats.Losses, stats.Pushes)
        };
        Deck deck = new Deck();
        deck.CreateDeck();
        deck.ShuffleDeck();
        DealNewHandAndShow(players, deck);
    }
    // Displays saved players in the same centered casino style as the main menu.
    private Spectre.Console.Rendering.IRenderable BuildSavedPlayersMenu(
        List<string> names,
        Dictionary<string, PlayerStatistics> results,
        int selected)
    {
        List<Spectre.Console.Rendering.IRenderable> rows = new();
        rows.Add(new Align(new Markup(GameMessages.GameTitleMarkup),
            HorizontalAlignment.Center));
        rows.Add(new Align(new Markup(GameMessages.GameSubtitleMarkup),
            HorizontalAlignment.Center));
        rows.Add(new Text(""));
        rows.Add(new Rule(GameMessages.SavedPlayersTitleMarkup).RuleStyle(Color.Gold1));
        rows.Add(new Text(""));
        for (int i = 0; i < names.Count; i++)
        {
            string name = names[i];
            PlayerStatistics stats = results[name];
            string label = GameMessages.SavedPlayerStatistics(
                name, stats.Wins, stats.Losses, stats.Pushes);
            string escaped = Markup.Escape(label);
            string line = i == selected
                ? $"[bold white on #961923] ❯ {escaped} [/]"
                : $"[white]   {escaped} [/]";
            rows.Add(new Align(new Markup(line), HorizontalAlignment.Center));
            rows.Add(new Text(""));
        }
        rows.Add(new Rule().RuleStyle(Color.Gold1));
        rows.Add(new Align(new Markup(GameMessages.SavedPlayersNavigationMarkup),
            HorizontalAlignment.Center));
        var panel = new Panel(new Rows(rows))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1)
            .Expand();
        int terminalWidth = Math.Max(1, Console.WindowWidth);
        int panelWidth = Math.Min(68, terminalWidth);
        int left = (terminalWidth - panelWidth) / 2;
        return new Padder(panel).PadLeft(left).PadRight(terminalWidth - panelWidth - left);
    }
    // Displays a centered message and waits for input.
    private void ShowMessage(string message)
    {
        ui.ShowCenteredMessage(message);
    }
}
