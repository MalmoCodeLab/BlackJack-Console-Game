using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;
public partial class ConsoleUi
{
    private static readonly Color TableGreen = new Color(7, 80, 46);
    private static readonly Color TableGold = Color.Gold1;
    internal Panel BuildBlackjackTable(
        List<Player> players,
        Dealer dealer,
        int tableHeight = 0,
        string? notice = null,
        string[]? playerStatuses = null,
        bool cardsFaceDown = false,
        int? runningCount = null,
        double? trueCount = null,
        int activePlayerIndex = 0,
        bool showEmptyTable = false,
        int dealerCardsShown = -1,
        int[]? playerCardsShown = null)
    {
        List<IRenderable> tableContent = new();
        if (notice != null)
        {
            tableContent.Add(new Align(new Markup($"[bold white on #961923]  ★ {Markup.Escape(notice)} ★  [/]"), HorizontalAlignment.Center));
        }
        else if (runningCount.HasValue && trueCount.HasValue)
        {
            // Use the same two lines that previously displayed the marker stand.
            // This keeps the overall table height and card positions unchanged.
            string running = runningCount.Value.ToString("+0;-0;0");
            string actual = trueCount.Value.ToString("+0.0;-0.0;0.0", System.Globalization.CultureInfo.GetCultureInfo("sv-SE"));
            string label = trueCount.Value >= 1 ? "Gynnsamt" :
                trueCount.Value <= -1 ? "Ogynnsamt" : "Neutralt";
            string labelColor = trueCount.Value >= 1 ? "green1" :
                trueCount.Value <= -1 ? "red" : "gold1";
        }
        else
        {
            tableContent.Add(new Align(new Markup("[gold1]╭──────── MARKERSTÄLL ────────╮[/]"), HorizontalAlignment.Center));
            tableContent.Add(new Align(new Markup("[white]● ●[/]  [red]● ●[/]  [green]● ●[/]  [blue]● ●[/]  [yellow]● ●[/]"), HorizontalAlignment.Center));
        }
        // Reserve extra vertical space only when it fits inside the table.
        // This prevents the bottom action buttons from being pushed off screen.
        int innerHeight = tableHeight > 0 ? Math.Max(1, tableHeight - 2) : 0;
        int coreRows = players.Count > 1 ? 15 : 19; // Multiplayer: dealer (1) + cards (5) + players (7) + chips (2).
        int headerRows = notice is not null ? 1 :
            (runningCount.HasValue && trueCount.HasValue ? 0 : 2);
        int extraRows = Math.Max(0, innerHeight - coreRows - headerRows);

        // The largest gap belongs between the dealer's cards and the players.
        int playerGap = Math.Min(2, extraRows);
        extraRows -= playerGap;
        int topGap = Math.Min(1, extraRows);
        extraRows -= topGap;
        int dealerCardGap = Math.Min(1, extraRows);

        for (int i = 0; i < topGap; i++)
            tableContent.Add(new Text(" "));

        tableContent.Add(new Rule("[gold1]GIVARE[/]").RuleStyle(TableGold));

        for (int i = 0; i < dealerCardGap; i++)
            tableContent.Add(new Text(" "));

        // During betting the board is empty. During the animated deal, show
        // only the cards whose turn to appear has arrived. Do not touch hands.
        if (showEmptyTable || dealerCardsShown == 0)
        {
            int emptyCardRows = players.Count > 1 ? CardArtHeight : 7;
            for (int r = 0; r < emptyCardRows; r++)
                tableContent.Add(new Text(" "));
        }
        else
        {
            List<Card> shownDealerCards = dealerCardsShown < 0
                ? dealer.Hand.CardsOnHand
                : dealer.Hand.CardsOnHand.Take(dealerCardsShown).ToList();
            if (players.Count > 1)
                AddSmallDealerCards(tableContent, shownDealerCards,
                    !dealer.HoleCardVisible, cardsFaceDown);
            else
                AddRenderedCards(tableContent, shownDealerCards,
                    !dealer.HoleCardVisible, cardsFaceDown);
        }

        for (int i = 0; i < playerGap; i++)
            tableContent.Add(new Text(" "));

        // Solo uses full-size cards; multiplayer uses equally-sized compact cards.
        tableContent.Add(BuildPlayerRow(players, playerStatuses, cardsFaceDown,
            activePlayerIndex, showEmptyTable, playerCardsShown));
        tableContent.Add(BettingUI.Create());
        var greenTable = new GreenTableBackground(new Rows(tableContent), TableGreen, innerHeight);
        return new Panel(greenTable)
            .Header("[gold1] ♠ BLACKJACK ♠ [/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(TableGold)
            .Expand();
    }
    private static string GetHandType(Player player)
    {
        if (player.Score != Player.BlackjackValue) return "";
        if (player.HasBlackjack) return "NATURAL BLACKJACK";
        int total = 0, aces = 0;
        foreach (Card card in player.Hand.CardsOnHand)
        {
            total += card.IndividualCardValue(card);
            if (card.Rank == Rank.Ace) aces++;
        }
        while (total > Player.BlackjackValue && aces > 0) { total -= 10; aces--; }
        return aces > 0 ? "MJUK BLACKJACK" : "HÅRD BLACKJACK";
    }
    // Five equal 15-column seats (total 75 cells). Two 7-cell cards fit
    // next to each other in every seat, including the active player's seat.
    private const int SmallSeatWidth = 15;
    private const int SeatingWidth = 5 * SmallSeatWidth;
    private const int SmallCardWidth = 7;
    private const int CardArtHeight = 5;

    private IRenderable BuildPlayerRow(List<Player> players, string[]? playerStatuses,
        bool cardsFaceDown, int activePlayerIndex, bool showEmptyTable,
        int[]? playerCardsShown)
    {
        if (players.Count == 1)
            return BuildSoloPlayerRow(players[0], playerStatuses, cardsFaceDown,
                showEmptyTable, playerCardsShown);

        int activeIndex = Math.Clamp(activePlayerIndex, 0, players.Count - 1);
        int[] order = { -1, -1, activeIndex, -1, -1 };
        int[] sideSeats = players.Count switch
        {
            2 => new[] { 1 },
            3 => new[] { 1, 3 },
            4 => new[] { 0, 1, 3 },
            _ => new[] { 0, 1, 3, 4 }
        };

        int nextSideSeat = 0;
        for (int index = 0; index < players.Count; index++)
        {
            if (index != activeIndex)
                order[sideSeats[nextSideSeat++]] = index;
        }

        List<string[]> seatLines = new();
        foreach (int index in order)
        {
            string[] lines = new string[7]; // name + 5 card rows + status
            for (int row = 0; row < lines.Length; row++)
                lines[row] = new string(' ', SmallSeatWidth);

            if (index < 0)
            {
                seatLines.Add(lines);
                continue;
            }

            Player player = players[index];
            bool isActive = index == activeIndex;
            bool dealing = playerCardsShown != null;
            int numberShown = dealing && index < playerCardsShown!.Length
                ? Math.Clamp(playerCardsShown[index], 0, player.Hand.CardsOnHand.Count)
                : player.Hand.CardsOnHand.Count;
            List<Card> visibleCards = player.Hand.CardsOnHand.Take(numberShown).ToList();

            string name = player.Name.ToUpperInvariant();
            string handType = cardsFaceDown || showEmptyTable || dealing ? "" : GetHandType(player);
            if (handType.Length > 0)
                name += player.HasBlackjack ? " · BJ" : " · 21";
            // With very large hands the fan displays the latest nine cards.
            if (visibleCards.Count > 9)
                name += $" +{visibleCards.Count - 9}";
            if (name.Length > SmallSeatWidth)
                name = name[..SmallSeatWidth];
            lines[0] = CenterText(Markup.Escape(name), name.Length,
                SmallSeatWidth, isActive ? "bold gold1" : "white");

            if (!showEmptyTable && visibleCards.Count > 0)
            {
                bool[] hidden = Enumerable.Repeat(cardsFaceDown, visibleCards.Count).ToArray();
                List<string> cardLines = BuildSmallCardLines(
                    visibleCards, hidden, SmallSeatWidth, out int visibleWidth);
                for (int row = 0; row < CardArtHeight; row++)
                    lines[row + 1] = CenterMarkup(cardLines[row], visibleWidth, SmallSeatWidth);
            }

            string status = playerStatuses != null && index < playerStatuses.Length
                ? playerStatuses[index] : "";
            string style = status switch
            {
                "DIN TUR" => "bold black on gold1",
                "TJOCK" or "FÖRLORADE" => "bold red",
                "STANNAT" => "bold white",
                "BLACKJACK" or "21 POÄNG" or "OAVGJORT" => "bold gold1",
                "VANN" => "bold green1",
                _ => "grey"
            };
            if (isActive)
            {
                // Keep the bet visible without exceeding the equal-width seat.
                string bet = $"{player.CurrentBet} kr";
                status = string.IsNullOrEmpty(status) ? $"INSATS {bet}" : $"{status} · {bet}";
                style = "bold gold1";
            }
            if (status.Length > SmallSeatWidth)
                status = status[..SmallSeatWidth];
            lines[6] = CenterText(Markup.Escape(status), status.Length, SmallSeatWidth, style);
            seatLines.Add(lines);
        }

        List<IRenderable> output = new();
        for (int row = 0; row < 7; row++)
            output.Add(new Markup(string.Concat(seatLines.Select(seat => seat[row]))));
        return new Align(new Rows(output), HorizontalAlignment.Center);
    }

    private IRenderable BuildSoloPlayerRow(Player player, string[]? playerStatuses,
        bool cardsFaceDown, bool showEmptyTable, int[]? playerCardsShown)
    {
        // Both the dealer and the focused player are centered in SeatingWidth.
        // This stays aligned in solo mode and with up to five players.

        string name = player.Name.ToUpperInvariant();
        string handType = cardsFaceDown || showEmptyTable || playerCardsShown is not null
            ? "" : GetHandType(player);
        string label = handType.Length > 0 ? $"{name} · {handType}" : name;

        List<IRenderable> rows = new()
        {
            new Markup(CenterText(Markup.Escape(label), label.Length, SeatingWidth, "bold gold1"))
        };

        int shownCount = playerCardsShown is { Length: > 0 }
            ? Math.Max(0, playerCardsShown[0])
            : player.Hand.CardsOnHand.Count;
        List<Card> visibleHand = player.Hand.CardsOnHand.Take(shownCount).ToList();
        if (showEmptyTable || visibleHand.Count == 0)
        {
            for (int row = 0; row < 7; row++)
                rows.Add(new Text(" "));
        }
        else
        {
            int cardWidth = GetFanWidth(visibleHand.Count);
            foreach (string line in BuildFanCardLines(visibleHand, cardsFaceDown))
                rows.Add(new Markup(CenterMarkup(line, cardWidth, SeatingWidth)));
        }

        string status = playerStatuses is { Length: > 0 } ? playerStatuses[0] : "";
        string style = status switch
        {
            "DIN TUR" => "bold black on gold1",
            "TJOCK" or "FÖRLORADE" => "bold red",
            "BLACKJACK" or "21 POÄNG" or "OAVGJORT" => "bold gold1",
            "VANN" => "bold green1",
            "STANNAT" => "bold white",
            _ => "grey"
        };
        // Show the current bet, not the remaining balance.
        string chipsLine = $"{status}   INSATS: {player.CurrentBet}".Trim();
        rows.Add(new Markup(CenterText(Markup.Escape(chipsLine),
            chipsLine.Length, SeatingWidth, "bold gold1")));
        return new Align(new Rows(rows), HorizontalAlignment.Center);
    }

    private static string CenterMarkup(string markup, int visibleWidth, int width)
    {
        int left = Math.Max(0, (width - visibleWidth) / 2);
        int right = Math.Max(0, width - visibleWidth - left);
        return new string(' ', left) + markup + new string(' ', right);
    }
    private static string CenterText(string text, int visibleWidth, int width, string style) =>
        CenterMarkup($"[{style}]{text}[/]", visibleWidth, width);
    // Two full-size cards. Later cards overlap horizontally with a thin
    // dark border at each visible edge; the newest card is full-size.
    private List<string> BuildFanCardLines(List<Card> cards, bool cardsFaceDown = false)
    {
        List<string> result = new();
        if (cards.Count == 0)
        {
            result.Add("[grey]Inga kort[/]");
            for (int r = 1; r < 7; r++) result.Add("        ");
            return result;
        }
        CardRenderer renderer = new();
        int start = Math.Max(0, cards.Count - 10);
        List<string[]> rendered = new();
        for (int i = start; i < cards.Count; i++)
            rendered.Add(cardsFaceDown ? renderer.RenderHiddenCard() : renderer.RenderCard(cards[i]));
        for (int row = 0; row < 7; row++)
        {
            // Build each visual row from left to right. Colors are added here,
            // not inside CardRenderer, so changing the card art does not affect rules.
            string line = StyleCardRow(rendered[0][row], cards[start], cardsFaceDown, row);
            if (rendered.Count == 2)
            {
                line += "  " + StyleCardRow(rendered[1][row], cards[start + 1], cardsFaceDown, row);
            }
            else if (rendered.Count > 2)
            {
                line += "  ";
                for (int i = 1; i < rendered.Count - 1; i++)
                    line += cardsFaceDown ? "[gold1 on #172A60]│◇ [/]" : BuildCardEdge(cards[start + i], row);
                int last = rendered.Count - 1;
                line += StyleCardRow(rendered[last][row], cards[start + last], cardsFaceDown, row);
            }
            result.Add(line);
        }
        return result;
    }
    private static int GetFanWidth(int cardCount)
    {
        if (cardCount == 0) return 8;
        if (cardCount == 1) return 11;
        if (cardCount == 2) return 24;
        return 24 + 3 * (Math.Min(cardCount, 5) - 2);
    }
    // Three visible cells per covered card, including its dark outline.
    private string BuildCardEdge(Card card, int row)
    {
        string rank = GetRankLabel(card);
        string suit = GetSuitLabel(card);
        string color = GetCardColor(card);
        string text = row switch
        {
            0 => "╭──",
            1 => "│" + rank.PadRight(2),
            2 => "│" + suit + " ",
            6 => "╰──",
            _ => "│  "
        };
        return $"[{color} on white]{text}[/]";
    }
    // Kept for compatibility with a previous Game.cs version.
    // The screen is now rendered only by GameTableUI during betting.
    internal void ShowEmptyTableBeforeDeal(List<Player> players, Dealer dealer)
    {
        // No standalone screen; avoids the earlier terminal conflict.
    }

    internal void ShowBlackjackTable(List<Player> players, Dealer dealer)
    {
        AnsiConsole.Write(BuildBlackjackTable(players, dealer));
    }
    // Multiplayer dealer and players share the same 7x5 card artwork.
    private void AddSmallDealerCards(List<IRenderable> content, List<Card> cards,
        bool hideSecondCard, bool hideAllCards)
    {
        if (cards.Count == 0)
        {
            for (int row = 0; row < CardArtHeight; row++)
                content.Add(new Text(" "));
            return;
        }

        bool[] hidden = cards.Select((_, index) =>
            hideAllCards || (hideSecondCard && index == 1)).ToArray();
        List<string> artwork = BuildSmallCardLines(cards, hidden,
            SeatingWidth, out int renderedWidth);
        List<IRenderable> rows = new();
        foreach (string line in artwork)
            rows.Add(new Markup(CenterMarkup(line, renderedWidth, SeatingWidth)));
        content.Add(new Align(new Rows(rows), HorizontalAlignment.Center));
    }

    // The renderers supply only geometry; all color stays in ConsoleUi.
    // The two initial cards are shown in full, side by side. With 3+ cards,
    // older cards become narrow visible edges while the newest stays full.
    private List<string> BuildSmallCardLines(List<Card> cards,
        IReadOnlyList<bool> hidden, int maxWidth, out int renderedWidth)
    {
        int count = cards.Count;
        int fullRowWidth = count * SmallCardWidth + Math.Max(0, count - 1);
        bool showFullCards = fullRowWidth <= maxWidth;
        int edgeWidth = 0;
        int start = 0;

        if (showFullCards)
        {
            renderedWidth = fullRowWidth;
        }
        else
        {
            // Two cells per older card when possible; one for large hands.
            edgeWidth = 2 * (count - 1) + SmallCardWidth <= maxWidth ? 2 : 1;
            int numberToShow = Math.Min(count, 1 + (maxWidth - SmallCardWidth) / edgeWidth);
            start = count - numberToShow;
            renderedWidth = SmallCardWidth + (numberToShow - 1) * edgeWidth;
        }

        CardRenderer renderer = new();
        List<string[]> art = new();
        for (int index = start; index < count; index++)
            art.Add(hidden[index]
                ? renderer.RenderSmallHiddenCard()
                : renderer.RenderSmallCard(cards[index]));

        List<string> result = new();
        for (int row = 0; row < CardArtHeight; row++)
        {
            if (showFullCards)
            {
                List<string> pieces = new();
                for (int index = 0; index < count; index++)
                    pieces.Add(StyleCardRow(art[index][row], cards[index], hidden[index], row));
                result.Add(string.Join(" ", pieces));
            }
            else
            {
                string line = "";
                for (int index = start; index < count - 1; index++)
                    line += BuildSmallCardEdge(cards[index], hidden[index], row, edgeWidth);
                line += StyleCardRow(art[^1][row], cards[^1], hidden[^1], row);
                result.Add(line);
            }
        }
        return result;
    }

    private string BuildSmallCardEdge(Card card, bool hidden, int row, int width)
    {
        string text;
        if (width == 1)
        {
            text = row switch
            {
                0 => "╭",
                1 => hidden ? "◇" : GetRankLabel(card)[..1],
                2 => hidden ? "◇" : GetSuitLabel(card),
                4 => "╰",
                _ => "│"
            };
        }
        else
        {
            text = row switch
            {
                0 => "╭─",
                1 => hidden ? "│◇" : GetRankLabel(card).PadRight(2)[..2],
                2 => hidden ? "│◇" : "│" + GetSuitLabel(card),
                4 => "╰─",
                _ => "│ "
            };
        }
        return hidden
            ? $"[gold1 on #172A60]{Markup.Escape(text)}[/]"
            : $"[{GetCardColor(card)} on white]{Markup.Escape(text)}[/]";
    }

    private void AddRenderedCards(List<IRenderable> content, List<Card> cards, bool hideSecondCard, bool hideAllCards = false)
    {
        if (cards.Count == 0)
        {
            content.Add(new Markup("[grey]Inga kort[/]"));
            return;
        }
        CardRenderer renderer = new();
        List<string[]> renderedCards = new();
        List<bool> hiddenCards = new();
        for (int i = 0; i < cards.Count; i++)
        {
            bool hidden = hideAllCards || (hideSecondCard && i == 1);
            hiddenCards.Add(hidden);
            renderedCards.Add(hidden ? renderer.RenderHiddenCard() : renderer.RenderCard(cards[i]));
        }

        // Cards always remain 11 by 7 characters. The dealer's typical hand
        // uses full cards and never gets resized when the terminal changes.
        // Six cards fit with one space; very large hands use the fixed fan.
        if (cards.Count >= 7)
        {
            List<string> fanRows = BuildDealerFanCardLines(cards, hiddenCards);
            int fanWidth = GetFanWidth(cards.Count);
            var fanRenderables = new List<IRenderable>();
            foreach (string line in fanRows)
                fanRenderables.Add(new Markup(CenterMarkup(line, fanWidth, SeatingWidth)));
            content.Add(new Align(new Rows(fanRenderables), HorizontalAlignment.Center));
            return;
        }

        int gap = cards.Count == 6 ? 1 : 2;
        int cardsWidth = renderedCards.Count * 11 + Math.Max(0, renderedCards.Count - 1) * gap;
        List<IRenderable> alignedRows = new();
        for (int row = 0; row < 7; row++)
        {
            List<string> pieces = new();
            for (int i = 0; i < renderedCards.Count; i++)
                pieces.Add(StyleCardRow(renderedCards[i][row], cards[i], hiddenCards[i], row));

            string cardLine = string.Join(new string(' ', gap), pieces);
            alignedRows.Add(new Markup(CenterMarkup(cardLine, cardsWidth, SeatingWidth)));
        }
        content.Add(new Align(new Rows(alignedRows), HorizontalAlignment.Center));
    }

    // Extra-large dealer hands keep the original 11x7 card art. Middle cards
    // are represented by narrow edges instead of deforming the card graphics.
    private List<string> BuildDealerFanCardLines(List<Card> cards, List<bool> hidden)
    {
        CardRenderer renderer = new();
        int start = Math.Max(0, cards.Count - 5);
        List<string[]> shapes = new();
        for (int i = start; i < cards.Count; i++)
            shapes.Add(hidden[i] ? renderer.RenderHiddenCard() : renderer.RenderCard(cards[i]));

        List<string> rows = new();
        for (int row = 0; row < 7; row++)
        {
            string line = StyleCardRow(shapes[0][row], cards[start], hidden[start], row);
            line += "  ";
            for (int i = 1; i < shapes.Count - 1; i++)
            {
                int originalIndex = start + i;
                line += hidden[originalIndex]
                    ? "[gold1 on #172A60]│◇ [/]"
                    : BuildCardEdge(cards[originalIndex], row);
            }
            int last = shapes.Count - 1;
            line += StyleCardRow(shapes[last][row], cards[start + last], hidden[start + last], row);
            rows.Add(line);
        }
        return rows;
    }

    // Own the colors in ConsoleUi: the renderer supplies only plain card rows.
    // Works for both the full-size and compact multiplayer cards.
    private string StyleCardRow(string rawRow, Card card, bool hidden, int rowIndex)
    {
        if (hidden)
            return $"[gold1 on #172A60]{Markup.Escape(rawRow)}[/]";

        // The bottom border is on row 4 for compact cards and row 6 for solo cards.
        if (rawRow.StartsWith('╭') || rawRow.StartsWith('╰'))
            return $"[grey on white]{Markup.Escape(rawRow)}[/]";

        string color = GetCardColor(card);
        string leftBorder = Markup.Escape(rawRow[..1]);
        string middle = Markup.Escape(rawRow[1..^1]);
        string rightBorder = Markup.Escape(rawRow[^1..]);
        return $"[grey on white]{leftBorder}[/]" +
               $"[{color} on white]{middle}[/]" +
               $"[grey on white]{rightBorder}[/]";
    }
    private string GetRankLabel(Card card) => card.Rank switch
    {
        Rank.Ace => "A", Rank.King => "K", Rank.Queen => "Q", Rank.Jack => "J",
        Rank.Ten => "10", Rank.Nine => "9", Rank.Eight => "8", Rank.Seven => "7",
        Rank.Six => "6", Rank.Five => "5", Rank.Four => "4", Rank.Three => "3",
        Rank.Two => "2", _ => "?"
    };
    private string GetSuitLabel(Card card) => card.Suit switch
    {
        Suit.Hearts => "♥", Suit.Diamonds => "♦", Suit.Clubs => "♣",
        Suit.Spades => "♠", _ => "?"
    };
    private string GetCardLabel(Card card) => GetRankLabel(card) + GetSuitLabel(card);
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
                Style finalStyle = original.Background == Color.Default || original.Background == Color.Black
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
        // The content may already end with a line break. Do not add a second
        // one before the first padding row, or an unpainted black stripe appears.
        bool needsLineBreak = !atLineStart;
        while (finishedRows < minimumRows)
        {
            if (needsLineBreak)
                yield return Segment.LineBreak;
            yield return new Segment(new string(' ', width), greenStyle);
            finishedRows++;
            needsLineBreak = true;
        }
    }
}