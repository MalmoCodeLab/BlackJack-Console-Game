
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

    // Displays the Blackjack table for all players.
    internal void ShowTable(List<Player> players, Dealer dealer)
    {
        AnsiConsole.MarkupLine(
            $"[{Gold}]╭{new string('─', TableWidth)}╮[/]"
        );

        WriteTableLine("♠ BLACKJACK 21 ♠");
        WriteTableLine("");

        // Displays Player 2-5 with small colored cards.
        if (players.Count > 1)
        {
            for (int i = 1; i < players.Count; i++)
            {
                WritePlayerSummary(players[i]);
            }

            WriteTableLine("");
            WriteTableLine("────────────────────────────");
            WriteTableLine("");
        }

        WriteTableLine("DEALER");
        WriteTableLine("");

        // Displays the dealer's cards.
        WriteCards(dealer.Hand, !dealer.HoleCardVisible);

        WriteTableLine("");
        WriteTableLine("────────────────────────────");
        WriteTableLine("");

        WriteTableLine(players[0].Name.ToUpper());
        WriteTableLine("");

        // Displays Player 1 with large playing cards.
        WriteCards(players[0].Hand, false);

        WriteTableLine("");

        AnsiConsole.MarkupLine(
            $"[{Gold}]╰{new string('─', TableWidth)}╯[/]"
        );
    }

    // Displays a player's cards with colored suits.
    private void WritePlayerSummary(Player player)
    {
        string cards = "";

        foreach (Card card in player.Hand.CardsOnHand)
        {
            string cardText = GetCardText(card);

            if (card.Suit == Suit.Hearts ||
                card.Suit == Suit.Diamonds)
            {
                cards += $"[red on white]{cardText}[/] ";
            }
            else
            {
                cards += $"[black on white]{cardText}[/] ";
            }
        }

        WriteColoredTableLine(
            $"{Markup.Escape(player.Name)}: {cards.TrimEnd()}"
        );
    }

    // Converts a card into a short text representation.
    private string GetCardText(Card card)
    {
        string rank = card.Rank switch
        {
            Rank.Ace => "A",
            Rank.King => "K",
            Rank.Queen => "Q",
            Rank.Jack => "J",
            Rank.Ten => "10",
            _ => ((int)card.Rank + 2).ToString()
        };

        string suit = card.Suit switch
        {
            Suit.Hearts => "♥",
            Suit.Diamonds => "♦",
            Suit.Clubs => "♣",
            Suit.Spades => "♠",
            _ => ""
        };

        return rank + suit;
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

    // Writes a colored line on the Blackjack table.
    private void WriteColoredTableLine(string text)
    {
        string plainText = Markup.Remove(text);

        if (plainText.Length > TableWidth)
        {
            throw new ArgumentException(
                "Player summary is wider than the table."
            );
        }

        int left = (TableWidth - plainText.Length) / 2;
        int right = TableWidth - plainText.Length - left;

        AnsiConsole.MarkupLine(
            $"[{Gold}]│[/]" +
            $"[on {Felt}]{new string(' ', left)}[/]" +
            $"[white on {Felt}]{text}[/]" +
            $"[on {Felt}]{new string(' ', right)}[/]" +
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

            cards.Add(hidden
                ? cardRenderer.RenderHiddenCard()
                : cardRenderer.RenderCard(hand.CardsOnHand[i]));
        }

        if (cards.Count == 0)
        {
            WriteTableLine("No cards yet.");
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
        WriteCards(dealer.Hand, !dealer.HoleCardVisible);
    }
}
