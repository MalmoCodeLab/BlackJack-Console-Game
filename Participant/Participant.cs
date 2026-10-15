abstract class Participant
{
    public const int BlackjackValue = Hand.BlackjackValue;

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
        ArgumentNullException.ThrowIfNull(deck);
        Hand.ReceiveCard(deck);
    }

    public int Score => Hand.Score;

    public bool IsBust => Score > BlackjackValue;
    public bool HasBlackjack => Hand.CardsOnHand.Count == 2 && Score == BlackjackValue;

    public override string ToString()
    {
        return Name;
    }

    public virtual void NewRound(Deck deck)
    {
        ArgumentNullException.ThrowIfNull(deck);
        List<Card> discarded = Hand.DiscardCards();
        deck.DiscardedCards.AddRange(discarded);
    }

    public void NewRound()
    {
        Hand.DiscardCards();
    }
}
