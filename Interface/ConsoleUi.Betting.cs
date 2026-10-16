using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

public partial class ConsoleUi
{
    // Opens the interactive betting screen.
    internal void ShowBettingMode(
        List<Player> players,
        Dealer dealer,
        int tableHeight = 0)
    {
        IRenderable BuildScreen()
        {
            var table = BuildBlackjackTable(
                players,
                dealer,
                tableHeight);

            var information = BettingUI.CreateInfoPanel();
            return new Columns(table, information);
        }

        AnsiConsole.Clear();

        AnsiConsole.Live(BuildScreen())
            .AutoClear(true)
            .Start(context =>
            {
                bool isBetting = true;

                while (isBetting)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;

                    // Space confirms a non-zero bet and returns to the table.
                    // Enter still adds chips, so multiple chips can be selected.
                    if (key == ConsoleKey.Spacebar)
                    {
                        if (players.Count > 0 && players[0].CurrentBet > 0)
                            isBetting = false;

                        continue;
                    }

                    // Handles chip selection and betting.
                    bool updated = BettingUI.HandleKey(key, players[0]);

                    if (updated)
                    {
                        context.UpdateTarget(BuildScreen());
                        context.Refresh();
                    }
                }
            });
    }
}
