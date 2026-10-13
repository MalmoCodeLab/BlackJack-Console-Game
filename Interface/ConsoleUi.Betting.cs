
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
        // Builds the table and betting information.
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

        // Updates the screen when a chip is selected.
        AnsiConsole.Live(BuildScreen())
            .AutoClear(true)
            .Start(context =>
            {
                bool isBetting = true;

                while (isBetting)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;

                    // Escape returns to the normal game menu.
                    if (key == ConsoleKey.Escape)
                    {
                        isBetting = false;
                        continue;
                    }

                    // Handles chip selection and betting.
                    bool updated = BettingUI.HandleKey(key);

                    if (updated)
                    {
                        // Redraws the table and balance.
                        context.UpdateTarget(BuildScreen());
                        context.Refresh();
                    }
                }
            });
    }
}
