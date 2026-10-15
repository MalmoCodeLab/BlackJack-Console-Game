using Spectre.Console;

public partial class ConsoleUi
{
    public string? SelectSavedPlayer(
        IReadOnlyDictionary<string, PlayerStatistics> savedResults,
        Func<string, bool> deletePlayer)
    {
        Dictionary<string, PlayerStatistics> results = new(
            savedResults, StringComparer.OrdinalIgnoreCase);
        List<string> names = new(results.Keys);
        names.Sort(StringComparer.OrdinalIgnoreCase);
        int selected = 0;
        bool confirmingDelete = false;
        string? error = null;
        string? selectedName = null;
        bool finished = false;

        AnsiConsole.Clear();
        AnsiConsole.Live(BuildSavedPlayersMenu(
            names, results, selected, confirmingDelete, error))
            .Start(context =>
            {
                context.Refresh();
                while (!finished)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    if (confirmingDelete)
                    {
                        if (key == ConsoleKey.Enter ||
                            key == ConsoleKey.Y ||
                            key == ConsoleKey.J)
                        {
                            string name = names[selected];
                            if (deletePlayer(name))
                            {
                                results.Remove(name);
                                names.RemoveAt(selected);
                                if (names.Count == 0)
                                {
                                    finished = true;
                                    break;
                                }

                                selected = Math.Min(selected, names.Count - 1);
                            }
                            else
                            {
                                error = GameMessages.DeleteSavedPlayerError;
                            }
                        }

                        confirmingDelete = false;
                    }
                    else
                    {
                        if (key == ConsoleKey.UpArrow)
                            selected = (selected - 1 + names.Count) % names.Count;
                        else if (key == ConsoleKey.DownArrow)
                            selected = (selected + 1) % names.Count;
                        else if (key == ConsoleKey.Delete)
                            confirmingDelete = true;
                        else if (key == ConsoleKey.Escape)
                        {
                            finished = true;
                            break;
                        }
                        else if (key == ConsoleKey.Enter)
                        {
                            selectedName = names[selected];
                            finished = true;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    context.UpdateTarget(
                        BuildSavedPlayersMenu(
                            names, results, selected, confirmingDelete, error));
                    context.Refresh();
                }
            });

        return selectedName;
    }

    private static Spectre.Console.Rendering.IRenderable BuildSavedPlayersMenu(
        List<string> names,
        IReadOnlyDictionary<string, PlayerStatistics> results,
        int selected,
        bool confirmingDelete,
        string? error)
    {
        List<Spectre.Console.Rendering.IRenderable> rows = new()
        {
            new Align(
                new Markup(GameMessages.GameTitleMarkup),
                HorizontalAlignment.Center),
            new Align(
                new Markup(GameMessages.GameSubtitleMarkup),
                HorizontalAlignment.Center),
            new Text(""),
            new Rule(GameMessages.SavedPlayersTitleMarkup).RuleStyle(Color.Gold1),
            new Align(
                new Markup($"[grey]SPARADE SPELARE  ({names.Count}/{GameMessages.MaximumSavedPlayers})[/]"),
                HorizontalAlignment.Center),
            new Text("")
        };

        for (int i = 0; i < names.Count; i++)
        {
            string name = names[i];
            PlayerStatistics stats = results[name];
            string label = GameMessages.SavedPlayerStatistics(
                name, stats.Wins, stats.Losses, stats.Pushes);
            string escaped = Markup.Escape(label);
            string line = i == selected
                ? $"[bold white on #961923] ❯  {escaped}  [/]"
                : $"[white]    {escaped}[/]";
            rows.Add(new Align(new Markup(line), HorizontalAlignment.Center));
        }

        for (int i = names.Count; i < GameMessages.MaximumSavedPlayers; i++)
            rows.Add(new Align(
                new Markup("[grey]    — Tom plats —[/]"),
                HorizontalAlignment.Center));

        PlayerStatistics selectedStats = results[names[selected]];
        rows.Add(new Text(""));
        rows.Add(new Rule("[gold1]SPELARSTATISTIK[/]").RuleStyle(Color.Gold1));
        var table = new Table().NoBorder().HideHeaders();
        table.AddColumn(new TableColumn("").RightAligned());
        table.AddColumn(new TableColumn("").LeftAligned());
        table.AddColumn(new TableColumn("").RightAligned());
        table.AddColumn(new TableColumn("").LeftAligned());
        table.AddRow(
            "[white]Vinster[/]", $"[green1]{selectedStats.Wins}[/]",
            "[white]Förluster[/]", $"[red]{selectedStats.Losses}[/]");
        table.AddRow(
            "[white]Oavgjorda[/]", $"[gold1]{selectedStats.Pushes}[/]",
            "[white]Rundor[/]",
            $"[deepskyblue1]{selectedStats.Wins + selectedStats.Losses + selectedStats.Pushes}[/]");
        rows.Add(new Align(table, HorizontalAlignment.Center));
        rows.Add(new Text(""));

        if (confirmingDelete)
        {
            string prompt = string.Format(
                GameMessages.DeleteSavedPlayerPromptFormat,
                Markup.Escape(names[selected]));
            rows.Add(new Align(
                new Markup($"[bold red]{prompt}[/]"),
                HorizontalAlignment.Center));
            rows.Add(new Align(
                new Markup("[white]Enter: Ja     Esc: Nej[/]"),
                HorizontalAlignment.Center));
        }
        else
        {
            rows.Add(new Align(
                new Markup("[green1]Enter[/] [grey]Ladda spel[/]   [red]Delete[/] [grey]Ta bort[/]"),
                HorizontalAlignment.Center));
            rows.Add(new Align(
                new Markup("[grey]↑ / ↓: Välj spelare     Esc: Tillbaka[/]"),
                HorizontalAlignment.Center));
        }

        if (error is not null)
            rows.Add(new Align(
                new Markup($"[bold red]{Markup.Escape(error)}[/]"),
                HorizontalAlignment.Center));

        rows.Add(new Rule().RuleStyle(Color.Gold1));
        var panel = new Panel(new Rows(rows))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1)
            .Expand();
        int terminalWidth = Math.Max(1, Console.WindowWidth);
        int panelWidth = Math.Min(72, terminalWidth);
        int left = (terminalWidth - panelWidth) / 2;
        return new Padder(panel)
            .PadLeft(left)
            .PadRight(terminalWidth - panelWidth - left);
    }
}
