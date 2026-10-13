using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

public partial class ConsoleUi
{
    private static readonly Color TableGreen = new Color(7, 80, 46);
    private static readonly Color TableGold = Color.Gold1;

    internal Panel BuildBlackjackTable(List<Player> players, Dealer dealer, int tableHeight = 0)
    {
        List<IRenderable> tableContent = new();

        if (players.Count > 1)
        {
            var playerGrid = new Grid();
            for (int i = 1; i < players.Count; i++)
                playerGrid.AddColumn(new GridColumn().NoWrap());

            List<IRenderable> names = new(), cards = new();
            for (int i = 1; i < players.Count; i++)
            {
                Player player = players[i];
                names.Add(new Markup($"[bold gold1]SPELARE {i + 1}[/]"));
                cards.Add(new Markup(BuildSmallCards(player.Hand.CardsOnHand)));
            }
            playerGrid.AddRow(names.ToArray());
            playerGrid.AddRow(cards.ToArray());
            tableContent.Add(new Align(playerGrid, HorizontalAlignment.Center));
        }
        else if (players.Count == 1)
        {
            // Show a gold title when playing alone against the dealer.
            tableContent.Add(new Align(
                new Markup("[bold gold1]♣ PRIVAT SPELBORD ♣[/]"),
                HorizontalAlignment.Center));

            tableContent.Add(new Align(
                new Markup("[gold1]♥ Endast du och givaren ♥[/]"),
                HorizontalAlignment.Center));
        }

        tableContent.Add(new Rule("[gold1]GIVARE[/]").RuleStyle(TableGold));
        // Always render all seven rows of each card.
        AddRenderedCards(tableContent, dealer.Hand.CardsOnHand, !dealer.HoleCardVisible);

        // Keep the chips between the dealer and Player 1.
        // Do not add blank rows here: the cards must fit inside the table.
        tableContent.Add(BettingUI.Create());

        if (players.Count > 0)
        {
            tableContent.Add(new Rule("[gold1]SPELARE 1[/]").RuleStyle(TableGold));
            AddRenderedCards(tableContent, players[0].Hand.CardsOnHand, false);
        }

        int innerHeight = tableHeight > 0 ? Math.Max(1, tableHeight - 2) : 0;
        var greenTable = new GreenTableBackground(
            new Rows(tableContent), TableGreen, innerHeight);

        return new Panel(greenTable)
            .Header("[gold1] ♠ BLACKJACK ♠ [/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(TableGold)
            .Expand();
    }

    internal void ShowBlackjackTable(List<Player> players, Dealer dealer)
    {
        AnsiConsole.Write(BuildBlackjackTable(players, dealer));
    }

    private string BuildSmallCards(List<Card> cards)
    {
        if (cards.Count == 0)
            return "[grey]Inga kort[/]";

        List<string> labels = new();
        foreach (Card card in cards)
        {
            string color = GetCardColor(card);
            labels.Add($"[{color} on white] {GetCardLabel(card)} [/]");
        }
        return string.Join(" ", labels);
    }

    private void AddRenderedCards(List<IRenderable> content, List<Card> cards, bool hideSecondCard)
    {
        if (cards.Count == 0)
        {
            content.Add(new Markup("[grey]Inga kort[/]"));
            return;
        }

        CardRenderer renderer = new();
        List<string[]> renderedCards = new();
        for (int i = 0; i < cards.Count; i++)
            renderedCards.Add(hideSecondCard && i == 1
                ? renderer.RenderHiddenCard()
                : renderer.RenderCard(cards[i]));

        List<IRenderable> cardRows = new();
        for (int row = 0; row < 7; row++)
        {
            List<string> pieces = new();
            foreach (string[] card in renderedCards)
                pieces.Add(card[row]);
            cardRows.Add(new Markup(string.Join("  ", pieces)));
        }
        content.Add(new Align(new Rows(cardRows), HorizontalAlignment.Center));
    }

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

    private string GetCardColor(Card card) =>
        card.Suit == Suit.Hearts || card.Suit == Suit.Diamonds ? "red" : "black";
}

internal sealed class GreenTableBackground : Renderable
{
    private readonly IRenderable content;
    private readonly Color background;
    private readonly int minimumRows;

    public GreenTableBackground(IRenderable content, Color background, int minimumRows = 0)
    {
        this.content = content;
        this.background = background;
        this.minimumRows = minimumRows;
    }

    protected override Measurement Measure(RenderOptions options, int maxWidth) =>
        content.Measure(options, maxWidth);

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        Style greenStyle = new Style(background: background);
        int width = Math.Max(1, maxWidth - 1);
        int currentWidth = 0;
        int renderedRows = 0;
        bool rowHasContent = false;

        // Paint every actual row, including its unused cells, green.
        foreach (Segment segment in content.Render(options, maxWidth))
        {
            if (segment.IsLineBreak)
            {
                if (currentWidth < width)
                    yield return new Segment(new string(' ', width - currentWidth), greenStyle);
                yield return Segment.LineBreak;
                renderedRows++;
                currentWidth = 0;
                rowHasContent = false;
            }
            else
            {
                Style style = segment.Style.Background == Color.Default
                    ? segment.Style.Combine(greenStyle)
                    : segment.Style;
                yield return new Segment(segment.Text, style);
                currentWidth += segment.CellCount();
                rowHasContent = true;
            }
        }

        // The last line may already have ended with a line break.
        // Do not add another one before padding: it creates a black stripe.
        if (rowHasContent)
        {
            if (currentWidth < width)
                yield return new Segment(new string(' ', width - currentWidth), greenStyle);
            renderedRows++;
        }

        // Fill only the remaining rows, without inserting empty black rows.
        bool lastEndedWithBreak = !rowHasContent && renderedRows > 0;
        while (renderedRows < minimumRows)
        {
            if (!lastEndedWithBreak)
                yield return Segment.LineBreak;
            yield return new Segment(new string(' ', width), greenStyle);
            renderedRows++;
            lastEndedWithBreak = false;
        }
    }
}
