
using System;
using Spectre.Console;

public partial class ConsoleUi : IGameUI
{
    // Clears the console before drawing a new screen.
    public void Clear()
    {
        AnsiConsole.Clear();
    }

    // Displays a notice and waits for the player to return.
    public void ShowNotice(string message)
    {
        Clear();
        ShowHeader();
        ShowMessage(message);

        AnsiConsole.WriteLine();
        ShowMessage("Tryck på valfri tangent för att återgå.");
        Console.ReadKey(true);
    }

    // Displays saved player statistics.
    public void ShowStatistics(
        System.Collections.Generic.IReadOnlyDictionary<string, PlayerStatistics> statistics)
    {
        Clear();
        ShowHeader();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Gold1);

        table.AddColumn("[gold1]Spelare[/]");
        table.AddColumn("[green]Vinster[/]");
        table.AddColumn("[red]Förluster[/]");
        table.AddColumn("[yellow]Oavgjorda[/]");

        foreach (var result in statistics)
        {
            table.AddRow(
                Markup.Escape(result.Key),
                result.Value.Wins.ToString(),
                result.Value.Losses.ToString(),
                result.Value.Pushes.ToString());
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        ShowMessage("Tryck på valfri tangent för att återgå.");
        Console.ReadKey(true);
    }

    // Displays the Blackjack header.
    public void ShowHeader()
    {
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
    }

    // Displays a message.
    public void ShowMessage(string message)
    {
        AnsiConsole.MarkupLine(
            $"[white]{Markup.Escape(message)}[/]"
        );
    }

    // Asks the player to buy a card or stand.
    public string AskMove()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold gold1]Vad vill du göra?[/]")
                .HighlightStyle(
                    new Style(
                        foreground: Color.White,
                        background: new Color(0, 100, 50)
                    )
                )
                .AddChoices(
                    "♠ Köpa",
                    "♥ Stanna"
                )
        );
    }

    // Displays the round result.
    public void ShowResult(string result)
    {
        var panel = new Panel(
            new Markup(
                $"[bold gold1]{Markup.Escape(result)}[/]"
            )
        )
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Gold1);

        AnsiConsole.Write(panel);
    }

    // Displays the Blackjack rules.
    public void ShowRules()
    {
        AnsiConsole.Clear();

        var rules = new Panel(
            new Markup(
                "[bold gold1]♥ SPELREGLER ♥[/]\n\n" +

                "[bold gold1]MÅL[/]\n" +
                "[white]Kom så nära 21 som möjligt utan att gå över.[/]\n" +
                "[white]Din hand ska vara högre än givarens.[/]\n\n" +

                "[bold gold1]KORTENS VÄRDE[/]\n" +
                "[white]2–10 = Kortets värde[/]\n" +
                "[white]J, Q, K = 10 poäng[/]\n" +
                "[white]Ess = 1 eller 11 poäng[/]\n\n" +

                "[bold gold1]SPELETS GÅNG[/]\n" +
                "[white]Varje spelare får två öppna kort.[/]\n" +
                "[white]Givaren får ett öppet och ett dolt kort.[/]\n" +
                "[white]Ess + tiokort på de två första korten[/]\n" +
                "[white]är naturlig tjugoett.[/]\n\n" +

                "[bold gold1]DIN TUR[/]\n" +
                "[white]♠ Köpa – Ta ett nytt kort.[/]\n" +
                "[white]♥ Stanna – Avsluta din tur.[/]\n" +
                "[white]Över 21 innebär att du spräcker och förlorar.[/]\n\n" +

                "[bold gold1]GIVARENS TUR[/]\n" +
                "[white]Givaren visar sitt dolda kort.[/]\n" +
                "[white]Givaren köper på 16 eller lägre.[/]\n" +
                "[white]Givaren stannar på 17 eller högre,[/]\n" +
                "[white]även på mjuk 17.[/]\n\n" +

                "[bold gold1]VINST OCH FÖRLUST[/]\n" +
                "[white]Givaren spräcker = spelaren vinner,[/]\n" +
                "[white]om spelaren inte redan har spräckt.[/]\n" +
                "[white]Annars vinner högsta handen.[/]\n" +
                "[white]Lika poäng innebär oavgjort.[/]\n" +
                "[white]Naturlig 21 slår vanlig 21.[/]\n" +
                "[white]Två naturliga 21 ger oavgjort.[/]"
            )
        )
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Gold1);

        AnsiConsole.Write(rules);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            "[grey]Tryck på valfri tangent för att återgå.[/]"
        );

        Console.ReadKey(true);
    }

    // Displays the contributors.
    public void ShowCredits()
    {
        AnsiConsole.Clear();

        var credits = new Panel(
            new Markup(
                "[bold gold1]♣ MEDVERKANDE ♣[/]\n\n" +
                "[white]Alice       - Interface[/]\n" +
                "[white]Sofia       - GameStorage[/]\n" +
                "[white]Christoffer - Cards[/]\n" +
                "[white]Niclas      - Participant[/]"
            )
        )
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Gold1);

        AnsiConsole.Write(credits);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            "[grey]Tryck på valfri tangent för att återgå.[/]"
        );

        Console.ReadKey(true);
    }

    // Asks how many players will join.
    public int AskPlayerCount()
    {
        AnsiConsole.Clear();
        ShowHeader();

        return AnsiConsole.Prompt(
            new SelectionPrompt<int>()
                .Title("[bold gold1]♠ Hur många spelare? ♠[/]")
                .HighlightStyle(
                    new Style(
                        foreground: Color.White,
                        background: new Color(0, 100, 50)
                    )
                )
                .AddChoices(1, 2, 3, 4, 5)
        );
    }
}
