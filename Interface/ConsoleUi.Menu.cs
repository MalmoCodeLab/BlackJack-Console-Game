
using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

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

    // Displays the main menu with arrow-key navigation.
    public string ShowMenu()
    {
        int selectedOption = 0;
        string choice = "";

        AnsiConsole.Clear();
        ShowHeader();

        AnsiConsole.MarkupLine(
            "[bold gold1]♠ HUVUDMENY ♠[/]"
        );

        AnsiConsole.WriteLine();

        // Updates the menu without clearing the console.
        AnsiConsole.Live(CreateMenu(selectedOption))
            .Start(context =>
            {
                // Displays the menu before waiting for a key.
                context.Refresh();

                while (true)
                {
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
                        choice = menuOptions[selectedOption];
                        break;
                    }
                    else
                    {
                        continue;
                    }

                    context.UpdateTarget(CreateMenu(selectedOption));
                    context.Refresh();
                }
            });

        return choice;
    }

    // Creates the menu with navigation instructions at the bottom.
    private IRenderable CreateMenu(int selectedOption)
    {
        List<IRenderable> menuRows = new();

        for (int i = 0; i < menuOptions.Length; i++)
        {
            string option = Markup.Escape(menuOptions[i]);

            if (i == selectedOption)
            {
                menuRows.Add(
                    new Markup(
                        $"[bold white on darkgreen]❯ {option}[/]"
                    )
                );
            }
            else
            {
                menuRows.Add(
                    new Markup(
                        $"  [white]{option}[/]"
                    )
                );
            }
        }

        menuRows.Add(new Text(""));

        menuRows.Add(
            new Markup(
                "[grey]↑ ↓ Navigera med piltangenterna - Enter för att välja[/]"
            )
        );

        return new Rows(menuRows);
    }
}



