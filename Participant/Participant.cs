abstract class Participant
{
    private const int NaturalBlackjackCards = 2;
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

    public void ReceiveCard(Deck deck, bool faceUp = true)
    {
        ArgumentNullException.ThrowIfNull(deck);
        Hand.ReceiveCard(deck, faceUp);
    }

    public int Score => Hand.Score;
    public bool IsBust => Score > Hand.BlackjackValue;
    public bool HasBlackjack => Hand.CardsOnHand.Count == NaturalBlackjackCards && Score == Hand.BlackjackValue;

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
}
