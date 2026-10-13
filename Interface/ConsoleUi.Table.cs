
using System;
using System.Collections.Generic;
using Spectre.Console;

public partial class ConsoleUi
{
    private readonly CardRenderer cardRenderer = new();

    private const int TableWidth = 40;
    private const int CardWidth = 11;
    private const int CardSpacing = 2;

    private const string Felt = "#004B28";
    private const string Gold = "gold1";

    // Displays the Blackjack table.
    internal void ShowTable(Hand playerHand, Hand dealerHand)
    {
        AnsiConsole.MarkupLine(
            $"[{Gold}]╭{new string('─', TableWidth)}╮[/]"
        );

        WriteTableLine("♠ BLACKJACK 21 ♠");
        WriteTableLine("");
        WriteTableLine("DEALER");
        WriteTableLine("");

        WriteCards(dealerHand, true);

        WriteTableLine("");
        WriteTableLine("────────────────────────────");
        WriteTableLine("");
        WriteTableLine("SPELARE 1");
        WriteTableLine("");

        WriteCards(playerHand, false);

        WriteTableLine("");
        WriteTableLine("Tur kan inte kompileras.");
        WriteTableLine("");

        AnsiConsole.MarkupLine(
            $"[{Gold}]╰{new string('─', TableWidth)}╯[/]"
        );
    }

    // Writes a centered line on the table.
    private void WriteTableLine(string text)
    {
        if (text.Length > TableWidth)
        {
            text = text.Substring(0, TableWidth);
        }

        int left = (TableWidth - text.Length) / 2;

        string content =
            new string(' ', left) +
            text.PadRight(TableWidth - left);

        AnsiConsole.MarkupLine(
            $"[{Gold}]│[/]" +
            $"[white on {Felt}]" +
            Markup.Escape(content) +
            "[/]" +
            $"[{Gold}]│[/]"
        );
    }

    // Places the playing cards on the table.
    private void WriteCards(Hand hand, bool hideSecondCard)
    {
        List<string[]> cards = new();

        for (int i = 0; i < hand.CardsOnHand.Count; i++)
        {
            bool hidden = hideSecondCard && i == 1;

            cards.Add(
                hidden
                    ? cardRenderer.RenderHiddenCard()
                    : cardRenderer.RenderCard(hand.CardsOnHand[i])
            );
        }

        if (cards.Count == 0)
        {
            WriteTableLine("Inga kort ännu.");
            return;
        }

        int totalWidth =
            cards.Count * CardWidth +
            (cards.Count - 1) * CardSpacing;

        int left = Math.Max(
            0,
            (TableWidth - totalWidth) / 2
        );

        int right = Math.Max(
            0,
            TableWidth - totalWidth - left
        );

        for (int row = 0; row < cards[0].Length; row++)
        {
            string line = "";

            for (int i = 0; i < cards.Count; i++)
            {
                line += cards[i][row];

                if (i < cards.Count - 1)
                {
                    line +=
                        $"[on {Felt}]" +
                        new string(' ', CardSpacing) +
                        "[/]";
                }
            }

            AnsiConsole.MarkupLine(
                $"[{Gold}]│[/]" +
                $"[on {Felt}]{new string(' ', left)}[/]" +
                line +
                $"[on {Felt}]{new string(' ', right)}[/]" +
                $"[{Gold}]│[/]"
            );
        }
    }

    // Displays a player's cards.
    internal void ShowHand(Hand hand)
    {
        WriteCards(hand, false);
    }

    // Displays the dealer's cards.
    internal void ShowDealerCards(Dealer dealer)
    {
        WriteCards(
            dealer.Hand,
            !dealer.HoleCardVisible
        );
    }
}
