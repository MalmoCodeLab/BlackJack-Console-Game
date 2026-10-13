
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
                players.Add(
                    new Player(
                        name,
                        stats.Wins,
                        stats.Losses,
                        stats.Pushes
                    )
                );
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

        // Deals two cards to each player and the dealer.
        for (int round = 0; round < 2; round++)
        {
            foreach (Player player in players)
            {
                player.ReceiveCard(deck);
            }

            dealer.ReceiveCard(deck);
        }

        // Opens the Blackjack table.
        ShowGame(players, dealer);
    }

    // Displays the Blackjack table and the side menu.
    private void ShowGame(
        List<Player> players,
        Dealer dealer)
    {
        string[] menuOptions =
        {
            "♦ Spara och avsluta",
            "♣ Till huvudmenyn"
        };

        int selectedOption = 0;
        bool playing = true;

        while (playing)
        {
            Console.Clear();

            // Gives the table more space than the side menu.
            var layout = new Layout("Root")
                .SplitColumns(
                    new Layout("Table").Ratio(5),
                    new Layout("Menu").Ratio(1)
                );

            // Displays the green Blackjack table on the left.
            layout["Table"].Update(
                ui.BuildBlackjackTable(players, dealer)
            );

            // Displays the game menu and scores on the right.
            layout["Menu"].Update(
                BuildGameMenu(
                    menuOptions,
                    selectedOption,
                    players
                )
            );

            AnsiConsole.Write(layout);

            // Reads the player's keyboard input.
            ConsoleKey key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedOption--;

                    if (selectedOption < 0)
                    {
                        selectedOption = menuOptions.Length - 1;
                    }

                    break;

                case ConsoleKey.DownArrow:
                    selectedOption++;

                    if (selectedOption >= menuOptions.Length)
                    {
                        selectedOption = 0;
                    }

                    break;

                case ConsoleKey.Enter:
                    switch (selectedOption)
                    {
                        case 0:
                            // Saves the statistics and exits.
                            if (SaveGame(players))
                            {
                                playing = false;
                            }

                            break;

                        case 1:
                            // Returns to the main menu.
                            playing = false;
                            break;
                    }

                    break;
            }
        }
    }

    // Builds the menu displayed next to the table.
    private Panel BuildGameMenu(
        string[] menuOptions,
        int selectedOption,
        List<Player> players)
    {
        List<Spectre.Console.Rendering.IRenderable> content = new();

        // Displays the side menu title.
        content.Add(new Align(
            new Markup("[bold gold1]♠ SPELMENY ♠[/]"),
            HorizontalAlignment.Center
        ));

        content.Add(new Text(""));

        // Displays the menu options.
        for (int i = 0; i < menuOptions.Length; i++)
        {
            string option = Markup.Escape(menuOptions[i]);

            if (i == selectedOption)
            {
                // Highlights the selected option in red.
                content.Add(new Markup(
                    $"[bold white on #961923] ❯ {option} [/]"
                ));
            }
            else
            {
                content.Add(new Markup(
                    $"[white]   {option}[/]"
                ));
            }

            content.Add(new Text(""));
        }

        // Adds space above the player scores.
        for (int i = 0; i < 8; i++)
        {
            content.Add(new Text(""));
        }

        // Displays the score heading.
        content.Add(
            new Rule("[gold1]POÄNG[/]")
                .RuleStyle(Color.Gold1)
        );

        content.Add(new Text(""));

        // Displays each player's current score.
        foreach (Player player in players)
        {
            content.Add(new Markup(
                $"[white]{Markup.Escape(player.Name)}: " +
                $"[bold gold1]{player.Score}[/][/]"
            ));
        }

        // Keeps the side menu on a dark background.
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

    // Loads previously saved player statistics.
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

    // Displays a message and waits for keyboard input.
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
