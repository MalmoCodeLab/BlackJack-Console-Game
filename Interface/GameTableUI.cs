using Spectre.Console;

internal enum GameTableAction
{
    RoundCompleted,
    StartNewRound,
    ReturnToMainMenu
}

internal sealed class GameTableUI
{
    // Fit the table and the action buttons within the available terminal height.
    private static int TableHeight => Math.Max(4, Console.WindowHeight);
    private const int BlackjackNoticeDurationMs = 2400;

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
   
    public Action? OpenSettings { get; set; }

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
        bool playing = true;
        GameTableAction action = GameTableAction.ReturnToMainMenu;
        string? notice = $"{players[0].Name.ToUpperInvariant()}: SATSA FÖRST – ENTER LÄGGER MARKER, MELLANSLAG BEKRÄFTAR";
        DateTime noticeUntil = DateTime.MinValue;
        // Guard against keyboard auto-repeat confirming consecutive bets.
        DateTime lastBetConfirmation = DateTime.MinValue;
        statuses[0] = "SATSAR";

        void FinishBetting()
        {
            betsComplete = true;
            bettingMode = false;
            currentPlayerIndex = 0;
            selectedGameOption = 0;
            notice = null;

            // Cards were dealt already; revealing them is handled by BuildTable.
            for (int i = 0; i < players.Count; i++)
                statuses[i] = "VÄNTAR";

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
                    noticeUntil = DateTime.UtcNow.AddMilliseconds(
                        BlackjackNoticeDurationMs);
                    pendingBlackjackAdvance = true;
                    return;
                }

                statuses[currentPlayerIndex] = "DIN TUR";
                return;
            }

            action = GameTableAction.RoundCompleted;
            playing = false;
        }

        OpenTableScreen();
        try
        {
            // An increase in the remaining-card count indicates that cards
            // were returned to the shoe between rounds. This is not a complete
            // shuffle event; shuffling without returning cards is undetectable.
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
                {
                        DrawTable(BuildTable(
                            round, statuses, notice, selectedGameOption,
                            selectedMenuOption, sideMenuActive,
                            roundFinished: false, bettingMode: bettingMode, cardsFaceDown: !betsComplete,
                            activePlayerIndex: currentPlayerIndex));
                        while (playing && !saveRequested)
                        {

                            int timeout = pendingBlackjackAdvance
                                ? Math.Max(1, (int)(noticeUntil - DateTime.UtcNow)
                                    .TotalMilliseconds)
                                : -1;
                            ConsoleKey key = ReadTableKey(timeout);
                            if (pendingBlackjackAdvance)
                            {
                                if (DateTime.UtcNow < noticeUntil)
                                    continue;

                                notice = null;
                                pendingBlackjackAdvance = false;
                                AdvancePlayer();
                                if (playing)
                                    DrawTable(BuildTable(
                                        round, statuses, notice, selectedGameOption,
                                        selectedMenuOption, sideMenuActive,
                                        roundFinished: false, bettingMode: bettingMode, cardsFaceDown: !betsComplete,
                                        activePlayerIndex: currentPlayerIndex));
                                continue;
                            }

                            if (key == ConsoleKey.NoName)
                                continue;

                            // The game menu must be accessible during betting too.
                            // Esc opens it with Huvudmeny already selected.
                            if (key == ConsoleKey.Escape)
                            {
                                sideMenuActive = !sideMenuActive;
                                if (sideMenuActive)
                                    selectedMenuOption = 1;

                                DrawTable(BuildTable(
                                    round, statuses, notice, selectedGameOption,
                                    selectedMenuOption, sideMenuActive,
                                    roundFinished: false, bettingMode: bettingMode,
                                    cardsFaceDown: !betsComplete,
                                    activePlayerIndex: currentPlayerIndex));
                                continue;
                            }

                            // Up also opens the game menu, as before.
                            if (!sideMenuActive && key == ConsoleKey.UpArrow)
                            {
                                sideMenuActive = true;
                                selectedMenuOption = 0;
                                DrawTable(BuildTable(
                                    round, statuses, notice, selectedGameOption,
                                    selectedMenuOption, sideMenuActive,
                                    roundFinished: false, bettingMode: bettingMode,
                                    cardsFaceDown: !betsComplete,
                                    activePlayerIndex: currentPlayerIndex));
                                continue;
                            }

                            // Process menu keys BEFORE the betting block.
                            // The existing Settings option is index 3, Save and Quit is 4.
                            if (sideMenuActive)
                            {
                                if (key == ConsoleKey.UpArrow)
                                    selectedMenuOption = (selectedMenuOption - 1 +
                                        GameMessages.InGameMenuOptions.Length) %
                                        GameMessages.InGameMenuOptions.Length;
                                else if (key == ConsoleKey.DownArrow)
                                    selectedMenuOption = (selectedMenuOption + 1) %
                                        GameMessages.InGameMenuOptions.Length;
                                else if (key == ConsoleKey.Enter)
                                {
                                    switch (selectedMenuOption)
                                    {
                                        case 0:
                                            action = GameTableAction.StartNewRound;
                                            playing = false;
                                            break;
                                        case 1:
                                            action = GameTableAction.ReturnToMainMenu;
                                            playing = false;
                                            break;
                                        case 2:
                                            ShowControlsScreen();
                                            break;
                                        case 3:
                                            OpenSettings?.Invoke();
                                            Console.Write("\u001b[?25l");
                                            break;
                                        case 4:
                                            saveRequested = true;
                                            break;
                                    }
                                }

                                if (playing && !saveRequested)
                                    DrawTable(BuildTable(
                                        round, statuses, notice, selectedGameOption,
                                        selectedMenuOption, sideMenuActive,
                                        roundFinished: false, bettingMode: bettingMode,
                                        cardsFaceDown: !betsComplete,
                                        activePlayerIndex: currentPlayerIndex));
                                continue;
                            }

                            if (!betsComplete)
                            {
                                Player bettingPlayer = players[currentPlayerIndex];
                                if (key == ConsoleKey.I)
                                {
                                    OpenSettings?.Invoke();
                                    Console.Write("\u001b[?25l");
                                }
                                else if (key == ConsoleKey.LeftArrow ||
                                         key == ConsoleKey.RightArrow ||
                                         key == ConsoleKey.Enter ||
                                         key == ConsoleKey.Backspace)
                                {
                                    BettingUI.HandleKey(key, bettingPlayer);
                                }
                                else if (key == ConsoleKey.Spacebar)
                                {
                                    // Key repeat can send several Spacebar events. Ignore
                                    // extra events immediately after confirming a bet.
                                    bool isRepeat = (DateTime.UtcNow - lastBetConfirmation)
                                        .TotalMilliseconds < 400;
                                    if (!isRepeat && bettingPlayer.CurrentBet > 0)
                                    {
                                        lastBetConfirmation = DateTime.UtcNow;
                                        statuses[currentPlayerIndex] = "SATSAT";
                                        currentPlayerIndex++;
                                        if (currentPlayerIndex == players.Count)
                                            FinishBetting();
                                        else
                                        {
                                            statuses[currentPlayerIndex] = "SATSAR";
                                            notice = $"{players[currentPlayerIndex].Name.ToUpperInvariant()}: SATSA FÖRST – ENTER LÄGGER MARKER, MELLANSLAG BEKRÄFTAR";
                                        }
                                    }
                                    else if (!isRepeat)
                                    {
                                        notice = "DU MÅSTE SATSA MINST EN MARKER FÖRST!";
                                    }
                                }

                                DrawTable(BuildTable(
                                    round, statuses, notice, selectedGameOption,
                                    selectedMenuOption, sideMenuActive,
                                    roundFinished: false, bettingMode: bettingMode, cardsFaceDown: !betsComplete,
                                    activePlayerIndex: currentPlayerIndex));
                                continue;
                            }

                            if (bettingMode)
                            {
                                if (key == ConsoleKey.Spacebar ||
                                    key == ConsoleKey.UpArrow ||
                                    key == ConsoleKey.DownArrow)
                                    bettingMode = false;
                                else if (key == ConsoleKey.LeftArrow ||
                                         key == ConsoleKey.RightArrow ||
                                         key == ConsoleKey.Enter ||
                                         key == ConsoleKey.Backspace)
                                    BettingUI.HandleKey(key, players[currentPlayerIndex]);

                                if (bettingMode)
                                {
                                    DrawTable(BuildTable(
                                        round, statuses, notice, selectedGameOption,
                                        selectedMenuOption, sideMenuActive,
                                        roundFinished: false, bettingMode: bettingMode, cardsFaceDown: !betsComplete,
                                        activePlayerIndex: currentPlayerIndex));
                                    continue;
                                }
                            }

                            // Main table actions. The side menu has already
                            // been handled above, so it cannot steal betting keys.
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
                                    }
                                    else
                                    {
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
                                    }
                                    break;
                            }

                            if (playing && !saveRequested)
                                DrawTable(BuildTable(
                                    round, statuses, notice, selectedGameOption,
                                    selectedMenuOption, sideMenuActive,
                                    roundFinished: false, bettingMode: bettingMode, cardsFaceDown: !betsComplete,
                                    activePlayerIndex: currentPlayerIndex));
                        }
                    }

                if (saveRequested && saveGame(players))
                {
                    action = GameTableAction.ReturnToMainMenu;
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
            // Keep the same terminal screen for the result and the next round.
            if (action == GameTableAction.ReturnToMainMenu)
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
                                    OpenSettings?.Invoke();
                                    Console.Write("\u001b[?25l");
                                }
                                else if (selectedMenuOption == 4)
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
                    action = GameTableAction.ReturnToMainMenu;
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

    // Card rows have a fixed width. Below this size, showing the full table
    // would cut off cards or the action buttons.
    private const int MinimumTableWidth = 80;
    private const int MinimumTerminalHeight = 26;
    private const int ActionPanelHeight = 3;

    private static bool TerminalTooSmall =>
        Console.WindowWidth < MinimumTableWidth ||
        Console.WindowHeight < MinimumTerminalHeight;

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
        int activePlayerIndex = 0)
    {
        // The same game state is rendered again when the VS Code terminal
        // changes size. This does not reset bets, cards, or menu selection.
        rebuildLastTable = () => BuildTable(
            round, statuses, notice, selectedGameOption, selectedMenuOption,
            sideMenuActive, roundFinished, bettingMode, cardsFaceDown,
            activePlayerIndex);

        int width = Math.Max(1, Console.WindowWidth);
        int height = Math.Max(1, Console.WindowHeight);
        builtWidth = width;
        builtHeight = height;
        var layout = new Layout("Root").Size(height);

        // Keep the size guard without displaying an error message.
        // A blank frame clears old content and protects the fixed-size cards.
        // ReadTableKey pauses input until the terminal is large enough again.
        if (TerminalTooSmall)
        {
            layout.Update(new Text(string.Empty));
            return layout;
        }

        // Preserve the full-size card artwork on narrower terminals.
        // When there is not room for both columns, Esc/Up shows the same
        // in-game menu as a full-width view instead of squeezing the cards.
        int menuWidth = Math.Clamp(width / 5, 33, 38);
        bool showSideMenu = width - menuWidth >= MinimumTableWidth;

        if (showSideMenu)
        {
            layout.SplitColumns(
                new Layout("Table"),
                new Layout("Menu").Size(menuWidth));
        }
        else if (sideMenuActive || roundFinished)
        {
            layout.Update(ui.BuildGameMenu(
                GameMessages.InGameMenuOptions, selectedMenuOption,
                round.Players, sideMenuActive, roundFinished, statuses,
                activePlayerIndex));
            return layout;
        }

        Layout tableArea = showSideMenu ? layout["Table"] : layout;
        int tableHeight = height - ActionPanelHeight;

        // Keep these as TWO named layouts. Replacing them with one Rows()
        // renderable caused the action buttons to disappear previously.
        tableArea.SplitRows(
            new Layout("BlackjackTable").Size(tableHeight),
            new Layout("PlayerActions").Size(ActionPanelHeight));

        if (ReferenceEquals(observedDeck, round.Deck))
            lowestCardsRemaining = Math.Min(
                lowestCardsRemaining, round.Deck.DeckCards.Count);

        visibleCardCounter.Observe(
            round.Deck, round.Players, round.Dealer, cardsFaceDown);

        tableArea["BlackjackTable"].Update(
            ui.BuildBlackjackTable(
                round.Players, round.Dealer, tableHeight, notice, statuses,
                cardsFaceDown, visibleCardCounter.RunningCount,
                visibleCardCounter.TrueCount, activePlayerIndex));

        tableArea["PlayerActions"].Update(
            ui.BuildPlayerActions(
                GameMessages.GameOptions, selectedGameOption,
                sideMenuActive, roundFinished, bettingMode));

        if (showSideMenu)
        {
            layout["Menu"].SplitRows(
                new Layout("MenuContent").Size(height - 3),
                new Layout("MenuSpace").Ratio(1));
            layout["MenuContent"].Update(ui.BuildGameMenu(
                GameMessages.InGameMenuOptions, selectedMenuOption,
                round.Players, sideMenuActive, roundFinished, statuses,
                activePlayerIndex));
            layout["MenuSpace"].Update(new Text(""));
        }

        return layout;
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
        Outcome.Lose => "FÖRLORADE",
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
        double remainingDecks = deck.DeckCards.Count / 52.0;
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
