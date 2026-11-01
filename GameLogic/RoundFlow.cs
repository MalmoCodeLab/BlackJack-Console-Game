internal enum RoundPhase
{
    Betting,
    Dealing,
    Insurance,
    PlayerTurns,
    Completed
}

// Something the UI should celebrate (a natural Blackjack or a hit to 21).
// Statuses are a snapshot so the animation shows the state at that moment.
internal sealed record Celebration(
    int PlayerIndex,
    bool NaturalBlackjack,
    string Message,
    string[] Statuses);

// The turn-by-turn flow of one round: betting, insurance and player turns.
// It has no console, timing or drawing code, so it can be tested directly.
// The UI reads the state and sends semantic actions.
internal sealed class RoundFlow
{
    private readonly Round round;
    private readonly Queue<Celebration> celebrations = new();

    public RoundPhase Phase { get; private set; } = RoundPhase.Betting;
    public int CurrentPlayerIndex { get; private set; }
    public string[] Statuses { get; }
    public string? Notice { get; set; }

    // True while a natural Blackjack message is shown before the turn moves on.
    public bool AwaitingNaturalPause { get; private set; }

    public Round Round => round;
    public Player CurrentPlayer => round.Players[CurrentPlayerIndex];

    public RoundFlow(Round round)
    {
        ArgumentNullException.ThrowIfNull(round);
        this.round = round;
        Statuses = Enumerable.Repeat("VÄNTAR", round.Players.Count).ToArray();
        Statuses[0] = "SATSAR";
        Notice = BetTurnNotice(round.Players[0]);
    }

    public static string BetTurnNotice(Player player) =>
        $"{player.Name.ToUpperInvariant()} – DIN TUR ATT SATSA: ENTER LÄGGER MARKER, MELLANSLAG BEKRÄFTAR";

    public static string PlayTurnNotice(Player player) =>
        $"{player.Name.ToUpperInvariant()} – DIN TUR: TA KORT ELLER STANNA";

    public static string InsuranceNotice(Player player) =>
        $"{player.Name.ToUpperInvariant()} – GIVAREN VISAR ESS. FÖRSÄKRING FÖR {player.InsuranceCost}? J = JA, N = NEJ";

    public static string NaturalBlackjackNotice(Player player) =>
        $"{player.Name.ToUpperInvariant()} – NATURAL BLACKJACK!";

    public static string TwentyOneNotice(Player player) =>
        $"{player.Name.ToUpperInvariant()} – 21 POÄNG!";

    public static string GetTurnStatus(Player player, bool stood)
    {
        if (player.IsBust)
            return "TJOCK";
        if (player.HasBlackjack)
            return "BLACKJACK";
        if (player.Score == Hand.BlackjackValue)
            return "21 POÄNG";
        return stood ? "STANNAT" : "KLAR";
    }

    // Celebrations created since the last call, oldest first.
    public IReadOnlyList<Celebration> TakeCelebrations()
    {
        List<Celebration> result = [.. celebrations];
        celebrations.Clear();
        return result;
    }

    // Confirms the current player's bet. Returns false if no bet was placed.
    public bool ConfirmBet()
    {
        RequirePhase(RoundPhase.Betting);

        if (CurrentPlayer.CurrentBet <= 0)
        {
            Notice = "DU MÅSTE SATSA MINST EN MARKER FÖRST!";
            return false;
        }

        Statuses[CurrentPlayerIndex] = "SATSAT";
        CurrentPlayerIndex++;
        if (CurrentPlayerIndex == round.Players.Count)
        {
            // The UI deals the cards and then calls BeginPlay.
            Phase = RoundPhase.Dealing;
            CurrentPlayerIndex = 0;
            Array.Fill(Statuses, "VÄNTAR");
            return true;
        }

        Statuses[CurrentPlayerIndex] = "SATSAR";
        Notice = BetTurnNotice(CurrentPlayer);
        return true;
    }

    // Called when the initial cards are visible. Offers insurance or starts the turns.
    public void BeginPlay()
    {
        RequirePhase(RoundPhase.Dealing);
        round.RevealInitialCards();

        if (round.Dealer.ShowsAce)
        {
            Phase = RoundPhase.Insurance;
            Statuses[0] = "FÖRSÄKRING?";
            Notice = InsuranceNotice(round.Players[0]);
            return;
        }

        StartPlaying();
    }

    public void AnswerInsurance(bool take)
    {
        RequirePhase(RoundPhase.Insurance);

        Player player = CurrentPlayer;
        Statuses[CurrentPlayerIndex] = take
            ? (player.TryTakeInsurance() ? "FÖRSÄKRAD" : "FÖR LITE MARKER")
            : "EJ FÖRSÄKRAD";

        CurrentPlayerIndex++;
        if (CurrentPlayerIndex < round.Players.Count)
        {
            Statuses[CurrentPlayerIndex] = "FÖRSÄKRING?";
            Notice = InsuranceNotice(CurrentPlayer);
            return;
        }

        StartPlaying();
    }

    // Takes a card for the current player. Returns false if the action was refused.
    public bool Hit()
    {
        if (!CanAct())
            return false;

        Player player = CurrentPlayer;
        player.ReceiveCard(round.Deck);

        if (!player.CanHit)
        {
            Statuses[CurrentPlayerIndex] = GetTurnStatus(player, stood: false);
            if (player.Score == Hand.BlackjackValue && !player.HasBlackjack)
                AddCelebration(CurrentPlayerIndex, naturalBlackjack: false, TwentyOneNotice(player));
            AdvancePlayer();
        }

        return true;
    }

    public bool Stand()
    {
        if (!CanAct())
            return false;

        Statuses[CurrentPlayerIndex] = GetTurnStatus(CurrentPlayer, stood: true);
        AdvancePlayer();
        return true;
    }

    // Moves on after the natural Blackjack message has been shown.
    public void FinishNaturalPause()
    {
        if (!AwaitingNaturalPause)
            throw new InvalidOperationException("No natural Blackjack is being shown.");

        Notice = null;
        AwaitingNaturalPause = false;
        AdvancePlayer();
    }

    private bool CanAct()
    {
        RequirePhase(RoundPhase.PlayerTurns);
        if (AwaitingNaturalPause)
            throw new InvalidOperationException("Wait until the natural Blackjack message is done.");

        if (CurrentPlayer.CurrentBet <= 0)
        {
            Notice = "DU MÅSTE SATSA INNAN DU KAN SPELA!";
            return false;
        }

        return true;
    }

    private void StartPlaying()
    {
        if (round.Dealer.PeeksAndHasBlackjack)
        {
            // The dealer's natural ends the round; every player skips their turn.
            Array.Fill(Statuses, "KLAR");
            Phase = RoundPhase.Completed;
            return;
        }

        Phase = RoundPhase.PlayerTurns;
        CurrentPlayerIndex = 0;
        Notice = PlayTurnNotice(CurrentPlayer);

        AwaitingNaturalPause = CurrentPlayer.HasBlackjack;
        Statuses[0] = AwaitingNaturalPause ? "BLACKJACK" : "DIN TUR";
        if (AwaitingNaturalPause)
        {
            Notice = NaturalBlackjackNotice(CurrentPlayer);
            AddCelebration(0, naturalBlackjack: true, Notice);
        }
    }

    private void AdvancePlayer()
    {
        CurrentPlayerIndex++;

        if (CurrentPlayerIndex < round.Players.Count)
        {
            Player next = CurrentPlayer;
            if (next.HasBlackjack)
            {
                Statuses[CurrentPlayerIndex] = "BLACKJACK";
                Notice = NaturalBlackjackNotice(next);
                AwaitingNaturalPause = true;
                AddCelebration(CurrentPlayerIndex, naturalBlackjack: true, Notice);
                return;
            }

            Statuses[CurrentPlayerIndex] = "DIN TUR";
            Notice = PlayTurnNotice(next);
            return;
        }

        Phase = RoundPhase.Completed;
    }

    private void AddCelebration(int playerIndex, bool naturalBlackjack, string message) =>
        celebrations.Enqueue(new Celebration(
            playerIndex, naturalBlackjack, message, (string[])Statuses.Clone()));

    private void RequirePhase(RoundPhase expected)
    {
        if (Phase != expected)
            throw new InvalidOperationException(
                $"This action needs phase {expected}, but the round is in {Phase}.");
    }
}
