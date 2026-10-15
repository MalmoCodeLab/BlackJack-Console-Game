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

    public bool ShouldHit()
    {
        return Score < DealerStandsOn;
    }
    
    public override void NewRound(Deck deck)
    {
        base.NewRound(deck);
        HoleCardVisible = false;
    }
}