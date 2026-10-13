
using System;

public class Game
{
    private readonly ConsoleUi ui = new ConsoleUi();

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
                    ShowTemporaryMessage("Ladda spel kommer snart!");
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

        // Clears the menu header before displaying the table.
        Console.Clear();

        // Creates the dealer and player.
        Dealer dealer = new Dealer();
        Player player = new Player("Player 1");

        // Creates and shuffles a deck of 52 cards.
        Deck deck = new Deck();
        deck.CreateDeck();
        deck.ShuffleDeck();

        // Deals two cards to the player and dealer.
        player.ReceiveCard(deck);
        dealer.ReceiveCard(deck);

        player.ReceiveCard(deck);
        dealer.ReceiveCard(deck);

        // Displays the cards on the Blackjack table.
        ui.ShowTable(player.Hand, dealer.Hand);

        Console.WriteLine();

        ui.ShowMessage(
            "Tryck på valfri tangent för att återgå."
        );

        Console.ReadKey(true);
    }

    // Displays a temporary message before returning to the menu.
    private void ShowTemporaryMessage(string message)
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
