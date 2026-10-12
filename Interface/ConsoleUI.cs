
using Spectre.Console;

public class ConsoleUi
{
    // Displays the main menu and returns the selected option.
    public string ShowMenu()
    {
        AnsiConsole.Clear();

        // Creates the Blackjack header.
        var header = new Panel(
            new Markup(
                "[bold gold1]♠ BLACKJACK 21 ♠[/]\n" +
                "[italic green3]Your next move is compiling...[/]"
            )
        )
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Gold1);

        AnsiConsole.Write(header);
        AnsiConsole.WriteLine();

        // Creates the main menu.
        string selectedOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold gold1]♠ MAIN MENU ♠[/]")
                .PageSize(5)
                .HighlightStyle(
                    new Style(
                        foreground: Color.White,
                        background: new Color(0, 100, 50)
                    )
                )
                .AddChoices(
                    "♠ New Game",
                    "♦ Load Game",
                    "♥ Instructions",
                    "♣ Credits",
                    "♠ Exit"
                )
        );

        return selectedOption;
    }
}
