
using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

public partial class ConsoleUi
{
    private readonly (MainMenuChoice Choice, string Label)[] menuOptions =
    {
        (MainMenuChoice.NewGame, "♠ Nytt spel"),
        (MainMenuChoice.LoadGame, "♦ Ladda spel"),
        (MainMenuChoice.Rules, "♥ Spelregler"),
        (MainMenuChoice.Credits, "♣ Medverkande"),
        (MainMenuChoice.Exit, "♠ Avsluta")
    };

    // Displays the main menu with arrow-key navigation.
    public MainMenuChoice ShowMenu()
    {
        int selectedOption = 0;
        MainMenuChoice choice = MainMenuChoice.Exit;

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
                        choice = menuOptions[selectedOption].Choice;
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

    // Displays the menu available while a game is in progress.
    public GameMenuChoice ShowGameMenu()
    {
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

        return choice == "♦ Spara och avsluta"
            ? GameMenuChoice.SaveAndExit
            : GameMenuChoice.ReturnToMainMenu;
    }

    // Creates the menu with navigation instructions at the bottom.
    private IRenderable CreateMenu(int selectedOption)
    {
        List<IRenderable> menuRows = new();

        for (int i = 0; i < menuOptions.Length; i++)
        {
            string option = Markup.Escape(menuOptions[i].Label);

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


