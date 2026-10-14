
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Spectre.Console;

public class Game
{
    private readonly ConsoleUi ui = new ConsoleUi();
    private readonly GameStorage gameStorage = new GameStorage();

    // Starts the game and displays the main menu.
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

    // Starts a new Blackjack game.
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

        Dealer dealer = new Dealer();
        List<Player> players = new List<Player>();

        // Creates players with their saved statistics.
        for (int i = 1; i <= playerCount; i++)
        {
            string name = $"Spelare {i}";

            if (results.TryGetValue(name, out PlayerStatistics? stats))
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

        Deck deck = new Deck();
        deck.CreateDeck();
        deck.ShuffleDeck();

        // Deals two cards to every player and the dealer.
        for (int round = 0; round < 2; round++)
        {
            foreach (Player player in players)
            {
                player.ReceiveCard(deck);
            }

            dealer.ReceiveCard(deck);
        }

        ShowGame(players, dealer);
    }

    // Displays the Blackjack table and game menu.
    private void ShowGame(List<Player> players, Dealer dealer)
    {
        bool playing = true;

        while (playing)
        {
            Console.Clear();

            ui.ShowTable(players, dealer);

            AnsiConsole.WriteLine();

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold gold1]♠ Spelmeny ♠[/]")
                    .PageSize(3)
                    .HighlightStyle(
                        new Style(
                            foreground: Color.White,
                            background: new Color(150, 25, 35)
                        )
                    )
                    .AddChoices(
                        "♦ Spara och avsluta",
                        "♣ Till huvudmenyn"
                    )
            );

            switch (choice)
            {
                case "♦ Spara och avsluta":
                    if (SaveGame(players))
                    {
                        playing = false;
                    }
                    break;

                case "♣ Till huvudmenyn":
                    playing = false;
                    break;
            }
        }
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

            Dictionary<string, PlayerStatistics> savedResults =
                gameStorage.LoadResults();
            ui.ShowStatistics(savedResults);
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

    // Loads and displays saved player statistics.
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

            ui.ShowStatistics(results);
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException ||
            ex is JsonException)
        {
            ShowMessage("Kunde inte ladda sparade resultat.");
        }
    }

    // Displays a message before returning to the menu.
    private void ShowMessage(string message)
    {
        Console.Clear();

        ui.ShowHeader();
        ui.ShowMessage(message);

        Console.WriteLine();

        ui.ShowMessage("Tryck på valfri tangent för att återgå.");
        Console.ReadKey(true);
    }
}
