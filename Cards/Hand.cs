class Hand
{
    public List<Card> CardsOnHand { get; } = [];

    public void ReceiveCard(Deck deck)
    {
        Card recievedCard = deck.DealCard();
        CardsOnHand.Add(recievedCard);
    }

    
}
