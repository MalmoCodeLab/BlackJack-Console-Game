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
        bool running = true;
        while (running)
        {
            string choice = ui.ShowMenu();
            switch (choice)
            {
                case "♠ Nytt spel":
                    StartNewGame();
                    break;
                case "♦ Ladda spel":
                    LoadGame();
                    break;
                case "♥ Spelregler":
                    ui.ShowRules();
                    break;
                case "♣ Medverkande":
                    ui.ShowCredits();
                    break;
                case "♠ Avsluta":
                    running = false;
                    break;
            }
        }
        Console.Clear();
    }
    // Creates a new Blackjack game.
    private void StartNewGame()
    {
        int playerCount = ui.AskPlayerCount();
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
            ShowMessage("Kunde inte ladda spelarstatistiken.");
            return;
        }
        List<Player> players = new();
        // Creates the selected number of players.
        for (int i = 1; i <= playerCount; i++)
        {
            string name = $"Spelare {i}";
            if (results.TryGetValue(
                name,
                out PlayerStatistics? stats))
            {
                players.Add(new Player(
                    name,
                    stats.Wins,
                    stats.Losses,
                    stats.Pushes
                ));
            }
            else
            {
                players.Add(new Player(name));
            }
        }
        Dealer dealer = new Dealer();
        Deck deck = new Deck();
        // Creates and shuffles the deck.
        deck.CreateDeck();
        deck.ShuffleDeck();
        // Deals the starting cards.
        for (int round = 0; round < 2; round++)
        {
            foreach (Player player in players)
            {
                player.ReceiveCard(deck);
            }
            dealer.ReceiveCard(deck);
        }
        ShowGame(players, dealer, deck);
    }
    // Displays the Blackjack table and handles input.
    private void ShowGame(List<Player> players, Dealer dealer, Deck deck)
    {
        // The third action activates the chips already drawn on the table.
        string[] gameOptions = { "♥ Ta kort", "♠ Stanna", "♦ Satsa" };
        string[] menuOptions = { "♦ Spara och avsluta", "♣ Till huvudmenyn" };
        int selectedGameOption = 0;
        int selectedMenuOption = 0;
        bool sideMenuActive = false;
        bool bettingMode = false;
        int currentPlayerIndex = 0;
        bool playing = true;
        bool roundFinished = false;
        AnsiConsole.Clear();
        try
        {
            Console.CursorVisible = false;
            while (playing)
            {
                // Reuse the original layout, without clearing on every key press.
                Console.SetCursorPosition(0, 0);
                int totalHeight = Math.Max(22, Console.WindowHeight - 1);
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
                ConsoleKey key = Console.ReadKey(true).Key;
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
                                if (SaveGame(players))
                                    playing = false;
                            }
                            else
                            {
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
                            else if (selectedGameOption == 1)
                            {
                                if (currentPlayerIndex < players.Count - 1)
                                {
                                    currentPlayerIndex++;
                                    selectedGameOption = 0;
                                }
                                else
                                {
                                    roundFinished = true;
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
            new Markup("[bold gold1]♠ SPELMENY ♠[/]"),
            HorizontalAlignment.Center));
        content.Add(new Text(""));
        string[] shortOptions = { "♦ Spara/avsluta", "♣ Huvudmeny" };
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
        content.Add(new Rule("[gold1]POÄNG[/]").RuleStyle(Color.Gold1));
        var scoreTable = new Table()
            .NoBorder()
            .HideHeaders()
            .Expand();
        scoreTable.AddColumn(new TableColumn("").LeftAligned());
        scoreTable.AddColumn(new TableColumn("").RightAligned());
        foreach (Player player in players)
        {
            scoreTable.AddRow(
                $"[white]{Markup.Escape(player.Name)}:[/]",
                $"[bold gold1]{player.Score}[/]");
        }
        content.Add(scoreTable);

        content.Add(new Text(""));
        content.Add(new Rule("[gold1]MARKER[/]").RuleStyle(Color.Gold1));
        var moneyTable = new Table()
            .NoBorder()
            .HideHeaders()
            .Expand();
        moneyTable.AddColumn(new TableColumn("").LeftAligned());
        moneyTable.AddColumn(new TableColumn("").RightAligned());
        moneyTable.AddRow(
            "[white]Saldo:[/]",
            $"[green1]{BettingUI.Balance} kr[/]");
        moneyTable.AddRow(
            "[white]Aktuell insats:[/]",
            $"[gold1]{BettingUI.CurrentBet} kr[/]");
        content.Add(moneyTable);
        return new Panel(new Rows(content))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1)
            .Expand();
    }
    // Saves player statistics.
    private bool SaveGame(List<Player> players)
    {
        try
        {
            Dictionary<string, PlayerStatistics> results =
                gameStorage.LoadResults();
            foreach (Player player in players)
            {
                results[player.Name] = new PlayerStatistics
                {
                    Wins = player.Wins,
                    Losses = player.Losses,
                    Pushes = player.Pushes
                };
            }
            gameStorage.SaveResults(results);
            ShowMessage("Spelarstatistiken har sparats!");
            return true;
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException ||
            ex is JsonException)
        {
            ShowMessage("Kunde inte spara spelarstatistiken.");
            return false;
        }
    }
    // Displays saved player statistics.
    private void LoadGame()
    {
        Console.Clear();
        try
        {
            Dictionary<string, PlayerStatistics> results =
                gameStorage.LoadResults();
            if (results.Count == 0)
            {
                ShowMessage("Det finns inga sparade resultat.");
                return;
            }
            ui.ShowHeader();
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Gold1);
            table.AddColumn("[gold1]Spelare[/]");
            table.AddColumn("[green]Vinster[/]");
            table.AddColumn("[red]Förluster[/]");
            table.AddColumn("[yellow]Oavgjorda[/]");
            foreach (var result in results)
            {
                table.AddRow(
                    Markup.Escape(result.Key),
                    result.Value.Wins.ToString(),
                    result.Value.Losses.ToString(),
                    result.Value.Pushes.ToString()
                );
            }
            AnsiConsole.Write(table);
            Console.WriteLine();
            ui.ShowMessage(
                "Tryck på valfri tangent för att återgå."
            );
            Console.ReadKey(true);
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException ||
            ex is JsonException)
        {
            ShowMessage("Kunde inte ladda sparade resultat.");
        }
    }
    // Displays a message and waits for input.
    private void ShowMessage(string message)
    {
        Console.Clear();
        ui.ShowHeader();
        ui.ShowMessage(message);
        Console.WriteLine();
        ui.ShowMessage(
            "Tryck på valfri tangent för att återgå."
        );
        Console.ReadKey(true);
    }
}
