using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

public partial class ConsoleUi
{
    private readonly string[] menuOptions = GameMessages.MainMenuOptions;

    // Displays the main menu with arrow-key navigation.
    public string ShowMenu()
    {
        int selectedOption = 0;
        string choice = "";
        AnsiConsole.Clear();

        // Live updates only the menu instead of clearing the terminal.
        AnsiConsole.Live(CreateMenu(selectedOption))
            .Start(context =>
            {
                context.Refresh();
                while (true)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.UpArrow)
                    {
                        selectedOption = (selectedOption - 1 + menuOptions.Length) % menuOptions.Length;
                    }
                    else if (key == ConsoleKey.DownArrow)
                    {
                        selectedOption = (selectedOption + 1) % menuOptions.Length;
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

    // Lets the player select the number of players in a centered panel.
    public int AskPlayerCountCentered()
    {
        int selectedOption = 0;
        int playerCount = 1;
        AnsiConsole.Clear();

        AnsiConsole.Live(CreatePlayerCountMenu(selectedOption))
            .Start(context =>
            {
                context.Refresh();
                while (true)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.UpArrow)
                        selectedOption = (selectedOption + 4) % 5;
                    else if (key == ConsoleKey.DownArrow)
                        selectedOption = (selectedOption + 1) % 5;
                    else if (key == ConsoleKey.Enter)
                    {
                        playerCount = selectedOption + 1;
                        break;
                    }
                    else
                        continue;

                    context.UpdateTarget(CreatePlayerCountMenu(selectedOption));
                    context.Refresh();
                }
            });

        return playerCount;
    }

    // Creates the player count menu using the same casino design.
    private IRenderable CreatePlayerCountMenu(int selectedOption)
    {
        List<IRenderable> rows = new();
        rows.Add(new Align(new Markup(GameMessages.GameTitleMarkup), HorizontalAlignment.Center));
        rows.Add(new Align(new Markup(GameMessages.GameSubtitleMarkup), HorizontalAlignment.Center));
        rows.Add(new Text(""));
        rows.Add(new Rule(GameMessages.PlayerCountTitleMarkup).RuleStyle(Color.Gold1));
        rows.Add(new Text(""));

        for (int i = 0; i < 5; i++)
        {
            string label = string.Format(GameMessages.PlayerCountFormat, i + 1).PadRight(17);
            string line = i == selectedOption
                ? $"[bold white on #961923] ❯ {label} [/]"
                : $"[white]   {label} [/]";
            rows.Add(new Align(new Markup(line), HorizontalAlignment.Center));
            rows.Add(new Text(""));
        }

        rows.Add(new Rule().RuleStyle(Color.Gold1));
        rows.Add(new Align(new Markup(GameMessages.MenuNavigationMarkup), HorizontalAlignment.Center));
        return CenterMenuPanel(rows);
    }

    // Builds one centered casino panel containing the header and menu.
    private IRenderable CreateMenu(int selectedOption)
    {
        List<IRenderable> rows = new();
        rows.Add(new Align(
            new Markup(GameMessages.GameTitleMarkup),
            HorizontalAlignment.Center));
        rows.Add(new Align(
            new Markup(GameMessages.GameSubtitleMarkup),
            HorizontalAlignment.Center));
        rows.Add(new Text(""));
        rows.Add(new Rule(GameMessages.MainMenuTitleMarkup).RuleStyle(Color.Gold1));
        rows.Add(new Text(""));

        for (int i = 0; i < menuOptions.Length; i++)
        {
            string option = Markup.Escape(menuOptions[i]);
            string paddedOption = option.PadRight(21);
            string line = i == selectedOption
                ? $"[bold white on #961923] ❯ {paddedOption} [/]"
                : $"[white]   {paddedOption} [/]";

            rows.Add(new Align(new Markup(line), HorizontalAlignment.Center));
            rows.Add(new Text(""));
        }

        rows.Add(new Rule().RuleStyle(Color.Gold1));
        rows.Add(new Align(
            new Markup(GameMessages.MenuNavigationMarkup),
            HorizontalAlignment.Center));

        return CenterMenuPanel(rows);
    }

    // Gives both menus the same width and horizontal position.
    private IRenderable CenterMenuPanel(List<IRenderable> rows)
    {
        var panel = new Panel(new Rows(rows))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1)
            .Expand();

        // Keep the complete panel centered and at most 68 columns wide.
        int terminalWidth = Math.Max(1, Console.WindowWidth);
        int menuWidth = Math.Min(68, terminalWidth);
        int leftPadding = (terminalWidth - menuWidth) / 2;
        int rightPadding = terminalWidth - menuWidth - leftPadding;
        return new Padder(panel)
            .PadLeft(leftPadding)
            .PadRight(rightPadding);
    }
}
