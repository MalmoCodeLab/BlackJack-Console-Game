
using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

public partial class ConsoleUi
{
    // Colors used for the Blackjack table.
    private static readonly Color TableGreen = new Color(7, 80, 46);
    private static readonly Color TableGold = Color.Gold1;

    // Builds the Blackjack table.
    internal Panel BuildBlackjackTable(
        List<Player> players,
        Dealer dealer)
    {
        List<IRenderable> content = new();

        // Displays the Blackjack title.
        content.Add(new Align(
            new Markup("[bold gold1]♠ BLACKJACK 21 ♠[/]"),
            HorizontalAlignment.Center
        ));

        content.Add(new Text(""));

        // Displays players 2–5 at the top.
        if (players.Count > 1)
        {
            var playerGrid = new Grid();

            for (int i = 1; i < players.Count; i++)
            {
                playerGrid.AddColumn(
                    new GridColumn().NoWrap()
                );
            }

            List<IRenderable> names = new();
            List<IRenderable> cards = new();

            for (int i = 1; i < players.Count; i++)
            {
                Player player = players[i];

                names.Add(new Markup(
                    $"[bold gold1]SPELARE {i + 1}[/]"
                ));

                cards.Add(new Markup(
                    BuildSmallCards(player.Hand.CardsOnHand)
                ));
            }

            playerGrid.AddRow(names.ToArray());
            playerGrid.AddRow(cards.ToArray());

            content.Add(new Align(
                playerGrid,
                HorizontalAlignment.Center
            ));
        }

        // Space between players 2–5 and the dealer.
        for (int i = 0; i < 5; i++)
        {
            content.Add(new Text(""));
        }

        // Displays the dealer in the middle.
        content.Add(
            new Rule("[gold1]GIVARE[/]")
                .RuleStyle(TableGold)
        );

        AddRenderedCards(
            content,
            dealer.Hand.CardsOnHand,
            !dealer.HoleCardVisible
        );

        // Space between the dealer and player 1.
        for (int i = 0; i < 5; i++)
        {
            content.Add(new Text(""));
        }

        // Displays player 1 near the bottom.
        content.Add(
            new Rule("[gold1]SPELARE 1[/]")
                .RuleStyle(TableGold)
        );

        if (players.Count > 0)
        {
            AddRenderedCards(
                content,
                players[0].Hand.CardsOnHand,
                false
            );
        }

        // Adds green background to the table.
        var greenContent = new GreenTableBackground(
            new Rows(content),
            TableGreen
        );

        return new Panel(greenContent)
            .Header("[gold1] ♠ BLACKJACK ♠ [/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(TableGold)
            .Expand();
    }

    // Displays the table without the side menu.
    internal void ShowBlackjackTable(
        List<Player> players,
        Dealer dealer)
    {
        AnsiConsole.Write(
            BuildBlackjackTable(players, dealer)
        );
    }

    // Builds compact cards for players 2–5.
    private string BuildSmallCards(List<Card> cards)
    {
        if (cards.Count == 0)
        {
            return "[grey]Inga kort[/]";
        }

        List<string> cardTexts = new();

        foreach (Card card in cards)
        {
            string color = GetCardColor(card);
            string label = GetCardLabel(card);

            cardTexts.Add(
                $"[{color} on white] {label} [/]"
            );
        }

        return string.Join(" ", cardTexts);
    }

    // Uses the existing CardRenderer for full-size cards.
    private void AddRenderedCards(
        List<IRenderable> content,
        List<Card> cards,
        bool hideSecondCard)
    {
        if (cards.Count == 0)
        {
            content.Add(new Markup("[grey]Inga kort[/]"));
            return;
        }

        CardRenderer renderer = new CardRenderer();

        // Each card is rendered as seven separate lines.
        List<string[]> renderedCards = new();

        for (int i = 0; i < cards.Count; i++)
        {
            if (hideSecondCard && i == 1)
            {
                renderedCards.Add(
                    renderer.RenderHiddenCard()
                );
            }
            else
            {
                renderedCards.Add(
                    renderer.RenderCard(cards[i])
                );
            }
        }

        // Combines the cards horizontally, row by row.
        List<IRenderable> cardRows = new();

        for (int row = 0; row < 7; row++)
        {
            List<string> pieces = new();

            foreach (string[] card in renderedCards)
            {
                pieces.Add(card[row]);
            }

            cardRows.Add(new Markup(
                string.Join("  ", pieces)
            ));
        }

        // Keeps all card rows centered.
        content.Add(new Align(
            new Rows(cardRows),
            HorizontalAlignment.Center
        ));
    }

    // Returns the rank and suit of a compact card.
    private string GetCardLabel(Card card)
    {
        string rank = card.Rank switch
        {
            Rank.Ace => "A",
            Rank.King => "K",
            Rank.Queen => "Q",
            Rank.Jack => "J",
            Rank.Ten => "10",
            Rank.Nine => "9",
            Rank.Eight => "8",
            Rank.Seven => "7",
            Rank.Six => "6",
            Rank.Five => "5",
            Rank.Four => "4",
            Rank.Three => "3",
            Rank.Two => "2",
            _ => "?"
        };

        string suit = card.Suit switch
        {
            Suit.Hearts => "♥",
            Suit.Diamonds => "♦",
            Suit.Clubs => "♣",
            Suit.Spades => "♠",
            _ => "?"
        };

        return rank + suit;
    }

    // Returns red or black depending on the suit.
    private string GetCardColor(Card card)
    {
        return card.Suit == Suit.Hearts ||
               card.Suit == Suit.Diamonds
            ? "red"
            : "black";
    }
}

// Applies green to segments without an existing background.
internal sealed class GreenTableBackground : Renderable
{
    private readonly IRenderable content;
    private readonly Color background;

    public GreenTableBackground(
        IRenderable content,
        Color background)
    {
        this.content = content;
        this.background = background;
    }

    protected override Measurement Measure(
        RenderOptions options,
        int maxWidth)
    {
        return content.Measure(options, maxWidth);
    }

    protected override IEnumerable<Segment> Render(
        RenderOptions options,
        int maxWidth)
    {
        Style greenStyle = new Style(
            background: background
        );

        foreach (Segment segment in content.Render(options, maxWidth))
        {
            // Preserves the white and blue card backgrounds.
            Style original = segment.Style;

            Style finalStyle = original.Background == Color.Default
                ? original.Combine(greenStyle)
                : original;

            yield return new Segment(
                segment.Text,
                finalStyle
            );
        }
    }
}
