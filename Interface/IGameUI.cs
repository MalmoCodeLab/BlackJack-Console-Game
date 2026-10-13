using System.Collections.Generic;

internal interface IGameUI
{
    MainMenuChoice ShowMenu();
    int AskPlayerCount();
    GameMenuChoice ShowGameMenu();
    void ShowRules();
    void ShowCredits();
    void ShowTable(IReadOnlyList<Player> players, Dealer dealer);
    void ShowStatistics(IReadOnlyDictionary<string, PlayerStatistics> statistics);
    void ShowNotice(string message);
    void Clear();
}

public enum MainMenuChoice
{
    NewGame,
    LoadGame,
    Rules,
    Credits,
    Exit
}

public enum GameMenuChoice
{
    SaveAndExit,
    ReturnToMainMenu
}
