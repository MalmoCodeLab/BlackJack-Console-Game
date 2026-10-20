using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

public partial class ConsoleUi
{
    private readonly string[] menuOptions = GameMessages.MainMenuOptions;

    // Main menu with arrow-key navigation.
    public string ShowMenu(string? notice = null)
    {
        int selectedOption = 0;
        string choice = "";
        RunScreen(
            () => CreateMenu(selectedOption, notice),
            key =>
            {
                if (key == ConsoleKey.UpArrow)
                    selectedOption = (selectedOption - 1 + menuOptions.Length) % menuOptions.Length;
                else if (key == ConsoleKey.DownArrow)
                    selectedOption = (selectedOption + 1) % menuOptions.Length;
                else if (key == ConsoleKey.Enter)
                {
                    choice = menuOptions[selectedOption];
                    return true;
                }
                return false;
            });
        return choice;
    }

    // Choose 1–5 players. Esc returns 0 so Game can return to the main menu.
    public int AskPlayerCountCentered()
    {
        const int maxPlayers = 5;
        int selectedOption = 0;
        int playerCount = 0;
        RunScreen(
            () => CreatePlayerCountMenu(selectedOption),
            key =>
            {
                if (key == ConsoleKey.UpArrow)
                    selectedOption = (selectedOption - 1 + maxPlayers) % maxPlayers;
                else if (key == ConsoleKey.DownArrow)
                    selectedOption = (selectedOption + 1) % maxPlayers;
                else if (key == ConsoleKey.Enter)
                {
                    playerCount = selectedOption + 1;
                    return true;
                }
                else if (key == ConsoleKey.Escape)
                {
                    return true;
                }
                return false;
            });
        // Hide the cursor before entering the name prompt, but not on Esc.
        if (playerCount > 0)
            Console.Write("\u001b[?25l");
        return playerCount;
    }

    // Keep the full-screen menus away from the shell's scrollback buffer.
    // GameTableUI uses a separate alternate screen for gameplay as well.
    private static void EnterMenuScreen()
    {
        Console.Write("\u001b[0m\u001b[?1049h\u001b[?25l\u001b[H");
        Console.Out.Flush();
    }

    private static void LeaveMenuScreen()
    {
        Console.Write("\u001b[0m\u001b[?25h\u001b[?1049l");
        Console.Out.Flush();
    }

    // Only clear when transitioning between views or on a resize.
    // Do not clear for each arrow key; that makes the menus flash.
    private static void ClearMenuTransition()
    {
        Console.Write("\u001b[0m\u001b[?25l\u001b[2J\u001b[H");
        Console.Out.Flush();
    }

    // Shows a menu screen in the middle of the window and redraws it when the
    // window changes size. handleKey returns true when the menu should close.
    private void RunScreen(Func<IRenderable> build, Func<ConsoleKey, bool> handleKey)
    {
        int shownWidth = Math.Max(1, Console.WindowWidth);
        int shownHeight = Math.Max(1, Console.WindowHeight);

        void Draw()
        {
            // Reserve two rows so the Spectre renderer cannot scroll the buffer
            // when it writes its final newline.
            int height = Math.Max(1, Console.WindowHeight - 2);
            var screen = new Align(build(), HorizontalAlignment.Center, VerticalAlignment.Middle)
                .Height(height);
            int previousHeight = AnsiConsole.Profile.Height;
            AnsiConsole.Profile.Height = height;
            try
            {
                // On terminals that support synchronized updates, present the
                // whole redraw at once instead of showing intermediate frames.
                // Unsupported terminals simply ignore these ANSI sequences.
                Console.Write("\u001b[?2026h\u001b[0m\u001b[H\u001b[?25l");
                AnsiConsole.Write(screen);
            }
            finally
            {
                AnsiConsole.Profile.Height = previousHeight;
                Console.Write("\u001b[?2026l");
                Console.Out.Flush();
            }
        }

        // Keep navigation on an isolated terminal screen. This prevents
        // fragments of previous menus being mixed into the name prompt.
        EnterMenuScreen();
        try
        {
            Draw();
            while (true)
            {
                // A resize can leave remnants at old positions; clear only then.
                if (Console.WindowWidth != shownWidth || Console.WindowHeight != shownHeight)
                {
                    shownWidth = Math.Max(1, Console.WindowWidth);
                    shownHeight = Math.Max(1, Console.WindowHeight);
                    ClearMenuTransition();
                    Draw();
                }

                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(35);
                    continue;
                }

                ConsoleKey key = Console.ReadKey(true).Key;
                if (handleKey(key))
                    return;

                // Only Up/Down changes the selected menu option. Avoid
                // rebuilding the entire screen for unrelated key presses.
                if (key is ConsoleKey.UpArrow or ConsoleKey.DownArrow)
                    Draw();
            }
        }
        finally
        {
            LeaveMenuScreen();
        }
    }

    private IRenderable CreatePlayerCountMenu(int selectedOption)
    {
        List<IRenderable> rows = new()
        {
            new Align(new Markup(GameMessages.GameTitleMarkup), HorizontalAlignment.Center),
            new Align(new Markup(GameMessages.GameSubtitleMarkup), HorizontalAlignment.Center),
            new Text(""),
            new Rule(GameMessages.PlayerCountTitleMarkup).RuleStyle(Color.Gold1),
            new Text("")
        };
        for (int i = 0; i < 5; i++)
        {
            string text = string.Format(GameMessages.PlayerCountFormat, i + 1);
            string label = text.PadRight(17);
            string line = i == selectedOption
                ? $"[bold white on #961923] ❯ {label} [/]"
                : $"[white]   {label} [/]";
            rows.Add(new Align(new Markup(line), HorizontalAlignment.Center));
            rows.Add(new Text(""));
        }
        rows.Add(new Rule().RuleStyle(Color.Gold1));
        rows.Add(new Align(new Markup("[grey]↑ ↓ Navigera  •  Enter Välj  •  Esc Tillbaka[/]"), HorizontalAlignment.Center));
        return CenterMenuPanel(rows);
    }

    private IRenderable CreateMenu(int selectedOption, string? notice)
    {
        List<IRenderable> rows = new()
        {
            new Align(new Markup(GameMessages.GameTitleMarkup), HorizontalAlignment.Center),
            new Align(new Markup(GameMessages.GameSubtitleMarkup), HorizontalAlignment.Center),
            new Text(""),
            new Rule(GameMessages.MainMenuTitleMarkup).RuleStyle(Color.Gold1),
            new Text("")
        };
        if (notice is not null)
        {
            rows.Add(new Align(
                new Markup($"[bold yellow]{Markup.Escape(notice)}[/]"),
                HorizontalAlignment.Center));
            rows.Add(new Text(""));
        }

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
        rows.Add(new Align(new Markup(GameMessages.MenuNavigationMarkup), HorizontalAlignment.Center));
        return CenterMenuPanel(rows);
    }

    private IRenderable CenterMenuPanel(List<IRenderable> rows)
    {
        var panel = new Panel(new Rows(rows))
            .Border(BoxBorder.Rounded).BorderColor(Color.Gold1).Expand();
        int terminalWidth = Math.Max(1, Console.WindowWidth);
        int menuWidth = Math.Min(68, terminalWidth);
        int leftPadding = (terminalWidth - menuWidth) / 2;
        return new Padder(panel).PadLeft(leftPadding)
            .PadRight(terminalWidth - menuWidth - leftPadding);
    }

    // Builds the horizontal Hit, Stand and Bet buttons.
    public Panel BuildPlayerActions(
        string[] options,
        int selectedOption,
        bool sideMenuActive,
        bool roundFinished,
        bool bettingMode)
    {
        var actions = new Grid();
        for (int i = 0; i < options.Length; i++)
            actions.AddColumn(new GridColumn().NoWrap().PadRight(5));
        var buttons = new Spectre.Console.Rendering.IRenderable[options.Length];
        for (int i = 0; i < options.Length; i++)
        {
            string label = Markup.Escape(options[i]);
            bool selected = !sideMenuActive &&
                            !roundFinished && i == selectedOption &&
                            !(bettingMode && i == 0);

            // Hit cannot be used during betting, so keep it unhighlighted.
            if (i == 0 && !roundFinished)
            {
                buttons[i] = bettingMode
                    ? new Markup($"[grey]   {label}[/]")
                    : new Markup(selected
                        ? $"[bold white on green3] ❯ {label} [/]"
                        : $"[white]   {label}[/]");
                continue;
            }

            // All other button colors remain unchanged.
            string highlight = bettingMode && i == 2
                ? "[bold black on gold1]"
                : "[bold white on #961923]";
            buttons[i] = new Markup(selected
                ? $"{highlight} ❯ {label} [/]"
                : $"[white]   {label}[/]");
        }
        actions.AddRow(buttons);
        return new Panel(new Align(actions, HorizontalAlignment.Center))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1)
            .Expand();
    }

    // Builds the in-game menu in two responsive layouts.
    // A compact sidebar is used when the table has enough horizontal space;
    // otherwise the menu opens as a centered, two-column screen.
    internal Panel BuildGameMenu(
        string[] menuOptions,
        int selectedOption,
        List<Player> players,
        bool sideMenuActive,
        bool roundFinished,
        string[] playerStatuses,
        int activePlayerIndex = 0,
        bool forceSidebar = false)
    {
        bool menuOpen = sideMenuActive || roundFinished;
        // At a normal terminal width the menu is always on the right. Esc
        // changes selection focus; it does not hide the player overview.
        bool useFullScreen = menuOpen && !forceSidebar;

        int focusedIndex = players.Count > 0
            ? Math.Clamp(activePlayerIndex, 0, players.Count - 1)
            : 0;

        // On narrow screens Esc opens a separate menu while the game waits.
        if (useFullScreen)
        {
            var actionRows = new List<IRenderable>
            {
                new Align(new Markup("[bold gold1]VÄLJ HANDLING[/]"),
                    HorizontalAlignment.Center),
                new Text("")
            };

            for (int i = 0; i < menuOptions.Length; i++)
            {
                string option = Markup.Escape(menuOptions[i]);
                string line = i == selectedOption
                    ? $"[bold white on #961923] ❯ {option.PadRight(23)} [/]"
                    : $"[white]   {option}[/]";
                actionRows.Add(new Markup(line));
            }

            actionRows.Add(new Text(""));
            actionRows.Add(new Rule().RuleStyle(Color.Gold1));
            actionRows.Add(new Align(new Markup("[grey]↑ ↓  Navigera[/]"),
                HorizontalAlignment.Center));
            actionRows.Add(new Align(new Markup(roundFinished
                    ? "[grey]Enter  Välj[/]"
                    : "[grey]Enter  Välj  •  Esc  Tillbaka[/]"),
                HorizontalAlignment.Center));

            var actionPanel = new Panel(new Rows(actionRows))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Gold1)
                .Padding(1, 0, 1, 0);

            var infoRows = BuildGameMenuInformation(players, playerStatuses,
                focusedIndex, compact: Console.WindowWidth < 116 || Console.WindowHeight < 32);
            infoRows.Insert(0, new Align(
                new Markup("[bold gold1]SPELARÖVERSIKT[/]"),
                HorizontalAlignment.Center));
            var infoPanel = new Panel(new Rows(infoRows))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Gold1)
                .Padding(1, 0, 1, 0);

            var columns = new Grid();
            columns.AddColumn(new GridColumn().NoWrap());
            columns.AddColumn(new GridColumn().NoWrap());
            columns.AddColumn(new GridColumn().NoWrap());
            columns.AddRow(actionPanel, new Text("   "), infoPanel);

            var main = new Rows(
                new Align(new Markup("[bold gold1]♠ SPELMENY ♠[/]"),
                    HorizontalAlignment.Center),
                new Align(new Markup("[grey]BLACKJACK 21[/]"),
                    HorizontalAlignment.Center),
                new Text(""),
                new Align(columns, HorizontalAlignment.Center));

            // Center the menu vertically instead of leaving a large empty area
            // below the content. The outer gold border still fills the screen.
            int innerHeight = Math.Max(1, Console.WindowHeight - 2);
            return new Panel(new Align(main,
                    HorizontalAlignment.Center, VerticalAlignment.Middle)
                    .Height(innerHeight))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Gold1)
                .Expand();
        }

        // Always show the menu choices, player stats, balance and bets.
        // The selected option is highlighted only when Esc has focused it.
        var sidebarRows = new List<IRenderable>
        {
            new Align(new Markup("[bold gold1]♠ SPELMENY ♠[/]"),
                HorizontalAlignment.Center),
            new Align(new Markup(menuOpen
                ? "[gold1]↑ ↓ Välj   Enter Bekräfta[/]"
                : "[grey]Esc: Aktivera menyn[/]"),
                HorizontalAlignment.Center),
            new Rule("[gold1]VAL[/]").RuleStyle(Color.Gold1)
        };

        for (int i = 0; i < menuOptions.Length; i++)
        {
            string option = Markup.Escape(menuOptions[i]);
            sidebarRows.Add(new Markup(menuOpen && i == selectedOption
                ? $"[bold white on #961923] ❯ {option} [/]"
                : $"[white]   {option}[/]"));
        }

        sidebarRows.AddRange(BuildGameMenuInformation(players, playerStatuses,
            focusedIndex, compact: true));

        return new Panel(new Rows(sidebarRows))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1)
            .Expand();
    }

    // Detailed information is used in the full-screen fallback. The compact
    // sidebar uses one line per player so five players fit in the terminal.
    private static List<IRenderable> BuildGameMenuInformation(
        List<Player> players,
        string[] playerStatuses,
        int focusedIndex,
        bool compact)
    {
        var rows = new List<IRenderable>();
        rows.Add(new Rule("[gold1]STATISTIK[/]").RuleStyle(Color.Gold1));

        if (compact)
        {
            rows.Add(new Markup("[grey]Spelare   V / F / O / R[/]"));
            foreach (Player player in players)
            {
                string name = player.Name.Length > 8
                    ? player.Name[..7] + "…" : player.Name;
                rows.Add(new Markup(
                    $"[white]{Markup.Escape(name.PadRight(8))}[/] " +
                    $"[green1]{player.Wins}[/]/[red]{player.Losses}[/]/" +
                    $"[gold1]{player.Pushes}[/]/[deepskyblue1]{player.GamesPlayed}[/]"));
            }
        }
        else
        {
            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];
                rows.Add(new Markup($"[bold]{Markup.Escape(player.Name)}[/]"));
                rows.Add(new Markup(
                    $"[green1]V: {player.Wins}[/]  [red]F: {player.Losses}[/]  " +
                    $"[gold1]O: {player.Pushes}[/]  [blue]R: {player.GamesPlayed}[/]"));
                if (i < playerStatuses.Length &&
                    !string.IsNullOrWhiteSpace(playerStatuses[i]))
                    rows.Add(new Markup($"[grey]{Markup.Escape(playerStatuses[i])}[/]"));
                if (i < players.Count - 1)
                    rows.Add(new Rule().RuleStyle(Color.Grey30));
            }
        }

        rows.Add(new Rule("[gold1]SPELMARKER[/]").RuleStyle(Color.Gold1));
        rows.Add(new Markup("[grey]SPELARE     SALDO INSATS[/]"));
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            string name = player.Name.Length > 9
                ? player.Name[..8] + "…" : player.Name;
            string selected = i == focusedIndex ? "[bold gold1]❯[/]" : " ";
            string style = i == focusedIndex ? "bold white" : "grey";
            rows.Add(new Markup(
                $"{selected}[{style}]{Markup.Escape(name.PadRight(9))}[/] " +
                $"[green1]{player.Chips.ToString().PadLeft(5)}[/] " +
                $"[gold1]{player.CurrentBet.ToString().PadLeft(6)}[/]"));
        }
        return rows;
    }

    // Shows controls on a separate screen without changing the current game state.
    public void ShowGameControls()
    {
        ClearMenuTransition();

        var rows = new List<IRenderable>
        {
            new Align(new Markup("[bold gold1]♠ SPELSTYRNING ♠[/]"), HorizontalAlignment.Center),
            new Text(""),
            new Rule("[gold1]MENYN[/]").RuleStyle(Color.Gold1),
            new Markup("[white]↑ ↓[/]  Navigera mellan menyval"),
            new Markup("[white]Enter[/]  Bekräfta val"),
            new Text(""),
            new Rule("[gold1]UNDER SPELET[/]").RuleStyle(Color.Gold1),
            new Markup("[white]← →[/]  Välj handling"),
            new Markup("[white]Enter[/]  Bekräfta handling"),
            new Text(""),
            new Rule("[gold1]SATSNING[/]").RuleStyle(Color.Gold1),
            new Markup("[white]← →[/]  Välj marker"),
            new Markup("[white]Enter[/]  Lägg till marker"),
            new Markup("[white]Mellanslag[/]  Bekräfta insats"),
            new Markup("[white]Esc[/]  Öppna spelmenyn"),
            new Markup("[white]Backspace[/]  Ta bort marker"),
            new Markup("[grey]Inställningar ändras i huvudmenyn[/]"),
            new Text(""),
            new Align(new Markup("[grey]Tryck Enter för att återgå till spelet.[/]"), HorizontalAlignment.Center)
        };

        AnsiConsole.Write(CenterMenuPanel(rows));
        while (Console.ReadKey(true).Key != ConsoleKey.Enter)
        {
        }
        ClearMenuTransition();
    }

    // Returns null if the player presses Esc.
    public string? AskPlayerNameCentered(int playerNumber)
    {
        var name = new System.Text.StringBuilder();
        bool showError = false;
        // Hide the hardware cursor as well as the ANSI terminal cursor.
        void HideInputCursor()
        {
            try
            {
                if (!Console.IsOutputRedirected)
                    Console.CursorVisible = false;
            }
            catch (IOException) { }
            catch (PlatformNotSupportedException) { }
            Console.Write("\u001b[?25l");
        }

        // The name prompt must not share scrollback with the previous menu.
        // It also uses cursor-positioning commands, so isolate its screen.
        EnterMenuScreen();
        HideInputCursor();
        int previousPromptWidth = Math.Max(1, Console.WindowWidth);
        int previousPromptHeight = Math.Max(1, Console.WindowHeight);

        try
        {
            while (true)
            {
                int width = Math.Max(1, Console.WindowWidth);
                int height = Math.Max(1, Console.WindowHeight);
                // The prompt moves on resize; remove it from its old position.
                if (width != previousPromptWidth || height != previousPromptHeight)
                {
                    previousPromptWidth = width;
                    previousPromptHeight = height;
                    ClearMenuTransition();
                    HideInputCursor();
                }
                string prompt = $"Ange namn för spelare {playerNumber}:";
                int top = Math.Max(0, height / 2 - 2);
                int left = Math.Max(0, (width - 44) / 2);
                int available = Math.Max(0, width - left - 1);
                int maxNameLength = Math.Max(1,
                    Math.Min(24, Math.Max(1, available - prompt.Length - 1)));

                // Draw all three lines in one terminal write to minimize cursor flashes.
                var frame = new System.Text.StringBuilder("\u001b[?25l\u001b[?2026h");

                void AppendLine(int row, string text, string color)
                {
                    if (row >= height || available == 0)
                        return;

                    string visible = text.Length > available
                        ? text[..available]
                        : text;
                    frame.Append($"\u001b[{row + 1};{left + 1}H\u001b[0K");
                    frame.Append(color);
                    frame.Append(visible);
                    frame.Append("\u001b[0m");
                }

                AppendLine(top, prompt + " " + name, "\u001b[93m");
                AppendLine(top + 1,
                    "Enter: Bekräfta    Esc: Tillbaka till huvudmenyn", "\u001b[90m");
                AppendLine(top + 2,
                    showError ? "Du måste ange ett namn. Försök igen." : "",
                    "\u001b[91m");
                frame.Append("\u001b[?2026l\u001b[?25l");
                Console.Write(frame.ToString());
                HideInputCursor();

                ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                HideInputCursor();
                if (key.Key == ConsoleKey.Escape)
                    return null;
                if (key.Key == ConsoleKey.Enter)
                {
                    string entered = name.ToString().Trim();
                    if (entered.Length > 0)
                        return entered;
                    showError = true;
                }
                else if (key.Key == ConsoleKey.Backspace && name.Length > 0)
                {
                    name.Length--;
                    showError = false;
                }
                else if (!char.IsControl(key.KeyChar) && name.Length < maxNameLength)
                {
                    name.Append(key.KeyChar);
                    showError = false;
                }
            }
        }
        finally
        {
            LeaveMenuScreen();
            try
            {
                if (!Console.IsOutputRedirected)
                    Console.CursorVisible = true;
            }
            catch (IOException) { }
            catch (PlatformNotSupportedException) { }
        }
    }

    public Spectre.Console.Rendering.IRenderable BuildSavedPlayersMenu(
        List<string> names, Dictionary<string, PlayerStatistics> results,
        int selected, bool confirmingDelete)
    {
        var rows = new List<Spectre.Console.Rendering.IRenderable>
        {
            new Align(new Markup(GameMessages.GameTitleMarkup), HorizontalAlignment.Center),
            new Align(new Markup(GameMessages.GameSubtitleMarkup), HorizontalAlignment.Center),
            new Text(""),
            new Rule("[bold gold1]♠ LADDA SPEL ♠[/]").RuleStyle(Color.Gold1),
            new Align(new Markup($"[grey]SPARADE SPELARE  ({names.Count}/3)[/]"), HorizontalAlignment.Center),
            new Text("")
        };
        for (int i = 0; i < names.Count; i++)
        {
            string label = Markup.Escape(names[i]);
            string line = i == selected
                ? $"[bold white on #961923] ❯  {label}  [/]"
                : $"[white]    {label}[/]";
            rows.Add(new Align(new Markup(line), HorizontalAlignment.Center));
        }
        for (int i = names.Count; i < 3; i++)
            rows.Add(new Align(new Markup("[grey]    — Tom plats —[/]"), HorizontalAlignment.Center));
        PlayerStatistics stats = results[names[selected]];
        rows.Add(new Text(""));
        rows.Add(new Rule("[gold1]SPELARSTATISTIK[/]").RuleStyle(Color.Gold1));
        var table = new Table().NoBorder().HideHeaders();
        table.AddColumn(new TableColumn("").RightAligned());
        table.AddColumn(new TableColumn("").LeftAligned());
        table.AddColumn(new TableColumn("").RightAligned());
        table.AddColumn(new TableColumn("").LeftAligned());
        table.AddRow("[white]Vinster[/]", $"[green1]{stats.Wins}[/]", "[white]Förluster[/]", $"[red]{stats.Losses}[/]");
        table.AddRow("[white]Oavgjorda[/]", $"[gold1]{stats.Pushes}[/]", "[white]Rundor[/]", $"[deepskyblue1]{stats.Wins + stats.Losses + stats.Pushes}[/]");
        rows.Add(new Align(table, HorizontalAlignment.Center));
        rows.Add(new Text(""));
        if (confirmingDelete)
        {
            rows.Add(new Align(new Markup($"[bold red]Ta bort {Markup.Escape(names[selected])} permanent?[/]"), HorizontalAlignment.Center));
            rows.Add(new Align(new Markup("[white]Enter: Ja     Esc: Nej[/]"), HorizontalAlignment.Center));
        }
        else
        {
            rows.Add(new Align(new Markup("[green1]Enter[/] [grey]Ladda spel[/]   [red]Delete[/] [grey]Ta bort[/]"), HorizontalAlignment.Center));
            rows.Add(new Align(new Markup("[grey]↑ / ↓: Välj spelare     Esc: Tillbaka[/]"), HorizontalAlignment.Center));
        }
        rows.Add(new Rule().RuleStyle(Color.Gold1));
        var panel = new Panel(new Rows(rows)).Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1).Expand();
        int terminalWidth = Math.Max(1, Console.WindowWidth);
        int panelWidth = Math.Min(72, terminalWidth);
        int left = (terminalWidth - panelWidth) / 2;
        return new Padder(panel).PadLeft(left).PadRight(terminalWidth - panelWidth - left);
    }
}
