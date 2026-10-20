using System;
using Spectre.Console;

// Other ConsoleUi methods are defined in the ConsoleUi.*.cs partial files.
public partial class ConsoleUi
{
    // Displays a centered message and closes it with Esc.
    public void ShowCenteredMessage(string message)
    {
        bool backToMainMenu =
            string.Equals(message, GameMessages.NoSavedGames, StringComparison.Ordinal) ||
            string.Equals(message, GameMessages.SaveStatisticsSuccess, StringComparison.Ordinal) ||
            string.Equals(message, GameMessages.AllPlayersOutOfChips, StringComparison.Ordinal);

        string instruction = backToMainMenu
            ? "Tryck Esc för att återgå till huvudmenyn."
            : "Tryck Esc för att fortsätta.";

        var content = new Rows(
            new Align(new Markup(GameMessages.GameTitleMarkup), HorizontalAlignment.Center),
            new Text(""),
            new Align(new Markup($"[white]{Markup.Escape(message)}[/]"), HorizontalAlignment.Center),
            new Text(""),
            new Align(new Markup($"[grey]{instruction}[/]"), HorizontalAlignment.Center)
        );

        ShowInformationPanel(content, ConsoleKey.Escape);
    }

    // Shows the basic Blackjack rules from the main menu.
    public void ShowRules()
    {
        var content = new Rows(
            new Align(new Markup(GameMessages.GameTitleMarkup), HorizontalAlignment.Center),
            new Text(""),
            new Rule("[gold1]♥ SPELREGLER ♥[/]").RuleStyle(Color.Gold1),
            new Text(""),
            new Markup("[white]Målet är att komma närmare 21 än givaren utan att gå över.[/]"),
            new Markup("[white]Kort 2–10 har sitt värde, klädda kort är värda 10.[/]"),
            new Markup("[white]Ess räknas som 1 eller 11, beroende på vad som är bäst.[/]"),
            new Text(""),
            new Markup("[gold1]♦ SATSNING ♦[/]"),
            new Markup("[white]Välj marker och bekräfta din insats innan du spelar.[/]"),
            new Text(""),
            new Markup("[gold1]♠ SPELA ♠[/]"),
            new Markup("[white]Ta kort för att få ett till kort, eller stanna för att avsluta din tur.[/]"),
            new Markup("[white]Över 21 betyder att du förlorar rundan.[/]"),
            new Markup("[white]Blackjack är ett ess och ett tiopoängskort på de första två korten.[/]"),
            new Text(""),
            new Align(new Markup("[grey]Tryck Esc för att återgå till huvudmenyn.[/]"), HorizontalAlignment.Center)
        );

        ShowInformationPanel(content, ConsoleKey.Escape);
    }

    // Displays team members and their individual features.
    public void ShowCredits()
    {
        var credits = new Grid();

        credits.AddColumn(new GridColumn().NoWrap().PadRight(5));
        credits.AddColumn(new GridColumn().NoWrap());

        credits.AddRow(
            new Markup("[white]Alice[/]"),
            new Markup("[gold1]Kortgrafik och animation[/]"));

        credits.AddRow(
            new Markup("[white]Sofia[/]"),
            new Markup("[gold1]Anpassningsbara husregler[/]"));

        credits.AddRow(
            new Markup("[white]Christoffer[/]"),
            new Markup("[gold1]Korträkning[/]"));

        credits.AddRow(
            new Markup("[white]Niclas[/]"),
            new Markup("[gold1]Insatser och marker[/]"));

        var content = new Rows(
            new Align(
                new Markup(GameMessages.GameTitleMarkup),
                HorizontalAlignment.Center),

            new Text(""),

            new Rule("[gold1]♣ MEDVERKANDE ♣[/]")
                .RuleStyle(Color.Gold1),

            new Text(""),

            new Align(
                new Markup("[bold gold1]GRUPP 1[/]"),
                HorizontalAlignment.Center),

            new Text(""),

            new Align(credits, HorizontalAlignment.Center),

            new Text(""),

            new Align(
                new Markup("[grey]Utvecklat i C# med Spectre.Console[/]"),
                HorizontalAlignment.Center),

            new Text(""),

            new Align(
                new Markup("[grey]Tryck Esc för att återgå till huvudmenyn.[/]"),
                HorizontalAlignment.Center)
        );

        ShowInformationPanel(content, ConsoleKey.Escape);
    }

    // Clears only when entering or leaving an information screen.
    private void ShowInformationPanel(
        Rows content,
        ConsoleKey returnKey = ConsoleKey.Enter)
    {
        // Hide the cursor using both Spectre.Console and the terminal.
        void HideCursor()
        {
            try
            {
                if (!Console.IsOutputRedirected)
                    Console.CursorVisible = false;
            }
            catch (System.IO.IOException) { }
            catch (PlatformNotSupportedException) { }

            AnsiConsole.Cursor.Hide();
            Console.Write("\u001b[?25l");
            Console.Out.Flush();
        }

        AnsiConsole.Clear();
        HideCursor();

        var panel = new Panel(content)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1)
            .Expand();

        // Match the width of the main menu.
        int terminalWidth = Math.Max(1, Console.WindowWidth);
        int panelWidth = Math.Min(68, terminalWidth);
        int leftPadding = (terminalWidth - panelWidth) / 2;

        var centeredPanel = new Padder(panel)
            .PadLeft(leftPadding)
            .PadRight(terminalWidth - panelWidth - leftPadding);

        try
        {
            AnsiConsole.Write(
                new Align(
                    centeredPanel,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Middle)
                .Height(Math.Max(1, Console.WindowHeight)));

            // Spectre.Console can update cursor state during rendering.
            HideCursor();

            while (Console.ReadKey(true).Key != returnKey)
            {
            }
        }
        finally
        {
            AnsiConsole.Clear();
            HideCursor();
        }
    }
}
