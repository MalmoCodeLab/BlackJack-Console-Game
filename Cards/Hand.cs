class Hand
{
    public List<Card> CardsOnHand { get; } = [];

    public void ReceiveCard(Deck deck)
    {
        Card recievedCard = deck.DealCard();
        CardsOnHand.Add(recievedCard);
    }
    public List<Card> DiscardCards()
    {
        List<Card> CardsToDiscard = new List<Card>();

        int numberOfCards = CardsOnHand.Count;
        for (int i = 0; i < numberOfCards; i++)
        {
            CardsToDiscard.Add(CardsOnHand[0]);
            CardsOnHand.RemoveAt(0);
        }
        return CardsToDiscard;

    }


}
