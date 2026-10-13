
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class Game
{
    private readonly IGameUI ui;
    private readonly GameStorage gameStorage;

    internal Game(IGameUI ui, GameStorage gameStorage)
    {
        this.ui = ui ?? throw new ArgumentNullException(nameof(ui));
        this.gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
    }

    // Starts the game and displays the main menu.
    public void Run()
    {
        bool running = true;

        while (running)
        {
            MainMenuChoice choice = ui.ShowMenu();

            switch (choice)
            {
                case MainMenuChoice.NewGame:
                    StartNewGame();
                    break;

                case MainMenuChoice.LoadGame:
                    LoadGame();
                    break;

                case MainMenuChoice.Rules:
                    ui.ShowRules();
                    break;

                case MainMenuChoice.Credits:
                    ui.ShowCredits();
                    break;

                case MainMenuChoice.Exit:
                    running = false;
                    break;
            }
        }

        ui.Clear();
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
            ui.Clear();

            ui.ShowTable(players, dealer);

            GameMenuChoice choice = ui.ShowGameMenu();

            switch (choice)
            {
                case GameMenuChoice.SaveAndExit:
                    if (SaveGame(players))
                    {
                        playing = false;
                    }
                    break;

                case GameMenuChoice.ReturnToMainMenu:
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

    // Loads and displays saved player statistics.
    private void LoadGame()
    {
        ui.Clear();

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
        ui.ShowNotice(message);
    }
}
