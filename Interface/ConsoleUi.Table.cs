
using System;
using System.Collections.Generic;
using System.Text;
using Spectre.Console;

public partial class ConsoleUi
{
    private readonly CardRenderer cardRenderer = new();

    private const int TableWidth = 40;
    private const int CardWidth = 11;
    private const int CardSpacing = 2;

    private const string FeltColor = "#004B28";
    private const string GoldColor = "gold1";
    private const string CardBackColor = "#172A60";

    // Displays the Blackjack table.
    internal void ShowTable(Hand playerHand, Hand dealerHand)
    {
        AnsiConsole.MarkupLine(
            $"[{GoldColor}]╭{new string('─', TableWidth)}╮[/]"
        );

        WriteGreenLine("♠ BLACKJACK 21 ♠", true);
        WriteGreenLine("");
        WriteGreenLine("DEALER", true);
        WriteGreenLine("");

        // Displays the dealer's cards.
        WriteCardsOnTable(dealerHand, true);

        WriteGreenLine("");
        WriteGreenLine("────────────────────────────", true);
        WriteGreenLine("");
        WriteGreenLine("SPELARE 1", true);
        WriteGreenLine("");

        // Displays the player's cards.
        WriteCardsOnTable(playerHand, false);

        WriteGreenLine("");
        WriteGreenLine("Tur kan inte kompileras.", true);
        WriteGreenLine("");

        AnsiConsole.MarkupLine(
            $"[{GoldColor}]╰{new string('─', TableWidth)}╯[/]"
        );
    }

    // Writes a green row with gold borders.
    private void WriteGreenLine(
        string text,
        bool centered = false)
    {
        if (text.Length > TableWidth)
        {
            text = text.Substring(0, TableWidth);
        }

        int leftPadding = centered
            ? (TableWidth - text.Length) / 2
            : 2;

        int rightPadding =
            TableWidth - leftPadding - text.Length;

        string content =
            new string(' ', leftPadding) +
            text +
            new string(' ', rightPadding);

        AnsiConsole.MarkupLine(
            $"[{GoldColor}]│[/]" +
            $"[white on {FeltColor}]" +
            Markup.Escape(content) +
            "[/]" +
            $"[{GoldColor}]│[/]"
        );
    }

    // Draws the cards centered on the table.
    private void WriteCardsOnTable(
        Hand hand,
        bool hideSecondCard)
    {
        List<string[]> renderedCards = new();

        for (int i = 0; i < hand.CardsOnHand.Count; i++)
        {
            bool hidden = hideSecondCard && i == 1;

            string[] cardRows = hidden
                ? cardRenderer.RenderHiddenCard()
                : cardRenderer.RenderCard(hand.CardsOnHand[i]);

            renderedCards.Add(cardRows);
        }

        if (renderedCards.Count == 0)
        {
            WriteGreenLine("Inga kort ännu.", true);
            return;
        }

        int totalWidth =
            renderedCards.Count * CardWidth +
            (renderedCards.Count - 1) * CardSpacing;

        int leftPadding = Math.Max(
            0,
            (TableWidth - totalWidth) / 2
        );

        int rightPadding = Math.Max(
            0,
            TableWidth - totalWidth - leftPadding
        );

        int cardHeight = renderedCards[0].Length;

        for (int row = 0; row < cardHeight; row++)
        {
            StringBuilder cardLine = new();

            for (int i = 0; i < renderedCards.Count; i++)
            {
                bool hidden = hideSecondCard && i == 1;

                Card card = hand.CardsOnHand[i];
                string rowText = renderedCards[i][row];

                string symbolColor =
                    card.Suit == Suit.Hearts ||
                    card.Suit == Suit.Diamonds
                        ? "red"
                        : "black";

                string cardBackground = hidden
                    ? CardBackColor
                    : "white";

                string foreground = hidden
                    ? GoldColor
                    : symbolColor;

                // Colors each part of the card separately.
                for (int j = 0; j < rowText.Length; j++)
                {
                    char character = rowText[j];

                    bool isTopOrBottom =
                        row == 0 || row == cardHeight - 1;

                    bool isCorner =
                        isTopOrBottom &&
                        (j == 0 || j == CardWidth - 1);

                    bool isHorizontalBorder =
                        isTopOrBottom && !isCorner;

                    bool isVerticalBorder =
                        !isTopOrBottom &&
                        (j == 0 || j == CardWidth - 1);

                    if (isCorner)
                    {
                        // Draws rounded corners on the green felt.
                        cardLine.Append(
                            $"[{cardBackground} on {FeltColor}]" +
                            Markup.Escape(character.ToString()) +
                            "[/]"
                        );
                    }
                    else if (isHorizontalBorder || isVerticalBorder)
                    {
                        // Fills the borders with the card's color.
                        cardLine.Append(
                            $"[{cardBackground} on {cardBackground}]" +
                            Markup.Escape(character.ToString()) +
                            "[/]"
                        );
                    }
                    else
                    {
                        // Displays colored symbols on the card surface.
                        cardLine.Append(
                            $"[{foreground} on {cardBackground}]" +
                            Markup.Escape(character.ToString()) +
                            "[/]"
                        );
                    }
                }

                // Adds green space between cards.
                if (i < renderedCards.Count - 1)
                {
                    cardLine.Append(
                        $"[on {FeltColor}]" +
                        new string(' ', CardSpacing) +
                        "[/]"
                    );
                }
            }

            AnsiConsole.MarkupLine(
                $"[{GoldColor}]│[/]" +
                $"[on {FeltColor}]" +
                new string(' ', leftPadding) +
                "[/]" +
                cardLine +
                $"[on {FeltColor}]" +
                new string(' ', rightPadding) +
                "[/]" +
                $"[{GoldColor}]│[/]"
            );
        }
    }

    // Displays a player's hand separately.
    internal void ShowHand(Hand hand)
    {
        WriteCardsOnTable(hand, false);
    }

    // Displays the dealer's cards separately.
    internal void ShowDealerCards(Dealer dealer)
    {
        WriteCardsOnTable(
            dealer.Hand,
            !dealer.HoleCardVisible
        );
    }
}
