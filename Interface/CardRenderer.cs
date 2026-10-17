// Builds the shape of playing cards without choosing their colors.
// ConsoleUi.Table.cs is responsible for color and layout.
class CardRenderer
{
    // Converts the rank to short text.
    public string GetRank(Rank rank)
    {
        return rank switch
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

    // Returns seven unstyled rows. ConsoleUi places and colors the card.
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

    // Returns the seven unstyled rows of a face-down card.
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
