using System;

internal static class GameMessages
{
    internal const string GameTitleMarkup = "[bold gold1]♠ BLACKJACK 21 ♠[/]";
    internal const string GameSubtitleMarkup = "[italic green3]Tur kan inte kompileras.[/]";
    internal const string MainMenuTitleMarkup = "[gold1]♠ HUVUDMENY ♠[/]";
    internal const string PlayerCountTitleMarkup = "[gold1]♠ VÄLJ ANTAL SPELARE ♠[/]";
    internal const string MenuNavigationMarkup = "[grey]↑ ↓ Navigera  •  Enter Välj[/]";
    internal const string PlayerCountFormat = "{0} spelare";

    internal const string NewGame = "♠ Nytt spel";
    internal const string LoadGame = "♦ Ladda spel";
    internal const string ShowRules = "♥ Spelregler";
    internal const string ShowCredits = "♣ Medverkande";
    internal const string Exit = "♠ Avsluta";

    internal const string LoadPlayerStatisticsError = "Kunde inte ladda spelarstatistiken.";
    internal const string LoadSavedResultsError = "Kunde inte ladda sparade resultat.";
    internal const string NoSavedGames = "Det finns inga sparade spel ännu.";
    internal const string SaveResultsError = "Kunde inte spara resultatet.";
    internal const string SaveStatisticsSuccess = "Spelarstatistiken har sparats!";
    internal const string SaveStatisticsError = "Kunde inte spara spelarstatistiken.";
    internal const string PlayerNamePromptFormat = "Ange namn för spelare {0}: ";
    internal const string DefaultPlayerName = "Spelare 1";
    internal const string PlayerNameFormat = "Spelare {0}";
    internal const string DuplicatePlayerName = "Namnet används redan. Välj ett annat namn.";
    internal const string DeleteSavedPlayerError = "Kunde inte ta bort den sparade spelaren.";
    internal const string DeleteSavedPlayerPromptFormat =
        "Ta bort {0} permanent?";

    internal static readonly string[] MainMenuOptions =
    {
        NewGame,
        LoadGame,
        ShowRules,
        ShowCredits,
        Exit
    };

    internal static readonly string[] GameOptions =
    {
        "♥ Ta kort",
        "♠ Stanna",
        "♦ Satsa"
    };

    // Menu displayed at the Blackjack table.
    internal const string GameControls = "♥ Spelstyrning";

    internal static readonly string[] InGameMenuOptions =
    {
        "♠ Ny runda",
        "♣ Huvudmeny",
        GameControls,
        "♦ Spara och avsluta"
    };

    internal static readonly string[] RoundEndMenuOptions = InGameMenuOptions;

    internal const string PlayerBustOutcome = "Förlust (över 21)";
    internal const string DealerBustOutcome = "Vinst (givaren sprack)";
    internal const string PlayerBlackjackOutcome = "Vinst (naturlig 21)";
    internal const string DealerBlackjackOutcome = "Förlust (givarens Blackjack)";
    internal const string WinOutcome = "Vinst";
    internal const string LossOutcome = "Förlust";
    internal const string PushOutcome = "Oavgjort";
    internal const string PlayerOutOfChipsFormat = "{0} har slut på marker och lämnar bordet.";
    internal const string AllPlayersOutOfChips = "Alla spelare har slut på marker. Spelet är slut.";
    internal const string SavedPlayerOutOfChipsFormat = "{0} har inga marker kvar och får inte spela.";

    internal const string SoloPlayerWin = "DU VANN!";
    internal const string SoloPlayerLoss = "DU FÖRLORADE!";
    internal const string WinHeadline = "VANN!";
    internal const string LossHeadline = "FÖRLORADE!";
    internal const string PushHeadline = "OAVGJORT!";
    internal const string ScoreComparisonFormat = "{0} ({1} mot {2})";
    internal const string NamedScoreComparisonFormat = "{0}: {1} ({2} mot {3})";
    internal const string NewRoundPrompt = "Vill du spela en ny runda?";
    internal const string ResultTitleMarkup = "[gold1]RESULTAT[/]";
    internal const string RoundEndNavigationMarkup = "[grey]↑ ↓ Navigera  •  Enter Välj[/]";

    internal const string GameMenuTitleMarkup = "[bold gold1]♠ SPELMENY ♠[/]";
    internal const string PointsTitleMarkup = "[gold1]POÄNG[/]";
    internal const string ChipsTitleMarkup = "[gold1]MARKER[/]";
    internal const string BalanceMarkup = "[white]Saldo:[/]";
    internal const string CurrentBetMarkup = "[white]Aktuell insats:[/]";
    internal const string BalanceFormat = "[green1]{0} kr[/]";
    internal const string CurrentBetFormat = "[gold1]{0} kr[/]";
    internal const string ScoreMarkupFormat = "[white]{0}:[/]";
    internal const string PlayerScoreMarkupFormat = "[bold gold1]{0}[/]";

    internal const string SavedPlayersTitleMarkup = "[gold1]♦ LADDA SPEL ♦[/]";
    internal const string SavedPlayerStatisticsFormat = "{0}  |  V: {1}  F: {2}  O: {3}";
    internal const string SavedPlayersNavigationMarkup =
        "[grey]↑ ↓ Välj spelare  •  Enter Spela  •  Esc Tillbaka[/]";
    internal const string OutcomeWinPrefix = "Vinst";
    internal const string OutcomeLossPrefix = "Förlust";

    internal static string PlayerName(int number) =>
        string.Format(PlayerNameFormat, number);

    internal static string PlayerOutOfChips(string playerName) =>
        string.Format(PlayerOutOfChipsFormat, playerName);

    internal static string SavedPlayerOutOfChips(string playerName) =>
        string.Format(SavedPlayerOutOfChipsFormat, playerName);

    internal static string SoloResult(string headline, int playerScore, int dealerScore) =>
        string.Format(ScoreComparisonFormat, headline, playerScore, dealerScore);

    internal static string NamedResult(
        string playerName, string headline, int playerScore, int dealerScore) =>
        string.Format(
            NamedScoreComparisonFormat, playerName, headline, playerScore, dealerScore);

    internal static string SavedPlayerStatistics(
        string name, int wins, int losses, int pushes) =>
        string.Format(SavedPlayerStatisticsFormat, name, wins, losses, pushes);

    internal static string Balance(int balance) =>
        string.Format(BalanceFormat, balance);

    internal static string CurrentBet(int currentBet) =>
        string.Format(CurrentBetFormat, currentBet);

    internal static string ScoreMarkup(string playerName) =>
        string.Format(ScoreMarkupFormat, playerName);

    internal static string PlayerScoreMarkup(int score) =>
        string.Format(PlayerScoreMarkupFormat, score);

    internal static string OutcomeHeadline(string outcome, bool soloPlayer)
    {
        if (outcome.StartsWith(OutcomeWinPrefix, StringComparison.Ordinal))
            return soloPlayer ? SoloPlayerWin : WinHeadline;

        if (outcome.StartsWith(OutcomeLossPrefix, StringComparison.Ordinal))
            return soloPlayer ? SoloPlayerLoss : LossHeadline;

        return PushHeadline;
    }

    internal static bool IsLossOutcome(string result) =>
        result.Contains(OutcomeLossPrefix, StringComparison.Ordinal) ||
        result.Contains(LossHeadline, StringComparison.Ordinal);

    internal static bool IsWinOutcome(string result) =>
        result.Contains(OutcomeWinPrefix, StringComparison.Ordinal) ||
        result.Contains(WinHeadline, StringComparison.Ordinal);

    internal static string AppendSaveResultsError(string summary) =>
        $"{summary}{Environment.NewLine}{SaveResultsError}";
}
