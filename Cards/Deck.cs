class Deck
{
    public List<Card> DeckCards { get; } = [];
    public List<Card> DiscardedCards { get; } = [];

    public void CreateDeck()
    {
        for (int s = 0; s < 4; s++)
        {
            for (int r = 0; r < 13; r++)
            {
                Card card = new((Suit)s, (Rank)r);
                DeckCards.Add(card);
            }
        }
    }
    public void ShuffleDeck()
    {
        for (int i = 51; i >= 0; i--)
        {
            int number = Random.Shared.Next(0, i+1);
            Card TempCard = DeckCards[number];
            DeckCards[number] = DeckCards[i];
            DeckCards[i] = TempCard;
        }

    }

}

