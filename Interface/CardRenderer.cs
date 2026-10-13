
class CardRenderer
{
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
            _ => ((int)rank + 2).ToString()
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

    // Creates a playing card with rounded borders.
    public string[] RenderCard(Card card)
    {
        string rank = GetRank(card.Rank);
        string suit = GetSuit(card.Suit);

        string center = card.Rank switch
        {
            Rank.King => "♚",
            Rank.Queen => "♛",
            Rank.Jack => "♞",
            _ => suit
        };

        return new string[]
        {
            "╭─────────╮",
            $"│{rank.PadRight(9)}│",
            $"│{suit.PadRight(9)}│",
            $"│    {center}    │",
            $"│{suit.PadLeft(9)}│",
            $"│{rank.PadLeft(9)}│",
            "╰─────────╯"
        };
    }

    // Creates the back of a hidden card.
    public string[] RenderHiddenCard()
    {
        return new string[]
        {
            "╭─────────╮",
            "│◇ ♠ ◇ ♠ ◇│",
            "│♠ ◇ ♠ ◇ ♠│",
            "│◇ ♠ ◇ ♠ ◇│",
            "│♠ ◇ ♠ ◇ ♠│",
            "│◇ ♠ ◇ ♠ ◇│",
            "╰─────────╯"
        };
    }
}
