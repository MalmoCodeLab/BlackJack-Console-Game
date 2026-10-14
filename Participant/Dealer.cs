class Dealer : Participant
{
    private const int DealerStandsOn = 17;

    public bool HoleCardVisible { get; private set; }

    public Dealer() : base("Dealer")
    {
    }

    public void ShowHoleCard()
    {
        HoleCardVisible = true;
    }

    // True when the dealer's face-up card is an ace (used for insurance).
    public bool ShowsAce => Hand.CardsOnHand.Count > 0 && Hand.CardsOnHand[0].Rank == Rank.Ace;

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
        return Score < DealerStandsOn;
    }

    public void PlayTurn(Deck deck)
    {
        ShowHoleCard();

        while (ShouldHit())
        {
            ReceiveCard(deck);
        }
    }

    public override void NewRound(Deck deck)
    {
        base.NewRound(deck);
        HoleCardVisible = false;
    }
}
