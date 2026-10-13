class Dealer : Participant
{
    public bool HoleCardVisible { get; private set; }

    public Dealer() : base("Dealer")
    {
    }

    public Card UpCard => Hand.CardsOnHand[0];

    public void ShowHoleCard()
    {
        HoleCardVisible = true;
    }

    public List<Card> VisibleCards()
    {
        if (HoleCardVisible)
        {
            return Hand.CardsOnHand;
        }

        List<Card> visible = new List<Card>();

        if (Hand.CardsOnHand.Count > 0)
        {
            visible.Add(Hand.CardsOnHand[0]);
        }

        return visible;
    }

    public bool ShouldHit()
    {
        return Score < 17;
    }

    public override void NewRound(Deck deck)
    {
        base.NewRound(deck);
        HoleCardVisible = false;
    }
}
