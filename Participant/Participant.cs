abstract class Participant
{
    public string Name { get; }
    public Hand Hand { get; } = new Hand();

    protected Participant(string name)
    {
        Name = name.Trim();
    }

    // Hand drar själv kortet från leken
    public void ReceiveCard(Deck deck)
    {
        Hand.ReceiveCard(deck);
    }

    // Handens värde. Ess räknas som 11, men blir 1 om summan går över 21.
    // (Kan flyttas in i Hand om gruppen vill.)
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

    // Ny runda: släng korten på handen
    public virtual void NewRound()
    {
        Hand.DiscardCards();
    }
}
