
using Spectre.Console;

class CardRenderer
{
    private const int LowestRankValue = 2;
    
    // Converts the rank to a short text.
    public string GetRank(Rank rank)
    {
        return rank switch
        {
            Rank.Ace => "A",
            Rank.King => "K",
            Rank.Queen => "Q",
            Rank.Jack => "J",
            Rank.Ten => "10",
           _ => ((int)rank + LowestRankValue).ToString()
        };
    }

    // Converts the suit to a symbol.
    public string GetSuit(Suit suit)
    {
        return suit switch
        {
            Suit.Hearts => "♥",
            Suit.Diamonds => "♦",
            Suit.Clubs => "♣",
            Suit.Spades => "♠",
            _ => "?"
        };
    }

    // Creates a standard playing card.
    public string[] RenderCard(Card card)
    {
        string rank = GetRank(card.Rank);
        string suit = GetSuit(card.Suit);

        string color =
            card.Suit == Suit.Hearts ||
            card.Suit == Suit.Diamonds
                ? "red"
                : "black";

        string center = card.Rank switch
        {
            Rank.King => "♚",
            Rank.Queen => "♛",
            Rank.Jack => "♞",
            _ => suit
        };

        return new string[]
        {
            "[white on white]╭─────────╮[/]",
            $"[white on white]│[/][{color} on white]{rank.PadRight(9)}[/][white on white]│[/]",
            $"[white on white]│[/][{color} on white]{suit.PadRight(9)}[/][white on white]│[/]",
            $"[white on white]│[/][{color} on white]    {center}    [/][white on white]│[/]",
            $"[white on white]│[/][{color} on white]{suit.PadLeft(9)}[/][white on white]│[/]",
            $"[white on white]│[/][{color} on white]{rank.PadLeft(9)}[/][white on white]│[/]",
            "[white on white]╰─────────╯[/]"
        };
    }

    // Creates the back of a hidden card.
    public string[] RenderHiddenCard()
    {
        return new string[]
        {
            "[gold1 on #172A60]╭─────────╮[/]",
            "[gold1 on #172A60]│◇ ♠ ◇ ♠ ◇│[/]",
            "[gold1 on #172A60]│♠ ◇ ♠ ◇ ♠│[/]",
            "[gold1 on #172A60]│◇ ♠ ◇ ♠ ◇│[/]",
            "[gold1 on #172A60]│♠ ◇ ♠ ◇ ♠│[/]",
            "[gold1 on #172A60]│◇ ♠ ◇ ♠ ◇│[/]",
            "[gold1 on #172A60]╰─────────╯[/]"
        };
    }
}
