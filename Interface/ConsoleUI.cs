using System;
using Spectre.Console;

// Other ConsoleUi methods are defined in the ConsoleUi.*.cs partial files.
public partial class ConsoleUi
{
    // Displays a centered message and waits for the player to acknowledge it.
    public void ShowCenteredMessage(string message)
    {
        AnsiConsole.Clear();

        var content = new Rows(
            new Align(new Markup(GameMessages.GameTitleMarkup), HorizontalAlignment.Center),
            new Text(""),
            new Align(new Markup($"[white]{Markup.Escape(message)}[/]"), HorizontalAlignment.Center),
            new Text(""),
            new Align(new Markup("[grey]Tryck Enter för att fortsätta.[/]"), HorizontalAlignment.Center)
        );

        ShowInformationPanel(content);
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
            new Align(new Markup("[grey]Tryck Enter för att återgå till huvudmenyn.[/]"), HorizontalAlignment.Center)
        );

        ShowInformationPanel(content);
    }

    // Shows the team credits from the main menu.
    public void ShowCredits()
    {
        var content = new Rows(
            new Align(new Markup(GameMessages.GameTitleMarkup), HorizontalAlignment.Center),
            new Text(""),
            new Rule("[gold1]♣ MEDVERKANDE ♣[/]").RuleStyle(Color.Gold1),
            new Text(""),
            new Align(new Markup("[bold white]Grupp 1 – Blackjack[/]"), HorizontalAlignment.Center),
            new Align(new Markup("[white]Utvecklat i C# med Spectre.Console.[/]"), HorizontalAlignment.Center),
            new Text(""),
            new Align(new Markup("[grey]Tryck Enter för att återgå till huvudmenyn.[/]"), HorizontalAlignment.Center)
        );

        ShowInformationPanel(content);
    }

    // Uses the same centered, gold-bordered layout for informational screens.
    private void ShowInformationPanel(Rows content)
    {
        AnsiConsole.Clear();

        var panel = new Panel(content)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1);

        AnsiConsole.Write(
            new Align(panel, HorizontalAlignment.Center, VerticalAlignment.Middle)
                .Height(Math.Max(1, Console.WindowHeight)));

        while (Console.ReadKey(true).Key != ConsoleKey.Enter)
        {
        }
    }
}
