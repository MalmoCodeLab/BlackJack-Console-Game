// Builds only the card artwork. ConsoleUi.Table.cs adds color and positioning.
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

    // 7 columns x 7 rows: same dimensions for EVERY player in multiplayer.
    public string[] RenderSmallCard(Card card)
    {
        string rank = GetRank(card.Rank);
        string suit = GetSuit(card.Suit);
        string center = GetCenterSymbol(card, suit);

        return new string[]
        {
            "╭─────╮",
            $"│{rank.PadRight(5)}│",
            $"│{suit.PadRight(5)}│",
            $"│  {center}  │",
            $"│{suit.PadLeft(5)}│",
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
