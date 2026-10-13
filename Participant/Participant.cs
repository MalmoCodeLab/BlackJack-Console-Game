abstract class Participant
{
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

            while (total > 21 && aces > 0)
            {
                total -= 10;
                aces--;
            }

            return total;
        }
    }

    public bool IsBust => Score > 21;
    public bool HasBlackjack => Hand.CardsOnHand.Count == 2 && Score == 21;

    public override string ToString()
    {
        return Name;
    }

    public virtual void NewRound()
    {
        Hand.DiscardCards();
    }
}
