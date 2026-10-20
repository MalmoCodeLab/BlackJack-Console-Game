using Spectre.Console;
using Spectre.Console.Rendering;

internal enum GameTableAction
{
    RoundCompleted,
    StartNewRound,
    ReturnToMainMenu,
    ExitGame
}

internal sealed class GameTableUI
{
    // Fit the table and the action buttons within the available terminal height.
    private static int TableHeight => Math.Max(4, Console.WindowHeight);
    private const int BlackjackNoticeDurationMs = 2400;
    private const int CardDealDelayMs = 400; // Delay between cards during the initial deal.

    private readonly ConsoleUi ui;
    private bool tableScreenActive;
    private Func<Layout>? rebuildLastTable;
    private int lastWidth;
    private int lastHeight;
    private int builtWidth;
    private int builtHeight;
    private readonly VisibleCardCounter visibleCardCounter = new();
    // Read-only observation: detect a refilled shoe without modifying Deck.
    private Deck? observedDeck;
    private int lowestCardsRemaining = int.MaxValue;


    public GameTableUI(ConsoleUi ui)
    {
        this.ui = ui;
    }

    public void ResetSession()
    {
        CloseTableScreen();
        BettingUI.Reset();
        visibleCardCounter.Reset();
        observedDeck = null;
        lowestCardsRemaining = int.MaxValue;
    }

    public GameTableAction PlayRound(
        Round round,
        Func<List<Player>, bool> saveGame)
    {
        List<Player> players = round.Players;
        string[] statuses = Enumerable.Repeat("VÄNTAR", players.Count).ToArray();
        int currentPlayerIndex = 0;
        int selectedGameOption = 0;
        int selectedMenuOption = 0;
        bool sideMenuActive = false;
        bool bettingMode = true;
        bool betsComplete = false;
        bool pendingBlackjackAdvance = false;
        bool insuranceMode = false;
        bool playing = true;
        GameTableAction action = GameTableAction.ReturnToMainMenu;

        static string BetTurnNotice(Player player) =>
            $"{player.Name.ToUpperInvariant()} – DIN TUR ATT SATSA: ENTER LÄGGER MARKER, MELLANSLAG BEKRÄFTAR";
        static string PlayTurnNotice(Player player) =>
            $"{player.Name.ToUpperInvariant()} – DIN TUR: TA KORT ELLER STANNA";
        static string InsuranceNotice(Player player) =>
            $"{player.Name.ToUpperInvariant()} – GIVAREN VISAR ESS. FÖRSÄKRING FÖR {player.InsuranceCost}? J = JA, N = NEJ";

        string? notice = BetTurnNotice(players[0]);
        DateTime noticeUntil = DateTime.MinValue;
        DateTime lastBetConfirmation = DateTime.MinValue;
        statuses[0] = "SATSAR";

        // Always redraw with the current round, selection and terminal size.
        void RedrawCurrentRound()
        {
            if (!playing)
                return;

            DrawTable(BuildTable(
                round, statuses, notice, selectedGameOption,
                selectedMenuOption, sideMenuActive,
                roundFinished: false, bettingMode: bettingMode,
                cardsFaceDown: !betsComplete,
                activePlayerIndex: currentPlayerIndex));
        }

        // Renders the already-dealt hands gradually. Round.StartRound has
        // created the cards before PlayRound; this does NOT deal again.
        void AnimateInitialDeal()
        {
            int[] visiblePlayers = new int[players.Count];
            int visibleDealer = 0;

            void DrawDealFrame()
            {
                DrawTable(BuildTable(
                    round, statuses, "GIVAREN DELAR UT KORTEN...",
                    selectedGameOption, selectedMenuOption,
                    sideMenuActive: false, roundFinished: false,
                    bettingMode: false, cardsFaceDown: false,
                    activePlayerIndex: 0,
                    dealerCardsShown: visibleDealer,
                    playerCardsShown: visiblePlayers));
                Thread.Sleep(CardDealDelayMs);
            }

            // First frame: table with no cards at all.
            DrawDealFrame();

            // Order: each player, dealer, each player, dealer.
            for (int card = 0; card < 2; card++)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].Hand.CardsOnHand.Count > card)
                    {
                        visiblePlayers[i]++;
                        DrawDealFrame();
                    }
                }
                if (round.Dealer.Hand.CardsOnHand.Count > card)
                {
                    visibleDealer++;
                    DrawDealFrame();
                }
            }
        }

        void FinishBetting()
        {
            bettingMode = false;
            currentPlayerIndex = 0;

            // No actual card-hand changes: reveal their existing hands in order.
            for (int i = 0; i < players.Count; i++)
                statuses[i] = "VÄNTAR";
            AnimateInitialDeal();
            betsComplete = true;

            // The dealer shows an ace: ask every player about insurance first.
            if (round.Dealer.ShowsAce)
            {
                insuranceMode = true;
                statuses[0] = "FÖRSÄKRING?";
                notice = InsuranceNotice(players[0]);
                return;
            }

            StartPlaying();
        }

        void StartPlaying()
        {
            currentPlayerIndex = 0;
            selectedGameOption = 0;
            notice = PlayTurnNotice(players[0]);

            pendingBlackjackAdvance = players[0].HasBlackjack;
            statuses[0] = pendingBlackjackAdvance ? "BLACKJACK" : "DIN TUR";
            if (pendingBlackjackAdvance)
            {
                notice = NaturalBlackjackNotice(players[0]);
                noticeUntil = DateTime.UtcNow.AddMilliseconds(BlackjackNoticeDurationMs);
            }
        }

        void AdvancePlayer()
        {
            currentPlayerIndex++;
            selectedGameOption = 0;

            while (currentPlayerIndex < players.Count)
            {
                Player nextPlayer = players[currentPlayerIndex];
                if (nextPlayer.HasBlackjack)
                {
                    statuses[currentPlayerIndex] = "BLACKJACK";
                    notice = NaturalBlackjackNotice(nextPlayer);
                    noticeUntil = DateTime.UtcNow.AddMilliseconds(BlackjackNoticeDurationMs);
                    pendingBlackjackAdvance = true;
                    return;
                }

                statuses[currentPlayerIndex] = "DIN TUR";
                notice = PlayTurnNotice(nextPlayer);
                return;
            }

            action = GameTableAction.RoundCompleted;
            playing = false;
        }

        OpenTableScreen();
        try
        {
            // The UI only observes a refill; Deck/Round own the shuffle rules.
            bool shoeRefilled = DetectRefilledShoe(round.Deck);
            if (shoeRefilled)
            {
                visibleCardCounter.Reset();
                DrawTable(BuildTable(
                    round, statuses, "KORTLEKEN HAR BLANDATS OM INFÖR DEN NYA RUNDAN!",
                    selectedGameOption, selectedMenuOption, sideMenuActive,
                    roundFinished: false, bettingMode: bettingMode,
                    cardsFaceDown: !betsComplete,
                    activePlayerIndex: currentPlayerIndex));
                Thread.Sleep(1600);
            }

            while (playing)
            {
                bool saveRequested = false;
                RedrawCurrentRound();

                while (playing && !saveRequested)
                {
                    // The Blackjack notice may expire, but Esc still opens the menu.
                    int timeout = pendingBlackjackAdvance && !sideMenuActive
                        ? Math.Max(1, (int)(noticeUntil - DateTime.UtcNow).TotalMilliseconds)
                        : -1;
                    ConsoleKey key = ReadTableKey(timeout);

                    // Only Esc opens or closes the in-game menu.
                    if (key == ConsoleKey.Escape)
                    {
                        sideMenuActive = !sideMenuActive;
                        if (sideMenuActive)
                            selectedMenuOption = 1; // Huvudmeny
                        RedrawCurrentRound();
                        continue;
                    }

                    // Handle the menu before betting or player actions.
                    if (sideMenuActive)
                    {
                        if (key == ConsoleKey.UpArrow)
                        {
                            selectedMenuOption = (selectedMenuOption - 1 +
                                GameMessages.InGameMenuOptions.Length) %
                                GameMessages.InGameMenuOptions.Length;
                        }
                        else if (key == ConsoleKey.DownArrow)
                        {
                            selectedMenuOption = (selectedMenuOption + 1) %
                                GameMessages.InGameMenuOptions.Length;
                        }
                        else if (key == ConsoleKey.Enter)
                        {
                            // Settings are available from the main menu only.
                            switch (selectedMenuOption)
                            {
                                case 0: // Ny runda
                                    action = GameTableAction.StartNewRound;
                                    playing = false;
                                    break;
                                case 1: // Huvudmeny
                                    action = GameTableAction.ReturnToMainMenu;
                                    playing = false;
                                    break;
                                case 2: // Spelstyrning
                                    ShowControlsScreen();
                                    break;
                                case 3: // Avsluta
                                    saveRequested = true;
                                    break;
                            }
                        }

                        RedrawCurrentRound();
                        continue;
                    }

                    // Give the natural-Blackjack message time to be displayed.
                    if (pendingBlackjackAdvance)
                    {
                        if (DateTime.UtcNow < noticeUntil)
                            continue;

                        notice = null;
                        pendingBlackjackAdvance = false;
                        AdvancePlayer();
                        RedrawCurrentRound();
                        continue;
                    }

                    if (key == ConsoleKey.NoName)
                        continue;

                    if (!betsComplete)
                    {
                        Player bettingPlayer = players[currentPlayerIndex];
                        if (key == ConsoleKey.LeftArrow ||
                            key == ConsoleKey.RightArrow ||
                            key == ConsoleKey.Enter ||
                            key == ConsoleKey.Backspace)
                        {
                            BettingUI.HandleKey(key, bettingPlayer);
                        }
                        else if (key == ConsoleKey.Spacebar)
                        {
                            // Prevent key-repeat from confirming the next player's bet.
                            bool isRepeat = (DateTime.UtcNow - lastBetConfirmation)
                                .TotalMilliseconds < 400;
                            if (!isRepeat && bettingPlayer.CurrentBet > 0)
                            {
                                lastBetConfirmation = DateTime.UtcNow;
                                statuses[currentPlayerIndex] = "SATSAT";
                                currentPlayerIndex++;

                                if (currentPlayerIndex == players.Count)
                                {
                                    FinishBetting();
                                }
                                else
                                {
                                    statuses[currentPlayerIndex] = "SATSAR";
                                    notice = BetTurnNotice(players[currentPlayerIndex]);
                                }
                            }
                            else if (!isRepeat)
                            {
                                notice = "DU MÅSTE SATSA MINST EN MARKER FÖRST!";
                            }
                        }

                        RedrawCurrentRound();
                        continue;
                    }

                    // Human player's turn. Card and betting logic stay in their
                    // existing classes; the UI only routes the chosen action.
                    switch (key)
                    {
                        case ConsoleKey.Spacebar:
                            selectedGameOption = 0;
                            break;
                        case ConsoleKey.LeftArrow:
                            selectedGameOption = Math.Max(0, selectedGameOption - 1);
                            break;
                        case ConsoleKey.RightArrow:
                            selectedGameOption = Math.Min(
                                GameMessages.GameOptions.Length - 1,
                                selectedGameOption + 1);
                            break;
                        case ConsoleKey.Enter:
                            if (selectedGameOption == 2)
                            {
                                notice = "INSATSEN ÄR REDAN BEKRÄFTAD FÖR DENNA RUNDA.";
                                break;
                            }

                            Player player = players[currentPlayerIndex];
                            if (player.CurrentBet <= 0)
                            {
                                notice = "DU MÅSTE SATSA INNAN DU KAN SPELA!";
                                break;
                            }

                            if (selectedGameOption == 0)
                                player.ReceiveCard(round.Deck);

                            if (selectedGameOption == 1 ||
                                player.IsBust ||
                                player.Score == Participant.BlackjackValue)
                            {
                                statuses[currentPlayerIndex] =
                                    GetTurnStatus(player, selectedGameOption);
                                AdvancePlayer();
                            }
                            break;
                    }

                    RedrawCurrentRound();
                }

                if (saveRequested && saveGame(players))
                {
                    action = GameTableAction.ExitGame;
                    playing = false;
                }
            }
        }
        catch
        {
            CloseTableScreen();
            throw;
        }
        finally
        {
            // Avoid flicker between the end of the round and the result view.
            if (action == GameTableAction.ReturnToMainMenu ||
                action == GameTableAction.ExitGame)
                CloseTableScreen();
        }

        return action;
    }

    public GameTableAction ShowRoundEndMenu(
        Round round,
        string summary,
        Func<List<Player>, bool> saveGame)
    {
        List<Player> players = round.Players;
        string[] statuses = players
            .Select(player => OutcomeStatus(round.Outcomes[player]))
            .ToArray();
        int selectedMenuOption = 0;
        GameTableAction action = GameTableAction.ReturnToMainMenu;
        string? notice = summary.Replace(
            Environment.NewLine, "  •  ", StringComparison.Ordinal);

        OpenTableScreen();
        try
        {
            bool finished = false;
            while (!finished)
            {
                bool saveRequested = false;
                {
                    DrawTable(BuildTable(
                        round, statuses, notice, selectedGameOption: 0,
                        selectedMenuOption: selectedMenuOption,
                        sideMenuActive: true,
                        roundFinished: true, bettingMode: false));
                    while (!finished && !saveRequested)
                    {

                        ConsoleKey key = ReadTableKey();
                        if (key == ConsoleKey.UpArrow)
                        {
                            selectedMenuOption =
                                (selectedMenuOption - 1 +
                                 GameMessages.InGameMenuOptions.Length) %
                                GameMessages.InGameMenuOptions.Length;
                        }
                        else if (key == ConsoleKey.DownArrow)
                        {
                            selectedMenuOption =
                                (selectedMenuOption + 1) %
                                GameMessages.InGameMenuOptions.Length;
                        }
                        else if (key == ConsoleKey.Enter)
                        {
                            if (selectedMenuOption == 0)
                            {
                                action = GameTableAction.StartNewRound;
                                finished = true;
                            }
                            else if (selectedMenuOption == 1)
                            {
                                action = GameTableAction.ReturnToMainMenu;
                                finished = true;
                            }
                            else if (selectedMenuOption == 2)
                            {
                                ShowControlsScreen();
                            }
                            else if (selectedMenuOption == 3)
                            {
                                saveRequested = true;
                            }
                        }

                        if (!finished && !saveRequested)
                        {
                            DrawTable(BuildTable(
                                round, statuses, notice, selectedGameOption: 0,
                                selectedMenuOption: selectedMenuOption,
                                sideMenuActive: true,
                                roundFinished: true, bettingMode: false));
                        }
                    }
                }

                if (saveRequested && saveGame(players))
                {
                    action = GameTableAction.ExitGame;
                    finished = true;
                }
            }
        }
        catch
        {
            CloseTableScreen();
            throw;
        }
        finally
        {
            // Starting a new round redraws in place instead of flashing.
            if (action != GameTableAction.StartNewRound)
                CloseTableScreen();
        }

        return action;
    }

    // Read-only detection of a shoe refill; never calls ShuffleDeck().
    private bool DetectRefilledShoe(Deck deck)
    {
        int remaining = deck.DeckCards.Count;
        if (!ReferenceEquals(observedDeck, deck))
        {
            observedDeck = deck;
            lowestCardsRemaining = remaining;
            visibleCardCounter.Reset();
            return false; // Initial shoe is not a reshuffle.
        }

        bool refilled = remaining > lowestCardsRemaining;
        lowestCardsRemaining = remaining;
        return refilled;
    }

    // Opens the controls view and returns to the current table without resetting the round.
    private void ShowControlsScreen()
    {
        // The table uses the terminal's alternate screen. Stay on that screen
        // so returning to the game does not recreate the terminal session.
        ui.ShowGameControls();
        Console.Write("\u001b[?25l");
        Console.Out.Flush();
    }

    // Use a fixed screen instead of Spectre.Live, which can move the cursor
    // and cause flicker when its display region is recreated.
    private void OpenTableScreen()
    {
        if (tableScreenActive)
            return;

        Console.Write("\u001b[?1049h\u001b[?25l\u001b[H\u001b[2J");
        Console.Out.Flush();
        tableScreenActive = true;
    }

    private void CloseTableScreen()
    {
        if (!tableScreenActive)
            return;

        Console.Write("\u001b[?25h\u001b[?1049l");
        Console.Out.Flush();
        tableScreenActive = false;
    }

    // Paint a complete frame without clearing the whole terminal between frames.
    // A window resize may happen in the middle of rendering, so try again
    // with a new layout instead of displaying a frame built for an old size.
    private void DrawTable(Layout layout)
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            int width = Math.Max(1, Console.WindowWidth);
            int height = Math.Max(1, Console.WindowHeight);

            if ((width != builtWidth || height != builtHeight) &&
                rebuildLastTable is not null)
            {
                layout = rebuildLastTable();
                width = Math.Max(1, Console.WindowWidth);
                height = Math.Max(1, Console.WindowHeight);
            }

            string[] lines = RenderLines(layout, width, height);

            // Never paint a half-old, half-new frame when VS Code is resizing.
            if (width != Console.WindowWidth || height != Console.WindowHeight)
            {
                if (rebuildLastTable is not null)
                    layout = rebuildLastTable();
                continue;
            }

            var frame = new System.Text.StringBuilder();
            frame.Append("\u001b[?2026h\u001b[?25l\u001b[H");

            int visibleRows = Math.Min(lines.Length, height);
            for (int row = 0; row < visibleRows; row++)
            {
                if (row > 0)
                    frame.Append("\r\n");

                frame.Append(lines[row]);
                // Clear the remainder of this row, including remnants of an
                // earlier frame with longer text. Reset styles first.
                frame.Append("\u001b[0m\u001b[K");
            }

            // Clear any old rows below the new frame. This avoids ghost menus
            // without flashing the screen with ESC[2J on every redraw.
            frame.Append("\u001b[0m\u001b[J\u001b[?2026l\u001b[?25l");

            Console.Write(frame.ToString());
            Console.Out.Flush();
            lastWidth = width;
            lastHeight = height;
            return;
        }
        // If the terminal changes size repeatedly, ReadTableKey will retry
        // when the size settles. The game state is left untouched.
    }

    // Renders the table to text with colors, exactly width x height cells.
    private static string[] RenderLines(Layout layout, int width, int height)
    {
        var writer = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Yes,
            ColorSystem = (ColorSystemSupport)AnsiConsole.Profile.Capabilities.ColorSystem,
            Out = new AnsiConsoleOutput(writer)
        });
        console.Profile.Width = width;
        console.Profile.Height = height;
        console.Profile.Capabilities.Unicode = AnsiConsole.Profile.Capabilities.Unicode;
        console.Write(layout);
        return writer.ToString().Replace("\r\n", "\n").TrimEnd('\n').Split('\n');
    }

    // Five multiplayer seats need 75 cells plus the Blackjack table border.
    // Reserve a sidebar only when the cards still fit without clipping.
    private const int MinimumTableWidth = 80;
    private const int MinimumTerminalHeight = 24;
    private const int SidebarMinimumWidth = 32;
    private const int SidebarMaximumWidth = 40;
    private const int ActionPanelHeight = 2;

    private static bool TerminalTooSmall =>
        Console.WindowWidth < MinimumTableWidth ||
        Console.WindowHeight < MinimumTerminalHeight;

    private static IRenderable BuildResizeMessage(int width, int height)
    {
        // Avoid rendering a blank/black terminal during a resize.
        // The game is paused while the cards cannot fit at their real size.
        string message = width >= 44
            ? "FÖRSTORA TERMINALEN FÖR ATT FORTSÄTTA"
            : width >= 22 ? "FÖRSTORA TERMINALEN" : "FÖR LITET";
        // Keep the felt-green background even in the small-window fallback.
        return new GreenTableBackground(
            new Align(
                new Markup($"[bold gold1]♠ {message} ♠[/]"),
                HorizontalAlignment.Center,
                VerticalAlignment.Middle).Height(height),
            new Color(7, 80, 46), height);
    }

    private Layout BuildTable(
        Round round,
        string[] statuses,
        string? notice,
        int selectedGameOption,
        int selectedMenuOption,
        bool sideMenuActive,
        bool roundFinished,
        bool bettingMode,
        bool cardsFaceDown = false,
        int activePlayerIndex = 0,
        int dealerCardsShown = -1,
        int[]? playerCardsShown = null)
    {
        // The same game state is rendered again when the VS Code terminal
        // changes size. This does not reset bets, cards, or menu selection.
        rebuildLastTable = () => BuildTable(
            round, statuses, notice, selectedGameOption, selectedMenuOption,
            sideMenuActive, roundFinished, bettingMode, cardsFaceDown,
            activePlayerIndex, dealerCardsShown, playerCardsShown);

        int width = Math.Max(1, Console.WindowWidth);
        int height = Math.Max(1, Console.WindowHeight);
        builtWidth = width;
        builtHeight = height;
        var layout = new Layout("Root").Size(height);

        // A tiny terminal cannot display five card seats reliably. Keep a
        // visible resize message instead of replacing the game with black.
        if (TerminalTooSmall)
        {
            layout.Update(BuildResizeMessage(width, height));
            return layout;
        }

        // Display the game menu and statistics on the right at normal widths.
        // With less space, let the table use the width it needs, and open a
        // centered menu using Esc. No hand/deck/bet state is changed.
        bool showSidebar = width >= MinimumTableWidth + SidebarMinimumWidth;
        if (!showSidebar && (sideMenuActive || roundFinished))
        {
            layout.Update(ui.BuildGameMenu(
                GameMessages.InGameMenuOptions, selectedMenuOption,
                round.Players, sideMenuActive, roundFinished, statuses,
                activePlayerIndex));
            return layout;
        }

        Layout tableArea = layout;
        if (showSidebar)
        {
            int sidebarWidth = Math.Min(SidebarMaximumWidth,
                width - MinimumTableWidth);
            layout.SplitColumns(
                new Layout("TableArea").Size(width - sidebarWidth),
                new Layout("Sidebar").Size(sidebarWidth));
            tableArea = layout["TableArea"];
            layout["Sidebar"].Update(ui.BuildGameMenu(
                GameMessages.InGameMenuOptions, selectedMenuOption,
                round.Players, sideMenuActive, roundFinished, statuses,
                activePlayerIndex, forceSidebar: true));
        }

        int tableHeight = height - ActionPanelHeight;

        // Keep these as TWO named layouts. Replacing them with one Rows()
        // renderable caused the action buttons to disappear previously.
        tableArea.SplitRows(
            new Layout("BlackjackTable").Size(tableHeight),
            new Layout("PlayerActions").Size(ActionPanelHeight));

        if (ReferenceEquals(observedDeck, round.Deck))
            lowestCardsRemaining = Math.Min(
                lowestCardsRemaining, round.Deck.DeckCards.Count);

        // An animation frame does not count cards that have not appeared yet.
        if (playerCardsShown is null)
            visibleCardCounter.Observe(
                round.Deck, round.Players, round.Dealer, cardsFaceDown);

        // Join the lower corners of the table to the footer borders.
        // The divider and both side borders now form one continuous frame.
        tableArea["BlackjackTable"].Update(
            new ConnectedTableBorder(ui.BuildBlackjackTable(
                round.Players, round.Dealer, tableHeight, notice, statuses,
                cardsFaceDown, visibleCardCounter.RunningCount,
                visibleCardCounter.TrueCount, activePlayerIndex,
                showEmptyTable: bettingMode && playerCardsShown is null,
                dealerCardsShown: dealerCardsShown,
                playerCardsShown: playerCardsShown)));

        // Render the action buttons WITHOUT a separate Panel top border.
        // Row 1 holds the buttons immediately below the table frame;
        // row 2 is a gold bottom rule. No blank row separates them.
        tableArea["PlayerActions"].Update(
            BuildInlinePlayerActions(
                GameMessages.GameOptions, selectedGameOption,
                sideMenuActive, roundFinished, bettingMode));

        return layout;
    }

    // Spectre draws every Panel with rounded bottom corners. The player
    // actions continue immediately below it, so join those corners to the
    // footer instead of closing the frame before the buttons.
    // This changes only the border's final row (never card data or input).
    private sealed class ConnectedTableBorder : Renderable
    {
        private readonly IRenderable table;

        public ConnectedTableBorder(IRenderable table)
        {
            this.table = table;
        }

        protected override Measurement Measure(RenderOptions options, int maxWidth) =>
            table.Measure(options, maxWidth);

        protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            List<Segment> segments = table.Render(options, maxWidth).ToList();
            int startOfLine = 0;
            int bottomStart = -1;
            int bottomEnd = -1;

            for (int i = 0; i <= segments.Count; i++)
            {
                if (i < segments.Count && !segments[i].IsLineBreak)
                    continue;

                bool leftCorner = false;
                bool rightCorner = false;
                for (int j = startOfLine; j < i; j++)
                {
                    leftCorner |= segments[j].Text.Contains('╰');
                    rightCorner |= segments[j].Text.Contains('╯');
                }

                if (leftCorner && rightCorner)
                {
                    bottomStart = startOfLine;
                    bottomEnd = i;
                }
                startOfLine = i + 1;
            }

            for (int i = 0; i < segments.Count; i++)
            {
                Segment segment = segments[i];
                if (i >= bottomStart && i < bottomEnd && !segment.IsLineBreak)
                {
                    string joined = segment.Text.Replace('╰', '├').Replace('╯', '┤');
                    yield return new Segment(joined, segment.Style);
                }
                else
                {
                    yield return segment;
                }
            }
        }
    }

    // Two-row footer: side borders around the actions and a bottom border.
    // No top border, so it stays flush with the Blackjack table above.
    private static IRenderable BuildInlinePlayerActions(
        string[] options,
        int selectedOption,
        bool sideMenuActive,
        bool roundFinished,
        bool bettingMode)
    {
        var buttons = new IRenderable[options.Length];
        for (int i = 0; i < options.Length; i++)
        {
            string label = Markup.Escape(options[i]);
            bool selected = !sideMenuActive && !roundFinished &&
                            i == selectedOption && !(bettingMode && i == 0);

            if (i == 0 && !roundFinished && bettingMode)
            {
                buttons[i] = new Markup($"[grey]   {label}[/]");
                continue;
            }

            string highlight = i == 0 && !roundFinished
                ? "bold white on green3"
                : bettingMode && i == 2
                    ? "bold black on gold1"
                    : "bold white on #961923";
            buttons[i] = new Markup(selected
                ? $"[{highlight}] ❯ {label} [/]"
                : $"[white]   {label}[/]");
        }

        return new CompactActionFooter(buttons);
    }

    // Draw one action row and one lower gold border, while keeping the
    // left and right borders. Distribute the actions evenly across the width.
    // Unlike Panel this renderable does not add a third (top-border) row.
    private sealed class CompactActionFooter : Renderable
    {
        private readonly IRenderable[] buttons;

        public CompactActionFooter(IRenderable[] buttons)
        {
            this.buttons = buttons;
        }

        protected override Measurement Measure(RenderOptions options, int maxWidth) =>
            new Measurement(maxWidth, maxWidth);

        protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            int width = Math.Max(2, maxWidth);
            int available = width - 2;
            var gold = new Style(foreground: Color.Gold1);

            yield return new Segment("│", gold);

            for (int i = 0; i < buttons.Length; i++)
            {
                // Allocate a third of the space to each button (or an equal
                // share if the number of actions changes in the future).
                int from = available * i / buttons.Length;
                int to = available * (i + 1) / buttons.Length;
                int cellWidth = to - from;

                var segments = new List<Segment>();
                int contentWidth = 0;
                foreach (Segment segment in buttons[i].Render(options, cellWidth))
                {
                    if (segment.IsLineBreak)
                        break;
                    segments.Add(segment);
                    contentWidth += segment.CellCount();
                }

                int leftPadding = Math.Max(0, (cellWidth - contentWidth) / 2);
                int rightPadding = Math.Max(0, cellWidth - contentWidth - leftPadding);
                if (leftPadding > 0)
                    yield return new Segment(new string(' ', leftPadding));
                foreach (Segment segment in segments)
                    yield return segment;
                if (rightPadding > 0)
                    yield return new Segment(new string(' ', rightPadding));
            }

            yield return new Segment("│", gold);
            yield return Segment.LineBreak;
            yield return new Segment("╰" + new string('─', available) + "╯", gold);
        }
    }

    private ConsoleKey ReadTableKey(int timeoutMs = -1)
    {
        DateTime until = timeoutMs < 0
            ? DateTime.MaxValue
            : DateTime.UtcNow.AddMilliseconds(timeoutMs);

        int pendingWidth = -1;
        int pendingHeight = -1;
        DateTime lastSizeChange = DateTime.MinValue;
        const int ResizeSettleMs = 120;

        while (true)
        {
            int width = Math.Max(1, Console.WindowWidth);
            int height = Math.Max(1, Console.WindowHeight);

            if (rebuildLastTable is not null &&
                (width != lastWidth || height != lastHeight))
            {
                // Dragging the VS Code panel sends many rapid size changes.
                // Repaint only after its dimensions have been stable briefly.
                if (pendingWidth != width || pendingHeight != height)
                {
                    pendingWidth = width;
                    pendingHeight = height;
                    lastSizeChange = DateTime.UtcNow;
                }

                if ((DateTime.UtcNow - lastSizeChange).TotalMilliseconds >= ResizeSettleMs)
                    DrawTable(rebuildLastTable());

                // Ignore keys that arrived while controls might be hidden.
                // Never accidentally place a bet after the panel expands.
                while (Console.KeyAvailable)
                    Console.ReadKey(true);

                Thread.Sleep(35);
                continue;
            }

            pendingWidth = -1;
            pendingHeight = -1;

            if (TerminalTooSmall)
            {
                while (Console.KeyAvailable)
                    Console.ReadKey(true);
                Thread.Sleep(35);
                continue;
            }

            if (Console.KeyAvailable)
                return Console.ReadKey(true).Key;
            if (DateTime.UtcNow >= until)
                return ConsoleKey.NoName;

            Thread.Sleep(35);
        }
    }

    private static string GetTurnStatus(Player player, int selectedGameOption)
    {
        if (player.IsBust)
            return "TJOCK";
        if (player.HasBlackjack)
            return "BLACKJACK";
        if (player.Score == Participant.BlackjackValue)
            return "21 POÄNG";
        return selectedGameOption == 1 ? "STANNAT" : "KLAR";
    }

    private static string OutcomeStatus(Outcome outcome) => outcome switch
    {
        Outcome.Blackjack or Outcome.Win => "VANN",
        Outcome.Loss => "FÖRLORADE",
        Outcome.Push => "OAVGJORT",
        _ => "KLAR"
    };

    private static string NaturalBlackjackNotice(Player player) =>
        $"{player.Name.ToUpperInvariant()} – NATURAL BLACKJACK!";
}

// Read-only observer for the UI. It does not draw or change cards,
// decide moves, change bets, or determine outcomes.
internal sealed class VisibleCardCounter
{
    private readonly HashSet<Card> seenCards = new(ReferenceEqualityComparer.Instance);
    private Deck? currentDeck;


    public int RunningCount { get; private set; }
    public double TrueCount { get; private set; }

    public void Reset()
    {
        seenCards.Clear();
        currentDeck = null;
        RunningCount = 0;
        TrueCount = 0;
    }

    public void Observe(Deck deck, List<Player> players, Dealer dealer, bool cardsFaceDown)
    {
        // The parent UI resets the counter on a detected shoe refill.
        // A different Deck instance also starts a fresh count.
        if (!ReferenceEquals(currentDeck, deck))
        {
            Reset();
            currentDeck = deck;
        }

        if (!cardsFaceDown)
        {
            foreach (Player player in players)
                foreach (Card card in player.Hand.CardsOnHand)
                    CountIfFirstSeen(card);

            // Never include the dealer's hidden hole card.
            for (int i = 0; i < dealer.Hand.CardsOnHand.Count; i++)
            {
                if (i != 1 || dealer.HoleCardVisible)
                    CountIfFirstSeen(dealer.Hand.CardsOnHand[i]);
            }
        }

        // Recalculate even when no new card appears, since the remaining
        // number of cards may have changed after the last render.
        double remainingDecks = deck.DeckCards.Count / (double)Deck.CardsPerDeck;
        TrueCount = remainingDecks > 0
            ? Math.Round(RunningCount / remainingDecks, 1, MidpointRounding.AwayFromZero)
            : 0;
    }

    private void CountIfFirstSeen(Card card)
    {
        if (!seenCards.Add(card))
            return;

        if (card.Rank >= Rank.Two && card.Rank <= Rank.Six)
            RunningCount++;
        else if (card.Rank >= Rank.Ten)
            RunningCount--;
    }
}
