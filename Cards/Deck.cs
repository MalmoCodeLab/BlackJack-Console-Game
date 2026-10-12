class Deck
{
    public List<Card> DeckCards { get; } = [];

    public void CreateDeck()
    {
        for (int s = 0; s < 4; s++)
        {
            for (int r = 0; r < 13; r++)
            {
                Card card = new ((Suit)s, (Rank)r);
                DeckCards.Add(card);
            }
        }
    }

}

