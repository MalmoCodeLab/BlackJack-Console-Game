using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

public partial class ConsoleUi
{
    // Minimum space for the full Blackjack table and its side menu.
    public const int MinWidth = 100;
    public const int MinHeight = 30;

    // Clears the screen and scrollback in terminals that support ANSI escapes.

    // Displays the centered Blackjack header.
    public void ShowHeader()
    {
        var header = new Panel(new Rows(
            new Align(new Markup("[bold gold1]♠ BLACKJACK 21 ♠[/]"), HorizontalAlignment.Center),
            new Align(new Markup("[italic green3]Tur kan inte kompileras.[/]"), HorizontalAlignment.Center)))
            .Border(BoxBorder.Rounded).BorderColor(Color.Gold1).Expand();
        AnsiConsole.Write(CenterPanel(header));
    }

    // Centers a panel at up to 68 terminal columns.
    private static IRenderable CenterPanel(Panel panel)
    {
        int terminalWidth = Math.Max(1, Console.WindowWidth);
        int panelWidth = Math.Min(68, terminalWidth);
        int left = (terminalWidth - panelWidth) / 2;
        return new Padder(panel).PadLeft(left).PadRight(terminalWidth - panelWidth - left);
    }

    // Displays a centered message with a return instruction.
    public void ShowCenteredMessage(string message)
    {
        AnsiConsole.Clear();
        var panel = new Panel(new Rows(
            new Align(new Markup("[bold gold1]♠ BLACKJACK 21 ♠[/]"), HorizontalAlignment.Center),
            new Align(new Markup("[italic green3]Tur kan inte kompileras.[/]"), HorizontalAlignment.Center),
            new Text(""),
            new Rule().RuleStyle(Color.Gold1),
            new Text(""),
            new Align(new Markup($"[bold white]{Markup.Escape(message)}[/]"), HorizontalAlignment.Center),
            new Text(""),
            new Align(new Markup("[grey]Tryck på valfri tangent för att återgå.[/]"), HorizontalAlignment.Center)))
            .Border(BoxBorder.Rounded).BorderColor(Color.Gold1).Expand();
        AnsiConsole.Write(CenterPanel(panel));
        Console.ReadKey(true);
    }

    // Displays a message without waiting for input.
    public void ShowMessage(string message)
    {
        AnsiConsole.MarkupLine($"[white]{Markup.Escape(message)}[/]");
    }

    // Displays the game actions.
    public string AskMove()
    {
        return AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("[bold gold1]Vad vill du göra?[/]")
            .PageSize(3)
            .HighlightStyle(new Style(foreground: Color.White, background: new Color(0, 100, 50)))
            .AddChoices("♠ Köpa", "♥ Stanna", "♦ Satsa"));
    }

    // Opens betting mode when the player selects Bet.
    internal string AskMove(List<Player> players, Dealer dealer)
    {
        while (true)
        {
            string choice = AskMove();
            if (choice == "♦ Satsa")
            {
                ShowBettingMode(players, dealer);
                continue;
            }
            return choice;
        }
    }

    // Displays the round result.
    public void ShowResult(string result)
    {
        var panel = new Panel(new Align(
            new Markup($"[bold gold1]{Markup.Escape(result)}[/]"),
            HorizontalAlignment.Center))
            .Border(BoxBorder.Rounded).BorderColor(Color.Gold1).Expand();
        AnsiConsole.Write(CenterPanel(panel));
    }

    // Displays cumulative player statistics loaded from the results file.
    public void ShowStatistics(IReadOnlyDictionary<string, PlayerStatistics> results)
    {
        AnsiConsole.Clear();
        ShowHeader();

        if (results.Count == 0)
        {
            ShowMessage("Det finns inga sparade resultat.");
        }
        else
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Gold1);

            table.AddColumn("[gold1]Spelare[/]");
            table.AddColumn("[green]Vinster[/]");
            table.AddColumn("[red]Förluster[/]");
            table.AddColumn("[yellow]Oavgjorda[/]");
            table.AddColumn("[blue]Rundor[/]");

            foreach (var (name, statistics) in results)
            {
                int roundsPlayed = statistics.Wins + statistics.Losses + statistics.Pushes;
                table.AddRow(
                    Markup.Escape(name),
                    statistics.Wins.ToString(),
                    statistics.Losses.ToString(),
                    statistics.Pushes.ToString(),
                    roundsPlayed.ToString());
            }

            AnsiConsole.Write(table);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Tryck på valfri tangent för att återgå.[/]");
        Console.ReadKey(true);
    }

    // Displays the Blackjack rules from the game's RuleSet configuration.
    public void ShowRules(RuleSet rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        ShowRules(
            rules.DealerHitsSoft17,
            rules.DeckCount,
            rules.ReshuffleThreshold,
            rules.NaturalBlackjackBeatsThreeCard21
        );
    }

    // Displays a compact and neatly aligned overview of the game rules.
    // Optional values can be provided by the RuleSet integration later.
    public void ShowRules(
        bool? dealerHitsSoft17 = null,
        int? numberOfDecks = null,
        int? reshuffleBelowCards = null,
        bool? naturalBlackjackBeats21 = null)
    {
        AnsiConsole.Clear();
        Console.Write("\u001b[?25l");

        // Fallback values from the existing game. Not read from a file yet.
        string soft17 = dealerHitsSoft17.HasValue
            ? (dealerHitsSoft17.Value ? "Tar kort" : "Stannar")
            : "Stannar";
        string decks = (numberOfDecks ?? 6).ToString();
        string reshuffle = $"Under {reshuffleBelowCards ?? 15} kort kvar";
        string natural21 = naturalBlackjackBeats21.HasValue
            ? (naturalBlackjackBeats21.Value ? "Slår vanlig 21" : "Samma värde som vanlig 21")
            : "Slår vanlig 21";

        bool valuesProvided = dealerHitsSoft17.HasValue &&
            numberOfDecks.HasValue &&
            reshuffleBelowCards.HasValue &&
            naturalBlackjackBeats21.HasValue;

        // Two columns keep rule names and values aligned.
        var houseRules = new Grid();
        houseRules.AddColumn(new GridColumn().NoWrap().PadRight(4));
        houseRules.AddColumn(new GridColumn().NoWrap());
        houseRules.AddRow(
            new Markup("[white]Givare vid mjuk 17[/]"),
            new Markup($"[bold green1]{soft17}[/]"));
        houseRules.AddRow(
            new Markup("[white]Antal kortlekar[/]"),
            new Markup($"[bold green1]{decks}[/]"));
        houseRules.AddRow(
            new Markup("[white]Omblandning[/]"),
            new Markup($"[bold green1]{reshuffle}[/]"));
        houseRules.AddRow(
            new Markup("[white]Naturlig Blackjack[/]"),
            new Markup($"[bold green1]{natural21}[/]"));

        var rules = new Panel(new Rows(
            new Align(new Markup("[bold gold1]♠ BLACKJACK 21 ♠[/]"), HorizontalAlignment.Center),
            new Align(new Markup("[italic green3]Tur kan inte kompileras.[/]"), HorizontalAlignment.Center),
            new Text(""),
            new Rule("[gold1]♥ SPELREGLER ♥[/]").RuleStyle(Color.Gold1),
            new Text(""),
            new Markup("[bold gold1]MÅL[/]"),
            new Markup("[white]Kom så nära 21 som möjligt utan att gå över.\nDin hand ska vara högre än givarens.[/]"),
            new Text(""),
            new Rule("[gold1]♦ AKTIVA HUSREGLER ♦[/]").RuleStyle(Color.Gold1),
            new Text(""),
            houseRules,
            new Text(""),
            new Rule().RuleStyle(Color.Gold1),
            new Align(new Markup(valuesProvided
                ? "[grey]Regler från spelets inställningar.[/]"
                : "[grey]Exempelvärden – ännu inte kopplade till konfigurationsfilen.[/]"),
                HorizontalAlignment.Center),
            new Align(new Markup("[grey]Tryck på valfri tangent för att återgå.[/]"), HorizontalAlignment.Center)))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1)
            .Expand();

        AnsiConsole.Write(CenterPanel(rules));
        Console.Write("\u001b[?25l");
        Console.ReadKey(true);
        Console.Write("\u001b[?25l");
    }

    // Displays centered contributors.
    public void ShowCredits()
    {
        AnsiConsole.Clear();
        Console.Write("\u001b[?25l"); // Dölj terminalmarkören.

        // Align names and roles in two columns.
        var credits = new Grid();
        credits.AddColumn(new GridColumn().NoWrap().PadRight(6));
        credits.AddColumn(new GridColumn().NoWrap());
        credits.AddRow(new Markup("[white]Alice[/]"), new Markup("[gold1]Interface[/]"));
        credits.AddRow(new Markup("[white]Sofia[/]"), new Markup("[gold1]GameStorage[/]"));
        credits.AddRow(new Markup("[white]Christoffer[/]"), new Markup("[gold1]Cards[/]"));
        credits.AddRow(new Markup("[white]Niclas[/]"), new Markup("[gold1]Participant[/]"));

        var panel = new Panel(new Rows(
            new Align(new Markup("[bold gold1]♠ BLACKJACK 21 ♠[/]"), HorizontalAlignment.Center),
            new Align(new Markup("[italic green3]Tur kan inte kompileras.[/]"), HorizontalAlignment.Center),
            new Rule("[gold1]♣ MEDVERKANDE ♣[/]").RuleStyle(Color.Gold1),
            new Text(""),
            new Align(credits, HorizontalAlignment.Center),
            new Text(""),
            new Rule().RuleStyle(Color.Gold1),
            new Align(new Markup("[grey]Tryck på valfri tangent för att återgå.[/]"), HorizontalAlignment.Center)))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1)
            .Expand();

        AnsiConsole.Write(CenterPanel(panel));
        Console.Write("\u001b[?25l"); // Håll markören dold även efter att panelen ritats.
        Console.ReadKey(true);
        Console.Write("\u001b[?25l");
    }

    // Original player-count method kept for compatibility.
    public int AskPlayerCount()
    {
        AnsiConsole.Clear();
        ShowHeader();
        string choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("[bold gold1]♠ Hur många spelare? ♠[/]")
            .PageSize(5)
            .HighlightStyle(new Style(foreground: Color.White, background: new Color(0, 100, 50)))
            .AddChoices("1 spelare", "2 spelare", "3 spelare", "4 spelare", "5 spelare"));
        return int.Parse(choice.Split(' ')[0]);
    }
}
