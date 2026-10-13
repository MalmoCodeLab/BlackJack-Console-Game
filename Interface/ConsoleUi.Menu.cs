
using System;
using Spectre.Console;

public partial class ConsoleUi
{
    private readonly string[] menuOptions =
    {
        "♠ Nytt spel",
        "♦ Ladda spel",
        "♥ Spelregler",
        "♣ Medverkande",
        "♠ Avsluta"
    };

    // Displays the main menu and returns the selected option.
    public string ShowMenu()
    {
        int selectedOption = 0;

        AnsiConsole.Clear();

        var header = new Panel(
            new Markup(
                "[bold gold1]♠ BLACKJACK 21 ♠[/]\n" +
                "[italic green3]Tur kan inte kompileras.[/]"
            )
        )
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Gold1);

        AnsiConsole.Write(header);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold gold1]♠ HUVUDMENY ♠[/]");
        AnsiConsole.WriteLine();

        int menuStartRow = Console.CursorTop;

        // Reserves space for the menu options.
        for (int i = 0; i < menuOptions.Length; i++)
        {
            Console.WriteLine();
        }

        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            "[grey]↑ ↓ Navigera med piltangenterna • Enter för att välja[/]"
        );

        try
        {
            Console.CursorVisible = false;

            while (true)
            {
                // Updates the menu without clearing the screen.
                Console.SetCursorPosition(0, menuStartRow);

                for (int i = 0; i < menuOptions.Length; i++)
                {
                    string option = Markup.Escape(menuOptions[i]);

                    if (i == selectedOption)
                    {
                        AnsiConsole.MarkupLine(
                            $"[bold gold1 on darkgreen]❯ {option}[/]"
                        );
                    }
                    else
                    {
                        AnsiConsole.MarkupLine(
                            $"  [white]{option}[/]"
                        );
                    }
                }

                ConsoleKey key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selectedOption--;

                    if (selectedOption < 0)
                    {
                        selectedOption = menuOptions.Length - 1;
                    }
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedOption++;

                    if (selectedOption >= menuOptions.Length)
                    {
                        selectedOption = 0;
                    }
                }
                else if (key == ConsoleKey.Enter)
                {
                    return menuOptions[selectedOption];
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }
}
