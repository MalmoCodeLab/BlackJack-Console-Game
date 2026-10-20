public class Game
{
    private readonly ConsoleUi ui = new();
    private readonly PlayerSessionService playerSessions;
    private readonly GameTableUI gameTable;
    private RuleSet rules;
    private readonly string? ruleConfigurationNotice;

    public Game()
    {
        string? notice = null;
        rules = RuleSet.LoadFromDefaultLocation(message => notice = message);
        ruleConfigurationNotice = notice;
        playerSessions = new PlayerSessionService(new GameStorage(), ui);
        gameTable = new GameTableUI(ui);
    }

    public void Run()
    {
        bool running = true;
        bool showRuleNotice = ruleConfigurationNotice is not null;
        while (running)
        {
            string choice = ui.ShowMenu(
                showRuleNotice ? ruleConfigurationNotice : null);
            showRuleNotice = false;
            switch (choice)
            {
                case GameMessages.NewGame:
                    StartNewGame();
                    break;
                case GameMessages.LoadGame:
                    LoadGame();
                    break;
                case GameMessages.ShowRules:
                    ui.ShowRules(rules);
                    break;
                case GameMessages.Settings:
                    rules = ui.EditRules(rules);
                    break;
                case GameMessages.ShowCredits:
                    ui.ShowCredits();
                    break;
                case GameMessages.Exit:
                    running = false;
                    break;
            }
        }
    }

    private void StartNewGame()
    {
        int playerCount = ui.AskPlayerCountCentered();

        // Esc returns 0 and goes back to the main menu.
        if (playerCount == 0)
            return;

        List<Player>? players = playerSessions.CreateNewPlayers(playerCount);
        if (players is not null)
            PlayGame(players);
    }

    private void LoadGame()
    {
        List<Player>? players = playerSessions.LoadPlayers();
        if (players is not null)
            PlayGame(players);
    }

    private void PlayGame(List<Player> players)
    {
        gameTable.ResetSession();

        RuleSet gameRules = rules;

        Deck deck = new(gameRules);
        deck.CreateDeck();
        deck.ShuffleDeck();

        Dealer dealer = new(gameRules);
        int roundNumber = 1;

        while (true)
        {
            Round round = new(roundNumber++, players, dealer, deck, gameRules);
            // Display an empty table before dealing. No cards are added yet.
            ui.ShowEmptyTableBeforeDeal(players, dealer);
            // Deal cards after Enter, keeping the existing round rules unchanged.
            round.StartRound();
            GameTableAction action = gameTable.PlayRound(
                round, playerSessions.SaveGame);
            if (action != GameTableAction.RoundCompleted)
            {
                // Return unplayed bets when leaving an unfinished round.
                foreach (Player player in players)
                    player.CancelBet();
            }
            if (action == GameTableAction.RoundCompleted)
            {
                round.EndRound();
                string summary = RoundResultFormatter.Format(round);
                summary = playerSessions.SaveResults(players)
                    ? GameMessages.AppendSaveResultsSuccess(
                        summary, players.Select(player => player.Name))
                    : GameMessages.AppendSaveResultsError(summary);

                action = gameTable.ShowRoundEndMenu(
                    round, summary, playerSessions.SaveGame);
            }

            if (action != GameTableAction.StartNewRound)
                return;

            // A player who runs out of chips leaves the table.
            players.RemoveAll(player => player.IsOutOfChips);
            if (players.Count == 0)
            {
                ui.ShowCenteredMessage(GameMessages.AllPlayersOutOfChips);
                return;
            }
        }
    }
}
