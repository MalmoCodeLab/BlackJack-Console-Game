// Builds only the card artwork. ConsoleUi.Table.cs adds color and positioning.
class CardRenderer
{
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

    // Original full-size artwork, retained for solo play.
    public string[] RenderCard(Card card)
    {
        string rank = GetRank(card.Rank);
        string suit = GetSuit(card.Suit);
        string center = GetCenterSymbol(card, suit);

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

    // Compact multiplayer cards: 7 columns x 5 rows.
    // Both visible and hidden cards have exactly the same dimensions.
    public string[] RenderSmallCard(Card card)
    {
        string rank = GetRank(card.Rank);
        string suit = GetSuit(card.Suit);
        string center = GetCenterSymbol(card, suit);

        return new string[]
        {
            "╭─────╮",
            $"│{rank.PadRight(3)} {suit}│",
            $"│  {center}  │",
            $"│{rank.PadLeft(5)}│",
            "╰─────╯"
        };
    }

    public string[] RenderSmallHiddenCard()
    {
        return new string[]
        {
            "╭─────╮",
            "│◇♠◇♠◇│",
            "│♠◇♠◇♠│",
            "│◇♠◇♠◇│",
            "╰─────╯"
        };
    }

    private static string GetCenterSymbol(Card card, string suit) => card.Rank switch
    {
        Rank.King => "♚",
        Rank.Queen => "♛",
        Rank.Jack => "♞",
        _ => suit
    };
}
