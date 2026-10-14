abstract class Participant
{
    public const int BlackjackValue = 21;

    public string Name { get; }
    public Hand Hand { get; } = new Hand();

    protected Participant(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Namnet får inte vara tomt.");
        }

        Name = name.Trim();
    }

    public void ReceiveCard(Deck deck)
    {
        Hand.ReceiveCard(deck);
    }

    public int Score
    {
        get
        {
            int total = 0;
            int aces = 0;

            foreach (Card card in Hand.CardsOnHand)
            {
                total += card.IndividualCardValue();
                if (card.Rank == Rank.Ace)
                {
                    aces++;
                }
            }

            while (total > BlackjackValue && aces > 0)
            {
                total -= 10;
                aces--;
            }

            return total;
        }
    }

    public bool IsBust => Score > BlackjackValue;
    public bool HasBlackjack => Hand.CardsOnHand.Count == 2 && Score == BlackjackValue;

    public override string ToString()
    {
        return Name;
    }

    public virtual void NewRound(Deck deck)
    {
        List<Card> discarded = Hand.DiscardCards();
        deck.DiscardedCards.AddRange(discarded);
    }
}
