
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

        Console.Clear();
        ui.ShowHeader();

        ui.ShowMessage($"Du har valt {playerCount} spelare.");

        Console.WriteLine();
        ui.ShowMessage("Tryck på valfri tangent för att återgå.");

        Console.ReadKey(true);
    }

    // Displays a temporary message.
    private void ShowTemporaryMessage(string message)
    {
        Console.Clear();
        ui.ShowHeader();

        ui.ShowMessage(message);

        Console.WriteLine();
        ui.ShowMessage("Tryck på valfri tangent för att återgå.");

        Console.ReadKey(true);
    }
}
