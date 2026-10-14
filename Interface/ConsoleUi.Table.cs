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

        if (players.Count == 1)
        {
            tableContent.Add(new Align(new Markup("[bold gold1]♣ PRIVAT SPELBORD ♣[/]"), HorizontalAlignment.Center));
            tableContent.Add(new Align(new Markup("[gold1]♥ Endast du och givaren ♥[/]"), HorizontalAlignment.Center));
        }
        else if (players.Count > 1)
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

        tableContent.Add(new Rule("[gold1]GIVARE[/]").RuleStyle(TableGold));
        AddRenderedCards(tableContent, dealer.Hand.CardsOnHand, !dealer.HoleCardVisible);

        int innerHeight = tableHeight > 0 ? Math.Max(1, tableHeight - 2) : 0;
        int dealerRows = dealer.Hand.CardsOnHand.Count > 0 ? 8 : 2;
        int playerRows = players.Count > 0 ? (players[0].Hand.CardsOnHand.Count > 0 ? 8 : 2) : 0;
        int multiplayerRows = players.Count > 1 ? 3 : 0;
        int bettingRows = 2;
        int fixedRows = dealerRows + playerRows + multiplayerRows + bettingRows;
        int spacerRows = Math.Max(0, innerHeight - fixedRows);

        // Avoid extra blank rows that push Player 1's cards below the table.
        int topSpace = 0;
        int bottomSpace = 0;
        for (int i = 0; i < topSpace; i++)
            tableContent.Add(new Text(" "));

        tableContent.Add(BettingUI.Create());
        for (int i = 0; i < bottomSpace; i++)
            tableContent.Add(new Text(" "));

        if (players.Count > 0)
        {
            tableContent.Add(new Rule($"[gold1]{Markup.Escape(players[0].Name.ToUpperInvariant())}[/]").RuleStyle(TableGold));
            AddRenderedCards(tableContent, players[0].Hand.CardsOnHand, false);

            // Leave a little space between the cards and the bottom border.
            tableContent.Add(new Text(" "));
            tableContent.Add(new Text(" "));
        }

        var greenTable = new GreenTableBackground(new Rows(tableContent), TableGreen, innerHeight);
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
            Rank.Ace => "A", Rank.King => "K", Rank.Queen => "Q", Rank.Jack => "J",
            Rank.Ten => "10", Rank.Nine => "9", Rank.Eight => "8", Rank.Seven => "7",
            Rank.Six => "6", Rank.Five => "5", Rank.Four => "4", Rank.Three => "3",
            Rank.Two => "2", _ => "?"
        };
        string suit = card.Suit switch
        {
            Suit.Hearts => "♥", Suit.Diamonds => "♦", Suit.Clubs => "♣",
            Suit.Spades => "♠", _ => "?"
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
        int width = Math.Max(1, maxWidth), currentWidth = 0, finishedRows = 0;
        bool atLineStart = true;

        foreach (Segment segment in content.Render(options, maxWidth))
        {
            if (segment.IsLineBreak)
            {
                if (currentWidth < width)
                    yield return new Segment(new string(' ', width - currentWidth), greenStyle);
                yield return segment;
                finishedRows++;
                currentWidth = 0;
                atLineStart = true;
            }
            else
            {
                Style original = segment.Style;
                Style finalStyle = original.Background == Color.Default
                    ? original.Combine(greenStyle)
                    : original;
                yield return new Segment(segment.Text, finalStyle);
                currentWidth += segment.CellCount();
                atLineStart = false;
            }
        }

        if (!atLineStart)
        {
            if (currentWidth < width)
                yield return new Segment(new string(' ', width - currentWidth), greenStyle);
            finishedRows++;
        }

        while (finishedRows < minimumRows)
        {
            if (finishedRows > 0)
                yield return Segment.LineBreak;
            yield return new Segment(new string(' ', width), greenStyle);
            finishedRows++;
        }
    }
}
